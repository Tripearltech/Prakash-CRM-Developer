using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Cors;
//using PrakashCRM.Service.App_Start;
using PrakashCRM.Service.Filters;

namespace PrakashCRM.Service
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.Filters.Add(new SiteActivityLogFilterAttribute());
            config.Filters.Add(new SiteErrorLogFilterAttribute());
            config.Filters.Add(new SiteErrorResponseLogFilterAttribute());

            // Enable CORS globally
            var cors = new EnableCorsAttribute("*", "*", "*");
            cors.ExposedHeaders.Add("X-RoleRights-Error");
            config.EnableCors(cors);

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

           // SwaggerConfig.Register(config);
        }
    }
}
