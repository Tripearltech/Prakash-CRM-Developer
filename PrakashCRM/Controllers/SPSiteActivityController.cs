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
                case 2:
                    orderByField = "Trace_Id " + orderDir;
                    break;
                case 3:
                    orderByField = "IP_Address " + orderDir;
                    break;
                case 4:
                    orderByField = "Browser " + orderDir;
                    break;
                case 5:
                    orderByField = "Description " + orderDir;
                    break;
                case 6:
                    orderByField = "Web_URL " + orderDir;
                    break;
                case 7:
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
                case 2:
                    orderByField = "Trace_Id " + orderDir;
                    break;
                case 3:
                    orderByField = "IP_Address " + orderDir;
                    break;
                case 4:
                    orderByField = "Browser " + orderDir;
                    break;
                case 5:
                    orderByField = "Description " + orderDir;
                    break;
                case 6:
                    orderByField = "Web_URL " + orderDir;
                    break;
                case 7:
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
            DataTable dt = ToDataTable(siteactivity);

            //Name of File  
            string fileName = "SiteActivityList.xlsx";
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