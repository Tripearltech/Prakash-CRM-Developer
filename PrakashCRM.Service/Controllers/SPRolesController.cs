using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PrakashCRM.Data.Models;
using PrakashCRM.Service.Classes;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace PrakashCRM.Service.Controllers
{
    [RoutePrefix("api/SPRoles")]
    public class SPRolesController : ApiController
    {
        public class EmployeeSearchItem
        {
            public string No { get; set; }
            public string EmployeeCode { get; set; }
            public string FullName { get; set; }
        }

        public class EmployeeSearchResponse
        {
            public List<EmployeeSearchItem> items { get; set; } = new List<EmployeeSearchItem>();
            public bool hasMore { get; set; }
        }

        public class RoleSearchItem
        {
            public string No { get; set; }
            public string Role_Name { get; set; }
        }

        public class RoleSearchResponse
        {
            public List<RoleSearchItem> items { get; set; } = new List<RoleSearchItem>();
            public bool hasMore { get; set; }
        }


        [Route("GetAllRoles")]
        public List<SPRoleList> GetAllRoles(int skip, int top, string orderby, string filter, bool isExport = false)
        {
            API ac = new API();
            List<SPRoleList> roles = new List<SPRoleList>();
            if (string.IsNullOrEmpty(filter))
                filter = "IsActive eq true";
            else
                filter += " and IsActive eq true";

            var result = (dynamic)null;

            if (isExport)
                result = ac.GetData<SPRoleList>("RolesListDotNetAPI", filter);
            else
                result = ac.GetData1<SPRoleList>("RolesListDotNetAPI", filter, skip, top, orderby);

            if (result.Result.Item1.value.Count > 0)
                roles = result.Result.Item1.value;

            //roles = (List<SPRoleList>)roles.Distinct();
            return roles;
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

        [Route("GetRoleFromNo")]
        public SPRoles GetRoleFromNo(string No)
        {
            API ac = new API();
            SPRoles role = new SPRoles();

            var result = ac.GetData<SPRoles>("RolesListDotNetAPI", "No eq '" + No + "'");

            if (result.Result.Item1.value.Count > 0)
                role = result.Result.Item1.value[0];

            return role;
        }

        [Route("Role")]
        public async Task<IHttpActionResult> Role(SPRoles role, bool isEdit, string RoleNo = "")
        {
            if (role == null)
                return BadRequest("Invalid role payload.");
            if (string.IsNullOrWhiteSpace(role.Role_Name))
                return BadRequest("Role Name is required.");

            if (isEdit && string.IsNullOrWhiteSpace(RoleNo))
                return BadRequest("RoleNo is required for edit.");

            SPRoles requestRole = new SPRoles
            {
                No = string.IsNullOrWhiteSpace(role.No) ? "" : role.No,
                Role_Name = role.Role_Name,
                IsActive = role.IsActive
            };

            SPRolesResponse responseRole = new SPRolesResponse();

            if (isEdit)
            {
                var result = await PatchItemRole<SPRolesResponse>("RolesListDotNetAPI", requestRole, responseRole, RoleNo);
                if (!result.Item2.isSuccess)
                    return Content(HttpStatusCode.BadRequest, result.Item2);

                responseRole = result.Item1;
            }
            else
            {
                var result = await PostItemRole<SPRolesResponse>("RolesListDotNetAPI", requestRole, responseRole);
                if (!result.Item2.isSuccess)
                    return Content(HttpStatusCode.BadRequest, result.Item2);

                responseRole = result.Item1;
            }

            return Ok(responseRole);
        }


        public async Task<(SPRolesResponse, errorDetails)> PostItemRole<SPRolesResponse>(string apiendpoint, SPRoles requestModel, SPRolesResponse responseModel)
        {
            string _baseURL = ConfigurationManager.AppSettings["BaseURL"];
            string _tenantId = ConfigurationManager.AppSettings["TenantID"];
            string _environment = ConfigurationManager.AppSettings["Environment"];
            string _companyName = ConfigurationManager.AppSettings["CompanyName"];

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

                var json = JsonConvert.SerializeObject(requestModel);
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

                var JsonData = await response.Content.ReadAsStringAsync();

                errorDetails errordetail = new errorDetails
                {
                    isSuccess = response.IsSuccessStatusCode,
                    code = response.StatusCode.ToString(),
                    message = response.ReasonPhrase
                };

                if (response.IsSuccessStatusCode)
                {
                    responseModel = JsonConvert.DeserializeObject<SPRolesResponse>(JsonData);
                }
                else
                {
                    try
                    {
                        var emd = JsonConvert.DeserializeObject<errorMaster<errorDetails>>(JsonData);
                        if (emd?.error != null)
                            errordetail = emd.error;
                        else
                            errordetail.message = string.IsNullOrWhiteSpace(JsonData) ? errordetail.message : JsonData;
                    }
                    catch
                    {
                        errordetail.message = string.IsNullOrWhiteSpace(JsonData) ? errordetail.message : JsonData;
                    }
                }

                return (responseModel, errordetail);
            }
        }

        public async Task<(SPRolesResponse, errorDetails)> PatchItemRole<SPRolesResponse>(string apiendpoint, SPRoles requestModel, SPRolesResponse responseModel, string RoleNo)
        {
            string _baseURL = ConfigurationManager.AppSettings["BaseURL"];
            string _tenantId = ConfigurationManager.AppSettings["TenantID"];
            string _environment = ConfigurationManager.AppSettings["Environment"];
            string _companyName = ConfigurationManager.AppSettings["CompanyName"];

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
                var roleNoEscaped = (RoleNo ?? "").Replace("'", "''");
                string url = Uri.EscapeUriString(
                    _baseURL.Replace("{TenantID}", _tenantId)
                            .Replace("{Environment}", _environment)
                            .Replace("{CompanyName}", _companyName) +
                    apiendpoint + "(No='" + roleNoEscaped + "')");

                var request = new HttpRequestMessage(new HttpMethod("PATCH"), new Uri(url));

                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken.Token);

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

                var JsonData = await response.Content.ReadAsStringAsync();

                errorDetails errordetail = new errorDetails
                {
                    isSuccess = response.IsSuccessStatusCode,
                    code = response.StatusCode.ToString(),
                    message = response.ReasonPhrase
                };

                if (response.IsSuccessStatusCode)
                {
                    responseModel = JsonConvert.DeserializeObject<SPRolesResponse>(JsonData);
                }
                else
                {
                    try
                    {
                        var emd = JsonConvert.DeserializeObject<errorMaster<errorDetails>>(JsonData);
                        if (emd?.error != null)
                            errordetail = emd.error;
                        else
                            errordetail.message = string.IsNullOrWhiteSpace(JsonData) ? errordetail.message : JsonData;
                    }
                    catch
                    {
                        errordetail.message = string.IsNullOrWhiteSpace(JsonData) ? errordetail.message : JsonData;
                    }
                }

                return (responseModel, errordetail);
            }
        }


        [HttpPost]
        [Route("DeleteRole")]
        public async Task<IHttpActionResult> DeleteRole(string No, string Name)
        {
            if (string.IsNullOrWhiteSpace(No))
                return BadRequest("No is required.");

            // Soft delete: mark inactive
            var result = await PatchRoleIsActive(No, false);
            if (!result.err.isSuccess)
                return Content(HttpStatusCode.BadRequest, result.err);

            return Ok(true);
        }

        private async Task<(bool ok, errorDetails err)> PatchRoleIsActive(string roleNo, bool isActive)
        {
            string _baseURL = ConfigurationManager.AppSettings["BaseURL"];
            string _tenantId = ConfigurationManager.AppSettings["TenantID"];
            string _environment = ConfigurationManager.AppSettings["Environment"];
            string _companyName = ConfigurationManager.AppSettings["CompanyName"];

            var ac = new API();
            var accessToken = await ac.GetAccessToken();
            if (accessToken == null || string.IsNullOrWhiteSpace(accessToken.Token))
            {
                return (false, new errorDetails
                {
                    isSuccess = false,
                    code = "Unauthorized",
                    message = "Failed to obtain access token."
                });
            }

            var roleNoEscaped = (roleNo ?? "").Replace("'", "''");
            var url = Uri.EscapeUriString(
                _baseURL.Replace("{TenantID}", _tenantId)
                        .Replace("{Environment}", _environment)
                        .Replace("{CompanyName}", _companyName) +
                "RolesListDotNetAPI(No='" + roleNoEscaped + "')");

            using (var httpClient = new HttpClient())
            {
                var request = new HttpRequestMessage(new HttpMethod("PATCH"), new Uri(url));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Token);
                request.Headers.TryAddWithoutValidation("If-Match", "*");

                var payload = JsonConvert.SerializeObject(new { IsActive = isActive });
                request.Content = new StringContent(payload, Encoding.UTF8, "application/json");

                HttpResponseMessage response;
                try
                {
                    response = await httpClient.SendAsync(request);
                }
                catch (Exception ex)
                {
                    return (false, new errorDetails
                    {
                        isSuccess = false,
                        code = "HttpRequestException",
                        message = ex.Message
                    });
                }

                var body = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    return (true, new errorDetails
                    {
                        isSuccess = true,
                        code = response.StatusCode.ToString(),
                        message = response.ReasonPhrase
                    });
                }

                // Try parse BC error payload
                try
                {
                    var emd = JsonConvert.DeserializeObject<errorMaster<errorDetails>>(body);
                    if (emd?.error != null)
                        return (false, emd.error);
                }
                catch
                {
                    // ignore
                }

                return (false, new errorDetails
                {
                    isSuccess = false,
                    code = response.StatusCode.ToString(),
                    message = string.IsNullOrWhiteSpace(body) ? response.ReasonPhrase : body
                });
            }
        }

        [HttpGet]
        [Route("GetUserRoleRelationFromId")]
        public SPUserRoleRelationList GetUserRoleRelationFromId(int id)
        {
            API ac = new API();
            SPUserRoleRelationList item = new SPUserRoleRelationList();

            var result = ac.GetData<SPUserRoleRelationList>("UserRoleRelation", "User_Relation_Role_ID eq " + id);
            if (result.Result.Item1.value.Count > 0)
                item = result.Result.Item1.value[0];

            return item;
        }

        [HttpGet]
        [Route("SearchEmployees")]
        public async Task<IHttpActionResult> SearchEmployees(string term = "", int skip = 0, int top = 20)
        {
            term = (term ?? "").Trim();
            if (top <= 0) top = 20;
            if (top > 50) top = 50;
            if (skip < 0) skip = 0;

            string t = term.Replace("'", "''");
            string filter = "";
            if (!string.IsNullOrWhiteSpace(t))
            {
                filter =
                    "startswith(No,'" + t + "') eq true" +
                    " or startswith(PCPL_Employee_Code,'" + t + "') eq true" +
                    " or startswith(First_Name,'" + t + "') eq true" +
                    " or startswith(Last_Name,'" + t + "') eq true";
            }

            var ac = new API();
            var result = await ac.GetDataWithODataQuery<SPProfile>(
                "EmployeesDotNetAPI",
                filter,
                top: top + 1,
                skip: skip,
                orderBy: "No asc");

            var values = (result.Item1?.value ?? new List<SPProfile>())
                .Where(x => x != null && !string.IsNullOrWhiteSpace(x.No))
                .ToList();

            bool hasMore = values.Count > top;
            if (hasMore) values = values.Take(top).ToList();

            var response = new EmployeeSearchResponse
            {
                hasMore = hasMore,
                items = values.Select(x => new EmployeeSearchItem
                {
                    No = x.No,
                    EmployeeCode = x.PCPL_Employee_Code ?? "",
                    FullName = (string.Join(" ", new[] { x.First_Name, x.Middle_Name, x.Last_Name }
                        .Where(s => !string.IsNullOrWhiteSpace(s)))).Trim()
                }).ToList()
            };

            return Ok(response);
        }

        [HttpGet]
        [Route("SearchRoles")]
        public async Task<IHttpActionResult> SearchRoles(string term = "", int skip = 0, int top = 20)
        {
            term = (term ?? "").Trim();
            if (top <= 0) top = 20;
            if (top > 50) top = 50;
            if (skip < 0) skip = 0;

            string t = term.Replace("'", "''");
            string filter = "";
            if (!string.IsNullOrWhiteSpace(t))
            {
                filter =
                    "startswith(No,'" + t + "') eq true" +
                    " or startswith(Role_Name,'" + t + "') eq true";
            }

            var ac = new API();
            var result = await ac.GetDataWithODataQuery<SPRolesForDDL>(
                "RolesListDotNetAPI",
                filter,
                top: top + 1,
                skip: skip,
                orderBy: "No asc");

            var values = (result.Item1?.value ?? new List<SPRolesForDDL>())
                .Where(x => x != null && !string.IsNullOrWhiteSpace(x.No))
                .ToList();

            bool hasMore = values.Count > top;
            if (hasMore) values = values.Take(top).ToList();

            var response = new RoleSearchResponse
            {
                hasMore = hasMore,
                items = values.Select(x => new RoleSearchItem
                {
                    No = x.No,
                    Role_Name = x.Role_Name ?? ""
                }).ToList()
            };

            return Ok(response);
        }

        [HttpPost]
        [Route("UserRoleRelation")]
        public SPUserRoleRelationList UserRoleRelation(SPUserRoleRelationList model, bool isEdit, string id = "")
        {
            API ac = new API();
            SPUserRoleRelationList responseModel = new SPUserRoleRelationList();

            int parsedId;
            if (!int.TryParse(id, out parsedId))
                parsedId = model.User_Relation_Role_ID;

            var result = (dynamic)null;

            if (isEdit)
            {
                result = ac.PatchItem("UserRoleRelation", model, responseModel, "User_Relation_Role_ID=" + parsedId);
                if (result.Result.Item1 != null)
                    responseModel = result.Result.Item1;
            }
            else
            {
                result = ac.PostItem("UserRoleRelation", model, responseModel);
                if (result.Result.Item1 != null)
                    responseModel = result.Result.Item1;
            }

            return responseModel;
        }

        [HttpPost]
        [Route("DeleteUserRoleRelation")]
        public bool DeleteUserRoleRelation(int id)
        {
            bool flag = false;

            API ac = new API();
            SPUserRoleRelationList requestModel = new SPUserRoleRelationList();
            SPUserRoleRelationList responseModel = new SPUserRoleRelationList();

            var result = ac.DeleteItem("UserRoleRelation", requestModel, responseModel, "User_Relation_Role_ID=" + id);
            if (result.Result.Item1 != null || result.Result.Item2.isSuccess)
                flag = result.Result.Item2.isSuccess;

            return flag;
        }
        public async Task<(SPRolesResponse, errorDetails)> PatchItemForDelRole<SPRolesResponse>(string apiendpoint, SPRoles requestModel, SPRolesResponse responseModel, string fieldWithValue)
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
            var request = new HttpRequestMessage(new HttpMethod("PATCH"), baseuri + "(" + fieldWithValue + ")");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Token);
            _httpClient.DefaultRequestHeaders.Add("If-Match", "*");

            string ItemCardObjString = JsonConvert.SerializeObject(requestModel);
            request.Content = new StringContent(ItemCardObjString, Encoding.UTF8, "application/json");

            HttpResponseMessage response = null;
            try
            {
                //response = _httpClient.PutAsync(baseuri, content).Result;
                response = _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).Result;
            }
            catch (Exception)
            {

            }

            errorDetails errordetail = new errorDetails();
            errordetail.isSuccess = response.IsSuccessStatusCode;
            if (response.IsSuccessStatusCode)
            {
                var JsonData = response.Content.ReadAsStringAsync().Result;
                try
                {
                    JObject res = JObject.Parse(JsonData);
                    responseModel = res.ToObject<SPRolesResponse>();


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
        [Route("GetUserRoleRelationList")]
        public List<SPUserRoleRelationList> GetUserRoleRelationList()
        {
            API ac = new API();
            List<SPUserRoleRelationList> userrolerelationlist = new List<SPUserRoleRelationList>();

            var Result = ac.GetData<SPUserRoleRelationList>("UserRoleRelation", "");

            if (Result.Result.Item1.value.Count > 0)
                userrolerelationlist = Result.Result.Item1.value;


            return userrolerelationlist;
        }
    }
}
