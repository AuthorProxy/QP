using System.Web.Mvc;
using System.Web.Routing;

namespace QP8.TestCustomActionsHost
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.MapRoute("Default", "{controller}/{action}/{id}", new { controller = "Action", action = "Index", id = UrlParameter.Optional });
        }
    }
}
