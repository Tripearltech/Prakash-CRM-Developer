using Newtonsoft.Json;
using PrakashCRM.Data.Models;
using System;
using System.Configuration;
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
    }
}
