using PrakashCRM.Service.Classes;
using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Filters;

namespace PrakashCRM.Service.Filters
{
    public class SiteErrorLogFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            base.OnException(actionExecutedContext);

            if (actionExecutedContext == null || actionExecutedContext.Exception == null)
                return;

            var controllerName = actionExecutedContext.ActionContext?.ControllerContext?.ControllerDescriptor?.ControllerName ?? string.Empty;
            if (controllerName.Equals("SPSiteError", StringComparison.OrdinalIgnoreCase) ||
                controllerName.Equals("SPSPSiteError", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            string userCode = "System";
            var queryParams = actionExecutedContext.ActionContext?.Request?.GetQueryNameValuePairs();
            if (queryParams != null)
            {
                var spCode = queryParams.FirstOrDefault(x => x.Key.Equals("SPCode", StringComparison.OrdinalIgnoreCase)).Value;
                if (!string.IsNullOrWhiteSpace(spCode))
                    userCode = spCode;
            }

            try
            {
                API ac = new API();
                ac.PostSiteError(actionExecutedContext.Exception);

                string message = actionExecutedContext.Exception.Message ?? "Unhandled exception";
                if (message.Length > 60)
                    message = message.Substring(0, 60);

                ac.PostSiteActivity("ERROR", "Error: " + message, userCode);
            }
            catch
            {
            }
        }
    }
}
