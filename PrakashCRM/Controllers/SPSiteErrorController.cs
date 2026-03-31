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

                if (string.IsNullOrWhiteSpace(siteError.UserID))
                {
                    siteError.UserID = ResolveLoggedInUserName();
                }

                if (string.IsNullOrWhiteSpace(siteError.CurrentDateTime))
                {
                    siteError.CurrentDateTime = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss");
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

            if (errorCode.Equals("ACTION_ERR", StringComparison.OrdinalIgnoreCase) &&
                source.StartsWith("Login Action", StringComparison.OrdinalIgnoreCase))
                return false;

            return true;
        }

        public async Task<JsonResult> GetSiteErrorListData(int orderBy, string orderDir, string filter, int skip, int top)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPSiteError/";

            string orderByField = "";

            switch (orderBy)
            {
                case 1:
                    orderByField = "CurrentDateTime " + orderDir;
                    break;
                case 2:
                    orderByField = "UserID " + orderDir;
                    break;
                case 3:
                    orderByField = "Error_Code " + orderDir;
                    break;
                case 4:
                    orderByField = "Exception_Message " + orderDir;
                    break;
                case 5:
                    orderByField = "Source " + orderDir;
                    break;
                case 6:
                    orderByField = "IP_Address " + orderDir;
                    break;
                case 7:
                    orderByField = "Browser " + orderDir;
                    break;
                case 8:
                    orderByField = "Description " + orderDir;
                    break;
                case 9:
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
                    orderByField = "CurrentDateTime " + orderDir;
                    break;
                case 2:
                    orderByField = "UserID " + orderDir;
                    break;
                case 3:
                    orderByField = "Error_Code " + orderDir;
                    break;
                case 4:
                    orderByField = "Exception_Message " + orderDir;
                    break;
                case 5:
                    orderByField = "Source " + orderDir;
                    break;
                case 6:
                    orderByField = "IP_Address " + orderDir;
                    break;
                case 7:
                    orderByField = "Browser " + orderDir;
                    break;
                case 8:
                    orderByField = "Description " + orderDir;
                    break;
                case 9:
                    orderByField = "Exception_Stack_Trace " + orderDir;
                    break;
            }

            if (top <= 0)
            {
                using (HttpClient countClient = new HttpClient())
                {
                    string countApiUrl = apiUrl + "GetApiRecordsCount?apiEndPointName=SiteErrorsListDotNetAPI&filter=" + filter;
                    countClient.BaseAddress = new Uri(countApiUrl);
                    countClient.DefaultRequestHeaders.Accept.Clear();
                    countClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    HttpResponseMessage countResponse = await countClient.GetAsync(countApiUrl);
                    if (countResponse.IsSuccessStatusCode)
                    {
                        var countData = await countResponse.Content.ReadAsStringAsync();
                        int totalRecords;
                        if (int.TryParse(countData, out totalRecords) && totalRecords > 0)
                        {
                            top = totalRecords;
                        }
                    }
                }
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

            //Name of File  
            string fileName = "SiteErrorList.xlsx";
            string fullPath = Path.Combine(Server.MapPath("~/temp"), fileName);
            using (XLWorkbook wb = new XLWorkbook())
            {
                var worksheet = wb.Worksheets.Add("Site Error List");

                worksheet.Cell(1, 1).Value = "Site Error List";
                worksheet.Range(1, 1, 1, 10).Merge();
                worksheet.Cell(1, 1).Style.Font.Bold = true;
                worksheet.Cell(1, 1).Style.Font.FontSize = 16;
                worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(1, 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#D9EAF7");

                worksheet.Cell(2, 1).Value = "Generated On";
                worksheet.Cell(2, 2).Value = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss");
                worksheet.Range(2, 1, 2, 2).Style.Font.Bold = true;

                string[] headers = new[]
                {
                    "Date & Time",
                    "User Name",
                    "Error Code",
                    "Exception Message",
                    "Source",
                    "IP Address",
                    "Browser",
                    "Description",
                    "Exception Trace",
                    "Web URL"
                };

                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cell(4, i + 1).Value = headers[i];
                }

                var headerRange = worksheet.Range(4, 1, 4, headers.Length);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#1F4E78");
                headerRange.Style.Font.FontColor = XLColor.White;
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                int row = 5;
                foreach (var item in siteerror)
                {
                    worksheet.Cell(row, 1).Value = item.CurrentDateTime ?? string.Empty;
                    worksheet.Cell(row, 2).Value = item.UserID ?? string.Empty;
                    worksheet.Cell(row, 3).Value = item.Error_Code ?? string.Empty;
                    worksheet.Cell(row, 4).Value = item.Exception_Message ?? string.Empty;
                    worksheet.Cell(row, 5).Value = item.Source ?? string.Empty;
                    worksheet.Cell(row, 6).Value = item.IP_Address ?? string.Empty;
                    worksheet.Cell(row, 7).Value = item.Browser ?? string.Empty;
                    worksheet.Cell(row, 8).Value = item.Description ?? string.Empty;
                    worksheet.Cell(row, 9).Value = item.Exception_Stack_Trace ?? string.Empty;
                    worksheet.Cell(row, 10).Value = item.Web_URL ?? string.Empty;
                    row++;
                }

                var dataRange = worksheet.Range(4, 1, Math.Max(row - 1, 4), headers.Length);
                dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                dataRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                worksheet.Range(5, 4, Math.Max(row - 1, 5), 10).Style.Alignment.WrapText = true;

                worksheet.SheetView.FreezeRows(4);
                worksheet.Columns().AdjustToContents();
                worksheet.Column(4).Width = 40;
                worksheet.Column(8).Width = 30;
                worksheet.Column(9).Width = 50;
                worksheet.Column(10).Width = 45;

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

        private string ResolveLoggedInUserName()
        {
            string firstName = Session["loggedInUserFName"] == null ? string.Empty : Session["loggedInUserFName"].ToString().Trim();
            string lastName = Session["loggedInUserLName"] == null ? string.Empty : Session["loggedInUserLName"].ToString().Trim();
            string fullName = string.Join(" ", new[] { firstName, lastName }.Where(value => !string.IsNullOrWhiteSpace(value)));

            if (!string.IsNullOrWhiteSpace(fullName))
                return fullName;

            if (Session["loggedInUserEmail"] != null && !string.IsNullOrWhiteSpace(Session["loggedInUserEmail"].ToString()))
                return Session["loggedInUserEmail"].ToString().Trim();

            if (Session["loggedInUserSPCode"] != null && !string.IsNullOrWhiteSpace(Session["loggedInUserSPCode"].ToString()))
                return Session["loggedInUserSPCode"].ToString().Trim();

            if (Session["loggedInUserNo"] != null && !string.IsNullOrWhiteSpace(Session["loggedInUserNo"].ToString()))
                return Session["loggedInUserNo"].ToString().Trim();

            return "Guest User";
        }
        
    }
}