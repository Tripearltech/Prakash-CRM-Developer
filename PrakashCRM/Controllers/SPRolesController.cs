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
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace PrakashCRM.Controllers
{
    [RedirectingAction]
    public class SPRolesController : Controller
    {
        private class EmployeeSearchItem
        {
            public string No { get; set; }
            public string EmployeeCode { get; set; }
            public string FullName { get; set; }
        }

        private class EmployeeSearchResponse
        {
            public List<EmployeeSearchItem> items { get; set; }
            public bool hasMore { get; set; }
        }

        private class RoleSearchItem
        {
            public string No { get; set; }
            public string Role_Name { get; set; }
        }

        private class RoleSearchResponse
        {
            public List<RoleSearchItem> items { get; set; }
            public bool hasMore { get; set; }
        }

        // GET: SPRoles
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult RoleList()
        {
            return View();
        }
        public ActionResult UserRoleRelationList()
        {
            return View();
        }

        [HttpGet]
        public ActionResult UserRolesRelationalCard(int? id = null)
        {
            return UserRoleRelationCard(id);
        }

        [HttpGet]
        public ActionResult UserRoleRelationCard(int? id = null)
        {
            SPUserRoleRelationList model = new SPUserRoleRelationList();

            Session["UserRoleRelationAction"] = "";
            Session["isUserRoleRelationEdit"] = false;
            Session["UserRoleRelationId"] = "";

            if (id.HasValue)
            {
                Session["isUserRoleRelationEdit"] = true;
                Session["UserRoleRelationId"] = id.Value.ToString();

                Task<SPUserRoleRelationList> task = Task.Run(async () => await GetUserRoleRelationForEdit(id.Value));
                model = task.Result ?? new SPUserRoleRelationList();
            }
            else
            {
                try
                {
                    Task<int> taskNext = Task.Run(async () => await GetNextUserRoleRelationId());
                    int nextId = taskNext.Result;
                    if (nextId > 0)
                        model.User_Relation_Role_ID = nextId;
                }
                catch
                {
                    // If anything fails, leave default; user can still save.
                }
            }

            // View file name differs from action name; render explicitly.
            return View("UserRolesRelationalCard", model);
        }

        private async Task<int> GetNextUserRoleRelationId()
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPRoles/GetUserRoleRelationList";
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(apiUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await client.GetAsync(apiUrl);
                if (!response.IsSuccessStatusCode)
                    return 0;

                var data = await response.Content.ReadAsStringAsync();
                var list = JsonConvert.DeserializeObject<List<SPUserRoleRelationList>>(data) ?? new List<SPUserRoleRelationList>();
                int maxId = 0;
                for (int i = 0; i < list.Count; i++)
                {
                    var it = list[i];
                    if (it == null) continue;
                    if (it.User_Relation_Role_ID > maxId) maxId = it.User_Relation_Role_ID;
                }
                return maxId + 1;
            }
        }

        public async Task<SPUserRoleRelationList> GetUserRoleRelationForEdit(int id)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPRoles/";
            apiUrl = apiUrl + "GetUserRoleRelationFromId?id=" + id;

            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(apiUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await client.GetAsync(apiUrl);
                if (!response.IsSuccessStatusCode)
                    return null;

                var data = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<SPUserRoleRelationList>(data);
            }
        }

        [HttpPost]
        public async Task<ActionResult> UserRoleRelationCard(SPUserRoleRelationList model)
        {
            if (model != null)
            {
                await TryHydrateUserRoleRelationNames(model);

                // Clear any previous validation for hydrated fields and validate again.
                ModelState.Remove(nameof(SPUserRoleRelationList.User_Name));
                ModelState.Remove(nameof(SPUserRoleRelationList.Full_Name));
                ModelState.Remove(nameof(SPUserRoleRelationList.Role_Name));
                TryValidateModel(model);
            }

            if (!ModelState.IsValid)
                return View("UserRolesRelationalCard", model);

            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPRoles/";

            bool isEdit = false;
            try
            {
                isEdit = Convert.ToBoolean(Session["isUserRoleRelationEdit"]);
            }
            catch
            {
                isEdit = false;
            }

            string id = Session["UserRoleRelationId"]?.ToString() ?? "";

            if (isEdit)
            {
                int sessionId;
                if (int.TryParse(id, out sessionId) && sessionId > 0)
                {
                    if (model != null && model.User_Relation_Role_ID > 0 && model.User_Relation_Role_ID != sessionId)
                    {
                        isEdit = false;
                        id = "";
                        Session["isUserRoleRelationEdit"] = false;
                        Session["UserRoleRelationId"] = "";
                    }
                }
                else
                {
                    // If session id is invalid, treat as create.
                    isEdit = false;
                    id = "";
                    Session["isUserRoleRelationEdit"] = false;
                    Session["UserRoleRelationId"] = "";
                }
            }

            apiUrl = apiUrl + "UserRoleRelation?isEdit=" + (isEdit ? "true" : "false") + "&id=" + id;

            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(apiUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                string objString = JsonConvert.SerializeObject(model);
                var content = new StringContent(objString, Encoding.UTF8, "application/json");

                HttpRequestMessage request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(apiUrl),
                    Content = content
                };

                HttpResponseMessage response = await client.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    ModelState.AddModelError("", "Save failed.");
                    return View("UserRolesRelationalCard", model);
                }
            }

            Session["UserRoleRelationAction"] = isEdit ? "Updated" : "Added";
            return RedirectToAction("UserRoleRelationCard");
        }

        private async Task TryHydrateUserRoleRelationNames(SPUserRoleRelationList model)
        {
            if (model == null)
                return;

            string apiBase = (ConfigurationManager.AppSettings["ServiceApiUrl"]?.ToString() ?? "").Trim();
            if (string.IsNullOrWhiteSpace(apiBase))
                return;
            if (!apiBase.EndsWith("/"))
                apiBase += "/";

            // Employee
            if ((!string.IsNullOrWhiteSpace(model.User_Security_ID)) &&
                (string.IsNullOrWhiteSpace(model.User_Name) || string.IsNullOrWhiteSpace(model.Full_Name)))
            {
                try
                {
                    var url = apiBase + "SPRoles/SearchEmployees?term=" + HttpUtility.UrlEncode(model.User_Security_ID) + "&skip=0&top=20";
                    using (var client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        var resp = await client.GetAsync(url);
                        if (resp.IsSuccessStatusCode)
                        {
                            var json = await resp.Content.ReadAsStringAsync();
                            var serviceResp = JsonConvert.DeserializeObject<EmployeeSearchResponse>(json);
                            var items = serviceResp?.items ?? new List<EmployeeSearchItem>();

                            string term = (model.User_Security_ID ?? "").Trim();
                            var match = items.FirstOrDefault(x =>
                                x != null &&
                                (!string.IsNullOrWhiteSpace(x.EmployeeCode) && x.EmployeeCode.Equals(term, StringComparison.OrdinalIgnoreCase)));
                            if (match == null)
                            {
                                match = items.FirstOrDefault(x =>
                                    x != null &&
                                    (!string.IsNullOrWhiteSpace(x.No) && x.No.Equals(term, StringComparison.OrdinalIgnoreCase)));
                            }

                            if (match != null)
                            {
                                if (string.IsNullOrWhiteSpace(model.User_Name))
                                    model.User_Name = string.IsNullOrWhiteSpace(match.EmployeeCode) ? match.No : match.EmployeeCode;
                                if (string.IsNullOrWhiteSpace(model.Full_Name))
                                    model.Full_Name = match.FullName ?? "";
                            }
                        }
                    }
                }
                catch
                {
                    // Best-effort hydration; fall back to validation errors if lookup fails.
                }
            }

            // Role
            if ((!string.IsNullOrWhiteSpace(model.Role_ID)) && string.IsNullOrWhiteSpace(model.Role_Name))
            {
                try
                {
                    var url = apiBase + "SPRoles/SearchRoles?term=" + HttpUtility.UrlEncode(model.Role_ID) + "&skip=0&top=20";
                    using (var client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        var resp = await client.GetAsync(url);
                        if (resp.IsSuccessStatusCode)
                        {
                            var json = await resp.Content.ReadAsStringAsync();
                            var serviceResp = JsonConvert.DeserializeObject<RoleSearchResponse>(json);
                            var items = serviceResp?.items ?? new List<RoleSearchItem>();

                            string term = (model.Role_ID ?? "").Trim();
                            var match = items.FirstOrDefault(x =>
                                x != null &&
                                (!string.IsNullOrWhiteSpace(x.No) && x.No.Equals(term, StringComparison.OrdinalIgnoreCase)));

                            if (match != null)
                                model.Role_Name = match.Role_Name ?? "";
                        }
                    }
                }
                catch
                {
                    // Best-effort hydration.
                }
            }
        }

        [HttpGet]
        public async Task<JsonResult> SearchEmployees(string term = "", int page = 1, int pageSize = 20)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 20;
            if (pageSize > 50) pageSize = 50;

            int skip = (page - 1) * pageSize;

            string apiBase = (ConfigurationManager.AppSettings["ServiceApiUrl"]?.ToString() ?? "").Trim();
            if (!string.IsNullOrWhiteSpace(apiBase) && !apiBase.EndsWith("/"))
                apiBase += "/";

            var url = apiBase + "SPRoles/SearchEmployees?term=" + HttpUtility.UrlEncode(term ?? "") + "&skip=" + skip + "&top=" + pageSize;

            EmployeeSearchResponse serviceResp = null;

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var resp = await client.GetAsync(url);
                if (resp.IsSuccessStatusCode)
                {
                    var json = await resp.Content.ReadAsStringAsync();
                    serviceResp = JsonConvert.DeserializeObject<EmployeeSearchResponse>(json);
                }
            }

            var items = serviceResp?.items ?? new List<EmployeeSearchItem>();
            var results = items
                .Where(x => x != null && !string.IsNullOrWhiteSpace(x.No))
                .Select(x => new
                {
                    // Use EmployeeCode as the primary id/text because UI expects EMP No like E0019.
                    id = string.IsNullOrWhiteSpace(x.EmployeeCode) ? x.No : x.EmployeeCode,
                    text = string.IsNullOrWhiteSpace(x.EmployeeCode) ? x.No : x.EmployeeCode,
                    no = x.No,
                    userName = string.IsNullOrWhiteSpace(x.EmployeeCode) ? x.No : x.EmployeeCode,
                    fullName = x.FullName ?? ""
                })
                .ToList();

            return Json(
                new
                {
                    results,
                    pagination = new { more = serviceResp?.hasMore ?? false }
                },
                JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<JsonResult> SearchRoles(string term = "", int page = 1, int pageSize = 20)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 20;
            if (pageSize > 50) pageSize = 50;

            int skip = (page - 1) * pageSize;

            string apiBase = (ConfigurationManager.AppSettings["ServiceApiUrl"]?.ToString() ?? "").Trim();
            if (!string.IsNullOrWhiteSpace(apiBase) && !apiBase.EndsWith("/"))
                apiBase += "/";

            var url = apiBase + "SPRoles/SearchRoles?term=" + HttpUtility.UrlEncode(term ?? "") + "&skip=" + skip + "&top=" + pageSize;

            RoleSearchResponse serviceResp = null;

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var resp = await client.GetAsync(url);
                if (resp.IsSuccessStatusCode)
                {
                    var json = await resp.Content.ReadAsStringAsync();
                    serviceResp = JsonConvert.DeserializeObject<RoleSearchResponse>(json);
                }
            }

            var items = serviceResp?.items ?? new List<RoleSearchItem>();
            var results = items
                .Where(x => x != null && !string.IsNullOrWhiteSpace(x.No))
                .Select(x => new
                {
                    id = x.No,
                    text = x.No,
                    roleName = x.Role_Name ?? ""
                })
                .ToList();

            return Json(
                new
                {
                    results,
                    pagination = new { more = serviceResp?.hasMore ?? false }
                },
                JsonRequestBehavior.AllowGet);
        }

        // Compatibility route: some menus link to /SPRoles/RoleRights
        // Actual page lives under SPRoleRightsController.RoleRights
        public ActionResult RoleRights()
        {
            return RedirectToAction("RoleRights", "SPRoleRights");
        }

        public async Task<JsonResult> GetRoleListData(int orderBy, string orderDir, string filter, int skip, int top)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPRoles/";

            string orderByField = "";

            switch (orderBy)
            {
                case 2:
                    orderByField = "No " + orderDir;
                    break;
                case 3:
                    orderByField = "Role_Name " + orderDir;
                    break;
                case 4:
                    orderByField = "IsActive " + orderDir;
                    break;
                default:
                    orderByField = "No asc";
                    break;
            }

            apiUrl = apiUrl + "GetAllRoles?skip=" + skip + "&top=" + top + "&orderby=" + orderByField + "&filter=" + filter;

            HttpClient client = new HttpClient();
            List<SPRoleList> roles = new List<SPRoleList>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                roles = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPRoleList>>(data);
            }

            Session["RoleNo"] = "";
            Session["isRoleEdit"] = false;
            Session["RoleAction"] = "";

            return Json(roles, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> ExportListData(int orderBy, string orderDir, string filter, int skip, int top)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPRoles/";

            string orderByField = "";

            switch (orderBy)
            {
                case 2:
                    orderByField = "No " + orderDir;
                    break;
                case 3:
                    orderByField = "Role_Name " + orderDir;
                    break;
                case 4:
                    orderByField = "IsActive " + orderDir;
                    break;
                default:
                    orderByField = "No asc";
                    break;
            }

            apiUrl = apiUrl + "GetAllRoles?skip=" + skip + "&top=" + top + "&orderby=" + orderByField + "&filter=" + filter + "&isExport=true";

            HttpClient client = new HttpClient();
            List<SPRoleList> roles = new List<SPRoleList>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                roles = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPRoleList>>(data);
            }
            DataTable dt = ToDataTable(roles);

            //Name of File  
            string fileName = "SPRoleList.xlsx";
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

        public ActionResult Role(string No = "")
        {
            SPRoles role = new SPRoles();

            if (No != "" || (Session["RoleNo"] != null && Session["RoleNo"].ToString() != ""))
            {
                if (Session["RoleNo"] == null || Session["RoleNo"].ToString() == "")
                    Session["RoleNo"] = No;

                Task<SPRoles> task = Task.Run<SPRoles>(async () => await GetRoleForEdit(Session["RoleNo"].ToString()));
                role = task.Result;

                Session["isRoleEdit"] = true;
            }
            else
            {
                // New role screen: clear any previous edit context.
                Session["RoleNo"] = "";
                Session["isRoleEdit"] = false;

                // New role: default Active so it appears in the default Role List filter.
                role.IsActive = true;
            }

            if (role != null)
                return View(role);
            else
                return View(new SPRoles());

        }

        public async Task<SPRoles> GetRoleForEdit(string No)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPRoles/";

            apiUrl = apiUrl + "GetRoleFromNo?No=" + No;

            HttpClient client = new HttpClient();
            SPRoles role = new SPRoles();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                role = Newtonsoft.Json.JsonConvert.DeserializeObject<SPRoles>(data);
            }

            return role;
        }

        [HttpPost]
        public async Task<ActionResult> Role(SPRoles role)
        {
            if (!ModelState.IsValid)
                return View(role);


            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPRoles/";

            string RoleNo = "";
            if (Convert.ToBoolean(Session["isRoleEdit"]) == true)
            {
                RoleNo = Session["RoleNo"].ToString();
                apiUrl = apiUrl + "Role?isEdit=true&RoleNo=" + RoleNo;
            }
            else
                apiUrl = apiUrl + "Role?isEdit=false&RoleNo=" + RoleNo;

            SPRolesResponse responseRole = new SPRolesResponse();

            HttpClient client = new HttpClient();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            string UserObjString = JsonConvert.SerializeObject(role);
            var content = new StringContent(UserObjString, Encoding.UTF8, "application/json");

            HttpRequestMessage request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(apiUrl),
                Content = content
            };

            HttpResponseMessage response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError("", "Save failed. " + (string.IsNullOrWhiteSpace(errorBody) ? "" : errorBody));
                return View(role);
            }

            var data = await response.Content.ReadAsStringAsync();
            responseRole = Newtonsoft.Json.JsonConvert.DeserializeObject<SPRolesResponse>(data);

            Session["RoleAction"] = Convert.ToBoolean(Session["isRoleEdit"]) ? "Updated" : "Added";
            return RedirectToAction("Role");
        }

        public bool NullRoleSession()
        {
            bool isSessionNull = false;

            Session["RoleAction"] = "";
            isSessionNull = true;

            return isSessionNull;
        }

        public async Task<JsonResult> GetUserRoleRelationList()
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPRoles/GetUserRoleRelationList";

            HttpClient client = new HttpClient();
            List<SPUserRoleRelationList> userrolerelationlist = new List<SPUserRoleRelationList>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                userrolerelationlist = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPUserRoleRelationList>>(data);
                //reportingperson = reportingperson.OrderBy(a => a.First_Name).ToList();
            }

            return Json(userrolerelationlist, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<JsonResult> GetMaxUserRoleRelationId()
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPRoles/GetUserRoleRelationList";
            HttpClient client = new HttpClient();
            List<SPUserRoleRelationList> userrolerelationlist = new List<SPUserRoleRelationList>();
            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                userrolerelationlist = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPUserRoleRelationList>>(data);
            }
            int maxId = 0;
            if (userrolerelationlist != null && userrolerelationlist.Count > 0)
            {
                maxId = userrolerelationlist.Max(x => x.User_Relation_Role_ID);
            }
            return Json(new { maxId = maxId }, JsonRequestBehavior.AllowGet);
        }
    }

}