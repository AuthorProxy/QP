﻿using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Resources;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Quantumart.QP8.WebMvc.ViewModels.PageTemplate
{
	public sealed class SearchInFormatsViewModel : AreaViewModel
	{
		#region creation

		private IPageTemplateService _service;

		public static SearchInFormatsViewModel Create(string tabId, int parentId, int siteId, IPageTemplateService service, int? templateId = null, int? pageId = null)
		{
			SearchInFormatsViewModel model = ViewModel.Create<SearchInFormatsViewModel>(tabId, parentId);			
			model._service = service;
			model.PageId = pageId;
			model.TemplateId = templateId;
			model.SiteId = siteId;
			return model;
		}

		#endregion

		public override string EntityTypeCode
		{
			get { return Constants.EntityTypeCode.Site; }
		}

		public override string ActionCode
		{
			get { return Constants.ActionCode.SearchInCode; }
		}


		public string GridElementId
		{
			get
			{
				return UniqueId("Grid");
			}
		}

		public string FilterElementId { get { return UniqueId("Filter"); } }

		public int SiteId { get; set; }

		public int? PageId { get; set; }

		public int? TemplateId { get; set; }

		public QPSelectListItem PageListItem
		{
			get
			{
				if (PageId == null)
					return null;
				return
					new QPSelectListItem { Value = PageId.ToString(), Text = _service.ReadPageProperties(PageId.Value).Name, Selected = true };
			}
		}

		private List<ListItem> _templates;
		public List<ListItem> Templates
		{
			get
			{
				if (_templates == null)
				{					
					_templates = _service.GetAllSiteTemplates(SiteId).Select(x => new ListItem { Value = x.Id.ToString(), Text = x.Name }).ToList();
					_templates.Insert(0, new ListItem { Value = null, Text = TemplateStrings.SelectTemplate, Selected = true });
				}
				return _templates;
			}
			set
			{
				_templates = value;
			}
		}
	}
}