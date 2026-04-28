using System.Web.Mvc;
using System.Web.Routing;

namespace PrakashCRM
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapMvcAttributeRoutes();

            //routes.MapRoute(
            //    name: "DailyVisitAttachment",
            //    url: "DailyVisitAttachment/{action}/{id}",
            //    defaults: new { controller = "DailyVisitAttachment", action = "Index", id = UrlParameter.Optional },
            //    namespaces: new[] { "PrakashCRM.Controllers" }
            //);

            //routes.MapRoute(
            //    name: "CustOverDueOutstandinglist",
            //    url: "SPReports/CustOverDueOutstandinglist",
            //    defaults: new { controller = "SPReports", action = "CustOverDueOutstandinglist" }
            //);

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Account", action = "Login", id = UrlParameter.Optional },
                namespaces: new[] { "PrakashCRM.Controllers" } // 👈 ADD THIS
            );
        }
    }
}
