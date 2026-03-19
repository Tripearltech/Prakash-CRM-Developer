using PrakashCRM.Data.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using DocumentFormat.OpenXml.Drawing.Charts;
using Newtonsoft.Json;
using System.Text;
using PrakashCRM.Security;

namespace PrakashCRM.Controllers
{
    [RedirectingAction]
    public class SPRoleRightsController : Controller
    {
        // GET: SPRoleRights
        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> RoleRights()
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPMenus/";
            string allMenusApiUrl = apiUrl + "GetAllMenus?skip=0&top=0&orderby=No asc&filter=Type eq 'Navigation'";

            List<SPMenuList> allMenus = new List<SPMenuList>();
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(allMenusApiUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await client.GetAsync(allMenusApiUrl);
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    allMenus = JsonConvert.DeserializeObject<List<SPMenuList>>(data) ?? new List<SPMenuList>();
                }
            }

            var menuLookupByParent = allMenus
                .Where(x => x != null && !string.IsNullOrWhiteSpace(x.Parent_Menu_No))
                .GroupBy(x => x.Parent_Menu_No)
                .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

            var menus = allMenus
                .Where(x =>
                    x != null &&
                    string.Equals((x.Type ?? "").Trim(), "Navigation", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals((x.ClassName ?? "").Trim(), "bx bx-right-arrow-alt", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals((x.ClassName ?? "").Trim(), "has-arrow", StringComparison.OrdinalIgnoreCase))
                .Select(x => new SPMenuList
                {
                    No = x.No,
                    Menu_Name = x.Menu_Name,
                    Parent_Menu_No = x.Parent_Menu_No,
                    Parent_Menu_Name = x.Parent_Menu_Name,
                    Serial_No = x.Serial_No,
                    Type = x.Type,
                    Path = x.Path,
                    ClassName = x.ClassName,
                    IsActive = x.IsActive
                })
                .ToList();

            for (int a = 0; a < menus.Count; a++)
            {
                var root = menus[a];
                if (root == null || string.IsNullOrWhiteSpace(root.No))
                    continue;

                List<SPMenuList> subMenuCandidates;
                if (!menuLookupByParent.TryGetValue(root.No, out subMenuCandidates))
                    subMenuCandidates = new List<SPMenuList>();

                var submenus = subMenuCandidates
                    .Where(x =>
                        x != null &&
                        !string.Equals((x.No ?? "").Trim(), (root.No ?? "").Trim(), StringComparison.OrdinalIgnoreCase) &&
                        string.Equals((x.Type ?? "").Trim(), "Navigation", StringComparison.OrdinalIgnoreCase))
                    .Select(x => new SPSubMenuList
                    {
                        No = x.No,
                        Menu_Name = x.Menu_Name
                    })
                    .ToList();

                foreach (var submenu in submenus)
                    root.subMenuList.Add(submenu);

                var sml = root.subMenuList.ToList();
                for (int c = 0; c < sml.Count; c++)
                {
                    var sub = sml[c];
                    if (sub == null || string.IsNullOrWhiteSpace(sub.No))
                        continue;

                    List<SPMenuList> subSubCandidates;
                    if (!menuLookupByParent.TryGetValue(sub.No, out subSubCandidates))
                        subSubCandidates = new List<SPMenuList>();

                    var subsubmenus = subSubCandidates
                        .Where(x =>
                            x != null &&
                            string.Equals((x.Type ?? "").Trim(), "Navigation", StringComparison.OrdinalIgnoreCase) &&
                            string.Equals((x.ClassName ?? "").Trim(), "bx bx-right-arrow-alt", StringComparison.OrdinalIgnoreCase))
                        .Select(x => new SPSubSubMenuList
                        {
                            No = x.No,
                            Menu_Name = x.Menu_Name
                        })
                        .ToList();

                    root.subSubListCnt = subsubmenus.Count;
                    foreach (var subsub in subsubmenus)
                        sub.subSubMenuList.Add(subsub);
                }
            }

            return View(menus);
        }

        public async Task<List<SPSubMenuList>> GetSubMenusForMenu(string menuNo)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPMenus/";

            apiUrl = apiUrl + "GetAllMenus?skip=0&top=0&orderby=No asc&filter=Parent_Menu_No eq \'" + menuNo + "\' and Type eq \'Navigation\' and No ne \'" + menuNo + "\'";

            HttpClient client = new HttpClient();
            List<SPSubMenuList> submenus = new List<SPSubMenuList>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                submenus = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPSubMenuList>>(data);
            }

            return submenus;
        }

        public async Task<List<SPSubSubMenuList>> GetSubSubMenusForSubMenu(string subMenuNo)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPMenus/";

            apiUrl = apiUrl + "GetAllMenus?skip=0&top=0&orderby=No asc&filter=Parent_Menu_No eq \'" + subMenuNo + "\' and Type eq \'Navigation\' and ClassName eq \'bx bx-right-arrow-alt\'";

            HttpClient client = new HttpClient();
            List<SPSubSubMenuList> subsubmenus = new List<SPSubSubMenuList>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                subsubmenus = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPSubSubMenuList>>(data);
            }

            return subsubmenus;
        }

        public async Task<JsonResult> GetAllRolesForDDL()
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPRoleRights/GetAllRolesForDDL";

            HttpClient client = new HttpClient();
            List<SPRolesForDDL> roles = new List<SPRolesForDDL>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                roles = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPRolesForDDL>>(data);
            }

            return Json(roles, JsonRequestBehavior.AllowGet);
        }

        public async Task<string> GetAllMenusSubMenusOfRole(string RoleNo)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPRoleRights/";

            HttpClient client = new HttpClient();
            List<SPMenusSubMenusOfRole> menussubmenus = new List<SPMenusSubMenusOfRole>();

            apiUrl = apiUrl + "GetAllMenusSubMenusOfRole?RoleNo=" + Uri.EscapeDataString(RoleNo ?? "");

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                menussubmenus = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPMenusSubMenusOfRole>>(data);
            }

            if (menussubmenus == null || menussubmenus.Count == 0)
                return "";

            var strMenusSubMenus = new StringBuilder();

            for (int i = 0; i < menussubmenus.Count; i++)
            {
                var menuNo = menussubmenus[i].Menu_No ?? "";
                var subNo = menussubmenus[i].Sub_Menu_No ?? "";
                if (string.IsNullOrWhiteSpace(menuNo))
                    continue;

                var token = string.IsNullOrWhiteSpace(subNo)
                    ? (menuNo + "-" + menuNo)
                    : (menuNo + "-" + subNo);

                // Per-menu rights encoding:
                // token:F (Full rights) OR token:AEVD (letters for Add/Edit/View/Delete)
                var rights = new StringBuilder();
                if (menussubmenus[i].Full_Rights)
                {
                    rights.Append('F');
                }
                else
                {
                    if (menussubmenus[i].Add_Rights) rights.Append('A');
                    if (menussubmenus[i].Edit_Rights) rights.Append('E');
                    if (menussubmenus[i].View_Rights) rights.Append('V');
                    if (menussubmenus[i].Delete_Rights) rights.Append('D');
                }

                strMenusSubMenus.Append(token);
                if (rights.Length > 0)
                {
                    strMenusSubMenus.Append(":");
                    strMenusSubMenus.Append(rights);
                }
                strMenusSubMenus.Append(",");
            }

            // remove trailing comma safely
            if (strMenusSubMenus.Length > 0 && strMenusSubMenus[strMenusSubMenus.Length - 1] == ',')
                strMenusSubMenus.Length -= 1;

            return strMenusSubMenus.ToString();
        }

        [HttpPost]
        public async Task<ActionResult> SaveRoleRights(string RoleNo, string PrevSavedMenusRights, string MenusWithRights)
        {
            // Proxy Save to Service API to avoid browser CORS/certificate/network issues.
            // Returns: { success: bool, message: string }
            try
            {
                string baseApiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString();
                string url = baseApiUrl + "SPRoleRights/RoleRights";

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var form = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("RoleNo", RoleNo ?? ""),
                        new KeyValuePair<string, string>("PrevSavedMenusRights", PrevSavedMenusRights ?? ""),
                        new KeyValuePair<string, string>("MenusWithRights", MenusWithRights ?? "")
                    });

                    HttpResponseMessage response = await client.PostAsync(url, form);

                    string body = await response.Content.ReadAsStringAsync();
                    body = (body ?? "").Trim().Trim('"');

                    bool ok = string.Equals(body, "true", StringComparison.OrdinalIgnoreCase) || body == "1";

                    string errHeader = null;
                    try
                    {
                        if (response.Headers.Contains("X-RoleRights-Error"))
                            errHeader = response.Headers.GetValues("X-RoleRights-Error").FirstOrDefault();
                    }
                    catch { }

                    if (ok)
                    {
                        await InvalidateSessionsForRole(RoleNo);
                        return Json(new { success = true, message = "" });
                    }

                    var msg = !string.IsNullOrWhiteSpace(errHeader)
                        ? errHeader
                        : (response.IsSuccessStatusCode ? "Role Rights Save failed" : ("Role Rights Save failed (HTTP " + (int)response.StatusCode + ")"));

                    return Json(new { success = false, message = msg });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Role Rights Save failed (" + ex.Message + ")" });
            }
        }

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public async Task<ActionResult> GetRolewiseMenuRight(string Usersecurityid)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"]
                + "SPRoleRights/GetRolewiseMenuRight?Usersecurityid=" + Usersecurityid;

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await client.GetAsync(apiUrl);
                var data = await response.Content.ReadAsStringAsync();

                Response.StatusCode = (int)response.StatusCode;
                if (string.IsNullOrWhiteSpace(data))
                    data = "{}";

                // return raw JSON from Service API (keeps userId/roles casing)
                return Content(data, "application/json");
            }
        }

        [HttpPost]
        public async Task<ActionResult> RefreshRoleWiseMenuData(string Usersecurityid)
        {
            if (string.IsNullOrWhiteSpace(Usersecurityid))
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(new { success = false, message = "Usersecurityid is required" });
            }

            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"]
                + "SPRoleRights/GetRolewiseMenuRight?Usersecurityid=" + Uri.EscapeDataString(Usersecurityid);

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response;
                try
                {
                    response = await client.GetAsync(apiUrl).ConfigureAwait(false);
                }
                catch
                {
                    Response.StatusCode = (int)HttpStatusCode.BadGateway;
                    return Json(new { success = false, message = "Service API call failed" });
                }

                var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                {
                    Response.StatusCode = (int)response.StatusCode;
                    return Json(new { success = false, message = "Service API returned error", detail = json });
                }

                RoleWiseMenuRightsResponse model;
                try
                {
                    model = JsonConvert.DeserializeObject<RoleWiseMenuRightsResponse>(json);
                }
                catch
                {
                    model = new RoleWiseMenuRightsResponse { UserId = Usersecurityid, Roles = new List<RoleWiseRole>() };
                }

                if (model != null && model.Roles == null)
                    model.Roles = new List<RoleWiseRole>();

                Session["RoleWiseMenuData"] = model;
                return Json(new { success = true });
            }
        }

        private async Task InvalidateSessionsForRole(string roleNo)
        {
            if (string.IsNullOrWhiteSpace(roleNo))
                return;

            var userNos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var serviceApiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString();

            // Primary source: explicit user-role mapping table.
            string roleRelationUrl = serviceApiUrl + "SPRoles/GetUserRoleRelationList";
            using (var relationClient = new HttpClient())
            {
                relationClient.DefaultRequestHeaders.Accept.Clear();
                relationClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var relationResponse = await relationClient.GetAsync(roleRelationUrl);
                if (relationResponse.IsSuccessStatusCode)
                {
                    var relationData = await relationResponse.Content.ReadAsStringAsync();
                    var relations = JsonConvert.DeserializeObject<List<SPUserRoleRelationList>>(relationData) ?? new List<SPUserRoleRelationList>();
                    foreach (var row in relations)
                    {
                        if (row == null) continue;
                        if (!string.Equals((row.Role_ID ?? "").Trim(), roleNo.Trim(), StringComparison.OrdinalIgnoreCase)) continue;
                        if (!string.IsNullOrWhiteSpace(row.User_Security_ID))
                            userNos.Add(row.User_Security_ID.Trim());
                    }
                }
            }

            // Fallback source: users table role fields.
            string safeRole = roleNo.Replace("'", "''");
            string filter = "Role_No eq '" + safeRole + "' or Role eq '" + safeRole + "'";
            string apiUrl = serviceApiUrl
                + "Salesperson/GetAllUsers?skip=0&top=0&orderby=No asc&filter=" + Uri.EscapeDataString(filter);

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var response = await client.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    var users = JsonConvert.DeserializeObject<List<SPProfile>>(data) ?? new List<SPProfile>();
                    foreach (var userNo in users.Select(x => x.No).Where(x => !string.IsNullOrWhiteSpace(x)))
                        userNos.Add(userNo.Trim());
                }
            }

            UserTokenStore.InvalidateAllTokensForUsers(userNos);
        }

    }
}
