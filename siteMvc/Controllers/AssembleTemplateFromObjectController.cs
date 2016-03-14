﻿using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.MultistepActions;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Extensions.ActionFilters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Quantumart.QP8.WebMvc.Controllers
{
	public class AssembleTemplateFromObjectController : AssembleTemplateBaseController
    {
		public AssembleTemplateFromObjectController(IMultistepActionService service)
			: base(service)
		{			
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[ActionAuthorize(ActionCode.AssembleTemplateFromTemplateObject)]
		public override ActionResult PreAction(int parentId, int id)
		{
			var template = service.ReadTemplateProperties(parentId);
			return Json(service.PreAction(template.SiteId, template.Id));
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[ActionAuthorize(ActionCode.AssembleTemplateFromTemplateObject)]
		public override ActionResult Setup(int parentId, int id, bool? boundToExternal)
		{
			var template = service.ReadTemplateProperties(parentId);
			MultistepActionSettings settings = service.Setup(template.SiteId, template.Id, boundToExternal);
			return Json(settings);
		}
    }
}
