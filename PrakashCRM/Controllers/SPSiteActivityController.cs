using ClosedXML.Excel;
using PrakashCRM.Data.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Tracing;
using System.Web.Mvc;

namespace PrakashCRM.Controllers
{
    [RedirectingAction]
    public class SPSiteActivityController : Controller
    {
        // GET: SPSiteActivity
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult SiteActivityList()
        {
            return View();
        }

        public async Task<JsonResult> GetSiteActivityListData(int orderBy, string orderDir, string filter, int skip, int top)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPSiteActivity/";

            string orderByField = "";

            switch (orderBy)
            {
                case 1:
                    orderByField = "Activity_Date " + orderDir;
                    break;
                case 2:
                    orderByField = "Activity_User_Name " + orderDir;
                    break;
                case 3:
                    orderByField = "Module_Name " + orderDir;
                    break;
                case 4:
                    orderByField = "Trace_Id " + orderDir;
                    break;
                case 5:
                    orderByField = "IP_Address " + orderDir;
                    break;
                case 6:
                    orderByField = "Browser " + orderDir;
                    break;
                case 7:
                    orderByField = "Description " + orderDir;
                    break;
                case 8:
                    orderByField = "Web_URL " + orderDir;
                    break;
                case 9:
                    orderByField = "Device_Name " + orderDir;
                    break;
            }

            apiUrl = apiUrl + "GetSiteActivity?SPCode=" + Session["loggedInUserSPCode"].ToString() + "&skip=" + skip + "&top=" + top + "&orderby=" + orderByField + "&filter=" + filter;

            HttpClient client = new HttpClient();
            List<SPSiteActivity> siteactivity = new List<SPSiteActivity>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                siteactivity = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPSiteActivity>>(data);
            }

            return Json(siteactivity, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> ExportListData(int orderBy, string orderDir, string filter, int skip, int top)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPSiteActivity/";
            string orderByField = "";

            switch (orderBy)
            {
                case 1:
                    orderByField = "Activity_Date " + orderDir;
                    break;
                case 2:
                    orderByField = "Activity_User_Name " + orderDir;
                    break;
                case 3:
                    orderByField = "Module_Name " + orderDir;
                    break;
                case 4:
                    orderByField = "Trace_Id " + orderDir;
                    break;
                case 5:
                    orderByField = "IP_Address " + orderDir;
                    break;
                case 6:
                    orderByField = "Browser " + orderDir;
                    break;
                case 7:
                    orderByField = "Description " + orderDir;
                    break;
                case 8:
                    orderByField = "Web_URL " + orderDir;
                    break;
                case 9:
                    orderByField = "Device_Name " + orderDir;
                    break;
            }

            if (top <= 0)
            {
                using (HttpClient countClient = new HttpClient())
                {
                    string countApiUrl = apiUrl + "GetApiRecordsCount?apiEndPointName=SiteActivitiesListDotNetAPI&filter=" + filter;
                    countClient.BaseAddress = new Uri(countApiUrl);
                    countClient.DefaultRequestHeaders.Accept.Clear();
                    countClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

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

            apiUrl = apiUrl + "GetSiteActivity?SPCode=" + Session["loggedInUserSPCode"].ToString() + "&skip=" + skip + "&top=" + top + "&orderby=" + orderByField + "&filter=" + filter;

            HttpClient client = new HttpClient();
            List<SPSiteActivity> siteactivity = new List<SPSiteActivity>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                siteactivity = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPSiteActivity>>(data);
            }

            //Name of File  
            string fileName = "SiteActivityList.xlsx";
            string fullPath = Path.Combine(Server.MapPath("~/temp"), fileName);
            using (XLWorkbook wb = new XLWorkbook())
            {
                var worksheet = wb.Worksheets.Add("Site Activity List");

                worksheet.Cell(1, 1).Value = "Site Activity List";
                worksheet.Range(1, 1, 1, 11).Merge();
                worksheet.Cell(1, 1).Style.Font.Bold = true;
                worksheet.Cell(1, 1).Style.Font.FontSize = 16;
                worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(1, 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#D9EAF7");

                worksheet.Cell(2, 1).Value = "Generated On";
                worksheet.Cell(2, 2).Value = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss");
                worksheet.Range(2, 1, 2, 2).Style.Font.Bold = true;

                string[] headers = new[]
                {
                    "Activity Date",
                    "Activity User Name",
                    "Module Name",
                    "Trace Id",
                    "IP Address",
                    "Browser",
                    "Description",
                    "Web URL",
                    "Device Name",
                    "Company Code",
                    "MAC Address"
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
                foreach (var item in siteactivity)
                {
                    worksheet.Cell(row, 1).Value = item.Activity_Date ?? string.Empty;
                    worksheet.Cell(row, 2).Value = item.Activity_User_Name ?? string.Empty;
                    worksheet.Cell(row, 3).Value = item.Module_Name ?? string.Empty;
                    worksheet.Cell(row, 4).Value = item.Trace_Id ?? string.Empty;
                    worksheet.Cell(row, 5).Value = item.IP_Address ?? string.Empty;
                    worksheet.Cell(row, 6).Value = item.Browser ?? string.Empty;
                    worksheet.Cell(row, 7).Value = item.Description ?? string.Empty;
                    worksheet.Cell(row, 8).Value = item.Web_URL ?? string.Empty;
                    worksheet.Cell(row, 9).Value = item.Device_Name ?? string.Empty;
                    worksheet.Cell(row, 10).Value = item.Company_Code ?? string.Empty;
                    worksheet.Cell(row, 11).Value = item.MAC_Address ?? string.Empty;
                    row++;
                }

                var dataRange = worksheet.Range(4, 1, Math.Max(row - 1, 4), headers.Length);
                dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                dataRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                worksheet.Range(5, 7, Math.Max(row - 1, 5), 11).Style.Alignment.WrapText = true;

                worksheet.SheetView.FreezeRows(4);
                worksheet.Columns().AdjustToContents();
                worksheet.Column(7).Width = 35;
                worksheet.Column(8).Width = 45;
                worksheet.Column(11).Width = 24;

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