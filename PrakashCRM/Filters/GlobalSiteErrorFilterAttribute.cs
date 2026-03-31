using Newtonsoft.Json;
using PrakashCRM.Data.Models;
using System;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace PrakashCRM.Filters
{
    public class GlobalSiteErrorFilterAttribute : HandleErrorAttribute
    {
        public override void OnException(ExceptionContext filterContext)
        {
            base.OnException(filterContext);

            if (filterContext == null || filterContext.Exception == null || filterContext.HttpContext == null)
                return;

            try
            {
                var request = filterContext.HttpContext.Request;
                string ip = request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? request.ServerVariables["REMOTE_ADDR"];
                string browser = request.Browser != null ? request.Browser.Browser : "Unknown";

                SPSiteError payload = new SPSiteError
                {
                    UserID = ResolveLoggedInUserName(filterContext.HttpContext),
                    CurrentDateTime = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"),
                    Error_Code = "MVC_ERR",
                    Exception_Message = filterContext.Exception.Message ?? "Unhandled exception",
                    Exception_Stack_Trace = filterContext.Exception.StackTrace ?? "",
                    Source = (filterContext.RouteData.Values["controller"] ?? "MVC").ToString(),
                    IP_Address = ip,
                    Browser = browser,
                    Description = "Severity: " + GetSeverity(filterContext.Exception),
                    Web_URL = request.RawUrl
                };

                string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"];
                if (!string.IsNullOrWhiteSpace(apiUrl))
                {
                    string endpoint = apiUrl.TrimEnd('/') + "/SPSiteError/LogError";
                    using (HttpClient client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
                        client.PostAsync(endpoint, content).GetAwaiter().GetResult();
                    }
                }
            }
            catch
            {
            }
        }

        private string GetSeverity(Exception ex)
        {
            if (ex == null)
                return "Warning";

            if (ex is OutOfMemoryException || ex is StackOverflowException || ex is AccessViolationException)
                return "Critical";

            if (ex is NullReferenceException || ex is InvalidOperationException || ex is HttpException)
                return "High";

            if (ex is ArgumentException || ex is FormatException)
                return "Medium";

            return "Warning";
        }

        private static string ResolveLoggedInUserName(HttpContextBase httpContext)
        {
            if (httpContext == null || httpContext.Session == null)
                return "Guest User";

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

            return "Guest User";
        }
    }
}
