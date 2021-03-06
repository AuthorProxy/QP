using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Transactions;
using System.Xml.Linq;
using Quantumart.QP8.BLL.Facades;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Repository.ArticleRepositories.SearchParsers;
using Quantumart.QP8.BLL.Repository.ContentRepositories;
using Quantumart.QP8.BLL.Repository.FieldRepositories;
using Quantumart.QP8.BLL.Repository.Helpers;
using Quantumart.QP8.BLL.Services.API.Models;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.BLL.Services.MultistepActions.Export;
using Quantumart.QP8.Constants;
using Quantumart.QP8.DAL;
using Quantumart.QP8.DAL.DTO;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Utils;
using Quantumart.QP8.Utils.Sorting;

namespace Quantumart.QP8.BLL.Repository.ArticleRepositories
{
    public class ArticleRepository : IArticleRepository
    {
        int IArticleRepository.GetIdByGuid(Guid guid) => GetIdByGuid(guid);

        List<Article> IArticleRepository.GetByIds(int[] ids) => GetByIds(ids);

        Article IArticleRepository.GetById(int id) => GetById(id);

        bool IArticleRepository.IsExist(int id)
        {
            return QPContext.EFContext.ArticleSet.Any(a => a.Id == id);
        }

        public static Article GetById(int id)
        {
            return MapperFacade.ArticleMapper.GetBizObject(QPContext.EFContext.ArticleSet
                .Include("Status")
                .Include("Content")
                .Include("LastModifiedByUser")
                .Include("LockedByUser")
                .SingleOrDefault(n => n.Id == id)
            );
        }

        internal int GetIdByGuid(Guid guid) => GetIdsByGuids(new[] { guid })[0];

        public static List<Article> GetByIds(int[] ids)
        {
            return MapperFacade.ArticleMapper.GetBizList(QPContext.EFContext.ArticleSet
                .Include("Status")
                .Include("Content")
                .Include("LastModifiedByUser")
                .Include("LockedByUser")
                .Where(n => ids.Contains((int)n.Id))
                .ToList()
            );
        }

        internal static void LockForUpdate(int id)
        {
            using (var scope = new QPConnectionScope())
            {
                Common.LockArticleForUpdate(scope.DbConnection, id);
            }
        }

        internal static StatusHistoryListItem GetStatusHistoryItem(int id)
        {
            using (new QPConnectionScope())
            {
                var item = Common.GetStatusHistoryItem(QPConnectionScope.Current.DbConnection, id).FirstOrDefault();
                return item == null ? null : MapperFacade.StatusHistoryItemMapper.GetBizObject(item);
            }
        }

        internal static Article GetVirtualById(int id, int contentId)
        {
            using (new QPConnectionScope())
            {
                var result = MapperFacade.ArticleRowMapper.GetBizObject(GetData(id, contentId, QPContext.IsLive));
                if (result != null)
                {
                    result.ContentId = contentId;
                    result.Content = ContentRepository.GetById(result.ContentId);
                    result.LastModifiedByUser = UserRepository.GetById(result.LastModifiedBy, true);
                    result.Status = StatusTypeRepository.GetById(result.StatusTypeId);
                }

                return result;
            }
        }

        internal static string FillFullTextSearchParams(int contentId, string filter, IList<ArticleSearchQueryParam> searchQueryParams, ArticleFullTextSearchQueryParser ftsParser, out ArticleFullTextSearchParameter ftsOptions, out int[] extensionContentIds, out ContentReference[] contentReferences)
        {
            ftsOptions = GetFtsSearchParameter(ftsParser, searchQueryParams, ArticleFullTextSearchSettings.SearchResultLimit);
            var availableForList = QPContext.IsAdmin || Common.IsEntityAccessible(QPConnectionScope.Current.DbConnection, QPContext.CurrentUserId, EntityTypeCode.Content, contentId, ActionTypeCode.List);
            if (!availableForList || ftsOptions.HasError.HasValue && ftsOptions.HasError.Value)
            {
                filter = SqlFilterComposer.Compose(filter, "1 = 0");
            }

            if (searchQueryParams == null)
            {
                extensionContentIds = new int[0];
                contentReferences = new ContentReference[0];
            }
            else
            {
                extensionContentIds = searchQueryParams.Select(p => p.ContentID).Distinct().Where(id => !string.IsNullOrEmpty(id)).Select(int.Parse).ToArray();
                contentReferences = searchQueryParams
                    .Where(p => !string.IsNullOrEmpty(p.ContentID) && !string.IsNullOrEmpty(p.ReferenceFieldID))
                    .Select(p => new ContentReference(int.Parse(p.ContentID), int.Parse(p.ReferenceFieldID)))
                    .ToArray();
            }

            return filter;
        }

        internal static EntityTreeItem GetByIdForTree(int id, bool loadChildArticles, string filter)
        {
            var result = new EntityTreeItem();
            var article = GetById(id);
            if (article == null)
            {
                throw new Exception(string.Format(ArticleStrings.ArticleRepository_GetByIdForTree_NotFound, id));
            }

            result.Id = article.Id.ToString();
            result.Alias = article.AliasForTree;
            result.LockedByAnyone = article.LockedByAnyone;
            result.LockedByYou = article.LockedByYou;
            result.LockedByToolTip = article.LockedByToolTip;

            var treeField = FieldRepository.GetById(ContentRepository.GetTreeFieldId(article.ContentId));
            var children = GetArticleTreeForParentResult(article.Id, filter, treeField).ToList();

            result.HasChildren = children.Any();
            if (loadChildArticles)
            {
                result.Children = children;
            }

            return result;
        }

        internal static int GetCount(int contentId)
        {
            using (new QPConnectionScope())
            {
                return Common.CountArticles(QPConnectionScope.Current.DbConnection, contentId, true);
            }
        }

        internal static int GetCount(int contentId, bool includeArchive)
        {
            using (new QPConnectionScope())
            {
                return Common.CountArticles(QPConnectionScope.Current.DbConnection, contentId, includeArchive);
            }
        }

        internal static int GetCountNonArchive(int contentId)
        {
            using (new QPConnectionScope())
            {
                return Common.CountArticles(QPConnectionScope.Current.DbConnection, contentId, false);
            }
        }

        internal static IEnumerable<DataRow> GetList(int contentId, int[] selectedArticleIDs, ListCommand cmd, IList<ArticleSearchQueryParam> searchQueryParams, IList<ArticleContextQueryParam> contextQueryParams, string filter, ArticleFullTextSearchQueryParser ftsParser, bool? onlyIds, int[] filterIds, out int totalRecords)
        {
            using (new QPConnectionScope())
            {
                var content = ContentRepository.GetById(contentId);
                var contextFilter = GetContextFilter(contextQueryParams, content.Fields.ToList(), out var useMainTable);
                filter = FillFullTextSearchParams(contentId, filter, searchQueryParams, ftsParser, out var ftsOptions, out var extensionContentIds, out var contentReferences);

                var sqlParams = new List<SqlParameter>();
                var options = new ArticlePageOptions
                {
                    ContentId = contentId,
                    ExtensionContentIds = extensionContentIds,
                    ContentReferences = contentReferences.Distinct().ToArray(),
                    SelectedIDs = selectedArticleIDs,
                    FilterIds = filterIds,
                    FullTextSearch = ftsOptions,
                    LinkFilters = GetLinkSearchParameter(searchQueryParams),
                    CommonFilter = GetCommonFilter(searchQueryParams, filter, sqlParams),
                    RelationSecurityFilters = QPContext.IsAdmin ? null : GetRelationSecurityFilters(content.Fields),
                    PageSize = cmd.PageSize,
                    StartRecord = cmd.StartRecord,
                    SortExpression = ReplaceDynamicColumnsNamesInSortExpressions(cmd.SortExpression, content.Fields.ToList()),
                    UserId = QPContext.CurrentUserId,
                    UseSecurity = !QPContext.IsAdmin && content.AllowItemsPermission,
                    IsVirtual = content.IsVirtual,
                    ContextFilter = contextFilter,
                    UseMainTableForVariations = useMainTable,
                    VariationFieldName = content.Fields.SingleOrDefault(n => n.UseForVariations)?.Name,
                    OnlyIds = onlyIds.HasValue && onlyIds.Value,
                    UseSql2012Syntax = QPContext.CurrentSqlVersion.Major >= 11
                };

                return Common.GetArticlesPage(QPConnectionScope.Current.DbConnection, options, sqlParams, out totalRecords);
            }
        }

        internal static IEnumerable<Article> GetList(IList<int> ids, bool loadFieldValues = false)
        {
            using (new QPConnectionScope())
            {
                var result = new List<Article>().AsEnumerable();
                if (ids != null && ids.Any())
                {
                    var contentId = (int)Common.GetContentIdForArticle(QPConnectionScope.Current.DbConnection, ids.First());
                    if (contentId != 0)
                    {
                        var data = GetData(ids, contentId);
                        result = InternalGetList(contentId, data, loadFieldValues);
                    }
                }

                return result;
            }
        }

        internal static IEnumerable<Article> GetList(int contentId)
        {
            var data = GetData(null, contentId);
            var result = InternalGetList(contentId, data, true);
            return result;
        }

        private static IEnumerable<Article> InternalGetList(int contentId, DataTable data, bool loadFieldValues)
        {
            IEnumerable<Article> result = new List<Article>();
            if (data != null)
            {
                var content = ContentRepository.GetById(contentId);
                result = data.AsEnumerable().Select(n => new Article
                {
                    ContentId = contentId,
                    Content = content,
                    Id = Converter.ToInt32(n["content_item_id"]),
                    Splitted = (bool)n["splitted"],
                    Delayed = (bool)n["schedule_new_version_publication"],
                    Created = (DateTime)n["created"],
                    Modified = (DateTime)n["modified"],
                    Archived = Converter.ToBoolean(n["archive"]),
                    Visible = Converter.ToBoolean(n["visible"]),
                    StatusTypeId = Converter.ToInt32(n["status_type_id"]),
                    LastModifiedBy = Converter.ToInt32(n["last_modified_by"]),
                    LockedBy = Converter.ToInt32(n["locked_by"])
                }).ToList();

                var statusTypeIds = result.Select(n => n.StatusTypeId).Distinct().ToArray();
                var userIds = result.Select(n => n.LastModifiedBy).Union(result.Select(n => n.LockedBy)).Where(n => n != 0).Distinct().ToArray();

                var users = UserRepository.GetList(userIds).ToDictionary(n => n.Id, n => n);
                var statuses = StatusTypeRepository.GetList(statusTypeIds).ToDictionary(n => n.Id, n => n);
                foreach (var article in result)
                {
                    article.Status = statuses[article.StatusTypeId];
                    article.LastModifiedByUser = users[article.LastModifiedBy];
                    if (article.LockedBy != 0)
                    {
                        article.LockedByUser = users[article.LockedBy];
                    }
                }

                if (loadFieldValues)
                {
                    var fields = FieldRepository.GetFullList(contentId);
                    Article.LoadFieldValuesForArticles(data, fields, result, contentId);
                }
            }

            return result;
        }

        internal static List<ArticleListItem> GetLockedList(ListCommand cmd, out int totalRecords)
        {
            using (var scope = new QPConnectionScope())
            {
                return MapperFacade.ArticleListItemRowMapper.GetBizList(Common.GetLockedArticlesList(scope.DbConnection, cmd.SortExpression, cmd.StartRecord, cmd.PageSize, QPContext.CurrentUserId, out totalRecords));
            }
        }

        internal static int GetLockedCount()
        {
            using (var scope = new QPConnectionScope())
            {
                return Common.GetLockedArticlesCount(scope.DbConnection, QPContext.CurrentUserId);
            }
        }

        internal static List<ArticleListItem> GetArticlesForApprovalList(ListCommand cmd, out int totalRecords)
        {
            using (var scope = new QPConnectionScope())
            {
                return MapperFacade.ArticleListItemRowMapper.GetBizList(Common.GetArticlesWaitingForApproval(scope.DbConnection, cmd.SortExpression, cmd.StartRecord, cmd.PageSize, QPContext.CurrentUserId, out totalRecords));
            }
        }

        internal static int GetForApprovalCount()
        {
            using (var scope = new QPConnectionScope())
            {
                return Common.GetArticlesWaitingForApprovalCount(scope.DbConnection, QPContext.CurrentUserId);
            }
        }

        private static IEnumerable<ArticleRelationSecurityParameter> GetRelationSecurityFilters(IEnumerable<Field> fields)
        {
            return fields.Where(n => n.UseRelationSecurity && !n.IsClassifier && n.RelatedToContent.AllowItemsPermission || n.UseTypeSecurity).Select(n => new ArticleRelationSecurityParameter
            {
                FieldId = n.Id,
                FieldName = n.Name,
                IsManyToMany = n.ExactType == FieldExactTypes.M2MRelation,
                IsClassifier = n.IsClassifier,
                AllowedContentIds = AllowedContentIds(n),
                RelatedContentId = n.RelateToContentId ?? 0,
                LinkId = n.LinkId
            });
        }

        private static int[] AllowedContentIds(Field n)
        {
            var result = new[] { 0 };
            if (n.IsClassifier)
            {
                var agg = FieldRepository.GetAggregatableContentIdsForClassifier(n);
                if (agg != null && agg.Any())
                {
                    result = agg;
                }
            }

            return result;
        }

        private static ArticleFullTextSearchParameter GetFtsSearchParameter(ArticleFullTextSearchQueryParser ftsParser, IEnumerable<ArticleSearchQueryParam> searchQueryParams, int searchResultLimit)
        {
            if (!ftsParser.Parse(searchQueryParams, out var ftsHasError, out var ftsFieldIdList, out var ftsQueryString, out var rawQueryString))
            {
                ftsHasError = null;
                ftsFieldIdList = null;
                ftsQueryString = null;
                rawQueryString = null;
            }

            return new ArticleFullTextSearchParameter { HasError = ftsHasError, FieldIdList = ftsFieldIdList, QueryString = ftsQueryString, RawQueryString = rawQueryString, SearchResultLimit = searchResultLimit };
        }

        public static string GetCommonFilter(IList<ArticleSearchQueryParam> searchQueryParams, string filter, IList<SqlParameter> sqlParams)
            => SqlFilterComposer.Compose(new ArticleFilterSearchQueryParser().GetFilter(searchQueryParams, sqlParams), filter);

        public static IEnumerable<ArticleLinkSearchParameter> GetLinkSearchParameter(IEnumerable<ArticleSearchQueryParam> searchQueryParams)
            => new ArticleLinkSearchQueryParser(new ArticleSearchRepository()).Parse(searchQueryParams);

        private static string GetContextFilter(IList<ArticleContextQueryParam> contextQueryParams, IList<Field> fields, out bool useMainTable)
        {
            var variationField = fields.SingleOrDefault(n => n.UseForVariations);
            if (variationField == null)
            {
                useMainTable = true;
                return string.Empty;
            }

            if (contextQueryParams == null || contextQueryParams.All(n => string.IsNullOrEmpty(n.Value)))
            {
                useMainTable = true;
                return $"c.[{variationField.Name}] IS NULL";
            }

            useMainTable = false;
            return SqlFilterComposer.Compose(contextQueryParams.Select(q => string.Format(string.IsNullOrEmpty(q.Value) ? "c.[{0}] IS NULL" : "c.[{0}] = {1}", fields.Single(n => n.Id == q.FieldId).Name, q.Value)));
        }

        internal static List<ListItem> GetSimpleList(int contentId, int? articleId, int? fieldId, ListSelectionMode selectionMode, int[] selectedArticleIDs, string filter, int testArticleId)
        {
            using (var scope = new QPConnectionScope())
            {
                var field = fieldId.HasValue ? FieldRepository.GetById(fieldId.Value) : null;
                var fields = ContentRepository.GetDisplayFields(contentId, field).ToList();
                var displayExpression = GetDisplayExpression(fields);

                var orderByExpression = field != null && (field.ExactType == FieldExactTypes.M2MRelation || field.ExactType == FieldExactTypes.M2ORelation)
                    ? GetSimpleListOrderExpression(field, fields)
                    : string.Empty;

                var selection = new HashSet<int>(selectedArticleIDs ?? new int[] { });
                if (field != null && testArticleId != 0 && articleId.HasValue && (field.ExactType == FieldExactTypes.M2MRelation || field.ExactType == FieldExactTypes.M2ORelation))
                {
                    var testResult = field.ExactType == FieldExactTypes.M2MRelation && field.LinkId.HasValue
                        ? Common.TestM2MValue(scope.DbConnection, field.LinkId.Value, articleId.Value, testArticleId)
                        : Common.TestM2OValue(scope.DbConnection, field.BackRelation.ContentId, field.BackRelation.Name, articleId.Value, testArticleId);

                    if (testResult)
                    {
                        selection.Add(testArticleId);
                    }
                    else
                    {
                        selection.Remove(testArticleId);
                    }
                }

                var isUserAdmin = QPContext.IsAdmin;
                var availableForList = isUserAdmin || Common.IsEntityAccessible(QPConnectionScope.Current.DbConnection, QPContext.CurrentUserId, EntityTypeCode.Content, contentId, ActionTypeCode.List);
                if (!availableForList)
                {
                    filter = SqlFilterComposer.Compose(filter, "1 = 0");
                }

                var useSecurity = !isUserAdmin && ContentRepository.IsArticlePermissionsAllowed(contentId);
                var extraFrom = GetExtraFromForRelations(fields);
                var rows = Common.GetArticlesSimpleList(
                    QPConnectionScope.Current.DbConnection,
                    QPContext.CurrentUserId,
                    contentId,
                    displayExpression,
                    selectionMode,
                    PermissionLevel.List,
                    filter,
                    useSecurity,
                    selection.ToArray(),
                    null,
                    null,
                    string.Empty,
                    extraFrom,
                    orderByExpression);

                return rows.Select(r => new ListItem
                {
                    Text = Cleaner.RemoveAllHtmlTags(r.Field<string>("title")),
                    Value = r["id"].ToString(),
                    Selected = bool.Parse(r["is_selected"].ToString())
                }).ToList();
            }
        }

        internal static IEnumerable<EntityTreeItem> GetArticlesTreeForFtsResult(string commonFilter, Field treeField, string filterQuery, IList<ArticleLinkSearchParameter> linkedFilters, IList<ArticleContextQueryParam> contextQuery, ICollection<SqlParameter> filterSqlParams, int[] extensionContentIds, ArticleFullTextSearchParameter ftsOptions)
        {
            using (var scope = new QPConnectionScope())
            {
                var searchFilterQuery = GetSearchFiltersQuery(commonFilter, treeField, filterQuery, linkedFilters, contextQuery, filterSqlParams, ftsOptions.SearchResultLimit);
                var searchIds = Common.GetFilterAndFtsSearchResult(scope.DbConnection, treeField.ContentId, extensionContentIds, ftsOptions, searchFilterQuery, filterSqlParams).ToList();
                var parentIds = Common.GetParentIdsTreeResult(scope.DbConnection, searchIds, treeField.Id, treeField.Name);
                var treeItems = GetArticleTreeFilteredResult(parentIds, commonFilter, treeField).ToList();
                var treeItemsDict = treeItems.ToDictionary(kv => kv.Id);
                foreach (var kv in treeItemsDict.Where(ti => searchIds.Contains(int.Parse(ti.Key))))
                {
                    kv.Value.IsHighlighted = true;
                }

                foreach (var kv in treeItemsDict.Where(kv => kv.Value.ParentId.HasValue))
                {
                    var parentEntity = treeItemsDict.Single(oe => oe.Key == kv.Value.ParentId.ToString());
                    var childList = (parentEntity.Value.Children ?? new List<EntityTreeItem>()).ToList();
                    childList.Add(kv.Value);
                    parentEntity.Value.Children = childList;
                }

                return treeItems.Where(kv => !kv.ParentId.HasValue);
            }
        }

        internal static string GetSearchFiltersQuery(string commonFilter, Field treeField, string filterQuery, IList<ArticleLinkSearchParameter> linkedFilters, IList<ArticleContextQueryParam> contextQuery, ICollection<SqlParameter> filterSqlParams, int searchLimit)
        {
            if (string.IsNullOrEmpty(filterQuery) && (linkedFilters == null || !linkedFilters.Any()))
            {
                return string.Empty;
            }

            var content = ContentRepository.GetById(treeField.ContentId);
            var contextFilter = GetContextFilter(contextQuery, content.Fields.ToList(), out bool _);
            var whereBuilder = new StringBuilder(SqlFilterComposer.Compose(filterQuery, commonFilter, contextFilter));

            Common.AddLinkFilteringToQuery(linkedFilters, whereBuilder, filterSqlParams);
            return $"SELECT DISTINCT TOP({searchLimit}) c.content_item_id from content_{treeField.ContentId}_united c WHERE {whereBuilder}";
        }

        internal static IEnumerable<EntityTreeItem> GetArticleTreeForParentResult(int? rootId, string commonFilter, Field treeField)
        {
            var extraFilter = commonFilter.Replace("c.", "cnt.");
            commonFilter = SqlFilterComposer.Compose(commonFilter, rootId.HasValue
                ? $"c.[{treeField.Name}] = {rootId.Value}"
                : $"c.[{treeField.Name}] IS NULL");

            return GetArticleTreeItemsResult(commonFilter, extraFilter, treeField, null);
        }

        internal static IEnumerable<EntityTreeItem> GetArticleTreeFilteredResult(IList<int> idsToFilter, string commonFilter, Field treeField)
        {
            var extraFilter = commonFilter.Replace("c.", "cnt.");
            return idsToFilter == null
                ? GetArticleTreeForParentResult(null, commonFilter, treeField)
                : GetArticleTreeItemsResult($"{commonFilter} AND c.content_item_id IN (SELECT id FROM @ids) ", extraFilter, treeField, idsToFilter);
        }

        internal static IEnumerable<EntityTreeItem> GetArticleTreeItemsResult(string commonFilter, string extraFilter, Field treeField, IList<int> idsToFilter)
        {
            using (var scope = new QPConnectionScope())
            {
                var useSecurity = !QPContext.IsAdmin && ContentRepository.IsArticlePermissionsAllowed(treeField.ContentId);
                var extraSelect = $", c.{treeField.Name} AS parentId" +
                    ", cil.locked_by" +
                    ", lu.first_name + ' ' + lu.last_name AS locker_name" +
                    ", CAST(CASE WHEN (" +
                    $"SELECT COUNT(content_item_id) FROM content_{treeField.ContentId}_united cnt " +
                    $"WHERE [{treeField.Name}] = c.content_item_id AND {extraFilter}" +
                    ") > 0 THEN 1 ELSE 0 END AS bit) AS has_children ";

                var fields = treeField.TreeFieldTitleCount <= 1
                    ? null
                    : ((IContentRepository)new ContentRepository()).GetDisplayFieldIds(treeField.ContentId, treeField.IncludeRelationsInTitle, treeField.Id)
                    .Take(treeField.TreeFieldTitleCount)
                    .Select(FieldRepository.GetById).ToList();

                fields = fields ?? new[] { treeField.Relation }.ToList();
                var extraFrom = " LEFT JOIN content_item cil ON c.content_item_id = cil.content_item_id AND locked_by IS NOT NULL LEFT JOIN users lu ON lu.user_id = cil.locked_by " + GetExtraFromForRelations(fields);
                var rows = Common.GetArticlesSimpleList(
                    scope.DbConnection,
                    QPContext.CurrentUserId,
                    treeField.ContentId,
                    GetDisplayExpression(fields),
                    ListSelectionMode.AllItems,
                    PermissionLevel.List,
                    commonFilter,
                    useSecurity,
                    null,
                    idsToFilter,
                    ArticleFullTextSearchSettings.SearchResultLimit,
                    extraSelect,
                    extraFrom,
                    GetSimpleListOrderExpression(treeField, fields));

                return rows.Select(dr =>
                {
                    var id = dr.Field<decimal>("id");
                    var result = new EntityTreeItem
                    {
                        Id = id.ToString(CultureInfo.InvariantCulture),
                        ParentId = (int?)dr.Field<decimal?>("parentId"),
                        Alias = Cleaner.RemoveAllHtmlTags(dr.Field<string>("title")),
                        HasChildren = dr.Field<bool>("has_children")
                    };

                    var lockedBy = (int?)dr.Field<decimal?>("locked_by");
                    if (lockedBy.HasValue)
                    {
                        result.LockedByToolTip = LockableEntityObject.GetLockedByToolTip(lockedBy.Value, dr.Field<string>("locker_name"));
                        result.LockedByYou = LockableEntityObject.IsLockedByYou(lockedBy.Value);
                        result.LockedByAnyone = LockableEntityObject.IsLockedByAnyone(lockedBy.Value);
                    }

                    return result;
                });
            }
        }

        private static string GetExtraFromForRelations(IEnumerable<Field> displayFields)
        {
            var sb = new StringBuilder();
            var relCounter = 0;
            foreach (var field in displayFields.Where(field => field.ExactType == FieldExactTypes.O2MRelation))
            {
                relCounter++;
                sb.AppendFormatLine(" left join content_{0} as rel_{1} on c.[{2}] = rel_{1}.content_item_id ", field.RelateToContentId, relCounter, field.Name);
            }

            return sb.ToString();
        }

        private static string GetExtraFromForRelations(IEnumerable<ExportSettings.FieldSetting> displayFields)
        {
            var sb = new StringBuilder();
            foreach (var field in displayFields.Where(n => n.ExactType == FieldExactTypes.O2MRelation))
            {
                sb.AppendFormatLine(" left join content_{0} as {1} on base.[{2}] = {1}.content_item_id ", field.RelatedContentId, field.TableAlias, field.Name);
                foreach (var f in field.Related.Where(n => n.ExactType == FieldExactTypes.O2MRelation))
                {
                    sb.AppendFormatLine(" left join content_{0} as {1} on {3}.[{2}] = {1}.content_item_id ", f.RelatedContentId, f.TableAlias, f.Name, field.TableAlias);
                }
            }

            return sb.ToString();
        }

        private static string GetDisplayExpression(IEnumerable<Field> displayFields)
        {
            var parts = new List<string>();
            var relCounter = 0;
            foreach (var field in displayFields)
            {
                if (field.ExactType == FieldExactTypes.M2MRelation)
                {
                    // ReSharper disable once PossibleInvalidOperationException
                    var content = ContentRepository.GetById(field.RelateToContentId.Value);
                    List<int> fieldIds;
                    if (content.UserQueryContentViewSchema.Any())
                    {
                        fieldIds = content.UserQueryContentViewSchema.SelectUniqContentIDs().OrderBy(i => i).ToList();
                    }
                    else
                    {
                        fieldIds = content.JoinRootId.HasValue
                            ? new[] { content.JoinRootId.Value }.ToList()
                            : (content.UnionSourceContentIDs.Any()
                                ? content.UnionSourceContentIDs
                                : new[] { field.RelateToContentId.Value }).ToList();
                    }

                    if (fieldIds.Any() && field.LinkId.HasValue)
                    {
                        foreach (var id in fieldIds)
                        {
                            var displayField = ContentRepository.GetTitleField(id);
                            parts.Add($"dbo.qp_link_titles({field.LinkId.Value}, c.content_item_id, {displayField.Id}, 255)");
                        }
                    }
                }
                else if (field.ExactType == FieldExactTypes.O2MRelation)
                {
                    relCounter++;
                    parts.Add($"isnull(cast ( rel_{relCounter}.[{field.Relation.Name}] as nvarchar(255)), '')");
                }
                else if (field.IsDateTime || field.IsBlob || field.Type.DbType == DbType.Decimal)
                {
                    parts.Add($"isnull(cast ( c.[{field.Name}] as nvarchar(255)), '')");
                }
                else
                {
                    parts.Add($"isnull(c.[{field.Name}], '')");
                }
            }

            return (parts.Any() ? string.Join(" + '; ' + ", parts) : "cast(c.content_item_id as nvarchar(255))") + " as title";
        }

        private static string GetSimpleListOrderExpression(Field field, IEnumerable<Field> displayFields)
        {
            var titleNames = displayFields.Select(n => n.Name).ToList();
            var orderFieldName = string.Empty;
            var orderFieldMatchesTitle = false;
            if (field.OrderFieldId.HasValue)
            {
                orderFieldName = FieldRepository.GetById(field.OrderFieldId.Value).Name;
                if (titleNames.Count == 1 && titleNames.First().Equals(orderFieldName, StringComparison.InvariantCultureIgnoreCase))
                {
                    orderFieldMatchesTitle = true;
                }
            }

            var orderExpression = field.OrderByTitle && !orderFieldMatchesTitle ? "title asc" : "c.content_item_id asc";
            if (!string.IsNullOrEmpty(orderFieldName))
            {
                orderExpression = $"c.[{orderFieldName}] asc, {orderExpression}";
            }

            return orderExpression;
        }

        internal static DataRow GetData(int id, int contentId, bool isLive)
        {
            using (new QPConnectionScope())
            {
                return id == 0
                    ? Common.GetDefaultArticleRow(QPConnectionScope.Current.DbConnection, contentId)
                    : Common.GetArticleRow(QPConnectionScope.Current.DbConnection, id, contentId, isLive);
            }
        }

        internal static DataTable GetData(IEnumerable<int> ids, int contentId)
        {
            using (new QPConnectionScope())
            {
                return Common.GetArticleTable(QPConnectionScope.Current.DbConnection, ids, contentId, QPContext.IsLive);
            }
        }

        internal static string GetFieldValue(int id, int contentId, string name)
        {
            using (new QPConnectionScope())
            {
                return Common.GetArticleFieldValue(QPConnectionScope.Current.DbConnection, id, contentId, name);
            }
        }

        internal static Dictionary<int, string> GetContentFieldValues(int contentId, string name)
        {
            using (new QPConnectionScope())
            {
                return Common.GetContentFieldValues(QPConnectionScope.Current.DbConnection, contentId, name);
            }
        }

        internal static int GetArticleIdByFieldValue(int contentId, string name, string value)
        {
            using (new QPConnectionScope())
            {
                return Common.GetArticleIdByFieldValue(QPConnectionScope.Current.DbConnection, contentId, name, value);
            }
        }

        internal static string[] GetFieldValues(int[] ids, int contentId, int fieldId)
        {
            using (new QPConnectionScope())
            {
                var fieldName = Common.GetFieldName(QPConnectionScope.Current.DbConnection, fieldId);
                return Common.GetArticleFieldValues(QPConnectionScope.Current.DbConnection, ids, contentId, fieldName);
            }
        }

        internal static string[] GetFieldValues(int[] ids, int contentId, string fieldName)
        {
            using (new QPConnectionScope())
            {
                return Common.GetArticleFieldValues(QPConnectionScope.Current.DbConnection, ids, contentId, fieldName);
            }
        }

        internal static Article Copy(Article article)
        {
            var id = article.Id;
            article.PrepareForCopy(false, true);
            var result = CreateOrUpdate(article);
            using (new QPConnectionScope())
            {
                Common.AdjustManyToMany(QPConnectionScope.Current.DbConnection, id, result.Id);
            }

            return result;
        }

        internal static Article CreateOrUpdate(Article article)
        {
            var articleUpdater = new ArticleUpdateService(article);
            var schedule = article.Schedule;
            article = articleUpdater.Update();
            article.Schedule = schedule;
            ScheduleRepository.UpdateSchedule(article);
            ScheduleRepository.CopyScheduleToChildDelays(article);
            return article;
        }

        internal static void MultipleDelete(IList<int> ids, bool withAggregated = false, bool withAutoArchive = false)
        {
            using (new QPConnectionScope())
            {
                if (withAutoArchive)
                {
                    var newIds = Common.GetIdsToAutoArchive(QPConnectionScope.Current.DbConnection, ids);
                    Common.SetArchiveFlag(QPConnectionScope.Current.DbConnection, newIds, QPContext.CurrentUserId, true, withAggregated);
                    Common.DeleteArticles(QPConnectionScope.Current.DbConnection, ids.Except(newIds).ToList(), withAggregated);
                }
                else
                {
                    Common.DeleteArticles(QPConnectionScope.Current.DbConnection, ids.ToList(), withAggregated);
                }
            }
        }

        internal static void UnlockArticlesByUser(int[] ids)
        {
            using (new QPConnectionScope())
            {
                Common.UnlockArticlesLockedByUser(QPConnectionScope.Current.DbConnection, QPContext.CurrentUserId, ids);
            }
        }

        internal static bool ValidateUnique(List<FieldValue> fieldValuesToTest) => ValidateUnique(fieldValuesToTest, out string _, out string _);

        internal static bool ValidateUnique(List<FieldValue> fieldValuesToTest, out string constraintToDisplay, out string conflictingIds)
        {
            if (fieldValuesToTest.Any())
            {
                var id = fieldValuesToTest[0].Article.Id;
                var contentId = fieldValuesToTest[0].Article.Content.Id;
                var filters = fieldValuesToTest.Select(GetSqlExpression).Union(Enumerable.Repeat("ARCHIVE = 0", 1));
                var condition = SqlFilterComposer.Compose(filters);
                var parameters = fieldValuesToTest.Select(n => new Common.FieldParameter { Name = n.Field.FormName, DbType = n.Field.Type.DbType, Value = n.Value }).ToList();
                using (new QPConnectionScope())
                {
                    conflictingIds = Common.GetConflictIds(QPConnectionScope.Current.DbConnection, id, contentId, condition, parameters);
                }

                constraintToDisplay = string.Join(", ", fieldValuesToTest.Select(GetDisplayExpression));
                return string.IsNullOrEmpty(conflictingIds);
            }

            constraintToDisplay = string.Empty;
            conflictingIds = string.Empty;
            return true;
        }

        internal static int CountDuplicates(ContentConstraint constraint, int[] restrictToIds, int exceptFieldId)
        {
            if (restrictToIds != null && restrictToIds.Length == 0)
            {
                return 0;
            }

            using (new QPConnectionScope())
            {
                return Common.CountDuplicates(
                    QPConnectionScope.Current.DbConnection,
                    constraint.ContentId,
                    Converter.ToIdCommaList(constraint.Rules.Select(n => n.FieldId).Where(n => n != exceptFieldId)),
                    Converter.ToIdCommaList(restrictToIds));
            }
        }

        /// <summary>
        /// Возвращает связанные статьи (для поля M2M)
        /// </summary>
        /// <param name="linkId">ID связи</param>
        /// <param name="id">ID статьи</param>
        /// <returns>список связанных статей через запятую</returns>
        internal static string GetLinkedItems(int linkId, int id)
        {
            using (new QPConnectionScope())
            {
                return Common.GetLinkedArticles(QPConnectionScope.Current.DbConnection, linkId, id, QPContext.IsLive);
            }
        }

        internal static Dictionary<int, string> GetLinkedItemsMultiple(int linkId, IEnumerable<int> ids)
        {
            using (new QPConnectionScope())
            {
                return Common.GetLinkedArticlesMultiple(QPConnectionScope.Current.DbConnection, linkId, ids, QPContext.IsLive);
            }
        }

        /// <summary>
        /// Возвращает связанные статьи (для поля M2O)
        /// </summary>
        /// <param name="fieldId">ID базового поля связи</param>
        /// <param name="id">ID статьи</param>
        /// <returns>список связанных статей через запятую</returns>
        internal static string GetRelatedItems(int fieldId, int? id)
        {
            var backField = FieldRepository.GetById(fieldId);
            if (backField == null)
            {
                return string.Empty;
            }

            using (new QPConnectionScope())
            {
                return Common.GetRelatedArticles(QPConnectionScope.Current.DbConnection, backField.ContentId, backField.Name, id, QPContext.IsLive);
            }
        }

        internal static int[] ExcludeArchived(int[] ids)
        {
            if (ids == null || !ids.Any())
            {
                return ids;
            }

            using (new QPConnectionScope())
            {
                return Common.ExcludeArchived(QPConnectionScope.Current.DbConnection, ids);
            }
        }

        internal static Dictionary<int, string> GetRelatedItemsMultiple(int fieldId, IEnumerable<int> ids)
        {
            var backField = FieldRepository.GetById(fieldId);
            if (backField == null)
            {
                return null;
            }

            using (new QPConnectionScope())
            {
                return Common.GetRelatedArticlesMultiple(QPConnectionScope.Current.DbConnection, backField.ContentId, backField.Name, ids, QPContext.IsLive);
            }
        }

        internal static void SetArchiveFlag(IList<int> ids, bool flag, bool withAggregated = false)
        {
            using (new QPConnectionScope())
            {
                if (ids != null && ids.Any())
                {
                    var stageIds = Enumerable.Empty<int>().ToList();
                    var liveIds = Enumerable.Empty<int>().ToList();
                    Common.GetContentModification(QPConnectionScope.Current.DbConnection, ids, withAggregated, true, ref liveIds, ref stageIds);
                    Common.SetArchiveFlag(QPConnectionScope.Current.DbConnection, ids, QPContext.CurrentUserId, flag, withAggregated);
                    Common.UpdateContentModification(QPConnectionScope.Current.DbConnection, liveIds, stageIds);
                }
            }
        }

        internal static void Publish(IList<int> ids, bool withAggregated = false)
        {
            using (new QPConnectionScope())
            {
                if (ids != null && ids.Any())
                {
                    var stageIds = Enumerable.Empty<int>().ToList();
                    var liveIds = Enumerable.Empty<int>().ToList();
                    Common.GetContentModification(QPConnectionScope.Current.DbConnection, ids, withAggregated, false, ref liveIds, ref stageIds);
                    Common.Publish(QPConnectionScope.Current.DbConnection, ids, QPContext.CurrentUserId, withAggregated);
                    Common.UpdateContentModification(QPConnectionScope.Current.DbConnection, liveIds, stageIds);
                }
            }
        }

        internal static bool Exists(int id)
        {
            return QPContext.EFContext.ArticleSet.Any(n => n.Id == id);
        }

        internal static int CountChildren(int articleId, bool countArchived)
        {
            using (new QPConnectionScope())
            {
                return Common.CountChildArticles(QPConnectionScope.Current.DbConnection, articleId, countArchived);
            }
        }

        private static readonly Regex DynamicColumnNamePattern = new Regex($@"^{Field.Prefix}(\d+)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static string GetDisplayExpression(FieldValue item) => $"[{item.Field.Name}]";

        private static string GetSqlExpression(FieldValue item) => string.IsNullOrEmpty(item.Value) ? $"([{item.Field.Name}] IS NULL)" : $"([{item.Field.Name}] = {item.Field.ParamName})";

        private static string ReplaceDynamicColumnsNamesInSortExpressions(string sortExpression, IList<Field> fieldList)
        {
            var sqlSortExpression = new StringBuilder();
            var sortInfoList = SqlSorting.GetSortingInformations(sortExpression);
            var sortInfoCount = sortInfoList.Count;
            if (sortInfoCount > 0)
            {
                for (var sortInfoIndex = 0; sortInfoIndex < sortInfoCount; sortInfoIndex++)
                {
                    var sortInfo = sortInfoList[sortInfoIndex];
                    var oldFieldName = sortInfo.FieldName;
                    var newFieldName = oldFieldName;
                    if (DynamicColumnNamePattern.IsMatch(oldFieldName))
                    {
                        var field = fieldList.SingleOrDefault(n => n.FormName == oldFieldName);
                        if (field == null)
                        {
                            return null;
                        }

                        newFieldName = $"[{field.Name}]";
                        if (field.Type.Name == FieldTypeName.Time)
                        {
                            newFieldName = $"dbo.qp_abs_time(c.{newFieldName})";
                        }
                    }
                    else if (!SqlSorting.ContainsSquareBrackets(newFieldName))
                    {
                        newFieldName = $"[{newFieldName}]";
                    }

                    if (sortInfoIndex > 0)
                    {
                        sqlSortExpression.Append(", ");
                    }

                    sqlSortExpression.AppendFormat("{0} {1}", newFieldName, sortInfo.Direction == SortDirection.Descending ? "DESC" : "ASC");
                }
            }

            return sqlSortExpression.ToString();
        }

        internal static bool CheckRelationCondition(int id, int contentId, string relCondition)
        {
            using (new QPConnectionScope())
            {
                return Common.CheckRelationCondition(QPConnectionScope.Current.DbConnection, id, contentId, relCondition);
            }
        }

        internal static int RemoveSiteArticles(int siteId, int articleToRemove)
        {
            using (var scope = new QPConnectionScope())
            {
                return Common.RemovingActions_RemoveSiteArticles(siteId, articleToRemove, scope.DbConnection);
            }
        }

        internal static IEnumerable<Article> LoadAggregatedArticles(Article article, bool isLive)
        {
            using (var scope = new QPConnectionScope())
            {
                if (article.IsNew)
                {
                    return Enumerable.Empty<Article>();
                }

                var values = article.FieldValues.Where(n => n.Field.IsClassifier).ToList();
                if (isLive)
                {
                    values = article.LiveFieldValues.Where(n => n.Field.IsClassifier).ToList();
                }

                if (!values.Any())
                {
                    return Enumerable.Empty<Article>();
                }

                var classifierFields = values.Select(n => n.Field.Id).ToArray();
                var types = values.Where(n => !string.IsNullOrEmpty(n.Value)).Select(n => int.Parse(n.Value)).ToArray();
                var aggregatedArticlesId = Common.GetAggregatedArticlesIDs(scope.DbConnection, article.Id, classifierFields, types, isLive).ToList();
                if (aggregatedArticlesId.Any())
                {
                    return MapperFacade.ArticleMapper.GetBizList(
                        QPContext.EFContext.ArticleSet
                            .Include("Status")
                            .Include("Content")
                            .Include("LastModifiedByUser")
                            .Include("LockedByUser")
                            .Where(a => aggregatedArticlesId.Contains(a.Id))
                            .ToList());
                }

                return Enumerable.Empty<Article>();
            }
        }

        internal static List<Article> LoadVariationArticles(Article article)
        {
            using (var scope = new QPConnectionScope())
            {
                if (article.IsNew || !article.UseVariations)
                {
                    return new List<Article>();
                }

                var variationArticlesId = Common.GetVariationArticlesIDs(scope.DbConnection, article.Id, article.ContentId, article.Content.VariationField.Name).ToList();
                if (variationArticlesId.Any())
                {
                    return MapperFacade.ArticleMapper.GetBizList(
                        QPContext.EFContext.ArticleSet
                            .Include("Status")
                            .Include("Content")
                            .Include("LastModifiedByUser")
                            .Include("LockedByUser")
                            .Where(a => variationArticlesId.Contains(a.Id))
                            .ToList());
                }

                return new List<Article>();
            }
        }

        internal static bool IsAnyAggregatedFields(int articleId)
        {
            return QPContext.EFContext.ArticleSet.Any(a => a.Id == articleId && a.Content.Fields.Any(f => f.Aggregated));
        }

        internal static bool CheckRelationSecurity(Article article, bool isDeletable)
        {
            var result = true;
            if (!QPContext.IsAdmin)
            {
                using (new QPConnectionScope())
                {
                    foreach (var item in article.GetRelationSecurityFields().Where(n => !string.IsNullOrEmpty(n.Value) && !n.Field.IsClassifier))
                    {
                        var testValues = new Dictionary<int, int[]> { { article.Id, Converter.ToInt32Collection(item.Value, ',') } };

                        // ReSharper disable once PossibleInvalidOperationException
                        var checkResult = CheckRelationSecurity(item.Field.RelateToContentId.Value, testValues, isDeletable);
                        if (!checkResult[article.Id])
                        {
                            result = false;
                            break;
                        }
                    }
                }
            }

            return result;
        }

        internal static Dictionary<int, bool> CheckRelationSecurity(int contentId, int[] testValues, bool isDeletable)
        {
            if (QPContext.IsAdmin)
            {
                return testValues.ToDictionary(n => n, m => true);
            }

            var dict = testValues.ToDictionary(n => n, m => Enumerable.Repeat(m, 1).ToArray());
            return CheckRelationSecurity(contentId, dict, isDeletable);
        }

        private static Dictionary<int, bool> CheckRelationSecurity(int contentId, Dictionary<int, int[]> testValues, bool isDeletable)
        {
            using (var scope = new QPConnectionScope())
            {
                var testIds = testValues.SelectMany(n => n.Value).Distinct().ToArray();
                var startLevel = isDeletable ? PermissionLevel.FullAccess : PermissionLevel.Modify;
                var securityInfo = CommonSecurity.GetRelationSecurityInfo(scope.DbConnection, contentId, testIds);
                if (securityInfo.IsEmpty)
                {
                    return testValues.ToDictionary(n => n.Key, m => true);
                }

                var partResult = new Dictionary<int, bool>();
                foreach (var currentContentId in securityInfo.ContentIds)
                {
                    var currentMapping = securityInfo.GetItemMapping(currentContentId);
                    var currentIds = currentMapping.Where(n => n.Value != null).SelectMany(n => n.Value).Distinct().ToArray();
                    var granted = CommonSecurity.CheckArticleSecurity(scope.DbConnection, currentContentId, currentIds, QPContext.CurrentUserId, startLevel);
                    foreach (var t in currentMapping)
                    {
                        if (t.Value != null)
                        {
                            var flag = t.Value.All(n => granted[n]);
                            partResult[t.Key] = partResult.ContainsKey(t.Key) ? partResult[t.Key] && flag : flag;
                        }
                    }
                }

                var contentIdsToCheck = securityInfo.GetContentIdsFromContentMapping();
                if (contentIdsToCheck.Any())
                {
                    var siteId = ContentRepository.GetSiteId(contentId);
                    var granted = CommonSecurity.CheckContentSecurity(scope.DbConnection, siteId, contentIdsToCheck, QPContext.CurrentUserId, startLevel);
                    foreach (var t in securityInfo.GetContentMapping())
                    {
                        var flag = granted[t.Value];
                        partResult[t.Key] = partResult.ContainsKey(t.Key) ? partResult[t.Key] && flag : flag;
                    }
                }

                return testValues.ToDictionary(n => n.Key, m => m.Value.All(k => partResult[k]));
            }
        }

        internal static Dictionary<int, bool> CheckSecurity(int contentId, int[] testIds, bool isDeletable, bool disableSecurityCheck)
        {
            if (QPContext.IsAdmin || disableSecurityCheck)
            {
                return testIds.ToDictionary(n => n, m => true);
            }

            var startLevel = isDeletable ? PermissionLevel.FullAccess : PermissionLevel.Modify;
            using (var scope = new QPConnectionScope())
            {
                return CommonSecurity.CheckArticleSecurity(scope.DbConnection, contentId, testIds, QPContext.CurrentUserId, startLevel);
            }
        }

        internal static Dictionary<int, bool> CheckLockedBy(int[] ids)
        {
            using (var scope = new QPConnectionScope())
            {
                return CommonSecurity.CheckLockedBy(scope.DbConnection, ids, QPContext.CurrentUserId, QPContext.IsAdmin || QPContext.CanUnlockItems);
            }
        }

        internal static void CopyPermissions(int fromId, int toId)
        {
            using (var scope = new QPConnectionScope())
            {
                Common.CopyArticleAccess(scope.DbConnection, fromId, toId, QPContext.CurrentUserId);
            }
        }

        internal static Dictionary<int, int> GetHierarchy(int contentId)
        {
            var treeName = ((IContentRepository)new ContentRepository()).GetTreeFieldName(contentId, 0);
            if (string.IsNullOrEmpty(treeName))
            {
                return null;
            }

            using (var scope = new QPConnectionScope())
            {
                return Common.GetArticleHierarchy(scope.DbConnection, contentId, treeName);
            }
        }

        internal static List<StatusHistoryListItem> GetStatusHistoryListItems(ListCommand cmd, int articleId, out int totalRecords)
        {
            using (var scope = new QPConnectionScope())
            {
                return MapperFacade.StatusHistoryItemMapper.GetBizList(
                    Common.GetAllHistoryStatusesForArticle(scope.DbConnection, articleId, cmd.SortExpression, cmd.StartRecord, cmd.PageSize, out totalRecords)
                );
            }
        }

        internal static List<DataRow> GetArticlesForExport(int contentId, string extensions, string columns, string filter, int startRow, int pageSize, string orderBy, IEnumerable<ExportSettings.FieldSetting> fieldsToExpand) => GetArticlesForExport(contentId, extensions, columns, filter, startRow, pageSize, orderBy, fieldsToExpand, out int _);

        internal static List<DataRow> GetArticlesForExport(int contentId, string extensions, string columns, string filter, int startRow, int pageSize, string orderBy, IEnumerable<ExportSettings.FieldSetting> fieldsToExpand, out int totalRecords)
        {
            using (var scope = new QPConnectionScope())
            {
                var allExtensions = $"{extensions} {GetExtraFromForRelations(fieldsToExpand)}";
                return Common.GetArticlesForExport(scope.DbConnection, contentId, allExtensions, columns, filter, startRow, pageSize, orderBy, out totalRecords);
            }
        }

        internal static List<int> InsertArticleIds(string query, bool preserveGuids = false)
        {
            using (var scope = new QPConnectionScope())
            {
                return preserveGuids
                    ? Common.InsertArticleIdsWithGuids(scope.DbConnection, query)
                    : Common.InsertArticleIds(scope.DbConnection, query);
            }
        }

        internal static void UpdateArticleGuids(List<Tuple<int, Guid>> guidsByIdToUpdate)
        {
            using (var scope = new QPConnectionScope())
            {
                Common.UpdateArticleGuids(scope.DbConnection, guidsByIdToUpdate);
            }
        }

        internal static void InsertArticleValues(string xmlParameter)
        {
            using (var scope = new QPConnectionScope())
            {
                Common.InsertArticleValues(scope.DbConnection, xmlParameter);
            }
        }

        internal static void ValidateO2MValues(string xmlParameter)
        {
            using (var scope = new QPConnectionScope())
            {
                Common.ValidateO2MValues(scope.DbConnection, xmlParameter, ImportStrings.IncorrectO2M);
            }
        }

        internal static void UpdateM2MValues(string xmlParameter)
        {
            using (var scope = new QPConnectionScope())
            {
                Common.UpdateM2MValues(scope.DbConnection, xmlParameter);
            }
        }

        internal static List<int> CheckForArticleExistence(List<int> relatedIds, string condition, int contentId)
        {
            using (var scope = new QPConnectionScope())
            {
                return Common.CheckForArticlesExistence(scope.DbConnection, relatedIds, condition, contentId);
            }
        }

        internal static Dictionary<string, int> GetExistingArticleIdsMap(List<string> values, string fieldName, string condition, int contentId)
        {
            using (var scope = new QPConnectionScope())
            {
                return Common.GetExistingArticleIdsMap(scope.DbConnection, values, fieldName, condition, contentId);
            }
        }

        internal static void UpdateArticlesDateTime(string xmlParameter)
        {
            using (var scope = new QPConnectionScope())
            {
                const string storedProc = "qp_update_acrticle_modification_date";
                Common.ModifyDataUsingXmlParameter(scope.DbConnection, storedProc, xmlParameter);
            }
        }

        internal static void RemoveLinksFromM2MField(int linkId, List<int> articleIds)
        {
            using (var scope = new QPConnectionScope())
            {
                Common.RemoveLinksFromM2MField(scope.DbConnection, linkId, articleIds);
            }
        }

        internal static void InsertO2MFieldValues(string xmlParameter)
        {
            using (var scope = new QPConnectionScope())
            {
                const string storedProc = "qp_update_o2mfieldvalues";
                Common.ModifyDataUsingXmlParameter(scope.DbConnection, storedProc, xmlParameter);
            }
        }

        internal static Dictionary<string, List<string>> GetM2MValuesBatch(List<int> ids, int linkId, string displayFieldName, int contentId)
        {
            using (var scope = new QPConnectionScope())
            {
                return Common.GetM2MValuesBatch(scope.DbConnection, ids, linkId, Default.MaxViewInListArticleNumber + 1, displayFieldName, contentId);
            }
        }

        internal static Dictionary<string, List<string>> GetM2OValuesBatch(List<int> ids, int contentId, int fieldId, string fieldName, string displayFieldName)
        {
            using (var scope = new QPConnectionScope())
            {
                return Common.GetM2OValuesBatch(scope.DbConnection, contentId, fieldId, fieldName, ids, displayFieldName, Default.MaxViewInListArticleNumber);
            }
        }

        internal static int[] SortIdsByFieldName(int[] ids, int contentId, string fieldName)
        {
            using (var scope = new QPConnectionScope())
            {
                return Common.SortIdsByFieldName(scope.DbConnection, ids, contentId, fieldName);
            }
        }

        internal static IList<int> GetParentIds(IList<int> ids, int fieldId, string fieldName)
        {
            using (var scope = new QPConnectionScope())
            {
                return Common.GetParentIdsTreeResult(scope.DbConnection, ids, fieldId, fieldName);
            }
        }

        internal static IList<int> GetChildArticles(IList<int> ids, string fieldName, int contentId, string filter)
        {
            using (var scope = new QPConnectionScope())
            {
                return Common.GetChildArticles(scope.DbConnection, ids, fieldName, contentId, filter);
            }
        }

        #region BatchUpdate

        internal static InsertData[] BatchUpdate(ArticleData[] articles, bool formatArticleData)
        {
            if (articles.Any())
            {
                using (var scope = new QPConnectionScope())
                using (var transaction = new TransactionScope())
                {
                    if (formatArticleData)
                    {
                        FormatArticleData(scope.DbConnection, articles);
                    }

                    var rows = Common.GetRelations(scope.DbConnection, GetBatchDataTable(articles));
                    var relations = MapperFacade.DataRowMapper.Map<RelationData>(rows);

                    var links = GetArticleLinks(articles, relations);
                    articles = UpdateArticleRelations(articles, relations);

                    rows = Common.BatchInsert(scope.DbConnection, GetBatchDataTable(articles), true, QPContext.CurrentUserId);
                    var insertData = MapperFacade.DataRowMapper.Map<InsertData>(rows);

                    links = UpdateLinkIds(links, insertData);
                    articles = UpdateArticleIds(articles, insertData);

                    Common.BatchUpdate(scope.DbConnection, GetBatchDataTable(articles), QPContext.CurrentUserId);
                    Common.ReplicateItems(scope.DbConnection, GetArticleIds(articles), GetFieldIds(articles));
                    Common.UpdateM2MValues(scope.DbConnection, GetLinksXml(links));

                    transaction.Complete();
                    return insertData;
                }
            }

            return new InsertData[0];
        }

        private static void FormatArticleData(SqlConnection cnn, ArticleData[] articles)
        {
            var fieldIds = articles.SelectMany(m => m.Fields).Select(n => n.Id).Distinct().ToArray();
            var types = Common.GetFieldTypes(cnn, fieldIds).AsEnumerable().ToDictionary(n => (int)n.Field<decimal>("attribute_id"), m =>
            {
                var attributeTypeId = (int)m.Field<decimal>("attribute_type_id");
                var linkId = (int?)m.Field<decimal?>("link_id");
                var backRelatedAttributeId = (int?)m.Field<decimal?>("BACK_RELATED_ATTRIBUTE_ID");
                var isClassifier = m.Field<bool>("is_classifier");
                var isStringEnum = m.Field<bool>("is_string_enum");
                return new FieldTypeInfo(Field.CreateExactType(attributeTypeId, linkId, isClassifier, isStringEnum), linkId ?? backRelatedAttributeId);
            });

            foreach (var field in articles.SelectMany(article => article.Fields))
            {
                field.Value = types[field.Id].FormatFieldValue(field.Value);
            }
        }

        private static DataTable GetBatchDataTable(ArticleData[] articles)
        {
            var dt = new DataTable();
            dt.Columns.Add("ArticleId", typeof(int));
            dt.Columns.Add("ContentId", typeof(int));
            dt.Columns.Add("FieldId", typeof(int));
            dt.Columns.Add("Value", typeof(string));

            if (articles != null)
            {
                foreach (var article in articles)
                {
                    foreach (var field in article.Fields)
                    {
                        dt.Rows.Add(article.Id, article.ContentId, field.Id, field.Value);
                    }
                }
            }

            return dt;
        }

        private static LinkData[] GetArticleLinks(IEnumerable<ArticleData> articles, IEnumerable<RelationData> relations) => (from a in articles
                from f in a.Fields
                from linkedItemId in f.ArticleIds != null && f.ArticleIds.Any() ? f.ArticleIds : new[] { 0 }
                join r in relations on f.Id equals r.FieldId
                where r.LinkId.HasValue
                select new LinkData
                {
                    // ReSharper disable once PossibleInvalidOperationException
                    LinkId = r.LinkId.Value,
                    ItemId = a.Id,
                    LinkedItemId = linkedItemId != 0 ? linkedItemId : (int?)null
                })
            .ToArray();

        private static LinkData[] UpdateLinkIds(LinkData[] links, IEnumerable<InsertData> insertData)
        {
            var map = insertData.ToDictionary(d => d.OriginalArticleId, d => d.CreatedArticleId);
            foreach (var link in links)
            {
                if (map.TryGetValue(link.ItemId, out var newItemId))
                {
                    link.ItemId = newItemId;
                }

                if (link.LinkedItemId.HasValue && map.TryGetValue(link.LinkedItemId.Value, out var newLinkedItemId))
                {
                    link.LinkedItemId = newLinkedItemId;
                }
            }

            return links;
        }

        private static ArticleData[] UpdateArticleRelations(ArticleData[] articles, IEnumerable<RelationData> relations)
        {
            var items = (from article in articles
                    from field in article.Fields
                    join relation in relations
                        on new { articleId = article.Id, fieldId = field.Id }
                        equals new { articleId = relation.ArticleId, fieldId = relation.FieldId }
                        into r
                    from relation in r.DefaultIfEmpty()
                    select new { article, field, relation })
                .ToArray();

            foreach (var item in items)
            {
                if (item.relation == null)
                {
                    item.field.ArticleIds = null;
                }
                else if (item.relation.RefFieldId.HasValue)
                {
                    if (item.relation.ContentId == item.relation.RefContentId && item.relation.FieldId == item.relation.RefFieldId)
                    {
                        if (item.field.ArticleIds != null && item.field.ArticleIds.Any())
                        {
                            item.field.Value = item.field.ArticleIds.First().ToString();
                        }
                        else
                        {
                            item.field.Value = null;
                        }
                    }
                    else
                    {
                        var backRelationArticles = articles.Where(a => a.ContentId == item.relation.RefContentId && item.field.ArticleIds != null && item.field.ArticleIds.Contains(a.Id)).ToArray();
                        item.field.ArticleIds = null;

                        foreach (var backArticle in backRelationArticles)
                        {
                            var field = backArticle.Fields.SingleOrDefault(f => f.Id == item.relation.RefFieldId);

                            if (field == null)
                            {
                                field = new FieldData { Id = item.relation.RefFieldId.Value };
                                backArticle.Fields.Add(field);
                            }

                            field.ArticleIds = new[] { item.article.Id };
                            field.Value = item.article.Id.ToString();
                        }
                    }
                }
                else if (item.relation.LinkId.HasValue)
                {
                    item.field.Value = item.relation.LinkId.Value.ToString();
                    item.field.ArticleIds = null;
                }
            }

            return articles;
        }

        private static ArticleData[] UpdateArticleIds(ArticleData[] articles, IEnumerable<InsertData> insertData)
        {
            var map = insertData.ToDictionary(d => d.OriginalArticleId, d => d.CreatedArticleId);
            foreach (var article in articles)
            {
                if (map.TryGetValue(article.Id, out var newId))
                {
                    article.Id = newId;
                }

                foreach (var field in article.Fields)
                {
                    if (field.ArticleIds != null)
                    {
                        if (field.ArticleIds.Any())
                        {
                            var id = field.ArticleIds.Single();

                            if (map.TryGetValue(id, out newId))
                            {
                                id = newId;
                            }

                            field.Value = id.ToString();
                        }
                        else
                        {
                            field.Value = null;
                        }
                    }
                }
            }

            return articles;
        }

        private static int[] GetArticleIds(IEnumerable<ArticleData> articles)
        {
            return articles.Select(a => a.Id).ToArray();
        }

        private static int[] GetFieldIds(IEnumerable<ArticleData> articles)
        {
            return articles.SelectMany(a => a.Fields.Select(f => f.Id)).Distinct().ToArray();
        }

        private static string GetLinksXml(IEnumerable<LinkData> links)
        {
            var doc = new XDocument();
            var items = new XElement("items");
            doc.Add(items);

            foreach (var group in links.GroupBy(l => new { l.ItemId, l.LinkId }))
            {
                var itemXml = new XElement("item");
                itemXml.Add(new XAttribute("id", group.Key.ItemId));
                itemXml.Add(new XAttribute("linkId", group.Key.LinkId));
                itemXml.Add(new XAttribute("value", string.Join(",", group.Select(l => l.LinkedItemId?.ToString() ?? string.Empty).Distinct().ToArray())));
                doc.Root.Add(itemXml);
            }

            return doc.ToString(SaveOptions.None);
        }

        public int[] GetIdsByGuids(Guid[] guids)
        {
            using (var scope = new QPConnectionScope())
            {
                return Common.GetArticleIdsByGuids(scope.DbConnection, guids);
            }
        }

        public Guid[] GetGuidsByIds(int[] ids)
        {
            using (var scope = new QPConnectionScope())
            {
                return Common.GetArticleGuidsByIds(scope.DbConnection, ids);
            }
        }

        #endregion
    }
}
