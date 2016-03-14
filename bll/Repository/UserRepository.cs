﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Quantumart.QP8.DAL;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Mappers;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Utils;
using Quantumart.QP8.Constants;
using Quantumart.QP8.DAL.DTO;

namespace Quantumart.QP8.BLL.Repository
{
    internal class UserRepository
    {

		internal static IEnumerable<UserListItem> List(ListCommand cmd, UserListFilter filter, IEnumerable<int> selectedIDs, out int totalRecords)
		{
			using (var scope = new QPConnectionScope())
			{
				UserPageOptions options = new UserPageOptions
				{
					SortExpression = !String.IsNullOrWhiteSpace(cmd.SortExpression) ? UserListItem.TranslateSortExpression(cmd.SortExpression) : null,
					StartRecord = cmd.StartRecord,
					PageSize = cmd.PageSize,
					SelectedIDs = selectedIDs
				};

				if (filter != null)
				{
					options.Email = filter.Email;
					options.FirstName = filter.FirstName;
					options.LastName = filter.LastName;
					options.Login = filter.Login;
				}
								
				IEnumerable<DataRow> rows = Common.GetUserPage(scope.DbConnection, 	options, out totalRecords);
				return MappersRepository.UserListItemRowMapper.GetBizList(rows.ToList());				
			}
		}

		/// <summary>
		/// Возвращает список по ids
		/// </summary>
		/// <returns></returns>
		internal static IEnumerable<User> GetList(IEnumerable<int> IDs)
		{
			List<User> result = new List<User>();
			var cache = QPContext.GetUserCache();
			if (cache != null)
			{
				foreach (var id in IDs)
				{
					if (cache.ContainsKey(id))
						result.Add(cache[id]);
					else
						result.Add(GetRealById(id));
				}

			}
			else
			{
				IEnumerable<decimal> decIDs = Converter.ToDecimalCollection(IDs).Distinct().ToArray();
				result = MappersRepository.UserMapper
					.GetBizList(QPContext.EFContext.UserSet
						.Where(f => decIDs.Contains(f.Id))
						.ToList()
					);
			}
			return result;
		}

		internal static bool CheckAuthenticate(string login, string password)
        {
            UserDAL result = null;
            result = QPContext.EFContext.Authenticate(login, password, false, false);
            return (result != null);
        }
        
        internal static User GetById(int id, bool stopRecursion = false)
        {
			User result = GetByIdFromCache(id);
			if (result != null)
				return result;
			else
				return GetRealById(id, stopRecursion);
        }

		internal static User GetByIdFromCache(int id)
		{
			User result = null;
			var cache = QPContext.GetUserCache();
			if (cache != null && cache.ContainsKey(id))
			{
				result = cache[id];
			}
			return result;
		}

		private static User GetRealById(int id, bool stopRecursion = false)
		{
			User user = MappersRepository.UserMapper.GetBizObject(QPContext.EFContext.UserSet.SingleOrDefault(u => u.Id == id));
			if (!stopRecursion && user != null)
				user.LastModifiedByUser = GetById(user.LastModifiedBy, true);

			return user;
		}

		internal static User GetPropertiesById(int id)
		{
			UserDAL dal = QPContext.EFContext.UserSet
				.Include("Groups")
				.Include("LastModifiedByUser")
				.SingleOrDefault(u => u.Id == id);
			User user = MappersRepository.UserMapper.GetBizObject(dal);
			return user;
		}

        internal static User UpdateProfile(User user)
        {
			return UpdateUser(user, true);	
        }

		internal static User UpdateProperties(User user)
		{
			return UpdateUser(user);			
		}

		private static User UpdateUser(User user, bool profileOnly = false)
		{
			QP8Entities entities = QPContext.EFContext;
			UserDAL dal = MappersRepository.UserMapper.GetDalObject(user);
			dal.LastModifiedBy = QPContext.CurrentUserId;
			using (new QPConnectionScope())
			{
				dal.Modified = Common.GetSqlDate(QPConnectionScope.Current.DbConnection);
			}
			entities.UserSet.Attach(dal);
			entities.ObjectStateManager.ChangeObjectState(dal, EntityState.Modified);

			if (!profileOnly)
			{
				// Save Groups
				UserDAL dalDB = entities.UserSet.Include("Groups").Single(u => u.Id == dal.Id);
				HashSet<decimal> inmemoryGroupIDs = new HashSet<decimal>(user.Groups.Select(g => Converter.ToDecimal(g.Id)));
				HashSet<decimal> indbGroupIDs = new HashSet<decimal>(dalDB.Groups.Select(g => g.Id));
				foreach (var g in dalDB.Groups.ToArray())
				{
					if (!inmemoryGroupIDs.Contains(g.Id) && !g.IsReadOnly && !(g.BuiltIn && user.BuiltIn))
					{
						entities.UserGroupSet.Attach(g);
						dalDB.Groups.Remove(g);
					}
				}
				foreach (var g in MappersRepository.UserGroupMapper.GetDalList(user.Groups.ToList()))
				{
					if (!indbGroupIDs.Contains(g.Id))
					{
						entities.UserGroupSet.Attach(g);
						dal.Groups.Add(g);
					}
				}
				//-------------------
			}

			// User Default Filters
			foreach (UserDefaultFilterItemDAL f in entities.UserDefaultFilterSet.Where(r => r.UserId == dal.Id))
			{
				entities.UserDefaultFilterSet.DeleteObject(f);
			}
			foreach (UserDefaultFilterItemDAL f in MapUserDefaultFilter(user, dal))
			{
				entities.UserDefaultFilterSet.AddObject(f);
			}
			//--------------------------

			entities.SaveChanges();

			if (!String.IsNullOrEmpty(user.Password))
				UpdatePassword(user.Id, user.Password);

			User updated = MappersRepository.UserMapper.GetBizObject(dal);
			return updated;
		}

        /// <summary>
        /// Возвращает список всех пользователей
        /// </summary>
        /// <returns></returns>
        internal static IEnumerable<User> GetAllUsersList()
        {
            return MappersRepository.UserMapper.GetBizList(QPContext.EFContext.UserSet.OrderBy(u => u.LogOn).ToList());
        }


        internal static void UpdatePassword(int userId, string password)
        {
            using (new QPConnectionScope())
            {
                Common.UpdatePassword(QPConnectionScope.Current.DbConnection, userId, password);
            }
        }

		internal static User SaveProperties(User user)
		{
			QP8Entities entities = QPContext.EFContext;
			UserDAL dal = MappersRepository.UserMapper.GetDalObject(user);
			dal.LastModifiedBy = QPContext.CurrentUserId;
			using (new QPConnectionScope())
			{
				dal.Created = Common.GetSqlDate(QPConnectionScope.Current.DbConnection);
				dal.Modified = dal.Created;
				dal.PasswordModified = dal.Created;
			}
			entities.UserSet.AddObject(dal);
			entities.SaveChanges();

			// Save Groups
			foreach (var s in MappersRepository.UserGroupMapper.GetDalList(user.Groups.ToList()))
			{
				entities.UserGroupSet.Attach(s);
				dal.Groups.Add(s);
			}
			//---------------

			// User Default Filters
			foreach (UserDefaultFilterItemDAL f in MapUserDefaultFilter(user, dal))
			{
				entities.UserDefaultFilterSet.AddObject(f); 
			}
			//----------------

			entities.SaveChanges();

			if (!String.IsNullOrEmpty(user.Password))
				UpdatePassword(user.Id, user.Password);

			User updated = MappersRepository.UserMapper.GetBizObject(dal);
			return updated;
		}
				
		/// <summary>
		/// Создает копию пользователя
		/// </summary>
		/// <param name="user"></param>
		internal static int CopyUser(User user, int currentUserId)
		{
			using (var scope = new QPConnectionScope())
			{
				return Common.CopyUser(user.Id, user.LogOn, currentUserId, scope.DbConnection);
			}
		}

		internal static IEnumerable<User> GetUsers(IEnumerable<int> userIDs)
		{
			return GetAllUsersList().Where(u => userIDs.Contains(u.Id)).ToArray();
		}

		internal static IEnumerable<ListItem> GetSimpleList(IEnumerable<int> userIDs)
		{
			return GetUsers(userIDs)
				.Select(u => new ListItem {Value = u.Id.ToString(), Text = u.Name })
				.ToArray();
		}

		internal static void Delete(int id)
		{
			DefaultRepository.Delete<UserDAL>(id);
		}

		internal static IEnumerable<UserDefaultFilter> GetContentDefaultFilters(int userId)
		{
			return QPContext.EFContext.UserDefaultFilterSet				
				.Where(r => r.UserId == userId)
				.ToArray()
				.GroupBy(r => r.ContentId)
				.Select(g => new UserDefaultFilter
				{
					ArticleIDs = Converter.ToInt32Collection(g.Select(r => r.ArticleId)),					
					ContentId = Converter.ToInt32(g.Key),					
					UserId = userId
				})
				.ToArray();
		}

		private static IEnumerable<UserDefaultFilterItemDAL> MapUserDefaultFilter(User biz, UserDAL dal)
		{
			return biz.ContentDefaultFilters
				.Where(f => f.ArticleIDs.Any())
				.SelectMany(f =>
					f.ArticleIDs.Select(aid => new UserDefaultFilterItemDAL
					{
						UserId = dal.Id,
						ContentId = f.ContentId.Value,
						ArticleId = aid
					}
				)
			);
		}
	}
}
