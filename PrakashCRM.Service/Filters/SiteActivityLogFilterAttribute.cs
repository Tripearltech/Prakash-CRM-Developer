using PrakashCRM.Service.Classes;
using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Filters;

namespace PrakashCRM.Service.Filters
{
    public class SiteActivityLogFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            base.OnActionExecuted(actionExecutedContext);

            if (actionExecutedContext == null || actionExecutedContext.Exception != null)
                return;

            var controllerName = actionExecutedContext.ActionContext?.ControllerContext?.ControllerDescriptor?.ControllerName ?? string.Empty;
            if (controllerName.Equals("SPSiteActivity", StringComparison.OrdinalIgnoreCase) ||
                controllerName.Equals("SPSiteError", StringComparison.OrdinalIgnoreCase) ||
                controllerName.Equals("SPSPSiteError", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var method = actionExecutedContext.ActionContext?.Request?.Method?.Method ?? "GET";
            var statusCode = actionExecutedContext.Response != null ? ((int)actionExecutedContext.Response.StatusCode).ToString() : "200";

            var userCode = "System";
            var queryParams = actionExecutedContext.ActionContext?.Request?.GetQueryNameValuePairs();
            if (queryParams != null)
            {
                var spCode = queryParams.FirstOrDefault(x => x.Key.Equals("SPCode", StringComparison.OrdinalIgnoreCase)).Value;
                if (!string.IsNullOrWhiteSpace(spCode))
                    userCode = spCode;
            }

            string actionDescription;
            switch (method.ToUpper())
            {
                case "POST":
                    actionDescription = "Added";
                    break;
                case "PUT":
                case "PATCH":
                    actionDescription = "Updated";
                    break;
                case "DELETE":
                    actionDescription = "Deleted";
                    break;
                default:
                    actionDescription = "Viewed";
                    break;
            }

            try
            {
                API ac = new API();
                ac.PostSiteActivity(method + statusCode, actionDescription, userCode);
            }
            catch
            {
            }
        }
    }
}
