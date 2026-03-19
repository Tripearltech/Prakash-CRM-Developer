using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PrakashCRM.Data.Models;
using PrakashCRM.Service.Classes;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Web.Http;

namespace PrakashCRM.Service.Controllers
{
    [RoutePrefix("api/SPRoleRights")]
    public class SPRoleRightsController : ApiController
    {
        [HttpPost]
        [Route("RoleRights")]
        public async Task<HttpResponseMessage> RoleRights()
        {
            // Always return HTTP 200 + boolean body (backward-compatible),
            // and attach a readable error message in a response header when false.
            string lastErrorMessage = null;

            // Parameters may come from query-string (legacy) OR from POST body (preferred).
            string RoleNo = null;
            string PrevSavedMenusRights = null;
            string MenusWithRights = null;

            try
            {
                // 1) Read from form-url-encoded body when present
                try
                {
                    if (Request?.Content != null)
                    {
                        var contentType = Request.Content.Headers?.ContentType?.MediaType;
                        if (!string.IsNullOrWhiteSpace(contentType) && contentType.IndexOf("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            var form = await Request.Content.ReadAsFormDataAsync();
                            if (form != null)
                            {
                                RoleNo = form["RoleNo"];
                                PrevSavedMenusRights = form["PrevSavedMenusRights"];
                                MenusWithRights = form["MenusWithRights"];
                            }
                        }
                    }
                }
                catch
                {
                    // Ignore body parse failures; we'll fall back to query-string.
                }

                // 2) Fall back to query-string
                try
                {
                    var query = Request.GetQueryNameValuePairs()
                        .GroupBy(kv => kv.Key, StringComparer.OrdinalIgnoreCase)
                        .ToDictionary(g => g.Key, g => g.First().Value, StringComparer.OrdinalIgnoreCase);

                    if (string.IsNullOrWhiteSpace(RoleNo) && query.TryGetValue("RoleNo", out var qRole)) RoleNo = qRole;
                    if (PrevSavedMenusRights == null && query.TryGetValue("PrevSavedMenusRights", out var qPrev)) PrevSavedMenusRights = qPrev;
                    if (MenusWithRights == null && query.TryGetValue("MenusWithRights", out var qMenus)) MenusWithRights = qMenus;
                }
                catch
                {
                    // ignore
                }
            }
            catch
            {
                // ignore parameter-read errors
            }

            try
            {
                // -------- SAFETY CHECKS --------
                if (string.IsNullOrWhiteSpace(RoleNo))
                {
                    lastErrorMessage = "RoleNo is required.";
                    var badRoleNo = Request.CreateResponse(HttpStatusCode.OK, false);
                    badRoleNo.Headers.Add("X-RoleRights-Error", lastErrorMessage);
                    return badRoleNo;
                }

                if (PrevSavedMenusRights == null)
                    PrevSavedMenusRights = "";

                if (MenusWithRights == null)
                    MenusWithRights = "";

                // First delete previous rights (if any)
                {
                    SPMenusRightsForDel requestMenusRightsForDel = new SPMenusRightsForDel();
                    SPMenusRightsForDelRes responseMenusRightsForDel = new SPMenusRightsForDelRes();

                    requestMenusRightsForDel.roleid = RoleNo;

                    var result = PostItemForMenusRightsDel("", requestMenusRightsForDel, responseMenusRightsForDel);

                    var delErr = result.Result.Item2;
                    if (delErr != null && !string.IsNullOrWhiteSpace(delErr.message))
                        lastErrorMessage = delErr.message;
                }

                // -------- EMPTY CHECK FOR NEW RIGHTS --------
                if (string.IsNullOrWhiteSpace(MenusWithRights))
                {
                    // If nothing selected → no rights to save → return true
                    return Request.CreateResponse(HttpStatusCode.OK, true);
                }

                // REMOVE last comma (safe)
                if (MenusWithRights.EndsWith(","))
                    MenusWithRights = MenusWithRights.Substring(0, MenusWithRights.Length - 1);

                // Safety again
                if (string.IsNullOrWhiteSpace(MenusWithRights))
                    return Request.CreateResponse(HttpStatusCode.OK, true);

                // SPLIT (supports legacy tokens and per-menu rights)
                // Formats supported:
                // 1) Legacy: "M029-M029,Full_Rights" (global rights apply to all menus)
                // 2) Per-menu: "M029-M031:AV" where rights letters are F/A/E/V/D
                string[] items = MenusWithRights
                    .Split(',')
                    .Select(x => (x ?? "").Trim())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct()
                    .ToArray();

                bool globalFull = items.Contains("Full_Rights");
                bool globalAdd = items.Contains("Add_Rights");
                bool globalEdit = items.Contains("Edit_Rights");
                bool globalView = items.Contains("View_Rights");
                bool globalDelete = items.Contains("Delete_Rights");

                // Parsed menu entries (menuToken without rights, optional rightsSpec)
                var menuEntries = new List<(string MenuToken, string RightsSpec)>();
                foreach (var it in items)
                {
                    if (it == "Full_Rights" || it == "Add_Rights" || it == "Edit_Rights" || it == "View_Rights" || it == "Delete_Rights")
                        continue;

                    var menuToken = it;
                    string rightsSpec = null;

                    int colon = it.IndexOf(':');
                    if (colon > 0)
                    {
                        menuToken = it.Substring(0, colon).Trim();
                        rightsSpec = it.Substring(colon + 1).Trim();
                    }

                    if (string.IsNullOrWhiteSpace(menuToken) || !menuToken.Contains("-"))
                        continue;

                    menuEntries.Add((menuToken, rightsSpec));
                }

                bool flag = false;

                // -------- PROCESS MENUS --------
                for (int b = 0; b < menuEntries.Count; b++)
                {
                    var entry = menuEntries[b];
                    string val = entry.MenuToken;
                    string rightsSpec = entry.RightsSpec;

                    // MenuNo format: M029-M029
                    if (!val.Contains("-"))
                        continue;

                    string[] MenuNo = val.Split('-');

                    if (MenuNo.Length < 2)
                        continue;

                    SPRoleRights roleRights = new SPRoleRights();
                    roleRights.Full_Rights = roleRights.Add_Rights = roleRights.Edit_Rights = roleRights.View_Rights = roleRights.Delete_Rights = false;

                    roleRights.Role_No = RoleNo;
                    roleRights.Menu_No = MenuNo[0];

                    int chkSubMenu = 0, chkSubSubMenu = 0;

                    // Check sub menu duplicates (compare against menu tokens only)
                    for (int c = b + 1; c < menuEntries.Count; c++)
                    {
                        var temp = menuEntries[c].MenuToken;
                        if (temp.Length >= 4)
                        {
                            if (temp.Substring(0, 4).Contains(MenuNo[0]))
                                chkSubMenu++;

                            if (temp.Substring(0, 4).Contains(MenuNo[1]))
                                chkSubSubMenu++;
                        }
                    }

                    if ((MenuNo[0] == MenuNo[1] && chkSubMenu > 0) ||
                        (MenuNo[0] != MenuNo[1] && chkSubSubMenu > 0))
                        continue;

                    roleRights.Sub_Menu_No = (MenuNo[0] == MenuNo[1]) ? "" : MenuNo[1];

                    // Assign rights:
                    // - If rightsSpec is provided per menu ("F" or "AEVD"), use it.
                    // - Else fall back to legacy global rights tokens.
                    if (!string.IsNullOrWhiteSpace(rightsSpec))
                    {
                        var rs = rightsSpec.ToUpperInvariant();
                        if (rs.Contains("F"))
                        {
                            roleRights.Full_Rights = true;
                            roleRights.Add_Rights = true;
                            roleRights.Edit_Rights = true;
                            roleRights.View_Rights = true;
                            roleRights.Delete_Rights = true;
                        }
                        else
                        {
                            roleRights.Full_Rights = false;
                            roleRights.Add_Rights = rs.Contains("A");
                            roleRights.Edit_Rights = rs.Contains("E");
                            roleRights.View_Rights = rs.Contains("V");
                            roleRights.Delete_Rights = rs.Contains("D");
                        }
                    }
                    else
                    {
                        if (globalFull)
                        {
                            roleRights.Full_Rights = true;
                            roleRights.Add_Rights = true;
                            roleRights.Edit_Rights = true;
                            roleRights.View_Rights = true;
                            roleRights.Delete_Rights = true;
                        }
                        else
                        {
                            roleRights.Full_Rights = false;
                            roleRights.Add_Rights = globalAdd;
                            roleRights.Edit_Rights = globalEdit;
                            roleRights.View_Rights = globalView;
                            roleRights.Delete_Rights = globalDelete;
                        }
                    }

                    roleRights.IsActive = true;

                    SPRoleRights requestRoleRights = roleRights;
                    SPRoleRightsResponse responseRoleRights = new SPRoleRightsResponse();

                    var result = PostItemRoleRights("RoleWiseMenuRightsListDotNetAPI", requestRoleRights, responseRoleRights);

                    // Treat HTTP success as save success even if the response body doesn't parse
                    // into a model with populated keys.
                    var saved = result.Result.Item1;
                    var saveErr = result.Result.Item2;
                    if ((saveErr != null && saveErr.isSuccess) || (saved != null && saved.No != null))
                    {
                        flag = true; // keep true once any record saves
                        responseRoleRights = saved;
                    }
                    else
                    {
                        // capture last error message for troubleshooting
                        if (saveErr != null && !string.IsNullOrWhiteSpace(saveErr.message))
                            lastErrorMessage = saveErr.message;
                    }
                }

                var resp = Request.CreateResponse(HttpStatusCode.OK, flag);
                if (!flag && !string.IsNullOrWhiteSpace(lastErrorMessage))
                    resp.Headers.Add("X-RoleRights-Error", lastErrorMessage);
                return resp;
            }
            catch (Exception ex)
            {
                lastErrorMessage = ex.Message;
                var resp = Request.CreateResponse(HttpStatusCode.OK, false);
                if (!string.IsNullOrWhiteSpace(lastErrorMessage))
                    resp.Headers.Add("X-RoleRights-Error", lastErrorMessage);
                return resp;
            }
        }


        [Route("GetAllRolesForDDL")]
        public List<SPRolesForDDL> GetAllRolesForDDL()
        {
            API ac = new API();
            List<SPRolesForDDL> roles = new List<SPRolesForDDL>();

            var result = ac.GetData<SPRolesForDDL>("RolesListDotNetAPI", "");

            if (result != null && result.Result.Item1.value.Count > 0)
                roles = result.Result.Item1.value;

            return roles;
        }


        [Route("GetAllMenusSubMenusOfRole")]
        public List<SPMenusSubMenusOfRole> GetAllMenusSubMenusOfRole(string RoleNo)
        {
            API ac = new API();
            List<SPMenusSubMenusOfRole> menussubmenus = new List<SPMenusSubMenusOfRole>();

            var result = ac.GetData<SPMenusSubMenusOfRole>("RoleWiseMenuRightsListDotNetAPI", "Role_No eq '" + RoleNo + "'");

            if (result != null && result.Result.Item1.value.Count > 0)
                menussubmenus = result.Result.Item1.value;

            return menussubmenus;
        }

        public async Task<(SPRoleRightsResponse, errorDetails)> PostItemRoleRights<SPRoleRightsResponse>(string apiendpoint, SPRoleRights requestModel, SPRoleRightsResponse responseModel)
        {
            string _baseURL = System.Configuration.ConfigurationManager.AppSettings["BaseURL"];
            string _tenantId = System.Configuration.ConfigurationManager.AppSettings["TenantID"];
            string _environment = System.Configuration.ConfigurationManager.AppSettings["Environment"];
            string _companyName = System.Configuration.ConfigurationManager.AppSettings["CompanyName"];

            API ac = new API();
            var accessToken = await ac.GetAccessToken();

            HttpClient _httpClient = new HttpClient();
            string encodeurl = Uri.EscapeUriString(_baseURL.Replace("{TenantID}", _tenantId).Replace("{Environment}", _environment).Replace("{CompanyName}", _companyName) + apiendpoint);
            Uri baseuri = new Uri(encodeurl);
            _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken.Token);


            string ItemCardObjString = JsonConvert.SerializeObject(requestModel);
            var content = new StringContent(ItemCardObjString, Encoding.UTF8, "application/json");
            HttpResponseMessage response = null;
            try
            {
                response = _httpClient.PostAsync(baseuri, content).Result;
            }
            catch (Exception)
            {

            }
            errorDetails errordetail = new errorDetails();
            if (response == null)
            {
                errordetail.isSuccess = false;
                errordetail.code = "RequestFailed";
                errordetail.message = "BC API request failed";
                return (responseModel, errordetail);
            }

            errordetail.isSuccess = response.IsSuccessStatusCode;
            if (response.IsSuccessStatusCode)
            {
                var JsonData = response.Content.ReadAsStringAsync().Result;
                try
                {
                    JObject res = JObject.Parse(JsonData);
                    responseModel = res.ToObject<SPRoleRightsResponse>();

                    errordetail.code = response.StatusCode.ToString();
                    errordetail.message = response.ReasonPhrase;
                }
                catch (Exception)
                {
                }
            }
            else
            {
                var JsonData = response.Content.ReadAsStringAsync().Result;

                try
                {
                    JObject res = JObject.Parse(JsonData);
                    errorMaster<errorDetails> emd = res.ToObject<errorMaster<errorDetails>>();
                    errordetail = emd.error;
                }
                catch (Exception)
                {
                }
            }
            return (responseModel, errordetail);
        }

        public async Task<(SPMenusRightsForDelRes, errorDetails)> PostItemForMenusRightsDel<SPMenusRightsForDelRes>(string apiendpoint, SPMenusRightsForDel requestModel, SPMenusRightsForDelRes responseModel)
        {
            string _baseURL = System.Configuration.ConfigurationManager.AppSettings["BaseURL"];
            string _tenantId = System.Configuration.ConfigurationManager.AppSettings["TenantID"];
            string _environment = System.Configuration.ConfigurationManager.AppSettings["Environment"];
            string _companyName = System.Configuration.ConfigurationManager.AppSettings["CompanyName"];

            API ac = new API();
            var accessToken = await ac.GetAccessToken();

            HttpClient _httpClient = new HttpClient();
            string encodeurl = Uri.EscapeUriString("https://api.businesscentral.dynamics.com/v2.0/e55ad508-ef1a-489f-afe3-ae21f856e440/Sandbox/ODataV4/DeleteDotNetAPIs_deleterolerisemenurights?Company=\'Prakash Company\'");
            Uri baseuri = new Uri(encodeurl);
            _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken.Token);


            string ItemCardObjString = JsonConvert.SerializeObject(requestModel);
            var content = new StringContent(ItemCardObjString, Encoding.UTF8, "application/json");
            HttpResponseMessage response = null;
            try
            {
                response = _httpClient.PostAsync(baseuri, content).Result;
            }
            catch (Exception)
            {

            }
            errorDetails errordetail = new errorDetails();
            if (response == null)
            {
                errordetail.isSuccess = false;
                errordetail.code = "RequestFailed";
                errordetail.message = "Delete request failed";
                return (responseModel, errordetail);
            }

            errordetail.isSuccess = response.IsSuccessStatusCode;
            if (response.IsSuccessStatusCode)
            {
                var JsonData = response.Content.ReadAsStringAsync().Result;
                try
                {
                    JObject res = JObject.Parse(JsonData);
                    responseModel = res.ToObject<SPMenusRightsForDelRes>();

                    errordetail.code = response.StatusCode.ToString();
                    errordetail.message = response.ReasonPhrase;
                }
                catch (Exception)
                {
                }
            }
            else
            {
                var JsonData = response.Content.ReadAsStringAsync().Result;

                try
                {
                    JObject res = JObject.Parse(JsonData);
                    errorMaster<errorDetails> emd = res.ToObject<errorMaster<errorDetails>>();
                    errordetail = emd.error;
                }
                catch (Exception)
                {
                }
            }
            return (responseModel, errordetail);
        }

        [HttpGet]
        [Route("GetRolewiseMenuRight")]
        public async Task<IHttpActionResult> GetRolewiseMenuRight(string Usersecurityid)
        {
            if (string.IsNullOrWhiteSpace(Usersecurityid))
                return BadRequest("Usersecurityid is required");

            RoleWiseMenuRightsResponse responseModel = new RoleWiseMenuRightsResponse();
            RolesRightRequest requestmodel = new RolesRightRequest
            {
                usersecurityid = Usersecurityid
            };

            var result = await PostRoleRightPermission("RolesManagement_GenerateUserPermissionJSON", requestmodel, responseModel);

            if (result.Item2 != null && !result.Item2.isSuccess)
            {
                return Ok(new RoleWiseMenuRightsResponse
                {
                    UserId = Usersecurityid,
                    Roles = new List<RoleWiseRole>()
                });
            }

            var payload = result.Item1 ?? new RoleWiseMenuRightsResponse();
            if (string.IsNullOrWhiteSpace(payload.UserId))
                payload.UserId = Usersecurityid;
            if (payload.Roles == null)
                payload.Roles = new List<RoleWiseRole>();

            try
            {
                foreach (var role in payload.Roles)
                {
                    if (role?.Menus == null) continue;
                    foreach (var menu in role.Menus)
                    {
                        if (menu?.Children == null) continue;
                        foreach (var child in menu.Children)
                        {
                            var p = child?.Permissions;
                            if (p == null) continue;
                            p.Normalize();
                        }
                    }
                }
            }
            catch { }
            try
            {
                bool IsBlank(string s) => string.IsNullOrWhiteSpace(s);
                bool IsAllFalse(RoleWisePermission p)
                {
                    if (p == null) return true;
                    return !p.Read && !p.Create && !p.Update && !p.Delete
                        && !p.Full_Rights && !p.Add_Rights && !p.Edit_Rights && !p.View_Rights && !p.Delete_Rights;
                }

                foreach (var role in payload.Roles)
                {
                    if (role?.Menus == null || string.IsNullOrWhiteSpace(role.RoleId)) continue;

                    // Target the Dashboard menu (by id if available, else by name)
                    var dashboardMenu = role.Menus.FirstOrDefault(m =>
                        m != null &&
                        (
                            (!string.IsNullOrWhiteSpace(m.MenuId) && m.MenuId.Equals("M001", StringComparison.OrdinalIgnoreCase)) ||
                            (!string.IsNullOrWhiteSpace(m.MenuName) && m.MenuName.Equals("Dashboard", StringComparison.OrdinalIgnoreCase))
                        ));

                    if (dashboardMenu?.Children == null || dashboardMenu.Children.Count == 0) continue;

                    // Find the blank child entry (commonly MenuId == parent, and fields empty)
                    var blankChild = dashboardMenu.Children.FirstOrDefault(c =>
                        c != null &&
                        !string.IsNullOrWhiteSpace(c.MenuId) && !string.IsNullOrWhiteSpace(dashboardMenu.MenuId) &&
                        c.MenuId.Equals(dashboardMenu.MenuId, StringComparison.OrdinalIgnoreCase) &&
                        IsBlank(c.MenuName) && IsBlank(c.Controller) && IsBlank(c.Action));

                    if (blankChild == null) continue;

                    if (blankChild.Permissions == null)
                        blankChild.Permissions = new RoleWisePermission();

                    // Only overlay when dashboard perms are totally false (avoid overriding valid payloads)
                    if (!IsAllFalse(blankChild.Permissions)) continue;

                    // Query the saved role rights for this role + dashboard menu.
                    // If present, map legacy rights into standard permissions.
                    var ac = new API();
                    var filter = $"Role_No eq '{role.RoleId}' and Menu_No eq '{dashboardMenu.MenuId}'";
                    var rightsResult = await ac.GetData<SPMenusSubMenusOfRole>("RoleWiseMenuRightsListDotNetAPI", filter);
                    if (!rightsResult.err.isSuccess || rightsResult.items?.value == null || rightsResult.items.value.Count == 0)
                        continue;

                    var rr = rightsResult.items.value.FirstOrDefault(x =>
                        x != null &&
                        (string.IsNullOrWhiteSpace(x.Sub_Menu_No) ||
                         (!string.IsNullOrWhiteSpace(x.Menu_No) && x.Sub_Menu_No != null && x.Sub_Menu_No.Equals(x.Menu_No, StringComparison.OrdinalIgnoreCase))));

                    if (rr == null) continue;

                    blankChild.Permissions.Full_Rights = rr.Full_Rights;
                    blankChild.Permissions.Add_Rights = rr.Add_Rights;
                    blankChild.Permissions.Edit_Rights = rr.Edit_Rights;
                    blankChild.Permissions.View_Rights = rr.View_Rights;
                    blankChild.Permissions.Delete_Rights = rr.Delete_Rights;
                    blankChild.Permissions.Normalize();

                    if (string.IsNullOrWhiteSpace(blankChild.MenuName))
                        blankChild.MenuName = dashboardMenu.MenuName;
                }
            }
            catch { }

            return Ok(payload); // ✅ Web API correct response
        }


        public async Task<(RoleWiseMenuRightsResponse, errorDetails)> PostRoleRightPermission<RolesRightRequest>(string apiendpoint, RolesRightRequest requestModel, RoleWiseMenuRightsResponse responseModel)
        {
            string _codeUnitBaseUrl = ConfigurationManager.AppSettings["CodeUnitBaseURL"];
            string _tenantId = ConfigurationManager.AppSettings["TenantID"];
            string _environment = ConfigurationManager.AppSettings["Environment"];
            string _companyName = ConfigurationManager.AppSettings["CompanyName"];

            API ac = new API();
            var accessToken = await ac.GetAccessToken();

            using (HttpClient client = new HttpClient())
            {
                string url = Uri.EscapeUriString(_codeUnitBaseUrl.Replace("{TenantID}", _tenantId).Replace("{Environment}", _environment).Replace("{CompanyName}", _companyName).Replace("{Endpoint}", apiendpoint));

                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken.Token);

                var content = new StringContent(JsonConvert.SerializeObject(requestModel), Encoding.UTF8, "application/json");
                var response = await client.PostAsync(url, content);

                errorDetails err = new errorDetails
                {
                    isSuccess = response.IsSuccessStatusCode
                };

                var json = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        JObject res = JObject.Parse(json);
                        JToken valueToken = res["value"];

                        // Some BC codeunits return the payload inside "value" either as an object or as an escaped JSON string.
                        if (valueToken == null || valueToken.Type == JTokenType.Null)
                        {
                            // Fallback: sometimes the whole response itself is the payload.
                            responseModel = JsonConvert.DeserializeObject<RoleWiseMenuRightsResponse>(json);
                        }
                        else if (valueToken.Type == JTokenType.Object)
                        {
                            responseModel = valueToken.ToObject<RoleWiseMenuRightsResponse>();
                        }
                        else
                        {
                            // Treat as string (possibly double-encoded)
                            var raw = valueToken.Type == JTokenType.String
                                ? valueToken.Value<string>()
                                : valueToken.ToString(Formatting.None);

                            raw = raw ?? string.Empty;

                            // Attempt 1: raw is directly a JSON object string
                            try
                            {
                                responseModel = JToken.Parse(raw).ToObject<RoleWiseMenuRightsResponse>();
                            }
                            catch
                            {
                                try
                                {
                                    var unquoted = JsonConvert.DeserializeObject<string>(raw);
                                    if (!string.IsNullOrWhiteSpace(unquoted))
                                        responseModel = JToken.Parse(unquoted).ToObject<RoleWiseMenuRightsResponse>();
                                }
                                catch
                                {
                                   
                                }
                            }
                        }
                    }
                    catch
                    {
                    }
                }
                else
                {
                    var emd = JObject.Parse(json).ToObject<errorMaster<errorDetails>>();
                    err = emd.error;
                }

                if (responseModel != null && responseModel.Roles == null)
                    responseModel.Roles = new List<RoleWiseRole>();

                return (responseModel, err);
            }
        }

    }
}
