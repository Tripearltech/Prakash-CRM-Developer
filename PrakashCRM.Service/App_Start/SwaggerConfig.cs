using System.Web.Http;
using Swashbuckle.Application;

namespace PrakashCRM.Service.App_Start
{
    public class SwaggerConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config
                .EnableSwagger(c =>
                {
                    c.SingleApiVersion("v1", "PrakashCRM Service API")
                        .Description("PrakashCRM service endpoints documentation");
                    c.UseFullTypeNameInSchemaIds();
                    c.PrettyPrint();
                })
                .EnableSwaggerUi();
        }
    }
}
