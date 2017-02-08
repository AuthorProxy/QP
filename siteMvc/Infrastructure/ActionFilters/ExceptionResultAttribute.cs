using System.Web;
using System.Web.Mvc;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;
using Quantumart.QP8.WebMvc.Infrastructure.Extensions;
using Quantumart.QP8.WebMvc.Infrastructure.Helpers;

namespace Quantumart.QP8.WebMvc.Infrastructure.ActionFilters
{
    public class ExceptionResultAttribute : HandleErrorAttribute
    {
        private readonly ExceptionResultMode _mode;

        public ExceptionResultAttribute(ExceptionResultMode mode)
        {
            _mode = mode;
        }

        public override void OnException(ExceptionContext filterContext)
        {
            if (!ShouldBeHandled(filterContext))
            {
                return;
            }

            if (IsAjaxRequest(filterContext))
            {
                filterContext.Result = ActionResultHelpers.GererateJsonResultFromException(_mode, filterContext.Exception);
            }
            else
            {
                var controllerName = (string)filterContext.RouteData.Values["controller"];
                var actionName = (string)filterContext.RouteData.Values["action"];
                var model = new HandleErrorInfo(filterContext.Exception, controllerName, actionName);
                filterContext.Result = new ViewResult
                {
                    ViewName = View,
                    MasterName = Master,
                    ViewData = new ViewDataDictionary<HandleErrorInfo>(model),
                    TempData = filterContext.Controller.TempData
                };
            }

            Logger.Log.Error($"������� exception �� ��������� URL: {HttpContext.Current.Request.RawUrl}", filterContext.Exception);

            filterContext.ExceptionHandled = true;
            filterContext.HttpContext.Response.Clear();
            filterContext.HttpContext.Response.StatusCode = 500;
            filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
        }

        private static bool IsAjaxRequest(ControllerContext filterContext)
        {
            return filterContext.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest";
        }

        private bool ShouldBeHandled(ExceptionContext filterContext)
        {
            Ensure.Argument.NotNull(filterContext);
            if (filterContext.IsChildAction)
            {
                return false;
            }

            if (filterContext.ExceptionHandled || !filterContext.HttpContext.IsCustomErrorEnabled)
            {
                return false;
            }

            if (filterContext.HttpContext.IsXmlDbUpdateReplayAction())
            {
                return false;
            }

            return new HttpException(null, filterContext.Exception).GetHttpCode() == 500 && ExceptionType.IsInstanceOfType(filterContext.Exception);
        }
    }
}
