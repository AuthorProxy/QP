﻿using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Quantumart.QP8.BLL.Services
{
    public interface IVisualEditorService
    {
        VisualEditorInitListResult InitList(int contentId);

        ListResult<VisualEditorPluginListItem> GetVisualEditorPlugins(ListCommand cmd, int contentId);

        VisualEditorPlugin ReadVisualEditorPluginProperties(int id);

        VisualEditorPlugin ReadVisualEditorPluginPropertiesForUpdate(int id);

        VisualEditorPlugin UpdateVisualEditorProperties(VisualEditorPlugin visualEditorPlugin);

        MessageResult Remove(int id);

        VisualEditorPlugin NewVisualEditorPluginProperties(int parentId);

        VisualEditorPlugin NewVisualEditorPluginPropertiesForUpdate(int parentId);

        VisualEditorPlugin SaveVisualEditorPluginProperties(VisualEditorPlugin visualEditorPlugin);

        VisualEditorStyleInitListResult InitVisualEditorStyleList(int parentId);

        ListResult<VisualEditorStyleListItem> GetVisualEditorStyles(ListCommand cmd, int contentId);

        VisualEditorStyle ReadVisualEditorStyleProperties(int id);

        VisualEditorStyle ReadVisualEditorStylePropertiesForUpdate(int id);

        VisualEditorStyle UpdateVisualEditorStyleProperties(VisualEditorStyle visualEditorStyle);

        MessageResult RemoveVisualEditorStyle(int id);

        VisualEditorStyle NewVisualEditorStyleProperties(int parentId);

        VisualEditorStyle NewVisualEditorStylePropertiesForUpdate(int parentId);

        VisualEditorStyle SaveVisualEditorStyleProperties(VisualEditorStyle visualEditorStyle);
    }

    public class VisualEditorService : IVisualEditorService
    {
        public VisualEditorStyleInitListResult InitVisualEditorStyleList(int parentId)
        {
            return new VisualEditorStyleInitListResult
            {
                IsAddNewAccessable = SecurityRepository.IsActionAccessible(ActionCode.AddNewVisualEditorStyle) &&
                SecurityRepository.IsEntityAccessible(EntityTypeCode.VisualEditorStyle, parentId, ActionTypeCode.Update)
            };
        }

        public VisualEditorInitListResult InitList(int contentId)
        {
            return new VisualEditorInitListResult
            {
                IsAddNewAccessable = SecurityRepository.IsActionAccessible(ActionCode.AddNewVisualEditorPlugin) &&
                SecurityRepository.IsEntityAccessible(EntityTypeCode.VisualEditorPlugin, contentId, ActionTypeCode.Update)
            };
        }

        public ListResult<VisualEditorPluginListItem> GetVisualEditorPlugins(ListCommand cmd, int contentId)
        {
            int totalRecords;
            var list = VisualEditorRepository.List(cmd, contentId, out totalRecords);
            return new ListResult<VisualEditorPluginListItem>
            {
                Data = list.ToList(),
                TotalRecords = totalRecords
            };
        }

        public ListResult<VisualEditorStyleListItem> GetVisualEditorStyles(ListCommand cmd, int contentId)
        {
            int totalRecords;
            var list = VisualEditorRepository.ListStyles(cmd, contentId, out totalRecords);
            return new ListResult<VisualEditorStyleListItem>
            {
                Data = list.ToList(),
                TotalRecords = totalRecords
            };
        }

        public VisualEditorPlugin ReadVisualEditorPluginProperties(int id)
        {
            var plugin = VisualEditorRepository.GetPluginPropertiesById(id);
            if (plugin == null)
            {
                throw new ApplicationException(string.Format(VisualEditorStrings.VisualEditorPluginNotFound, id));
            }

            return plugin;
        }

        public VisualEditorPlugin ReadVisualEditorPluginPropertiesForUpdate(int id)
        {
            return ReadVisualEditorPluginProperties(id);
        }

        public VisualEditorPlugin UpdateVisualEditorProperties(VisualEditorPlugin visualEditorPlugin)
        {
            return VisualEditorRepository.UpdatePluginProperties(visualEditorPlugin);
        }

        public MessageResult Remove(int id)
        {
            var plugin = VisualEditorRepository.GetPluginPropertiesById(id);
            if (plugin == null)
            {
                throw new ApplicationException(string.Format(VisualEditorStrings.VisualEditorPluginNotFound, id));
            }

            VisualEditorRepository.Delete(id);
            return null;
        }

        public VisualEditorPlugin NewVisualEditorPluginProperties(int parentId)
        {
            return VisualEditorPlugin.Create();
        }

        public VisualEditorPlugin NewVisualEditorPluginPropertiesForUpdate(int parentId)
        {
            return NewVisualEditorPluginProperties(parentId);
        }

        public VisualEditorPlugin SaveVisualEditorPluginProperties(VisualEditorPlugin visualEditorPlugin)
        {
            return VisualEditorRepository.SavePluginProperties(visualEditorPlugin);
        }

        public static IEnumerable<VisualEditorPlugin> GetVisualEditorPlugins(IEnumerable<VisualEditorCommand> veCommands)
        {
            return veCommands.Where(vec => vec.PluginId.HasValue).Select(c => c.PluginId.Value).Distinct().Select(VisualEditorRepository.GetPluginPropertiesById);
        }

        public VisualEditorStyle ReadVisualEditorStyleProperties(int id)
        {
            var style = VisualEditorRepository.GetStylePropertiesById(id);
            if (style == null)
            {
                throw new ApplicationException(string.Format(VisualEditorStrings.VisualEditorStyleNotFound, id));
            }

            return style;
        }

        public VisualEditorStyle ReadVisualEditorStylePropertiesForUpdate(int id)
        {
            return ReadVisualEditorStyleProperties(id);
        }

        public VisualEditorStyle UpdateVisualEditorStyleProperties(VisualEditorStyle visualEditorStyle)
        {
            return VisualEditorRepository.UpdateStyleProperties(visualEditorStyle);
        }

        public MessageResult RemoveVisualEditorStyle(int id)
        {
            var style = VisualEditorRepository.GetStylePropertiesById(id);
            if (style == null)
            {
                throw new ApplicationException(string.Format(VisualEditorStrings.VisualEditorStyleNotFound, id));
            }

            if (style.IsSystem)
            {
                throw new ApplicationException(string.Format(VisualEditorStrings.SystemWarning, id));
            }

            VisualEditorRepository.DeleteStyle(id);
            return null;
        }

        public VisualEditorStyle NewVisualEditorStyleProperties(int parentId)
        {
            return VisualEditorStyle.Create().Init();
        }

        public VisualEditorStyle NewVisualEditorStylePropertiesForUpdate(int parentId)
        {
            return NewVisualEditorStyleProperties(parentId);
        }

        public VisualEditorStyle SaveVisualEditorStyleProperties(VisualEditorStyle visualEditorStyle)
        {
            return VisualEditorRepository.SaveStyleProperties(visualEditorStyle);
        }
    }
}
