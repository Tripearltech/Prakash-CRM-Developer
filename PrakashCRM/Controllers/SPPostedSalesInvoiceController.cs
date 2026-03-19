using Newtonsoft.Json;
using PrakashCRM.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using ClosedXML.Excel;
using System.IO;
using System.Reflection;
using System.Data;

namespace PrakashCRM.Controllers
{
    [RedirectingAction]
    public class SPPostedSalesInvoiceController : Controller
    {
        // GET: SPPostedSalesInvoice
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult PostedSalesInvoiceList()
        {
            return View();
        }

        public async Task<JsonResult> GetPostedSalesInvoiceListData(int orderBy, string orderDir, string filter, int skip, int top)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPPostedSalesInvoice/";

            string orderByField = "";

            switch (orderBy)
            {                       
                case 1:
                    orderByField = "No " + orderDir;
                    break;
                case 2:
                    orderByField = "Posting_Date " + orderDir + ",No " + orderDir;
                    break;
                case 3:
                    orderByField = "Due_Date " + orderDir;
                    break;
                case 4:
                    orderByField = "Amount " + orderDir;
                    break;
                case 5:
                    orderByField = "PCPL_Amount_Including_GST " + orderDir;
                    break;
                case 6:
                    orderByField = "Remaining_Amount " + orderDir;
                    break;
                case 7:
                    orderByField = "Closed " + orderDir;
                    break;
                default:
                    orderByField = "No asc";
                    break;
            }

            apiUrl = apiUrl + "GetAllPostedSalesInvoice?SPCode=" + Session["loggedInUserSPCode"].ToString() + "&skip=" + skip + "&top=" + top + "&orderby=" + orderByField + "&filter=" + filter;

            HttpClient client = new HttpClient();
            List<SPPostedSalesInvoiceList> PostedSalesInvoice = new List<SPPostedSalesInvoiceList>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                PostedSalesInvoice = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPPostedSalesInvoiceList>>(data);
            }

            return Json(PostedSalesInvoice, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> ExportListData(int orderBy, string orderDir, string filter, int skip, int top)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPPostedSalesInvoice/";

            string orderByField = "";

            switch (orderBy)
            {
                case 1:
                    orderByField = "No " + orderDir;
                    break;
                case 2:
                    orderByField = "Posting_Date " + orderDir;
                    break;
                case 3:
                    orderByField = "Due_Date " + orderDir;
                    break;
                case 4:
                    orderByField = "Amount " + orderDir;
                    break;
                case 5:
                    orderByField = "PCPL_Amount_Including_GST " + orderDir;
                    break;
                case 6:
                    orderByField = "Remaining_Amount " + orderDir;
                    break;
                case 7:
                    orderByField = "Closed " + orderDir;
                    break;
                default:
                    orderByField = "No asc";
                    break;
            }

            apiUrl = apiUrl + "GetAllPostedSalesInvoice?SPCode=" + Session["loggedInUserSPCode"].ToString() + "&skip=" + skip + "&top=" + top + "&orderby=" + orderByField + "&filter=" + filter;

            HttpClient client = new HttpClient();
            List<SPPostedSalesInvoiceList> PostedSalesInvoice = new List<SPPostedSalesInvoiceList>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                PostedSalesInvoice = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPPostedSalesInvoiceList>>(data);
            }
            DataTable dt = ToDataTable(PostedSalesInvoice);

            //Name of File  
            string fileName = "SalesInvoiceList.xlsx";
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