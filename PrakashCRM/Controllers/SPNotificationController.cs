using ClosedXML.Excel;
using Newtonsoft.Json;
using PrakashCRM.Data.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace PrakashCRM.Controllers
{
    [RedirectingAction]
    public class SPNotificationController : Controller
    {
        // GET: SPNotification
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Notification(string Type = "", string Employee_No = "")
        {
            SPNotification notification = new SPNotification();

            if (Type != "" || Employee_No != "" || Session["NotificationType"] != null || Session["NotificationEmpNo"] != null)
            {
                if (Session["NotificationType"] == null)
                    Session["NotificationType"] = Type;

                if (Session["NotificationEmpNo"] == null)
                    Session["NotificationEmpNo"] = Employee_No;

                Task<SPNotification> task = Task.Run<SPNotification>(async () => await GetNotificationForEdit(Session["NotificationType"].ToString(), Session["NotificationEmpNo"].ToString()));
                notification = task.Result;
                Session["isNotificationEdit"] = true;
                
            }

            if (notification != null)
                return View(notification);
            else
                return View(new SPNotification());

        }

        [HttpPost]
        public async Task<ActionResult> Notification(SPNotification notification)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPNotification/";

            string NotifType = "";
            string NotifEmployee_No = "";
            if (Convert.ToBoolean(Session["isNotificationEdit"]) == true)
            {
                NotifType = Session["NotificationType"].ToString();
                NotifEmployee_No = Session["NotificationEmpNo"].ToString();
                apiUrl = apiUrl + "Notification?isEdit=true&NotifType=" + NotifType + "&NotifEmployee_No=" + NotifEmployee_No;
            }
            else
                apiUrl = apiUrl + "Notification?isEdit=false&NotifType=" + NotifType + "&NotifEmployee_No=" + NotifEmployee_No;

            SPNotification resNotification = new SPNotification();

            HttpClient client = new HttpClient();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            string UserObjString = JsonConvert.SerializeObject(notification);
            var content = new StringContent(UserObjString, Encoding.UTF8, "application/json");

            HttpRequestMessage request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(apiUrl),
                Content = content
            };

            HttpResponseMessage response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                resNotification = Newtonsoft.Json.JsonConvert.DeserializeObject<SPNotification>(data);
            }

            if (resNotification != null && Convert.ToBoolean(Session["isNotificationEdit"]) == true)
                Session["NotificationAction"] = "Notification Setup Updated";
            else if (resNotification != null)
                Session["NotificationAction"] = "Notification Setup Saved";
            else
                Session["NotificationAction"] = "Error";

            return RedirectToAction("Notification");
        }

        public bool NullNotificationSession()
        {
            bool isSessionNull = false;

            Session["NotificationAction"] = "";
            isSessionNull = true;

            return isSessionNull;
        }

        public async Task<JsonResult> GetAllSPNoCodeForDDL()
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPNotification/";

            apiUrl = apiUrl + "GetAllSPEmployeeCode";

            HttpClient client = new HttpClient();
            List<SPNoCodeForNotif> spNoCode = new List<SPNoCodeForNotif>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                spNoCode = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPNoCodeForNotif>>(data);
            }

            return Json(spNoCode, JsonRequestBehavior.AllowGet);

            //spNoCode.Add(new SPNoCodeForNotif
            //    {
            //        No = "E0150",
            //        PCPL_Employee_Code = "AA"
            //    }
            //);

            //spNoCode.Add(new SPNoCodeForNotif
            //{
            //    No = "E0160",
            //    PCPL_Employee_Code = "BB"
            //}
            //);

            //return Json(spNoCode, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetSalespersonDetails(string FromCode)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPNotification/";

            apiUrl = apiUrl + "GetSalespersonDetails?FromCode=" + FromCode;

            HttpClient client = new HttpClient();
            SPDetailsForNotif spDetails = new SPDetailsForNotif();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                spDetails = Newtonsoft.Json.JsonConvert.DeserializeObject<SPDetailsForNotif>(data);
            }

            return Json(spDetails, JsonRequestBehavior.AllowGet);
        }
        public ActionResult NotificationSetupList()
        {
            return View();
        }
        public async Task<JsonResult> GetNotificationSetupListData(int orderBy, string orderDir, string filter, int skip, int top)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPNotification/";

            string orderByField = "";

            switch (orderBy)
            {
                case 2:
                    orderByField = "Type " + orderDir;
                    break;
                case 3:
                    orderByField = "From_Code " + orderDir;
                    break;
                case 4:
                    orderByField = "To_E_mail " + orderDir;
                    break;
                case 5:
                    orderByField = "CC_E_mail " + orderDir;
                    break;
                case 6:
                    orderByField = "BCC_E_mail " + orderDir;
                    break;
                default:
                    orderByField = "Type asc";
                    break;
            }

            apiUrl = apiUrl + "GetAllNotificationSetups?skip=" + skip + "&top=" + top + "&orderby=" + orderByField + "&filter=" + filter;

            HttpClient client = new HttpClient();
            List<SPNotification> notifications = new List<SPNotification>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                notifications = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPNotification>>(data);
            }

            return Json(notifications, JsonRequestBehavior.AllowGet);
        }

        public async Task<SPNotification> GetNotificationForEdit(string Type, string Employee_No)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPNotification/";

            apiUrl = apiUrl + "GetNotificationFromTypeAndEmpNo?Type=" + Type + "&Employee_No=" + Employee_No;

            HttpClient client = new HttpClient();
            SPNotification notification = new SPNotification();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            ViewBag.Type = "";
            ViewBag.FromCode = "";
            ViewBag.Employee_No = "";
            if (response.IsSuccessStatusCode)
            {

                var data = await response.Content.ReadAsStringAsync();
                notification = Newtonsoft.Json.JsonConvert.DeserializeObject<SPNotification>(data);

                ViewBag.Type = notification.Type;
                ViewBag.FromCode = notification.From_Code;
                ViewBag.Employee_No = notification.Employee_No;
                
            }

            return notification;
        }

        public async Task<JsonResult> ExportListData(int orderBy, string orderDir, string filter, int skip, int top)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPNotification/";

            string orderByField = "";

            switch (orderBy)
            {
                case 2:
                    orderByField = "Type " + orderDir;
                    break;
                case 3:
                    orderByField = "From_Code " + orderDir;
                    break;
                case 4:
                    orderByField = "To_E_mail " + orderDir;
                    break;
                case 5:
                    orderByField = "CC_E_mail " + orderDir;
                    break;
                case 6:
                    orderByField = "BCC_E_mail " + orderDir;
                    break;
                default:
                    orderByField = "Type asc";
                    break;
            }

            apiUrl = apiUrl + "GetAllNotificationSetups?skip=" + skip + "&top=" + top + "&orderby=" + orderByField + "&filter=" + filter + "&isExport=true";

            HttpClient client = new HttpClient();
            List<SPNotification> notifications = new List<SPNotification>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                notifications = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPNotification>>(data);
            }

            DataTable dt = ToDataTable(notifications);
 
            string fileName = "NotificationSetupList.xlsx";
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