﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Quantumart.QP8.WebMvc.Extensions.ActionFilters;
using Quantumart.QP8.Constants;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.BLL.Services.MultistepActions;
using Microsoft.Practices.Unity;
using Quantumart.QP8.WebMvc.Extensions.Controllers;

namespace Quantumart.QP8.WebMvc.Controllers
{
	public class RebuildVirtualContentsController : QPController
	{
		private readonly IMultistepActionService service;

		public RebuildVirtualContentsController(IMultistepActionService service)
		{
			this.service = service;
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.RebuildVirtualContents)]
		[BackendActionContext(ActionCode.RebuildVirtualContents)]
		[BackendActionLog]
		public ActionResult Setup(int parentId, int id, bool? boundToExternal)
		{
			MultistepActionSettings settings = service.Setup(parentId, id, boundToExternal);
			return Json(settings);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		public ActionResult Step(int stage, int step)
		{
			MultistepActionStepResult stepResult = service.Step(stage, step);
			return Json(stepResult);
		}

		[HttpPost]
		public ActionResult TearDown(bool isError)
		{
			service.TearDown();
			return null;
		}

	}
}
