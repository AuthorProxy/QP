﻿using Quantumart.QP8.WebMvc.ViewModels.PageTemplate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.WebMvc.Extensions.ModelBinders
{
	public class ObjectViewModelBinder : QpModelBinder
	{
		protected override void BindProperty(System.Web.Mvc.ControllerContext controllerContext, System.Web.Mvc.ModelBindingContext bindingContext, System.ComponentModel.PropertyDescriptor propertyDescriptor)
		{
			ObjectViewModel model = bindingContext.Model as ObjectViewModel;
			Expression<Func<IEnumerable<int>>> p = () => model.ActiveStatusTypeIds;
			if (propertyDescriptor.Name == p.GetPropertyName())
			{
				string value = controllerContext.HttpContext.Request[GetModelPropertyName(bindingContext, p)];
				model.ActiveStatusTypeIds = Converter.ToIdArray(value);
			}
			else
				base.BindProperty(controllerContext, bindingContext, propertyDescriptor);
		}

		protected override void OnModelUpdated(System.Web.Mvc.ControllerContext controllerContext, System.Web.Mvc.ModelBindingContext bindingContext)
		{
			ObjectViewModel model = bindingContext.Model as ObjectViewModel;
			model.DoCustomBinding();
			base.OnModelUpdated(controllerContext, bindingContext);
		}
	}
}