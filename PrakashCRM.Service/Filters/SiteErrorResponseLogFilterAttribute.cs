using PrakashCRM.Service.Classes;
using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Filters;

namespace PrakashCRM.Service.Filters
{
    public class SiteErrorResponseLogFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            base.OnActionExecuted(actionExecutedContext);

            if (actionExecutedContext == null || actionExecutedContext.ActionContext == null)
                return;

            if (actionExecutedContext.Exception != null)
                return;

            var controllerName = actionExecutedContext.ActionContext.ControllerContext?.ControllerDescriptor?.ControllerName ?? string.Empty;
            if (controllerName.Equals("SPSiteError", StringComparison.OrdinalIgnoreCase) ||
                controllerName.Equals("SPSPSiteError", StringComparison.OrdinalIgnoreCase) ||
                controllerName.Equals("SPSiteActivity", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var method = actionExecutedContext.ActionContext.Request?.Method?.Method ?? "GET";
            if (!(method.Equals("POST", StringComparison.OrdinalIgnoreCase) ||
                  method.Equals("PUT", StringComparison.OrdinalIgnoreCase) ||
                  method.Equals("PATCH", StringComparison.OrdinalIgnoreCase) ||
                  method.Equals("DELETE", StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }

            var response = actionExecutedContext.Response;
            if (response == null || response.IsSuccessStatusCode)
                return;

            try
            {
                string errorCode = ((int)response.StatusCode).ToString();
                string requestUrl = actionExecutedContext.ActionContext.Request?.RequestUri != null
                    ? actionExecutedContext.ActionContext.Request.RequestUri.ToString()
                    : string.Empty;

                string body = string.Empty;
                try
                {
                    body = response.Content != null ? response.Content.ReadAsStringAsync().Result : string.Empty;
                }
                catch
                {
                }

                if (string.IsNullOrWhiteSpace(body))
                    body = response.ReasonPhrase;

                if (!string.IsNullOrWhiteSpace(body) && body.Length > 3000)
                    body = body.Substring(0, 3000);

                string spCode = "System";
                var queryParams = actionExecutedContext.ActionContext.Request?.GetQueryNameValuePairs();
                if (queryParams != null)
                {
                    var code = queryParams.FirstOrDefault(x => x.Key.Equals("SPCode", StringComparison.OrdinalIgnoreCase)).Value;
                    if (!string.IsNullOrWhiteSpace(code))
                        spCode = code;
                }

                API ac = new API();
                string trace = "TraceId: " + Guid.NewGuid().ToString("N") + " | User: " + spCode;
                ac.PostSiteErrorWithResponse(errorCode, body, requestUrl, method + " " + controllerName, trace);
            }
            catch
            {
            }
        }
    }
}
