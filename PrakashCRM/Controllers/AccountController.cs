using Newtonsoft.Json;
using PrakashCRM.Data.Models;
using PrakashCRM.Service.Classes;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using PrakashCRM.Security;

namespace PrakashCRM.Controllers
{

    public class AccountController : Controller
    {
        private async Task LogLoginActivity(string traceId, string description, string spCode)
        {
            try
            {
                string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPSiteActivity/LogActivity";

                SPSiteActivity activity = new SPSiteActivity
                {
                    Module_Name = "Account",
                    Trace_Id = string.IsNullOrWhiteSpace(traceId) ? "LOGIN" : traceId,
                    IP_Address = Request != null ? (Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? Request.ServerVariables["REMOTE_ADDR"]) : "",
                    Browser = ResolveBrowserName(Request),
                    Description = string.IsNullOrWhiteSpace(description) ? "Login Activity" : description,
                    Web_URL = Request != null ? Request.RawUrl : "/Account/Login",
                    Company_Code = string.IsNullOrWhiteSpace(spCode) ? "System" : spCode,
                    MAC_Address = "",
                    Device_Name = Environment.MachineName
                };

                if (activity.Description.Length > 100)
                    activity.Description = activity.Description.Substring(0, 100);

                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    string payload = JsonConvert.SerializeObject(activity);
                    var content = new StringContent(payload, Encoding.UTF8, "application/json");
                    await client.PostAsync(apiUrl, content);
                }
            }
            catch
            {
            }
        }

        private async Task LogLoginSiteError(string traceId, string description, string statusCode = "500", string exceptionMessage = "")
        {
            try
            {
                string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPSiteError/LogError";

                var payload = new
                {
                    Module_Name = "Account",
                    Trace_Id = string.IsNullOrWhiteSpace(traceId) ? "LOGIN_ERROR" : traceId,
                    Status_Code = string.IsNullOrWhiteSpace(statusCode) ? "500" : statusCode,
                    Browser = ResolveBrowserName(Request),
                    Description = string.IsNullOrWhiteSpace(description) ? "Login error" : description,
                    Exception = string.IsNullOrWhiteSpace(exceptionMessage) ? "Login error" : exceptionMessage,
                    Web_URL = Request != null ? Request.RawUrl : "/Account/Login",
                    Request_Data = "",
                    Company_Code = "System",
                    Device_Name = Environment.MachineName,
                    MAC_Address = ""
                };

                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    string body = JsonConvert.SerializeObject(payload);
                    var content = new StringContent(body, Encoding.UTF8, "application/json");
                    await client.PostAsync(apiUrl, content);
                }
            }
            catch
            {
            }
        }

        private string ResolveBrowserName(HttpRequestBase request)
        {
            if (request == null)
                return "Unknown";

            string browser = request.Browser != null ? request.Browser.Browser : "";
            if (!string.IsNullOrWhiteSpace(browser) && !browser.Equals("Unknown", StringComparison.OrdinalIgnoreCase))
                return browser;

            string ua = request.UserAgent ?? "";
            string ual = ua.ToLowerInvariant();

            if (ual.Contains("edg/")) return "Microsoft Edge";
            if (ual.Contains("opr/") || ual.Contains("opera")) return "Opera";
            if (ual.Contains("chrome/") && !ual.Contains("edg/")) return "Google Chrome";
            if (ual.Contains("firefox/")) return "Mozilla Firefox";
            if (ual.Contains("safari/") && !ual.Contains("chrome/")) return "Safari";
            if (ual.Contains("msie") || ual.Contains("trident/")) return "Internet Explorer";

            return "Unknown";
        }

        // GET: Account
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Login()
        {
            return View();
        }

        public async Task<JsonResult> CheckLoginAndSendOTP(string email, string pass, string adminContactNo)
        {
            try
            {
                string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "Salesperson/";

                apiUrl = apiUrl + "CheckEmailPassForOTP?email=" + Uri.EscapeDataString(email ?? "") + "&pass=" + Uri.EscapeDataString((pass ?? "").Trim());

                HttpClient client = new HttpClient();

                ContactNoOTPForLogin contactNoOTPForLogin = new ContactNoOTPForLogin();
                ContactNoOTPForLogin contactNoOTPForLoginRes = new ContactNoOTPForLogin();

                client.BaseAddress = new Uri(apiUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await client.GetAsync(apiUrl);
                var jsonData = "";
                if (response.IsSuccessStatusCode)
                {
                    jsonData = await response.Content.ReadAsStringAsync();
                    contactNoOTPForLogin = Newtonsoft.Json.JsonConvert.DeserializeObject<ContactNoOTPForLogin>(jsonData);

                    if (contactNoOTPForLogin.No != null)
                    {
                        if (contactNoOTPForLogin.PCPL_Enable_OTP_On_Login)
                        {
                            Task<ContactNoOTPForLogin> task = Task.Run<ContactNoOTPForLogin>(async () => await GenerateOTPAndSend(contactNoOTPForLogin, adminContactNo));
                            contactNoOTPForLoginRes = task.Result;
                            await LogLoginActivity("LOGIN_OTP", "Login OTP Sent", contactNoOTPForLogin.Salespers_Purch_Code);
                        }
                        else
                        {
                            Session["loggedInUserNo"] = contactNoOTPForLogin.No;
                            Session["loggedInUserFName"] = contactNoOTPForLogin.First_Name;
                            Session["loggedInUserLName"] = contactNoOTPForLogin.Last_Name;
                            Session["loggedInUserEmail"] = contactNoOTPForLogin.Company_E_Mail;
                            Session["loggedInUserJobTitle"] = contactNoOTPForLogin.Job_Title;
                            Session["loggedInUserRole"] = contactNoOTPForLogin.Role;
                            Session["loggedInUserMobile"] = contactNoOTPForLogin.Mobile_Phone_No;
                            Session["loggedInUserSPCode"] = contactNoOTPForLogin.Salespers_Purch_Code;
                            Session["loggedinUserBranch"] = contactNoOTPForLogin.Global_Dimension_1_Code ?? "";
                            RegisterCurrentSessionToken(contactNoOTPForLogin.No);

                            string SPCodesOfReportingPersonUser = "";
                            Task<string> task = Task.Run<string>(async () => await GetSPCodesOfReportingPersonUser(contactNoOTPForLogin.No));
                            SPCodesOfReportingPersonUser = task.Result;

                            if (SPCodesOfReportingPersonUser != "")
                                Session["SPCodesOfReportingPersonUser"] = SPCodesOfReportingPersonUser;
                            else
                                Session["SPCodesOfReportingPersonUser"] = "";

                            RoleWiseMenuRightsResponse menu = await GetRolewiseMenuRight(contactNoOTPForLogin.No);

                            Session["RoleWiseMenuData"] = menu;
                            contactNoOTPForLoginRes = contactNoOTPForLogin;
                            await LogLoginActivity("LOGIN_SUCCESS", "Login Success", contactNoOTPForLogin.Salespers_Purch_Code);
                        }
                    }
                }
                else
                {
                    await LogLoginActivity("LOGIN_FAIL", "Login Failed", "System");
                    await LogLoginSiteError("LOGIN_FAIL", "Login API failed", ((int)response.StatusCode).ToString(), "CheckEmailPassForOTP returned non-success status");
                }

                if (contactNoOTPForLogin == null || string.IsNullOrWhiteSpace(contactNoOTPForLogin.No))
                {
                    await LogLoginActivity("LOGIN_FAIL", "Login Failed", "System");
                    await LogLoginSiteError("LOGIN_FAIL", "Invalid login credentials", "401", "Login failed due to invalid credentials or user not found");
                }

                return Json(contactNoOTPForLoginRes, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                await LogLoginActivity("LOGIN_EXCEPTION", "Login Failed - Exception", "System");
                await LogLoginSiteError("LOGIN_EXCEPTION", "Exception in CheckLoginAndSendOTP", "500", ex.Message);
                return Json(new ContactNoOTPForLogin(), JsonRequestBehavior.AllowGet);
            }
        }

        public async Task<RoleWiseMenuRightsResponse>GetRolewiseMenuRight(string userSecurityId)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"]
                + "SPRoleRights/GetRolewiseMenuRight?Usersecurityid=" + userSecurityId;

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json")
                );

                HttpResponseMessage response = await client.GetAsync(apiUrl);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();

                // 🔹 Convert JSON → Model
                var model =
                    JsonConvert.DeserializeObject<RoleWiseMenuRightsResponse>(json);

                // safety
                if (model != null && model.Roles == null)
                    model.Roles = new List<RoleWiseRole>();

                return model;
            }
        }

        public async Task<string> GetSPCodesOfReportingPersonUser(string LoggedInUserNo)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "Salesperson/";

            apiUrl = apiUrl + "GetSPCodesOfReportingPersonUser?LoggedInUserNo=" + LoggedInUserNo;

            HttpClient client = new HttpClient();
            List<SPCodesOfReportingPersonUser> spCodes = new List<SPCodesOfReportingPersonUser>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            var jsonData = "";
            if (response.IsSuccessStatusCode)
            {
                jsonData = await response.Content.ReadAsStringAsync();
                spCodes = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPCodesOfReportingPersonUser>>(jsonData);
            }

            string spCodes_ = "";
            if(spCodes.Count > 0)
            {
                for (int a = 0; a < spCodes.Count; a++)
                {
                    spCodes_ += spCodes[a].Salespers_Purch_Code + ",";
                }

                spCodes_ = spCodes_.Substring(0, spCodes_.Length - 1);
            }

            return spCodes_;
        }

        public async Task<ContactNoOTPForLogin> GenerateOTPAndSend(ContactNoOTPForLogin contactNoOTPForLogin, string adminContactNo)
        {
            HttpClient client1 = new HttpClient();
            ContactNoOTPForLogin contactNoOTPForLoginRes = new ContactNoOTPForLogin();

            int length = 4;
            const string valid = "1234567890";
            StringBuilder res = new StringBuilder();
            Random rnd = new Random();
            while (0 < length--)
            {
                res.Append(valid[rnd.Next(valid.Length)]);
            }
            string generatedOTP = res.ToString();

            string smsApiUrl = ConfigurationManager.AppSettings["SMSApiUrl"].ToString();
            string smsFromMobile = ConfigurationManager.AppSettings["SMSFromMobile"].ToString();
            string smsFromPass = ConfigurationManager.AppSettings["SMSFromPass"].ToString();
            string smsFromSenderId = ConfigurationManager.AppSettings["SMSFromSenderId"].ToString();
            string smsToMobile = contactNoOTPForLogin.Phone_No_2;
            //var parameters = new Dictionary<string, string> {
            //        { "mobile", smsFromMobile },
            //        { "pass", smsFromPass },
            //        { "senderid", smsFromSenderId},
            //        { "to", smsToMobile },
            //        { "msg","Quote no " + generatedOTP + " of 123.59 has been approved. Click on https://www.google.com for further details." },
            //        { "templateid", "1207161552588210502" }
            //    };

            var parameters = new Dictionary<string, string> {
                    { "mobile", smsFromMobile },
                    { "pass", smsFromPass },
                    { "senderid", smsFromSenderId},
                    { "to", smsToMobile },
                    { "msg","Your OTP to log in to the PCAPL Web Portal is " + generatedOTP + ". Please do not share this OTP with anyone. If you did not attempt to log in, call " + adminContactNo + " immediately." },
                    { "templateid", "1207173708488227586" }
                };

            var encodedContent = new FormUrlEncodedContent(parameters);
            var response1 = await client1.PostAsync(smsApiUrl, encodedContent).ConfigureAwait(false);
            if (response1.StatusCode == HttpStatusCode.OK)
            {
                // Do something with response. Example get content:
                // var responseContent = await response.Content.ReadAsStringAsync ().ConfigureAwait (false);

                HttpClient client2 = new HttpClient();
                //ContactNoOTPForLogin contactNoOTPForLoginForUpdate = new ContactNoOTPForLogin();

                //contactNoOTPForLoginForUpdate.No = contactNoOTPForLogin.No;
                //contactNoOTPForLoginForUpdate.PCPL_OTP = generatedOTP;
                //contactNoOTPForLoginForUpdate.Phone_No_2 = contactNoOTPForLogin.Phone_No_2;
                //contactNoOTPForLoginForUpdate.First_Name = contactNoOTPForLogin.First_Name;
                //contactNoOTPForLoginForUpdate.Last_Name = contactNoOTPForLogin.Last_Name;
                //contactNoOTPForLoginForUpdate.Company_E_Mail = contactNoOTPForLogin.Company_E_Mail;
                //contactNoOTPForLoginForUpdate.Job_Title = contactNoOTPForLogin.Job_Title;
                //contactNoOTPForLoginForUpdate.Mobile_Phone_No = contactNoOTPForLogin.Mobile_Phone_No;
                //contactNoOTPForLoginForUpdate.Salespers_Purch_Code = contactNoOTPForLogin.Salespers_Purch_Code;

                ContactNoOTPForLoginUpdate contactNoOTPForLoginForUpdate = new ContactNoOTPForLoginUpdate();

                contactNoOTPForLoginForUpdate.PCPL_OTP = generatedOTP;

                string apiUrl1 = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "Salesperson/";

                apiUrl1 = apiUrl1 + "UpdateOTPForLogin?SPNo=" + contactNoOTPForLogin.No;

                client2.BaseAddress = new Uri(apiUrl1);
                client2.DefaultRequestHeaders.Accept.Clear();
                client2.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                string ObjString = JsonConvert.SerializeObject(contactNoOTPForLoginForUpdate);
                var content = new StringContent(ObjString, Encoding.UTF8, "application/json");

                HttpRequestMessage request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(apiUrl1),
                    Content = content
                };

                HttpResponseMessage response2 = await client2.SendAsync(request);
                if (response2.IsSuccessStatusCode)
                {
                    var data = await response2.Content.ReadAsStringAsync();
                    contactNoOTPForLoginRes = Newtonsoft.Json.JsonConvert.DeserializeObject<ContactNoOTPForLogin>(data);
                }
            }

            return contactNoOTPForLoginRes;
        }

        //OTP functionality on Login Start
        public async Task<string> CheckLogin(string SPNo, string OTP, string ContactNo)
        {
            try
            {
                string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "Salesperson/";

                apiUrl = apiUrl + "GetByNoOTP?SPNo=" + Uri.EscapeDataString(SPNo ?? "") + "&OTP=" + Uri.EscapeDataString(OTP ?? "");

                HttpClient client = new HttpClient();

                LoggedInUserProfile loggedInUserProfile = new LoggedInUserProfile();
                string loggedInUser = "";

                client.BaseAddress = new Uri(apiUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await client.GetAsync(apiUrl);
                var jsonData = "";

                if (response.IsSuccessStatusCode)
                {
                    jsonData = await response.Content.ReadAsStringAsync();
                    loggedInUserProfile = Newtonsoft.Json.JsonConvert.DeserializeObject<LoggedInUserProfile>(jsonData);
                }
                else
                {
                    await LogLoginActivity("LOGIN_OTP_FAIL", "Login Failed - OTP validation API failed", "System");
                    await LogLoginSiteError("LOGIN_OTP_FAIL", "OTP validation API failed", ((int)response.StatusCode).ToString(), "GetByNoOTP returned non-success status");
                }

                if (loggedInUserProfile.First_Name != null)
                {
                    loggedInUser = loggedInUserProfile.First_Name + ' ' + loggedInUserProfile.Last_Name;
                    Session["loggedInUserNo"] = loggedInUserProfile.No;
                    Session["loggedInUserFName"] = loggedInUserProfile.First_Name;
                    Session["loggedInUserLName"] = loggedInUserProfile.Last_Name;
                    Session["loggedInUserEmail"] = loggedInUserProfile.Company_E_Mail;
                    Session["loggedInUserJobTitle"] = loggedInUserProfile.Job_Title;
                    Session["loggedInUserRole"] = loggedInUserProfile.Role;
                    Session["loggedInUserMobile"] = loggedInUserProfile.Mobile_Phone_No;
                    Session["loggedInUserSPCode"] = loggedInUserProfile.Salespers_Purch_Code;
                    Session["loggedinUserBranch"] = loggedInUserProfile.Global_Dimension_1_Code ?? "";
                    RegisterCurrentSessionToken(loggedInUserProfile.No);

                    SPUpdateOTP contactNoOTPForLoginRes = new SPUpdateOTP();

                    Task<SPUpdateOTP> task = Task.Run<SPUpdateOTP>(async () => await UpdateOTPBlank(SPNo, ContactNo));
                    contactNoOTPForLoginRes = task.Result;

                    string SPCodesOfReportingPersonUser = "";
                    Task<string> task1 = Task.Run<string>(async () => await GetSPCodesOfReportingPersonUser(contactNoOTPForLoginRes.No));
                    SPCodesOfReportingPersonUser = task1.Result;

                    if (SPCodesOfReportingPersonUser != "")
                        Session["SPCodesOfReportingPersonUser"] = SPCodesOfReportingPersonUser;
                    else
                        Session["SPCodesOfReportingPersonUser"] = "";

                    await LogLoginActivity("LOGIN_SUCCESS", "Login Success", loggedInUserProfile.Salespers_Purch_Code);
                }
                else
                {
                    await LogLoginActivity("LOGIN_OTP_FAIL", "Login Failed - Invalid OTP", "System");
                    await LogLoginSiteError("LOGIN_OTP_FAIL", "Invalid OTP during login", "401", "OTP did not match or user profile was not returned");
                }

                return loggedInUser;
            }
            catch (Exception ex)
            {
                await LogLoginActivity("LOGIN_OTP_EXCEPTION", "Login Failed - OTP Exception", "System");
                await LogLoginSiteError("LOGIN_OTP_EXCEPTION", "Exception in CheckLogin", "500", ex.Message);
                return "";
            }
        }

        public async Task<SPUpdateOTP> UpdateOTPBlank(string SPNo, string ContactNo)
        {

            HttpClient client1 = new HttpClient();

            SPUpdateOTP contactNoOTPForLoginForUpdate = new SPUpdateOTP();
            SPUpdateOTP contactNoOTPForLoginRes = new SPUpdateOTP();
            
            string apiUrl1 = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "Salesperson/";

            contactNoOTPForLoginForUpdate.No = SPNo;
            contactNoOTPForLoginForUpdate.PCPL_OTP = "";
            contactNoOTPForLoginForUpdate.Phone_No_2 = ContactNo;

            apiUrl1 = apiUrl1 + "UpdateOTPBlank?SPNo=" + Uri.EscapeDataString(contactNoOTPForLoginForUpdate.No ?? "");

            client1.BaseAddress = new Uri(apiUrl1);
            client1.DefaultRequestHeaders.Accept.Clear();
            client1.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            string ObjString = JsonConvert.SerializeObject(contactNoOTPForLoginForUpdate);
            var content = new StringContent(ObjString, Encoding.UTF8, "application/json");

            HttpRequestMessage request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(apiUrl1),
                Content = content
            };

            HttpResponseMessage response1 = await client1.SendAsync(request);
            if (response1.IsSuccessStatusCode)
            {
                var data = await response1.Content.ReadAsStringAsync();
                contactNoOTPForLoginRes = Newtonsoft.Json.JsonConvert.DeserializeObject<SPUpdateOTP>(data);
            }

            return contactNoOTPForLoginRes;
        }
        //OTP functionality on Login End

        public async Task<bool> ResendOTP(string email, string pass, string adminContactNo)
        {
            bool flag = false;

            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "Salesperson/";

            apiUrl = apiUrl + "CheckEmailPassForOTP?email=" + Uri.EscapeDataString(email ?? "") + "&pass=" + Uri.EscapeDataString(pass ?? "");

            HttpClient client = new HttpClient();

            ContactNoOTPForLogin contactNoOTPForLogin = new ContactNoOTPForLogin();
            ContactNoOTPForLogin contactNoOTPForLoginRes = new ContactNoOTPForLogin();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            var jsonData = "";
            if (response.IsSuccessStatusCode)
            {
                jsonData = await response.Content.ReadAsStringAsync();
                contactNoOTPForLogin = Newtonsoft.Json.JsonConvert.DeserializeObject<ContactNoOTPForLogin>(jsonData);

                if (contactNoOTPForLogin.No != null)
                {
                    if(contactNoOTPForLogin.PCPL_Enable_OTP_On_Login)
                    {
                        Task<ContactNoOTPForLogin> task = Task.Run<ContactNoOTPForLogin>(async () => await GenerateOTPAndSend(contactNoOTPForLogin, adminContactNo));
                        contactNoOTPForLoginRes = task.Result;
                    }
                    else
                    {
                        contactNoOTPForLoginRes = contactNoOTPForLogin;
                        Session["loggedInUserNo"] = contactNoOTPForLogin.No;
                        Session["loggedInUserFName"] = contactNoOTPForLogin.First_Name;
                        Session["loggedInUserLName"] = contactNoOTPForLogin.Last_Name;
                        Session["loggedInUserEmail"] = contactNoOTPForLogin.Company_E_Mail;
                        Session["loggedInUserJobTitle"] = contactNoOTPForLogin.Job_Title;
                        Session["loggedInUserRole"] = contactNoOTPForLogin.Role;
                        Session["loggedInUserMobile"] = contactNoOTPForLogin.Mobile_Phone_No;
                        Session["loggedInUserSPCode"] = contactNoOTPForLogin.Salespers_Purch_Code;
                        RegisterCurrentSessionToken(contactNoOTPForLogin.No);

                        string SPCodesOfReportingPersonUser = "";
                        Task<string> task1 = Task.Run<string>(async () => await GetSPCodesOfReportingPersonUser(contactNoOTPForLoginRes.No));
                        SPCodesOfReportingPersonUser = task1.Result;

                        if (SPCodesOfReportingPersonUser != "")
                            Session["SPCodesOfReportingPersonUser"] = SPCodesOfReportingPersonUser;
                        else
                            Session["SPCodesOfReportingPersonUser"] = "";

                    }

                }

            }
            
            flag = true;

            return flag;
        }

        public ActionResult ChangePassword()
        {
            return View();
        }

        public ActionResult Logout()
        {
            InvalidateCurrentSessionToken();
            Session["loggedInUserNo"] = "";
            Session["loggedInUserFName"] = "";
            Session["loggedInUserLName"] = "";
            Session["loggedInUserEmail"] = "";
            Session["loggedinUserBranch"] = "";
            Session["loggedInUserJobTitle"] = "";
            Session["loggedInUserRole"] = "";
            Session["loggedInUserMobile"] = "";
            Session["SPProfileImage"] = null;
            Session["SPCodesOfReportingPersonUser"] = "";
            Session["AuthToken"] = "";

            return RedirectToAction("Login", "Account");
        }

        public ActionResult ResetForgotPassword()
        {
            return View();
        }

        public ActionResult ForgotPassword()
        {
            return View();
        }

        private void RegisterCurrentSessionToken(string userNo)
        {
            if (string.IsNullOrWhiteSpace(userNo) || Session == null)
                return;

            var token = Session.SessionID;
            Session["AuthToken"] = token;
            UserTokenStore.RegisterToken(userNo, token);

            Response.Cookies.Add(new HttpCookie("authToken", token)
            {
                HttpOnly = false,
                Secure = Request != null && Request.IsSecureConnection,
                Path = "/"
            });
        }

        private void InvalidateCurrentSessionToken()
        {
            var userNo = Session?["loggedInUserNo"]?.ToString();
            var token = Session?["AuthToken"]?.ToString();
            if (string.IsNullOrWhiteSpace(token))
                token = Session?.SessionID;

            UserTokenStore.InvalidateToken(userNo, token);

            Response.Cookies.Add(new HttpCookie("authToken", "")
            {
                Expires = DateTime.UtcNow.AddDays(-1),
                Path = "/"
            });
        }
    }
}
