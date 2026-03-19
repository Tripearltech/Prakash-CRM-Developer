using ClosedXML.Excel;
using DocumentFormat.OpenXml.Bibliography;
using PrakashCRM.Data.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace PrakashCRM.Controllers
{
    [RedirectingAction]
    public class SPSiteErrorController : Controller
    {
        // GET: SPSiteError
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult SiteErrorList()
        {
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> LogClientError(SPSiteError siteError)
        {
            bool isLogged = false;

            try
            {
                if (siteError == null && Request != null && Request.InputStream != null)
                {
                    try
                    {
                        Request.InputStream.Position = 0;
                        using (var reader = new StreamReader(Request.InputStream))
                        {
                            var rawBody = await reader.ReadToEndAsync();
                            if (!string.IsNullOrWhiteSpace(rawBody))
                                siteError = Newtonsoft.Json.JsonConvert.DeserializeObject<SPSiteError>(rawBody);
                        }
                    }
                    catch
                    {
                    }
                }

                if (siteError == null)
                    return Json(new { success = false }, JsonRequestBehavior.AllowGet);

                if (!ShouldLogSiteError(siteError))
                    return Json(new { success = true, skipped = true }, JsonRequestBehavior.AllowGet);

                string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString().TrimEnd('/') + "/SPSiteError/LogError";

                if (string.IsNullOrWhiteSpace(siteError.IP_Address))
                {
                    siteError.IP_Address = Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? Request.ServerVariables["REMOTE_ADDR"];
                }

                if (string.IsNullOrWhiteSpace(siteError.Browser))
                {
                    siteError.Browser = Request.Browser != null ? Request.Browser.Browser : "Unknown";
                }

                if (string.IsNullOrWhiteSpace(siteError.Web_URL))
                {
                    siteError.Web_URL = Request.Url != null ? Request.Url.PathAndQuery : "";
                }

                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri(apiUrl);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    string payload = Newtonsoft.Json.JsonConvert.SerializeObject(siteError);
                    var content = new StringContent(payload, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.PostAsync(apiUrl, content);
                    isLogged = response.IsSuccessStatusCode;
                }
            }
            catch
            {
                isLogged = false;
            }

            return Json(new { success = isLogged }, JsonRequestBehavior.AllowGet);
        }

        private static bool ShouldLogSiteError(SPSiteError siteError)
        {
            if (siteError == null)
                return false;

            var errorCode = (siteError.Error_Code ?? string.Empty).Trim();
            var message = (siteError.Exception_Message ?? string.Empty).Trim();
            var source = (siteError.Source ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(message))
                return false;

            if (errorCode.Equals("ACTION_ERR", StringComparison.OrdinalIgnoreCase))
                return false;

            if (source.StartsWith("Client Save", StringComparison.OrdinalIgnoreCase) ||
                source.StartsWith("Client Update", StringComparison.OrdinalIgnoreCase) ||
                source.StartsWith("Client Edit", StringComparison.OrdinalIgnoreCase) ||
                source.StartsWith("Login Action", StringComparison.OrdinalIgnoreCase))
                return false;

            string normalizedMessage = message.ToLowerInvariant();
            string[] ignoredPrefixes = new[]
            {
                "please ",
                "select ",
                "enter ",
                "added successfully",
                "updated successfully",
                "saved successfully",
                "sent successfully",
                "invalid otp",
                "invalid login",
                "login failed"
            };

            for (int i = 0; i < ignoredPrefixes.Length; i++)
            {
                if (normalizedMessage.StartsWith(ignoredPrefixes[i]))
                    return false;
            }

            return true;
        }

        public async Task<JsonResult> GetSiteErrorListData(int orderBy, string orderDir, string filter, int skip, int top)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPSiteError/";

            string orderByField = "";

            switch (orderBy)
            {
                case 1:
                    orderByField = "Error_Code " + orderDir;
                    break;
                case 2:
                    orderByField = "Exception_Message " + orderDir;
                    break;
                case 3:
                    orderByField = "Source " + orderDir;
                    break;
                case 4:
                    orderByField = "IP_Address " + orderDir;
                    break;
                case 5:
                    orderByField = "Browser " + orderDir;
                    break;
                case 6:
                    orderByField = "Description " + orderDir;
                    break;
                case 7:
                    orderByField = "Exception_Stack_Trace " + orderDir;
                    break;
            }

            //apiUrl = apiUrl + "GetSiteError?SPCode=" + Session["loggedInUserSPCode"].ToString() + "&skip=" + skip + "&top=" + top + "&orderby=" + orderByField + "&filter=" + filter;
            apiUrl = apiUrl + "GetSiteError?skip=" + skip + "&top=" + top + "&orderby=" + orderByField + "&filter=" + filter;

            HttpClient client = new HttpClient();
            List<SPSiteError> siteerror = new List<SPSiteError>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                siteerror = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPSiteError>>(data);
            }

            return Json(siteerror, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> ExportListData(int orderBy, string orderDir, string filter, int skip, int top)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPSiteError/";

            string orderByField = "";

            switch (orderBy)
            {
                case 1:
                    orderByField = "Error_Code " + orderDir;
                    break;
                case 2:
                    orderByField = "Exception_Message " + orderDir;
                    break;
                case 3:
                    orderByField = "Source " + orderDir;
                    break;
                case 4:
                    orderByField = "IP_Address " + orderDir;
                    break;
                case 5:
                    orderByField = "Browser " + orderDir;
                    break;
                case 6:
                    orderByField = "Description " + orderDir;
                    break;
                case 7:
                    orderByField = "Exception_Stack_Trace " + orderDir;
                    break;
            }

            apiUrl = apiUrl + "GetSiteError?skip=" + skip + "&top=" + top + "&orderby=" + orderByField + "&filter=" + filter;

            HttpClient client = new HttpClient();
            List<SPSiteError> siteerror = new List<SPSiteError>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                siteerror = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPSiteError>>(data);
            }
            DataTable dt = ToDataTable(siteerror);

            //Name of File  
            string fileName = "SiteErrorList.xlsx";
            string fullPath = Path.Combine(Server.MapPath("~/temp"), fileName);
            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(dt);

                using (var exportData = new MemoryStream())
                {
                    wb.SaveAs(exportData);
                    FileStream file = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
                    exportData.WriteTo(file);
                    file.Close();
                }
            }

            return Json(new { fileName = fileName, errorMessage = "" }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Download(string file)
        {
            //get the temp folder and file path in server
            string fullPath = Path.Combine(Server.MapPath("~/temp"), file);

            //return the file for download, this is an Excel 
            //so I set the file content type to "application/vnd.ms-excel"
            return File(fullPath, "application/vnd.ms-excel", file);
        }

        public static DataTable ToDataTable<T>(List<T> items)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);

            //Get all the properties
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Defining type of data column gives proper data table 
                var type = (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) ? Nullable.GetUnderlyingType(prop.PropertyType) : prop.PropertyType);
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name, type);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            //put a breakpoint here and check datatable
            return dataTable;
        }
        
    }
}