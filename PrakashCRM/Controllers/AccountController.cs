using Newtonsoft.Json;
using PrakashCRM.Data.Models;
using PrakashCRM.Service.Classes;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
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
        private const string TrustedDeviceCookieName = "trustedDevice";

        private string GetDashboardRedirectUrl()
        {
            return Url.Action("Index", "SPDashboard") ?? "/SPDashboard/Index";
        }

        private string PeekPostLoginRedirectUrl(string returnUrl = null)
        {
            string redirectUrl = LoginRedirectHelper.NormalizeReturnUrl(returnUrl);

            if (!string.IsNullOrWhiteSpace(redirectUrl))
            {
                Session[LoginRedirectHelper.PostLoginRedirectSessionKey] = redirectUrl;
                return redirectUrl;
            }

            redirectUrl = LoginRedirectHelper.NormalizeReturnUrl(Session[LoginRedirectHelper.PostLoginRedirectSessionKey] as string);
            if (!string.IsNullOrWhiteSpace(redirectUrl))
            {
                Session[LoginRedirectHelper.PostLoginRedirectSessionKey] = redirectUrl;
                return redirectUrl;
            }

            return GetDashboardRedirectUrl();
        }

        private string ConsumePostLoginRedirectUrl()
        {
            string redirectUrl = PeekPostLoginRedirectUrl();
            Session.Remove(LoginRedirectHelper.PostLoginRedirectSessionKey);
            return redirectUrl;
        }

        private bool IsAuthenticatedSessionActive()
        {
            if (Session == null)
                return false;

            string firstName = Session["loggedInUserFName"] == null ? string.Empty : Session["loggedInUserFName"].ToString();
            if (string.IsNullOrWhiteSpace(firstName))
                return false;

            string userNo = Session["loggedInUserNo"] == null ? string.Empty : Session["loggedInUserNo"].ToString();
            string token = Session["AuthToken"] == null ? string.Empty : Session["AuthToken"].ToString();
            if (string.IsNullOrWhiteSpace(token))
                token = Session.SessionID;

            if (UserTokenStore.IsTokenActive(userNo, token))
                return true;

            Session.Remove(LoginRedirectHelper.PostLoginRedirectSessionKey);
            Session.Clear();
            Session.Abandon();
            return false;
        }

        private void LogLoginRequestResponseToFile(string actionName, object requestPayload, string responseBody, HttpStatusCode? statusCode = null, string requestUrl = null)
        {
            try
            {
                string logDirectory = "";

                if (System.Web.HttpContext.Current != null)
                {
                    if (System.Web.HttpContext.Current.Server != null)
                        logDirectory = System.Web.HttpContext.Current.Server.MapPath("~/App_Data/LoginLogs");
                }

                if (string.IsNullOrWhiteSpace(logDirectory))
                {
                    string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    logDirectory = Path.Combine(baseDirectory, "App_Data", "LoginLogs");
                }

                if (!Directory.Exists(logDirectory))
                    Directory.CreateDirectory(logDirectory);

                string filePath = Path.Combine(logDirectory, "LoginLog_" + DateTime.Now.ToString("yyyyMMdd") + ".txt");
                string requestJson = requestPayload != null ? JsonConvert.SerializeObject(requestPayload) : "";
                string logText = "DateTime: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + Environment.NewLine + "Action: " + (actionName ?? "") + Environment.NewLine + "RequestUrl: " + (requestUrl ?? "") + Environment.NewLine + "RequestPayload: " + requestJson + Environment.NewLine + "StatusCode: " + (statusCode.HasValue ? ((int)statusCode.Value).ToString() : "") + Environment.NewLine + "ResponseBody: " + (responseBody ?? "") + Environment.NewLine + new string('-', 120) + Environment.NewLine;

                System.IO.File.AppendAllText(filePath, logText);
            }
            catch
            {
            }
        }

        private object BuildLoginRequestLogPayload(string email, string pass, string adminContactNo)
        {
            return new
            {
                email = email,
                pass = string.IsNullOrWhiteSpace(pass) ? "" : "****",
                adminContactNo = adminContactNo
            };
        }

        private int GetTrustedDeviceExpiryDays()
        {
            int expiryDays;
            return int.TryParse(ConfigurationManager.AppSettings["TrustedDeviceExpiryDays"], out expiryDays) && expiryDays > 0 ? expiryDays : 1;
        }

        private async Task CompleteLoginForOtpProfile(ContactNoOTPForLogin user)
        {
            if (user == null)
                return;

            await CompleteLoginSession(user.No,user.First_Name,user.Last_Name,user.Company_E_Mail,user.Job_Title,user.Role,user.Mobile_Phone_No,user.Salespers_Purch_Code,user.Global_Dimension_1_Code);
        }

        private async Task CompleteLoginForProfile(LoggedInUserProfile user)
        {
            if (user == null)
                return;

            await CompleteLoginSession(user.No,user.First_Name,user.Last_Name,user.Company_E_Mail,user.Job_Title,user.Role,user.Mobile_Phone_No,user.Salespers_Purch_Code,user.Global_Dimension_1_Code);
        }

        private async Task CompleteLoginSession(string userNo, string firstName, string lastName, string email, string jobTitle, string role, string mobilePhoneNo, string spCode, string branchCode)
        {
            Session["loggedInUserNo"] = userNo;
            Session["loggedInUserFName"] = firstName;
            Session["loggedInUserLName"] = lastName;
            Session["loggedInUserEmail"] = email;
            Session["loggedInUserJobTitle"] = jobTitle;
            Session["loggedInUserRole"] = role;
            Session["loggedInUserMobile"] = mobilePhoneNo;
            Session["loggedInUserSPCode"] = spCode;
            Session["loggedinUserBranch"] = branchCode ?? "";
            RegisterCurrentSessionToken(userNo);

            string reportingSpCodes = await GetSPCodesOfReportingPersonUser(userNo);
            Session["SPCodesOfReportingPersonUser"] = string.IsNullOrWhiteSpace(reportingSpCodes) ? "" : reportingSpCodes;

            RoleWiseMenuRightsResponse menu = await GetRolewiseMenuRight(userNo);
            Session["RoleWiseMenuData"] = menu;
        }

        //  login with OTP and then trusted device functionality will work for that device until the expiry of trusted device token or user clears cookies or logs out from that device or tries to login from another device or private mode
        private TrustedDeviceValidationResult ValidateTrustedDevice(string userNo, string storageToken, string deviceFingerprint, bool isPrivateMode)
        {
            TrustedDeviceValidationResult invalidResult = new TrustedDeviceValidationResult
            {
                IsValid = false,
                ShouldClearClientState = !string.IsNullOrWhiteSpace(storageToken)
            };

            try
            {
                if (isPrivateMode)
                {
                    ExpireTrustedDeviceCookie();
                    invalidResult.ShouldClearClientState = true;
                    return invalidResult;
                }

                HttpCookie trustedCookie = Request != null ? Request.Cookies[TrustedDeviceCookieName] : null;
                string cookieToken = trustedCookie != null ? trustedCookie.Value : string.Empty;
                    // login OTP validation for the old value.
                TrustedDeviceValidationResult validation = TrustedDeviceStore.ValidateAndRenewToken(userNo,cookieToken,storageToken,deviceFingerprint,GetTrustedDeviceExpiryDays());
                if (validation.IsValid && validation.Record != null)
                    WriteTrustedDeviceCookie(validation.Record.Token, validation.Record.ExpiryUtc);
                else if (validation.ShouldClearClientState)
                    ExpireTrustedDeviceCookie();

                return validation;
            }
            catch
            {
                ExpireTrustedDeviceCookie();
                invalidResult.ShouldClearClientState = true;
                return invalidResult;
            }
        }
        private void ApplyTrustedDevice(LoginOtpVerificationResult responseModel, string userNo, string deviceFingerprint, bool isPrivateMode)
        {
            if (responseModel == null)
                return;

            if (isPrivateMode || string.IsNullOrWhiteSpace(userNo) || string.IsNullOrWhiteSpace(deviceFingerprint))
            {
                responseModel.ClearTrustedDeviceState = true;
                ExpireTrustedDeviceCookie();
                return;
            }

            try
            {
                TrustedDeviceRecord record = TrustedDeviceStore.IssueToken(userNo, deviceFingerprint, GetTrustedDeviceExpiryDays());
                if (record == null)
                    return;

                WriteTrustedDeviceCookie(record.Token, record.ExpiryUtc);
                responseModel.TrustedDeviceToken = record.Token;
                responseModel.TrustedDeviceExpiresUtc = record.ExpiryUtc.ToString("o");
                responseModel.ClearTrustedDeviceState = false;
            }
            catch
            {
                responseModel.ClearTrustedDeviceState = true;
                ExpireTrustedDeviceCookie();
            }
        }
        // DeviceCookies will be cleared when user logs out, tries to login from another device or private mode or when the trusted device token expires or when user clears cookies from browser
        private void WriteTrustedDeviceCookie(string token, DateTime expiryUtc)
        {
            if (string.IsNullOrWhiteSpace(token) || Response == null)
                return;

            Response.Cookies.Add(new HttpCookie(TrustedDeviceCookieName, token)
            {
                HttpOnly = true,
                Secure = Request != null && Request.IsSecureConnection,
                Expires = expiryUtc,
                Path = "/"
            });
        }

        private void ExpireTrustedDeviceCookie()
        {
            if (Response == null)
                return;

            Response.Cookies.Add(new HttpCookie(TrustedDeviceCookieName, "")
            {
                HttpOnly = true,
                Secure = Request != null && Request.IsSecureConnection,
                Expires = DateTime.UtcNow.AddDays(-1),
                Path = "/"
            });
        }

        private async Task LogLoginActivity(string traceId, string description, string spCode)
        {
            try
            {
                string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPSiteActivity/LogActivity";

                SPSiteActivity activity = new SPSiteActivity
                {
                    Activity_User_Name = ResolveLoggedInUserName(),
                    Activity_Date = DateTime.Now.ToString("dd-MM-yyyy"),
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

        private string ResolveLoggedInUserName()
        {
            string firstName = Session["loggedInUserFName"] == null ? string.Empty : Session["loggedInUserFName"].ToString().Trim();
            string lastName = Session["loggedInUserLName"] == null ? string.Empty : Session["loggedInUserLName"].ToString().Trim();
            string fullName = string.Join(" ", new[] { firstName, lastName }.Where(value => !string.IsNullOrWhiteSpace(value)));

            if (!string.IsNullOrWhiteSpace(fullName))
                return fullName;

            if (Session["loggedInUserEmail"] != null && !string.IsNullOrWhiteSpace(Session["loggedInUserEmail"].ToString()))
                return Session["loggedInUserEmail"].ToString().Trim();

            if (Session["loggedInUserSPCode"] != null && !string.IsNullOrWhiteSpace(Session["loggedInUserSPCode"].ToString()))
                return Session["loggedInUserSPCode"].ToString().Trim();

            if (Session["loggedInUserNo"] != null && !string.IsNullOrWhiteSpace(Session["loggedInUserNo"].ToString()))
                return Session["loggedInUserNo"].ToString().Trim();

            return "System";
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

        public ActionResult Login(string returnUrl = null)
        {
            string redirectUrl = PeekPostLoginRedirectUrl(returnUrl);
            if (IsAuthenticatedSessionActive())
                return Redirect(redirectUrl);

            ViewBag.PostLoginRedirectUrl = redirectUrl;
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> CheckLoginAndSendOTP(string email, string pass, string adminContactNo, string trustedDeviceToken, string deviceFingerprint, bool isPrivateMode = false)
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
                    LogLoginRequestResponseToFile("CheckLoginAndSendOTP", BuildLoginRequestLogPayload(email, pass, adminContactNo), jsonData, response.StatusCode, apiUrl);
                    contactNoOTPForLogin = Newtonsoft.Json.JsonConvert.DeserializeObject<ContactNoOTPForLogin>(jsonData);

                    if (contactNoOTPForLogin.No != null)
                    {
                        if (contactNoOTPForLogin.PCPL_Enable_OTP_On_Login)
                        {
                            TrustedDeviceValidationResult trustedDeviceValidation = ValidateTrustedDevice(contactNoOTPForLogin.No, trustedDeviceToken, deviceFingerprint, isPrivateMode);
                            if (trustedDeviceValidation.IsValid)
                            {
                                await CompleteLoginForOtpProfile(contactNoOTPForLogin);
                                contactNoOTPForLogin.RequireOtpVerification = false;
                                contactNoOTPForLogin.TrustedDeviceRecognized = true;
                                contactNoOTPForLogin.TrustedDeviceToken = trustedDeviceValidation.Record.Token;
                                contactNoOTPForLogin.TrustedDeviceExpiresUtc = trustedDeviceValidation.Record.ExpiryUtc.ToString("o");
                                contactNoOTPForLogin.RedirectUrl = ConsumePostLoginRedirectUrl();
                                contactNoOTPForLoginRes = contactNoOTPForLogin;
                                await LogLoginActivity("LOGIN_TRUSTED_DEVICE", "Login Success - Trusted Device", contactNoOTPForLogin.Salespers_Purch_Code);
                            }
                            else
                            {
                                Task<ContactNoOTPForLogin> task = Task.Run<ContactNoOTPForLogin>(async () => await GenerateOTPAndSend(contactNoOTPForLogin, adminContactNo));
                                contactNoOTPForLoginRes = task.Result;
                                contactNoOTPForLoginRes.RequireOtpVerification = true;
                                contactNoOTPForLoginRes.TrustedDeviceRecognized = false;
                                contactNoOTPForLoginRes.ClearTrustedDeviceState = trustedDeviceValidation.ShouldClearClientState;
                                contactNoOTPForLoginRes.RedirectUrl = PeekPostLoginRedirectUrl();
                                await LogLoginActivity("LOGIN_OTP", "Login OTP Sent", contactNoOTPForLogin.Salespers_Purch_Code);
                            }
                        }
                        else
                        {
                            await CompleteLoginForOtpProfile(contactNoOTPForLogin);
                            contactNoOTPForLogin.RequireOtpVerification = false;
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
                            contactNoOTPForLogin.RedirectUrl = ConsumePostLoginRedirectUrl();
                            contactNoOTPForLoginRes = contactNoOTPForLogin;
                            await LogLoginActivity("LOGIN_SUCCESS", "Login Success", contactNoOTPForLogin.Salespers_Purch_Code);
                        }
                    }
                }
                else
                {
                    jsonData = await response.Content.ReadAsStringAsync();
                    LogLoginRequestResponseToFile("CheckLoginAndSendOTP", BuildLoginRequestLogPayload(email, pass, adminContactNo), jsonData, response.StatusCode, apiUrl);
                    await LogLoginActivity("LOGIN_FAIL", "Login Failed", "System");
                    await LogLoginSiteError("LOGIN_FAIL", "Login API failed", ((int)response.StatusCode).ToString(), "CheckEmailPassForOTP returned non-success status");
                }

                if (contactNoOTPForLogin == null || string.IsNullOrWhiteSpace(contactNoOTPForLogin.No))
                {
                    LogLoginRequestResponseToFile("CheckLoginAndSendOTP", BuildLoginRequestLogPayload(email, pass, adminContactNo), JsonConvert.SerializeObject(contactNoOTPForLoginRes), response.StatusCode, apiUrl);
                    await LogLoginActivity("LOGIN_FAIL", "Login Failed", "System");
                    await LogLoginSiteError("LOGIN_FAIL", "Invalid login credentials", "401", "Login failed due to invalid credentials or user not found");
                }

                return Json(contactNoOTPForLoginRes, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogLoginRequestResponseToFile("CheckLoginAndSendOTP", BuildLoginRequestLogPayload(email, pass, adminContactNo), ex.ToString(), HttpStatusCode.InternalServerError, Request != null ? Request.RawUrl : "");
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

            bool smsDelivered = false;
            bool emailDelivered = false;

            string smsApiUrl = ConfigurationManager.AppSettings["SMSApiUrl"].ToString();
            string smsFromMobile = ConfigurationManager.AppSettings["SMSFromMobile"].ToString();
            string smsFromPass = ConfigurationManager.AppSettings["SMSFromPass"].ToString();
            string smsFromSenderId = ConfigurationManager.AppSettings["SMSFromSenderId"].ToString();
            string smsToMobile = contactNoOTPForLogin.Phone_No_2;

            if (!string.IsNullOrWhiteSpace(smsToMobile))
            {
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
                smsDelivered = response1.StatusCode == HttpStatusCode.OK;
            }

            //  emailDelivered = SendOtpEmail(contactNoOTPForLogin, generatedOTP, adminContactNo);   //SendOtpEmail funcanality

            if (smsDelivered || emailDelivered)
            {
                HttpClient client2 = new HttpClient();
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
                    contactNoOTPForLoginRes = Newtonsoft.Json.JsonConvert.DeserializeObject<ContactNoOTPForLogin>(data) ?? new ContactNoOTPForLogin();
                }

                contactNoOTPForLoginRes.No = contactNoOTPForLoginRes.No ?? contactNoOTPForLogin.No;
                contactNoOTPForLoginRes.Company_E_Mail = contactNoOTPForLogin.Company_E_Mail;
                contactNoOTPForLoginRes.Phone_No_2 = contactNoOTPForLogin.Phone_No_2;
                contactNoOTPForLoginRes.OTPEmailSent = emailDelivered;
            }

            return contactNoOTPForLoginRes;
        }

        //OTP functionality on Login Start
        [HttpPost]
        public async Task<JsonResult> CheckLogin(string SPNo, string OTP, string ContactNo, string deviceFingerprint, bool isPrivateMode = false)
        {
            LoginOtpVerificationResult result = new LoginOtpVerificationResult();

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
                    LogLoginRequestResponseToFile("CheckLogin", new { SPNo = SPNo, OTP = string.IsNullOrWhiteSpace(OTP) ? "" : "****", ContactNo = ContactNo }, jsonData, response.StatusCode, apiUrl);
                    loggedInUserProfile = Newtonsoft.Json.JsonConvert.DeserializeObject<LoggedInUserProfile>(jsonData);
                }
                else
                {
                    jsonData = await response.Content.ReadAsStringAsync();
                    LogLoginRequestResponseToFile("CheckLogin", new { SPNo = SPNo, OTP = string.IsNullOrWhiteSpace(OTP) ? "" : "****", ContactNo = ContactNo }, jsonData, response.StatusCode, apiUrl);
                    await LogLoginActivity("LOGIN_OTP_FAIL", "Login Failed - OTP validation API failed", "System");
                    await LogLoginSiteError("LOGIN_OTP_FAIL", "OTP validation API failed", ((int)response.StatusCode).ToString(), "GetByNoOTP returned non-success status");
                }

                if (loggedInUserProfile.First_Name != null)
                {
                    result.IsSuccess = true;
                    result.LoggedInUser = loggedInUserProfile.First_Name + ' ' + loggedInUserProfile.Last_Name;
                    await CompleteLoginForProfile(loggedInUserProfile);
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
                    ApplyTrustedDevice(result, loggedInUserProfile.No, deviceFingerprint, isPrivateMode);
                    string SPCodesOfReportingPersonUser = "";
                    Task<string> task1 = Task.Run<string>(async () => await GetSPCodesOfReportingPersonUser(contactNoOTPForLoginRes.No));
                    SPCodesOfReportingPersonUser = task1.Result;

                    if (SPCodesOfReportingPersonUser != "")
                        Session["SPCodesOfReportingPersonUser"] = SPCodesOfReportingPersonUser;
                    else
                        Session["SPCodesOfReportingPersonUser"] = "";
                    // otp boolian enable hone par  ye right update hota hai 
                    RoleWiseMenuRightsResponse menu = await GetRolewiseMenuRight(loggedInUserProfile.No);
                    Session["RoleWiseMenuData"] = menu;
                    result.RedirectUrl = ConsumePostLoginRedirectUrl();

                    await LogLoginActivity("LOGIN_SUCCESS", "Login Success", loggedInUserProfile.Salespers_Purch_Code);
                }
                else
                {
                    LogLoginRequestResponseToFile("CheckLogin", new { SPNo = SPNo, OTP = string.IsNullOrWhiteSpace(OTP) ? "" : "****", ContactNo = ContactNo }, JsonConvert.SerializeObject(loggedInUserProfile), response.StatusCode, apiUrl);
                    await LogLoginActivity("LOGIN_OTP_FAIL", "Login Failed - Invalid OTP", "System");
                    await LogLoginSiteError("LOGIN_OTP_FAIL", "Invalid OTP during login", "401", "OTP did not match or user profile was not returned");
                }

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogLoginRequestResponseToFile("CheckLogin", new { SPNo = SPNo, OTP = string.IsNullOrWhiteSpace(OTP) ? "" : "****", ContactNo = ContactNo }, ex.ToString(), HttpStatusCode.InternalServerError, Request != null ? Request.RawUrl : "");
                await LogLoginActivity("LOGIN_OTP_EXCEPTION", "Login Failed - OTP Exception", "System");
                await LogLoginSiteError("LOGIN_OTP_EXCEPTION", "Exception in CheckLogin", "500", ex.Message);
                return Json(result, JsonRequestBehavior.AllowGet);
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
                        await CompleteLoginForOtpProfile(contactNoOTPForLogin);
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
            Session.Remove(LoginRedirectHelper.PostLoginRedirectSessionKey);

            return RedirectToAction("Login", "Account");
        }

        public ActionResult ResetForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> RequestPasswordReset(string email)
        {
            PasswordResetResult result = new PasswordResetResult
            {
                Success = false,
                Status = "Invalid",
                Message = "Please Enter Email ID Of Registered User."
            };

            if (string.IsNullOrWhiteSpace(email))
            {
                result.Message = "Please Enter Email ID.";
                return Json(result);
            }

            string baseApiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString();
            string portalUrl = ConfigurationManager.AppSettings["SPPortalUrl"].ToString();

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                string getUserUrl = baseApiUrl + "Salesperson/GetByEmail?email=" + Uri.EscapeDataString(email.Trim());
                HttpResponseMessage getUserResponse = await client.GetAsync(getUserUrl);
                if (!getUserResponse.IsSuccessStatusCode)
                {
                    result.Message = "Unable to process password reset right now.";
                    return Json(result);
                }

                string getUserPayload = await getUserResponse.Content.ReadAsStringAsync();
                UserCustVendor user = JsonConvert.DeserializeObject<UserCustVendor>(getUserPayload);
                if (user == null || string.IsNullOrWhiteSpace(user.No) || string.IsNullOrWhiteSpace(user.Company_E_Mail))
                    return Json(result);

                string forgotPasswordUrl = baseApiUrl
                    + "Salesperson/ForgotPassword?email=" + Uri.EscapeDataString(user.Company_E_Mail)
                    + "&userNo=" + Uri.EscapeDataString(user.No)
                    + "&role=" + Uri.EscapeDataString(user.Role ?? string.Empty)
                    + "&portalUrl=" + Uri.EscapeDataString(portalUrl);

                HttpResponseMessage forgotPasswordResponse = await client.GetAsync(forgotPasswordUrl);
                if (!forgotPasswordResponse.IsSuccessStatusCode)
                {
                    result.Message = "Unable to process password reset right now.";
                    return Json(result);
                }

                string forgotPasswordPayload = await forgotPasswordResponse.Content.ReadAsStringAsync();
                PasswordResetResult forgotPasswordResult = JsonConvert.DeserializeObject<PasswordResetResult>(forgotPasswordPayload);

                return Json(forgotPasswordResult ?? result);
            }
        }

        [HttpGet]
        public async Task<JsonResult> ValidatePasswordResetToken(string token)
        {
            PasswordResetValidationResult invalidResult = new PasswordResetValidationResult
            {
                IsValid = false,
                Status = "Invalid",
                Message = "Invalid link"
            };

            if (string.IsNullOrWhiteSpace(token))
                return Json(invalidResult, JsonRequestBehavior.AllowGet);

            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString()
                + "Salesperson/ValidateResetToken?token=" + Uri.EscapeDataString(token.Trim());

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await client.GetAsync(apiUrl);
                if (!response.IsSuccessStatusCode)
                    return Json(invalidResult, JsonRequestBehavior.AllowGet);

                string payload = await response.Content.ReadAsStringAsync();
                PasswordResetValidationResult validationResult = JsonConvert.DeserializeObject<PasswordResetValidationResult>(payload);

                return Json(validationResult ?? invalidResult, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public async Task<JsonResult> CompletePasswordReset(string token, string newPassword)
        {
            PasswordResetResult invalidResult = new PasswordResetResult
            {
                Success = false,
                Status = "Invalid",
                Message = "Invalid link"
            };

            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(newPassword))
                return Json(invalidResult);

            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString()
                + "Salesperson/ResetForgotPassword?token=" + Uri.EscapeDataString(token.Trim())
                + "&newPassword=" + Uri.EscapeDataString(newPassword);

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await client.PostAsync(apiUrl, null);
                if (!response.IsSuccessStatusCode)
                    return Json(invalidResult);

                string payload = await response.Content.ReadAsStringAsync();
                PasswordResetResult resetResult = JsonConvert.DeserializeObject<PasswordResetResult>(payload) ?? invalidResult;

                if (resetResult.Success && !string.IsNullOrWhiteSpace(resetResult.UserNo))
                {
                    UserTokenStore.InvalidateAllTokensForUsers(new[] { resetResult.UserNo });
                    TrustedDeviceStore.InvalidateAllDevicesForUser(resetResult.UserNo);
                }

                return Json(resetResult);
            }
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
        //SendOtpEmail funcanality
        //private bool SendOtpEmail(ContactNoOTPForLogin contactNoOTPForLogin, string generatedOTP, string adminContactNo)
        //{
        //    try
        //    {
        //        if (contactNoOTPForLogin == null || string.IsNullOrWhiteSpace(contactNoOTPForLogin.Company_E_Mail))
        //            return false;

        //        string serviceApiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"];
        //        if (string.IsNullOrWhiteSpace(serviceApiUrl))
        //            return false;

        //        string apiUrl = serviceApiUrl.TrimEnd('/') + "/Salesperson/SendOtpEmail" + "?email=" + Uri.EscapeDataString(contactNoOTPForLogin.Company_E_Mail ?? "") + "&firstName=" + Uri.EscapeDataString(contactNoOTPForLogin.First_Name ?? "") + "&lastName=" + Uri.EscapeDataString(contactNoOTPForLogin.Last_Name ?? "") + "&generatedOtp=" + Uri.EscapeDataString(generatedOTP ?? "") + "&adminContactNo=" + Uri.EscapeDataString(adminContactNo ?? "");

        //        using (HttpClient client = new HttpClient())
        //        {
        //            client.DefaultRequestHeaders.Accept.Clear();
        //            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        //            HttpResponseMessage response = client.GetAsync(apiUrl).GetAwaiter().GetResult();
        //            if (!response.IsSuccessStatusCode)
        //                return false;

        //            string payload = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        //            bool emailSent;
        //            return bool.TryParse(payload, out emailSent) && emailSent;
        //        }
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}

    }
}
