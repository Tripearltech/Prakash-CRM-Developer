using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Newtonsoft.Json;
using PrakashCRM.Data.Models;
using PrakashCRM.Security;

namespace PrakashCRM.Controllers
{
    public class RedirectingActionAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            string controllerName = filterContext.RouteData.Values["controller"] != null ? filterContext.RouteData.Values["controller"].ToString() : "";
            string actionName = filterContext.RouteData.Values["action"] != null ? filterContext.RouteData.Values["action"].ToString() : "";

            if (controllerName.Equals("SPSiteError", StringComparison.OrdinalIgnoreCase) &&
                actionName.Equals("LogClientError", StringComparison.OrdinalIgnoreCase))
            {
                HttpContextBase skipContext = filterContext.HttpContext;
                skipContext.Response.AddCacheItemDependency("Pages");
                return;
            }

            if (HttpContext.Current.Session["loggedInUserFName"] == null || HttpContext.Current.Session["loggedInUserFName"].ToString() == "")
            {
                SetUnauthorizedResult(filterContext);
                return;
            }

            var userNo = HttpContext.Current.Session["loggedInUserNo"]?.ToString();
            var token = HttpContext.Current.Session["AuthToken"]?.ToString();
            if (string.IsNullOrWhiteSpace(token))
                token = HttpContext.Current.Session.SessionID;

            if (!UserTokenStore.IsTokenActive(userNo, token))
            {
                HttpContext.Current.Session.Clear();
                HttpContext.Current.Session.Abandon();
                SetUnauthorizedResult(filterContext);
                return;
            }

            HttpContextBase httpContext = filterContext.HttpContext;
            httpContext.Response.AddCacheItemDependency("Pages");
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            base.OnActionExecuted(filterContext);

            try
            {
                if (filterContext == null || filterContext.HttpContext == null || filterContext.HttpContext.Request == null)
                    return;

                var routeData = filterContext.RouteData;
                string controllerName = routeData.Values["controller"] != null ? routeData.Values["controller"].ToString() : "";
                string actionName = routeData.Values["action"] != null ? routeData.Values["action"].ToString() : "";
                string method = filterContext.HttpContext.Request.HttpMethod ?? "GET";

                if (controllerName.Equals("SPSiteActivity", StringComparison.OrdinalIgnoreCase) ||
                    controllerName.Equals("SPSiteError", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                string description;
                if (filterContext.Exception != null)
                {
                    description = "Error on " + actionName;
                }
                else
                {
                    switch (method.ToUpper())
                    {
                        case "POST":
                            description = "Added";
                            break;
                        case "PUT":
                        case "PATCH":
                            description = "Updated";
                            break;
                        case "DELETE":
                            description = "Deleted";
                            break;
                        default:
                            description = "Viewed";
                            break;
                    }
                }

                LogSiteActivity(filterContext.HttpContext, controllerName, method, actionName, description);
                filterContext.HttpContext.Items["SiteActivityLogged"] = true;
            }
            catch
            {
            }
        }

        private static void LogSiteActivity(HttpContextBase httpContext, string controllerName, string method, string actionName, string description)
        {
            string serviceApiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"];
            if (string.IsNullOrWhiteSpace(serviceApiUrl))
                return;

            string spCode = "System";
            if (httpContext.Session != null && httpContext.Session["loggedInUserSPCode"] != null)
                spCode = httpContext.Session["loggedInUserSPCode"].ToString();

            var payload = new SPSiteActivity
            {
                Activity_User_Name = ResolveLoggedInUserName(httpContext),
                Activity_Date = DateTime.Now.ToString("dd-MM-yyyy"),
                Module_Name = controllerName,
                Trace_Id = method,
                IP_Address = httpContext.Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? httpContext.Request.ServerVariables["REMOTE_ADDR"],
                Browser = ResolveBrowserName(httpContext.Request),
                Description = description + " - " + actionName,
                Web_URL = httpContext.Request.RawUrl,
                Company_Code = spCode,
                MAC_Address = "",
                Device_Name = Environment.MachineName
            };

            if (payload.Description != null && payload.Description.Length > 100)
                payload.Description = payload.Description.Substring(0, 100);

            using (HttpClient client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(3);
                string endpoint = serviceApiUrl.TrimEnd('/') + "/SPSiteActivity/LogActivity";
                var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
                client.PostAsync(endpoint, content).GetAwaiter().GetResult();
            }
        }

        private static string ResolveLoggedInUserName(HttpContextBase httpContext)
        {
            if (httpContext == null || httpContext.Session == null)
                return "System";

            string firstName = httpContext.Session["loggedInUserFName"] == null ? string.Empty : httpContext.Session["loggedInUserFName"].ToString().Trim();
            string lastName = httpContext.Session["loggedInUserLName"] == null ? string.Empty : httpContext.Session["loggedInUserLName"].ToString().Trim();
            string fullName = string.Join(" ", new[] { firstName, lastName }.Where(value => !string.IsNullOrWhiteSpace(value)));

            if (!string.IsNullOrWhiteSpace(fullName))
                return fullName;

            if (httpContext.Session["loggedInUserEmail"] != null && !string.IsNullOrWhiteSpace(httpContext.Session["loggedInUserEmail"].ToString()))
                return httpContext.Session["loggedInUserEmail"].ToString().Trim();

            if (httpContext.Session["loggedInUserSPCode"] != null && !string.IsNullOrWhiteSpace(httpContext.Session["loggedInUserSPCode"].ToString()))
                return httpContext.Session["loggedInUserSPCode"].ToString().Trim();

            if (httpContext.Session["loggedInUserNo"] != null && !string.IsNullOrWhiteSpace(httpContext.Session["loggedInUserNo"].ToString()))
                return httpContext.Session["loggedInUserNo"].ToString().Trim();

            return "System";
        }

        private static void SetUnauthorizedResult(ActionExecutingContext filterContext)
        {
            var request = filterContext?.HttpContext?.Request;
            var isAjaxLikeRequest = request != null &&
                (request.IsAjaxRequest()
                || string.Equals(request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase)
                || ((request.Headers["Accept"] ?? string.Empty).IndexOf("application/json", StringComparison.OrdinalIgnoreCase) >= 0));

            if (isAjaxLikeRequest)
            {
                filterContext.Result = new HttpStatusCodeResult(HttpStatusCode.Unauthorized);
                return;
            }

            filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new
            {
                controller = "Account",
                action = "Login"
            }));
        }

        private static string ResolveBrowserName(HttpRequestBase request)
        {
            if (request == null)
                return "Unknown";

            string browser = request.Browser != null ? request.Browser.Browser : "";
            if (!string.IsNullOrWhiteSpace(browser) && !browser.Equals("Unknown", StringComparison.OrdinalIgnoreCase))
                return browser;

            string ua = request.UserAgent ?? "";
            string ual = ua.ToLowerInvariant();

            if (ual.Contains("edg/")) return "Microsoft Edge";
            if (ual.Contains("opr/") || ual.Contains("opera")) return "Opera";
            if (ual.Contains("chrome/") && !ual.Contains("edg/")) return "Google Chrome";
            if (ual.Contains("firefox/")) return "Mozilla Firefox";
            if (ual.Contains("safari/") && !ual.Contains("chrome/")) return "Safari";
            if (ual.Contains("msie") || ual.Contains("trident/")) return "Internet Explorer";

            return "Unknown";
        }
    }
    public class BaseController : Controller
    {
        // GET: Base
        public ActionResult Index()
        {
            return View();
        }
    }
}
