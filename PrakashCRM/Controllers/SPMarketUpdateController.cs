using ClosedXML.Excel;
using PrakashCRM.Data.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace PrakashCRM.Controllers
{
    [RedirectingAction]
    public class SPMarketUpdateController : Controller
    {
        // GET: SPMarketUpdate
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult MarketUpdateList()
        {
            return View();
        }
       
        public async Task<JsonResult> GetMarketUpdateListData(int orderBy, string orderDir, string filter, int skip, int top)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPMarketUpdate/";

            string orderByField = "";

            switch (orderBy)
            {
                case 1:
                    orderByField = "Update_Date " + orderDir;
                    break;
                case 2:
                    orderByField = "Employee_Name " + orderDir;
                    break;
                default:
                    orderByField = "Update_Date asc";
                    break;
            }

            apiUrl = apiUrl + "GetAllMarketUpdateDetails?SPNo=" + Session["loggedInUserNo"].ToString() + "&skip=" + skip + "&top=" + top + "&orderby=" + orderByField + "&filter=" + filter;

            HttpClient client = new HttpClient();
            List<SPMarketUpdateList> marketUpdateList = new List<SPMarketUpdateList>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                marketUpdateList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPMarketUpdateList>>(data);
            }

            return Json(marketUpdateList, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> ExportListData(int orderBy, string orderDir, string filter, int skip, int top)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPMarketUpdate/";

            string orderByField = "";

            switch (orderBy)
            {
                case 1:
                    orderByField = "Update_Date " + orderDir;
                    break;
                case 2:
                    orderByField = "Employee_Name " + orderDir;
                    break;
                default:
                    orderByField = "Update_Date asc";
                    break;
            }

            apiUrl = apiUrl + "GetAllMarketUpdateDetails?SPNo=" + Session["loggedInUserNo"].ToString() + "&skip=" + skip + "&top=" + top + "&orderby=" + orderByField + "&filter=" + filter + "&isExport=true";

            HttpClient client = new HttpClient();
            List<SPMarketUpdateList> marketUpdateList = new List<SPMarketUpdateList>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                marketUpdateList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPMarketUpdateList>>(data);
            }

            DataTable dt = ToDataTable(marketUpdateList);

            //Name of File  
            string fileName = "MarketUpdate.xlsx";
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