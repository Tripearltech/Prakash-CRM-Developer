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
using System.Web.Http;
using RouteAttribute = System.Web.Http.RouteAttribute;
using RoutePrefixAttribute = System.Web.Http.RoutePrefixAttribute;

namespace PrakashCRM.Service.Controllers
{
    [RoutePrefix("api/SPMenus")]
    public class SPMenusController : ApiController
    {
        // C#
        [Route("GetAllMenus")]
        public async Task<List<SPMenuList>> GetAllMenus(int skip, int top, string orderby, string filter, bool isExport = false)
        {
            API ac = new API();
            List<SPMenuList> menus = new List<SPMenuList>();
            filter = filter ?? "";

            if (isExport)
            {
                var (odata, err) = await ac.GetData<SPMenuList>("MenuListDotNetAPI", filter).ConfigureAwait(false);
                if (odata?.value != null && odata.value.Count > 0) menus = odata.value;
            }
            else
            {
                var (odata, err) = await ac.GetData1<SPMenuList>("MenuListDotNetAPI", filter, skip, top, orderby).ConfigureAwait(false);
                if (odata?.value != null && odata.value.Count > 0) menus = odata.value;
            }

            return menus;
        }


        [Route("GetApiRecordsCount")]
        public int GetApiRecordsCount(string apiEndPointName, string filter)
        {
            API ac = new API();
            if (filter == null)
                filter = "";

            var count = ac.CalculateCount(apiEndPointName, filter);

            return Convert.ToInt32(count.Result);
        }

        [Route("GetAllParentMenuNoForDDL")]
        public async Task<List<SPParentMenuNo>> GetAllParentMenuNoForDDL()
        {
            API ac = new API();
            List<SPParentMenuNo> parentmenuno = new List<SPParentMenuNo>();

            var (odata, err) = await ac.GetData<SPParentMenuNo>("MenuListDotNetAPI", "").ConfigureAwait(false);
            if (odata?.value != null && odata.value.Count > 0)
                parentmenuno = odata.value;

            return parentmenuno;
        }

        [Route("GetMenuFromNo")]
        public async Task<SPMenus> GetMenuFromNo(string No)
        {
            API ac = new API();
            if (string.IsNullOrWhiteSpace(No))
                return new SPMenus();

            var noEscaped = No.Replace("'", "''");
            var (odata, err) = await ac.GetData<SPMenus>("MenuListDotNetAPI", "No eq '" + noEscaped + "'").ConfigureAwait(false);

            if (odata?.value != null && odata.value.Count > 0)
                return odata.value[0];

            return new SPMenus();
        }

        [HttpPost]
        [Route("DeleteMenu")]
        public async Task<IHttpActionResult> DeleteMenu(string No)
        {
            if (string.IsNullOrWhiteSpace(No))
                return BadRequest("Menu No is required.");

            // Soft delete: mark inactive (so it disappears from Active list)
            var menu = await GetMenuFromNo(No).ConfigureAwait(false);
            if (menu == null || string.IsNullOrWhiteSpace(menu.No))
                return Ok(false);

            menu.IsActive = false;

            var responseModel = new SPMenusResponse();
            var result = await PatchItemMenu<SPMenusResponse>("MenuListDotNetAPI", menu, responseModel, No).ConfigureAwait(false);
            if (!result.Item2.isSuccess)
                return Content(HttpStatusCode.BadRequest, result.Item2);

            return Ok(true);
        }

        [HttpPost]
        [Route("Menu")]
        public async Task<IHttpActionResult> Menu(SPMenus Menu, bool isEdit, string MenuNo)
        {
            if (Menu == null)
                return BadRequest("Invalid menu payload.");

            // Basic validations (keeps failures readable in toast)
            if (string.IsNullOrWhiteSpace(Menu.Menu_Name))
                return BadRequest("Menu Name is required.");
            if (string.IsNullOrWhiteSpace(Menu.Serial_No) || !int.TryParse(Menu.Serial_No, out int serialNo) || serialNo <= 0)
                return BadRequest("Serial No is required.");
            if (string.IsNullOrWhiteSpace(Menu.ClassName))
                return BadRequest("Class Name is required.");

            if (isEdit && string.IsNullOrWhiteSpace(MenuNo))
                return BadRequest("MenuNo is required for edit.");

            SPMenus requestMenu = new SPMenus
            {
                No = string.IsNullOrWhiteSpace(Menu.No) ? null : Menu.No,
                Menu_Name = Menu.Menu_Name,
                Parent_Menu_No = Menu.Parent_Menu_No,
                Parent_Menu_Name = Menu.Parent_Menu_Name,
                Type = Menu.Type,
                Serial_No = Menu.Serial_No,
                ClassName = Menu.ClassName,
                IsActive = Menu.IsActive
            };

            var responseMenu = new SPMenusResponse();

            if (isEdit)
            {
                var result = await PatchItemMenu<SPMenusResponse>("MenuListDotNetAPI", requestMenu, responseMenu, MenuNo);
                if (!result.Item2.isSuccess)
                    return Content(HttpStatusCode.BadRequest, result.Item2);

                responseMenu = result.Item1;
            }
            else
            {
                var result = await PostItemMenu<SPMenusResponse>("MenuListDotNetAPI", requestMenu, responseMenu);
                if (!result.Item2.isSuccess)
                {
                    // Common BC setup issue: create without No can fail as duplicate key (often blank key).
                    var errCode = (result.Item2.code ?? "").ToLowerInvariant();
                    if (string.IsNullOrWhiteSpace(requestMenu.No) && errCode.Contains("entitywithsamekeyexists"))
                    {
                        requestMenu.No = GenerateMenuNo();
                        var retry = await PostItemMenu<SPMenusResponse>("MenuListDotNetAPI", requestMenu, responseMenu);
                        if (!retry.Item2.isSuccess)
                            return Content(HttpStatusCode.BadRequest, retry.Item2);

                        responseMenu = retry.Item1;
                    }
                    else
                    {
                        return Content(HttpStatusCode.BadRequest, result.Item2);
                    }
                }
                else
                {
                    responseMenu = result.Item1;
                }
            }

            return Ok(responseMenu);
        }

        private static string GenerateMenuNo()
        {
            // Short, sortable, unique-ish key. Example: M260128153012123
            return "M" + DateTime.UtcNow.ToString("yyMMddHHmmssfff");
        }

        public async Task<(SPMenusResponse, errorDetails)> PostItemMenu<SPMenusResponse>(string apiendpoint, SPMenus requestModel, SPMenusResponse responseModel)
        {
            string _baseURL = System.Configuration.ConfigurationManager.AppSettings["BaseURL"];
            string _tenantId = System.Configuration.ConfigurationManager.AppSettings["TenantID"];
            string _environment = System.Configuration.ConfigurationManager.AppSettings["Environment"];
            string _companyName = System.Configuration.ConfigurationManager.AppSettings["CompanyName"];

            API ac = new API();
            var accessToken = await ac.GetAccessToken();

            if (accessToken == null || string.IsNullOrWhiteSpace(accessToken.Token))
            {
                return (responseModel, new errorDetails
                {
                    isSuccess = false,
                    code = "Unauthorized",
                    message = "Failed to obtain access token."
                });
            }

            using (HttpClient _httpClient = new HttpClient())
            {
                string url = Uri.EscapeUriString(
                    _baseURL.Replace("{TenantID}", _tenantId)
                            .Replace("{Environment}", _environment)
                            .Replace("{CompanyName}", _companyName) + apiendpoint);

                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken.Token);

                string json = JsonConvert.SerializeObject(requestModel);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response;
                try
                {
                    response = await _httpClient.PostAsync(url, content);
                }
                catch (Exception ex)
                {
                    return (responseModel, new errorDetails
                    {
                        isSuccess = false,
                        code = "HttpRequestException",
                        message = ex.Message
                    });
                }

                var body = await response.Content.ReadAsStringAsync();

                errorDetails errordetail = new errorDetails
                {
                    isSuccess = response.IsSuccessStatusCode,
                    code = response.StatusCode.ToString(),
                    message = response.ReasonPhrase
                };

                if (response.IsSuccessStatusCode)
                {
                    // BC often returns the created entity.
                    if (!string.IsNullOrWhiteSpace(body))
                        responseModel = JsonConvert.DeserializeObject<SPMenusResponse>(body);
                }
                else
                {
                    try
                    {
                        var emd = JsonConvert.DeserializeObject<errorMaster<errorDetails>>(body);
                        if (emd?.error != null)
                            errordetail = emd.error;
                        else
                            errordetail.message = string.IsNullOrWhiteSpace(body) ? errordetail.message : body;
                    }
                    catch
                    {
                        errordetail.message = string.IsNullOrWhiteSpace(body) ? errordetail.message : body;
                    }
                }

                return (responseModel, errordetail);
            }
        }

        public async Task<(SPMenusResponse, errorDetails)> PatchItemMenu<SPMenusResponse>(string apiendpoint, SPMenus requestModel, SPMenusResponse responseModel, string MenuNo)
        {
            string _baseURL = System.Configuration.ConfigurationManager.AppSettings["BaseURL"];
            string _tenantId = System.Configuration.ConfigurationManager.AppSettings["TenantID"];
            string _environment = System.Configuration.ConfigurationManager.AppSettings["Environment"];
            string _companyName = System.Configuration.ConfigurationManager.AppSettings["CompanyName"];

            API ac = new API();
            var accessToken = await ac.GetAccessToken();

            if (accessToken == null || string.IsNullOrWhiteSpace(accessToken.Token))
            {
                return (responseModel, new errorDetails
                {
                    isSuccess = false,
                    code = "Unauthorized",
                    message = "Failed to obtain access token."
                });
            }

            using (HttpClient _httpClient = new HttpClient())
            {
                var menuNoEscaped = (MenuNo ?? "").Replace("'", "''");
                string url = Uri.EscapeUriString(
                    _baseURL.Replace("{TenantID}", _tenantId)
                            .Replace("{Environment}", _environment)
                            .Replace("{CompanyName}", _companyName) +
                    apiendpoint + "(No='" + menuNoEscaped + "')");

                var request = new HttpRequestMessage(new HttpMethod("PATCH"), new Uri(url));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Token);
                request.Headers.TryAddWithoutValidation("If-Match", "*");

                string json = JsonConvert.SerializeObject(requestModel);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response;
                try
                {
                    response = await _httpClient.SendAsync(request);
                }
                catch (Exception ex)
                {
                    return (responseModel, new errorDetails
                    {
                        isSuccess = false,
                        code = "HttpRequestException",
                        message = ex.Message
                    });
                }

                var body = await response.Content.ReadAsStringAsync();

                errorDetails errordetail = new errorDetails
                {
                    isSuccess = response.IsSuccessStatusCode,
                    code = response.StatusCode.ToString(),
                    message = response.ReasonPhrase
                };

                if (response.IsSuccessStatusCode)
                {
                    // BC PATCH may return empty body (204). Only parse if present.
                    if (!string.IsNullOrWhiteSpace(body))
                        responseModel = JsonConvert.DeserializeObject<SPMenusResponse>(body);
                }
                else
                {
                    try
                    {
                        var emd = JsonConvert.DeserializeObject<errorMaster<errorDetails>>(body);
                        if (emd?.error != null)
                            errordetail = emd.error;
                        else
                            errordetail.message = string.IsNullOrWhiteSpace(body) ? errordetail.message : body;
                    }
                    catch
                    {
                        errordetail.message = string.IsNullOrWhiteSpace(body) ? errordetail.message : body;
                    }
                }

                return (responseModel, errordetail);
            }
        }
    }
}
