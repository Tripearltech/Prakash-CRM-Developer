using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using ClosedXML.Excel;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using PrakashCRM.Data.Models;

namespace PrakashCRM.Controllers
{
    [RedirectingAction]
    public class SPMenusController : Controller
    {
        private const string FixedClassName = "bx bx-right-arrow-alt";
        // GET: SPMenus
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult MenuList()
        {
            return View();
        }

        public async Task<JsonResult> GetMenuListData(int orderBy, string orderDir, string filter, int skip, int top)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPMenus/";

            string orderByField = "";

            switch (orderBy)
            {
                case 2:
                    orderByField = "No " + orderDir;
                    break;
                case 3:
                    orderByField = "Menu_Name " + orderDir;
                    break;
                case 4:
                    orderByField = "Parent_Menu_No " + orderDir;
                    break;
                case 5:
                    orderByField = "Serial_No " + orderDir;
                    break;
                case 6:
                    orderByField = "Type " + orderDir;
                    break;
                case 7:
                    orderByField = "ClassName " + orderDir;
                    break;
                case 8:
                    orderByField = "IsActive " + orderDir;
                    break;
                default:
                    orderByField = "No desc";
                    break;
            }

            apiUrl = apiUrl + "GetAllMenus?skip=" + skip + "&top=" + top + "&orderby=" + orderByField + "&filter=" + filter;

            HttpClient client = new HttpClient();
            List<SPMenuList> menus = new List<SPMenuList>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                menus = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPMenuList>>(data);
            }

            Session["MenuNo"] = "";
            Session["isMenuEdit"] = false;
            ViewBag.isMenuEdit = false;
            Session["MenuAction"] = "";
            Session["MenuActionMsg"] = "";

            return Json(menus, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetSubMenuListData(int orderBy, string orderDir, string filter, int skip, int top)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPMenus/";

            string orderByField = "";

            switch (orderBy)
            {
                case 2:
                    orderByField = "No " + orderDir;
                    break;
                case 3:
                    orderByField = "Menu_Name " + orderDir;
                    break;
                case 4:
                    orderByField = "Parent_Menu_No " + orderDir;
                    break;
                case 5:
                    orderByField = "Serial_No " + orderDir;
                    break;
                case 6:
                    orderByField = "Type " + orderDir;
                    break;
                case 7:
                    orderByField = "ClassName " + orderDir;
                    break;
                case 8:
                    orderByField = "IsActive " + orderDir;
                    break;
                default:
                    orderByField = "No asc";
                    break;
            }

            apiUrl = apiUrl + "GetAllMenus?skip=" + skip + "&top=" + top + "&orderby=" + orderByField + "&filter=" + filter;

            HttpClient client = new HttpClient();
            List<SPMenuList> submenus = new List<SPMenuList>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                submenus = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPMenuList>>(data);
            }

            return Json(submenus, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> ExportListData(int orderBy, string orderDir, string filter, int skip, int top)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPMenus/";

            string orderByField = "";

            switch (orderBy)
            {
                case 2:
                    orderByField = "No " + orderDir;
                    break;
                case 3:
                    orderByField = "Menu_Name " + orderDir;
                    break;
                case 4:
                    orderByField = "Parent_Menu_No " + orderDir;
                    break;
                case 5:
                    orderByField = "Serial_No " + orderDir;
                    break;
                case 6:
                    orderByField = "Type " + orderDir;
                    break;
                case 7:
                    orderByField = "ClassName " + orderDir;
                    break;
                case 8:
                    orderByField = "IsActive " + orderDir;
                    break;
                default:
                    orderByField = "No asc";
                    break;
            }

            apiUrl = apiUrl + "GetAllMenus?skip=" + skip + "&top=" + top + "&orderby=" + orderByField + "&filter=" + filter + "&isExport=true";

            HttpClient client = new HttpClient();
            List<SPMenuList> menus = new List<SPMenuList>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                menus = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPMenuList>>(data);
            }
            DataTable dt = ToDataTable(menus);

            string fileName = "SPMenuList.xlsx";
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

        public async Task<ActionResult> Menu(string No = "")
        {
            SPMenus menu = new SPMenus();

            ViewBag.MenuType = "";
            ViewBag.ParentMenuNo = "";
            var sessionMenuNo = Convert.ToString(Session["MenuNo"]);

            // Decide edit mode safely
            if (!string.IsNullOrWhiteSpace(No))
            {
                sessionMenuNo = No;
                Session["MenuNo"] = No;
                Session["isMenuEdit"] = true;
            }
            else if (!string.IsNullOrWhiteSpace(sessionMenuNo))
            {
                Session["isMenuEdit"] = true;
            }
            else
            {
                Session["isMenuEdit"] = false;
            }

            if (Convert.ToBoolean(Session["isMenuEdit"]))
            {
                menu = await GetMenuForEdit(sessionMenuNo);
                ViewBag.MenuType = menu?.Type;
                ViewBag.ParentMenuNo = menu?.Parent_Menu_No;
            }

            if (menu == null)
                menu = new SPMenus();

            // Default for new menu
            if (string.IsNullOrWhiteSpace(No) && (Session["MenuNo"] == null || string.IsNullOrWhiteSpace(Convert.ToString(Session["MenuNo"]))))
            {
                menu.IsActive = true;
                menu.ClassName = FixedClassName;
            }

            return View(menu);

        }

        public async Task<SPMenus> GetMenuForEdit(string No)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPMenus/";

            apiUrl = apiUrl + "GetMenuFromNo?No=" + No;

            HttpClient client = new HttpClient();
            SPMenus menu = new SPMenus();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                menu = Newtonsoft.Json.JsonConvert.DeserializeObject<SPMenus>(data);
            }

            return menu;
        }

        [HttpPost]
        public async Task<ActionResult> Menu(SPMenus menu, string MenuNo, bool? isEdit)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPMenus/";
            menu.ClassName = FixedClassName;

            // Prefer posted values over Session (Session may be cleared / expired)
            var sessionIsEdit = Session["isMenuEdit"] != null && Convert.ToBoolean(Session["isMenuEdit"]);
            var effectiveIsEdit = (isEdit ?? false) || sessionIsEdit;

            var effectiveMenuNo = !string.IsNullOrWhiteSpace(MenuNo)
                ? MenuNo
                : Convert.ToString(Session["MenuNo"]);

            // If edit is requested but MenuNo is missing, fail fast with a clear message.
            if (effectiveIsEdit && string.IsNullOrWhiteSpace(effectiveMenuNo))
            {
                Session["MenuAction"] = "Failed";
                Session["MenuActionMsg"] = "MenuNo is missing; please reopen the Menu and try again.";
                return View(menu);
            }

            apiUrl = apiUrl + "Menu?isEdit=" + (effectiveIsEdit ? "true" : "false") + "&MenuNo=" + (effectiveMenuNo ?? "");

            SPMenusResponse responseMenu = new SPMenusResponse();

            HttpClient client = new HttpClient();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            string UserObjString = JsonConvert.SerializeObject(menu);
            var content = new StringContent(UserObjString, Encoding.UTF8, "application/json");

            HttpRequestMessage request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(apiUrl),
                Content = content
            };

            HttpResponseMessage response = await client.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                try
                {
                    responseMenu = Newtonsoft.Json.JsonConvert.DeserializeObject<SPMenusResponse>(responseBody);
                }
                catch
                {
                    // ignore parse errors
                }

                if (effectiveIsEdit)
                    Session["MenuAction"] = "Updated";
                else
                    Session["MenuAction"] = "Added";

                // Keep Session in sync
                Session["MenuNo"] = effectiveMenuNo;
                Session["isMenuEdit"] = effectiveIsEdit;

                Session["MenuActionMsg"] = "";
                return RedirectToAction("Menu");
            }

            // Failure: do NOT show success toast
            Session["MenuAction"] = "Failed";
            Session["MenuActionMsg"] = string.IsNullOrWhiteSpace(responseBody) ? response.ReasonPhrase : responseBody;
            return View(menu);

        }

        public bool NullMenuSession()
        {
            bool isSessionNull = false;

            Session["MenuAction"] = "";
            Session["MenuActionMsg"] = "";
            Session["MenuNo"] = "";
            Session["isMenuEdit"] = false;
            isSessionNull = true;

            return isSessionNull;
        }

        public async Task<JsonResult> GetAllParentMenuNoForDDL()
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPMenus/GetAllParentMenuNoForDDL";

            HttpClient client = new HttpClient();
            List<SPParentMenuNo> parentmenuno = new List<SPParentMenuNo>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                parentmenuno = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPParentMenuNo>>(data);
            }

            //List<SPParentMenuNo> parentmenuno1 = parentmenuno.DistinctBy(a => a.Parent_Menu_No).ToList();

            return Json(parentmenuno, JsonRequestBehavior.AllowGet);
        }
    }
}
