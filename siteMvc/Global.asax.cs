﻿using Microsoft.AspNet.SignalR;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Security;
using Quantumart.QP8.WebMvc.Extensions;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using Quantumart.QP8.WebMvc.Extensions.ModelBinders;
using Quantumart.QP8.WebMvc.Extensions.ValidatorProviders;
using Quantumart.QP8.WebMvc.Extensions.ValueProviders;
using Quantumart.QP8.WebMvc.ViewModels;
using Quantumart.QP8.WebMvc.ViewModels.CustomAction;
using Quantumart.QP8.WebMvc.ViewModels.EntityPermissions;
using Quantumart.QP8.WebMvc.ViewModels.Field;
using Quantumart.QP8.WebMvc.ViewModels.Notification;
using Quantumart.QP8.WebMvc.ViewModels.PageTemplate;
using Quantumart.QP8.WebMvc.ViewModels.VirtualContent;
using Quantumart.QP8.WebMvc.ViewModels.VisualEditor;
using Quantumart.QP8.WebMvc.ViewModels.Workflow;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using AutoMapper;

namespace Quantumart.QP8.WebMvc
{
    public class MvcApplication : HttpApplication
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("{resource}.ashx/{*pathInfo}");
            routes.IgnoreRoute("WebServices/{*pathInfo}");
            routes.MapRoute(
                "MultistepAction",
                "Multistep/{command}/{action}/{tabId}/{parentId}/{id}",
                new { controller = "Multistep", parentId = 0, id = 0 },
                new { parentId = @"\d+" }
            );

            routes.MapRoute(
                "Properties",
                "{controller}/{action}/{tabId}/{parentId}/{id}",
                new { action = "Properties", parentId = 0 },
                new { parentId = @"\d+" }
            );

            routes.MapRoute(
                "New",
                "{controller}/{action}/{tabId}/{parentId}",
                new { action = "New" },
                new { parentId = @"\d+" }
            );

            routes.MapRoute(
                "Default",
                "{controller}/{action}/{id}",
                new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }

        internal void Application_Start()
        {
            RegisterModelBinders();
            RegisterModelValidatorProviders();
            AreaRegistration.RegisterAllAreas();
            RegisterMappings();
            RegisterUnity();
            RegisterRoutes(RouteTable.Routes);
            RegisterValueProviders();
        }


        internal static void UnregisterRoutes()
        {
            RouteTable.Routes.Clear();
        }
        internal static void UnregisterValueProviders()
        {
            ValueProviderFactories.Factories.Clear();
        }
        internal static void RegisterValueProviders()
        {
            ValueProviderFactories.Factories.Remove(ValueProviderFactories.Factories.OfType<JsonValueProviderFactory>().FirstOrDefault());
            ValueProviderFactories.Factories.Add(new JsonNetValueProviderFactory());
        }

        internal static void UnregisterModelValidatorProviders()
        {
            ModelValidatorProviders.Providers.Clear();
        }

        internal static void RegisterModelValidatorProviders()
        {
            ModelValidatorProviders.Providers.Clear();
            ModelValidatorProviders.Providers.Add(new EntLibValidatorProvider());
        }

        internal static void RegisterUnity()
        {
            var dresolver = new UnityDependencyResolver();
            DependencyResolver.SetResolver(dresolver);
            QPContext.SetUnityContainer(dresolver.UnityContainer);
            GlobalHost.DependencyResolver = new SignalRUnityDependencyResolver(dresolver.UnityContainer);
        }

        internal static void RegisterMappings()
        {
            ViewModelMapper.CreateAllMappings();
            DTOMapper.CreateAllMappings();
        }

        internal static void UnregisterModelBinders()
        {
            ModelBinders.Binders.Clear();
        }

        internal static void RegisterModelBinders()
        {
            ModelBinders.Binders.Add(typeof(ArticleViewModel), new ArticleViewModelBinder());
            ModelBinders.Binders.Add(typeof(ArticleVersionViewModel), new ArticleVersionViewModelBinder());
            ModelBinders.Binders.Add(typeof(ArticleSchedule), new ArticleScheduleModelBinder());
            ModelBinders.Binders.Add(typeof(RecurringSchedule), new RecurringScheduleModelBinder());
            ModelBinders.Binders.Add(typeof(Site), new SiteModelBinder());
            ModelBinders.Binders.Add(typeof(Content), new ContentModelBinder());
            ModelBinders.Binders.Add(typeof(WorkflowViewModel), new WorkflowModelBinder());
            ModelBinders.Binders.Add(typeof(FieldViewModel), new FieldViewModelBinder());
            ModelBinders.Binders.Add(typeof(VirtualContentViewModel), new VirtualContentViewModelBinder());
            ModelBinders.Binders.Add(typeof(CustomActionViewModel), new CustomActionViewModelBinder());
            ModelBinders.Binders.Add(typeof(QPCheckedItem), new QPCheckedItemModelBinder());
            ModelBinders.Binders.Add(typeof(IList<QPCheckedItem>), new QPCheckedItemListModelBinder());
            ModelBinders.Binders.Add(typeof(UserViewModel), new UserViewModelBinder());
            ModelBinders.Binders.Add(typeof(UserGroupViewModel), new UserGroupViewModelBinder());
            ModelBinders.Binders.Add(typeof(PermissionViewModel), new PermissionViewModelBinder());
            ModelBinders.Binders.Add(typeof(NotificationViewModel), new NotificationViewModelBinder());
            ModelBinders.Binders.Add(typeof(VisualEditorPluginViewModel), new VisualEditorPluginViewModelBinder());
            ModelBinders.Binders.Add(typeof(VisualEditorStyleViewModel), new VisualEditorStyleViewModelBinder());
            ModelBinders.Binders.Add(typeof(SiteViewModel), new SiteViewModelBinder());
            ModelBinders.Binders.Add(typeof(UserDefaultFilter), new UserDefaultFilterBinder());
            ModelBinders.Binders.Add(typeof(PageTemplateViewModel), new PageTemplateViewModelBinder());
            ModelBinders.Binders.Add(typeof(ObjectViewModel), new ObjectViewModelBinder());
            ModelBinders.Binders.Add(typeof(DbViewModel), new DbViewModelBinder());
            ModelBinders.Binders.Add(typeof(ExportViewModel), new ExportViewModelBinder());
            ModelBinders.Binders.DefaultBinder = new QpModelBinder();
        }

        protected void Application_AcquireRequestState(object sender, EventArgs e)
        {
            var userIdentity = HttpContext.Current.User.Identity as QPIdentity;
            var cultureName = userIdentity != null
                ? userIdentity.CultureName
                : QPConfiguration.WebConfigSection.Globalization.DefaultCulture;

            var cultureInfo = new CultureInfo(cultureName);
            Thread.CurrentThread.CurrentUICulture = cultureInfo;
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(cultureInfo.Name);
        }

        protected void Session_Start(object sender, EventArgs e)
        {
            Session["theme"] = QPConfiguration.WebConfigSection.DefaultTheme;
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            var exp = Server.GetLastError();
            if (exp != null)
            {
                EnterpriseLibraryContainer.Current.GetInstance<ExceptionManager>().HandleException(exp, "Policy");
            }
        }
    }
}
