using System.Web.Http;
using Swashbuckle.Application;
using System.IO;
using Swashbuckle.Swagger;
using OpenXmlPowerTools;

namespace PrakashCRM.Service.App_Start
{
    public class SwaggerConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config
                .EnableSwagger(c =>
                {
                    c.SingleApiVersion("v1", "Whatsapp Attachment API");
                })
                .EnableSwaggerUi();
        }
    }
}
