using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PrakashCRM.Data.Models;
using PrakashCRM.Service.Classes;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace PrakashCRM.Service.Controllers
{
    [RoutePrefix("api/Salesperson")]
    public class SalespersonController : ApiController
    {
        private static readonly TimeSpan DefaultResetTokenLifetime = TimeSpan.FromMinutes(15);

        private static LoggedInUserProfile MapLoggedInUserProfile(SPProfile user)
        {
            LoggedInUserProfile loggedInUserProfile = new LoggedInUserProfile();

            if (user == null)
                return loggedInUserProfile;

            loggedInUserProfile.No = user.No;
            loggedInUserProfile.First_Name = user.First_Name;
            loggedInUserProfile.Last_Name = user.Last_Name;
            loggedInUserProfile.Company_E_Mail = user.Company_E_Mail;
            loggedInUserProfile.Mobile_Phone_No = user.Mobile_Phone_No;
            loggedInUserProfile.Job_Title = user.Job_Title;
            loggedInUserProfile.Address = user.Address;
            loggedInUserProfile.Address_2 = user.Address_2;
            loggedInUserProfile.Salespers_Purch_Code = user.Salespers_Purch_Code;
            loggedInUserProfile.Role = user.Role;

            return loggedInUserProfile;
        }

        private static ContactNoOTPForLogin MapOtpLoginProfile(ContactNoOTPForLogin user)
        {
            ContactNoOTPForLogin profile = new ContactNoOTPForLogin();

            if (user == null)
                return profile;

            profile.No = user.No;
            profile.First_Name = user.First_Name;
            profile.Last_Name = user.Last_Name;
            profile.Company_E_Mail = user.Company_E_Mail;
            profile.Job_Title = user.Job_Title;
            profile.Mobile_Phone_No = user.Mobile_Phone_No;
            profile.Salespers_Purch_Code = user.Salespers_Purch_Code;
            profile.Phone_No_2 = user.Phone_No_2;
            profile.Role = user.Role;
            profile.PCPL_OTP = user.PCPL_OTP;
            profile.PCPL_Enable_OTP_On_Login = user.PCPL_Enable_OTP_On_Login;
            profile.Status = user.Status;
            profile.Global_Dimension_1_Code = user.Global_Dimension_1_Code;

            return profile;
        }

        private static int GetPasswordResetTokenExpiryMinutes()
        {
            int expiryMinutes;
            return int.TryParse(ConfigurationManager.AppSettings["PasswordResetTokenExpiryMinutes"], out expiryMinutes)
                && expiryMinutes > 0
                    ? expiryMinutes
                    : (int)DefaultResetTokenLifetime.TotalMinutes;
        }

        private static string BuildSecurePortalUrl(string portalUrl)
        {
            if (string.IsNullOrWhiteSpace(portalUrl))
                return string.Empty;

            portalUrl = portalUrl.Trim();
            Uri absoluteUrl;
            if (Uri.TryCreate(portalUrl, UriKind.Absolute, out absoluteUrl))
            {
                UriBuilder builder = new UriBuilder(absoluteUrl)
                {
                    Scheme = Uri.UriSchemeHttps,
                    Port = absoluteUrl.Port == 80 ? 443 : absoluteUrl.Port
                };

                string formatted = builder.Uri.ToString();
                return formatted.EndsWith("/") ? formatted : formatted + "/";
            }

            return portalUrl.EndsWith("/") ? portalUrl : portalUrl + "/";
        }

        [Route("GetByEmailPass")]
        public LoggedInUserProfile GetByEmailPass(string email, string pass)
        {
            API ac = new API();
            LoggedInUserProfile loggedInUserProfile = new LoggedInUserProfile();

            var result = ac.GetData<SPProfile>("EmployeesDotNetAPI", "Company_E_Mail eq '" + email + "'");

            if (result.Result.Item1.value.Count > 0)
            {
                SPProfile user = result.Result.Item1.value[0];
                if (PasswordSecurity.VerifyPassword(pass, user.Password))
                    loggedInUserProfile = MapLoggedInUserProfile(user);
            }

            return loggedInUserProfile;
        }

        [Route("GetByNoOTP")]
        public LoggedInUserProfile GetByNoOTP(string SPNo, string OTP)
        {
            API ac = new API();
            LoggedInUserProfile loggedInUserProfile = new LoggedInUserProfile();

            var result = ac.GetData<SPProfile>("EmployeesDotNetAPI", "No eq '" + SPNo + "' and PCPL_OTP eq '" + OTP + "'");

            if (result.Result.Item1.value.Count > 0)
            {
                loggedInUserProfile.No = result.Result.Item1.value[0].No;

                loggedInUserProfile.First_Name = result.Result.Item1.value[0].First_Name;
                loggedInUserProfile.Last_Name = result.Result.Item1.value[0].Last_Name;
                loggedInUserProfile.Company_E_Mail = result.Result.Item1.value[0].Company_E_Mail;

                loggedInUserProfile.Mobile_Phone_No = result.Result.Item1.value[0].Mobile_Phone_No;
                loggedInUserProfile.Job_Title = result.Result.Item1.value[0].Job_Title;
                loggedInUserProfile.Address = result.Result.Item1.value[0].Address;
                loggedInUserProfile.Address_2 = result.Result.Item1.value[0].Address_2;
                loggedInUserProfile.Salespers_Purch_Code = result.Result.Item1.value[0].Salespers_Purch_Code;
                loggedInUserProfile.Role = result.Result.Item1.value[0].Role;
            }

            return loggedInUserProfile;
        }

        [HttpGet]
        [Route("CheckEmailPassForOTP")]
        public ContactNoOTPForLogin CheckEmailPassForOTP(string email, string pass)
        {
            API ac = new API();
            ContactNoOTPForLogin contactNoOTPForLogin = new ContactNoOTPForLogin();
            pass = pass.Trim();

            var result = ac.GetData<ContactNoOTPForLoginProfile>("EmployeesDotNetAPI", "Company_E_Mail eq '" + email + "'");

            if (result.Result.Item1.value.Count > 0)
            {
                ContactNoOTPForLoginProfile user = result.Result.Item1.value[0];
                if (PasswordSecurity.VerifyPassword(pass, user.Password))
                    contactNoOTPForLogin = MapOtpLoginProfile(user);
            }

            return contactNoOTPForLogin;
        }

        [Route("UpdateOTPForLogin")]
        public ContactNoOTPForLogin UpdateOTPForLogin(ContactNoOTPForLoginUpdate requestUser, string SPNo)
        {
            var ac = new API();
            errorDetails ed = new errorDetails();
            ContactNoOTPForLogin responseUser = new ContactNoOTPForLogin();

            var result = PatchItemOTPUpdate("EmployeesDotNetAPI", requestUser, responseUser, "No='" + SPNo + "'");

            if (result.Result.Item1.No != null)
                responseUser = result.Result.Item1;

            if (result.Result.Item2.message != null)
                ed = result.Result.Item2;

            return responseUser;
        }

        [Route("UpdateOTPBlank")]
        public SPUpdateOTP UpdateOTPBlank(SPUpdateOTP requestUser, string SPNo)
        {
            var ac = new API();
            errorDetails ed = new errorDetails();
            SPUpdateOTP responseUser = new SPUpdateOTP();

            var result = ac.PatchItem("EmployeesDotNetAPI", requestUser, responseUser, "No='" + SPNo + "'");

            if (result.Result.Item1.No != null)
                responseUser = result.Result.Item1;

            if (result.Result.Item2.message != null)
                ed = result.Result.Item2;

            return responseUser;
        }

        [Route("GetByEmail")]
        public UserCustVendor GetByEmail(string email, bool isEncrypted = false)
        {
            API ac = new API();

            if (isEncrypted)
                email = EncryptDecryptClass.Decrypt(email, true);

            UserCustVendor userCustVendor = new UserCustVendor();

            var result = ac.GetData<Salesperson>("EmployeesDotNetAPI", "Company_E_Mail eq '" + email + "'");

            if (result.Result.Item1.value.Count > 0)
            {
                userCustVendor.No = result.Result.Item1.value[0].No;

                userCustVendor.Company_E_Mail = result.Result.Item1.value[0].Company_E_Mail;
                userCustVendor.Role = result.Result.Item1.value[0].Role;

                userCustVendor.Password = result.Result.Item1.value[0].Password;
            }

            return userCustVendor;
        }

        [HttpGet]
        [Route("GetPassByEmail")]
        public string GetPassByEmail(string email)
        {
            API ac = new API();
            string userPassword = "";

            var result = ac.GetData<SPPass>("EmployeesDotNetAPI", "Company_E_Mail eq '" + email + "'");

            if (result.Result.Item1.value.Count > 0)
                userPassword = result.Result.Item1.value[0].Password;

            if (PasswordSecurity.IsHashedPassword(userPassword))
                return string.Empty;

            string decryptPass = EncryptDecryptClass.Decrypt(userPassword, true);

            return decryptPass.Trim();
        }

        [HttpGet]
        [Route("ForgotPassword")]
        public PasswordResetResult ForgotPassword(string email, string userNo, string role, string portalUrl)
        {
            PasswordResetResult result = new PasswordResetResult
            {
                Success = false,
                Status = "Invalid",
                Message = "Invalid link"
            };

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(userNo))
                return result;

            PasswordResetTokenRecord token = PasswordResetTokenStore.CreateToken(
                userNo.Trim(),
                email.Trim(),
                role,
                GetPasswordResetTokenExpiryMinutes());

            string securePortalUrl = BuildSecurePortalUrl(portalUrl);
            string resetUrl = securePortalUrl + "Account/ResetForgotPassword?token=" + Uri.EscapeDataString(token.Token);

            EmailService emailService = new EmailService();

            StringBuilder sbMailBody = new StringBuilder();
            sbMailBody.Append("");
            sbMailBody.Append("<p>Hi,</p>");
            sbMailBody.Append("<p>&nbsp;</p>");
            sbMailBody.Append("<p>Welcome to the <strong>Prakash CRM Portal</strong>.</p>");
            sbMailBody.Append("<p>User Email: " + email + "</p>");
            sbMailBody.Append("<p>User Role: " + role + "</p>");
            sbMailBody.Append("<p>&nbsp;</p>");
            sbMailBody.Append("<p>Reset Password Link : </p><a target='_self' href='" + resetUrl + "'>" + resetUrl + "</a>");
            sbMailBody.Append("<p>This link will expire in " + GetPasswordResetTokenExpiryMinutes() + " minutes and can be used only once.</p>");
            sbMailBody.Append("<p>&nbsp;</p>");
            sbMailBody.Append("<p>Warm Regards,</p>");
            sbMailBody.Append("<p>Support Team</p>");

            emailService.SendEmailTo(email, sbMailBody.ToString(), "Reset Password - PrakashCRM");

            result.Success = true;
            result.Status = "ResetLinkSent";
            result.Message = "Reset password link sent.";
            result.UserNo = userNo;
            result.Email = email;

            return result;
        }

        [HttpGet]
        [Route("ValidateResetToken")]
        public PasswordResetValidationResult ValidateResetToken(string token)
        {
            return PasswordResetTokenStore.ValidateToken(token);
        }

        //[HttpGet]
        //[Route("ForgotPassword")]
        //public Salesperson ForgotPassword(string email, string userNo, string portalUrl)
        //{
        //    var ac = new API();
        //    errorDetails ed = new errorDetails();
        //    Salesperson responseUser = new Salesperson();

        //    // 1️⃣ Generate a unique reset token
        //    string resetToken = Guid.NewGuid().ToString();
        //    DateTime expiryTime = DateTime.Now.AddMinutes(30); // Token expiry 30 min

        //    // 2️⃣ Save the token via API (instead of encrypting email)
        //    PasswordResetToken tokenRequest = new PasswordResetToken()
        //    {
        //        UserNo = userNo,
        //        Email = email,
        //        Token = resetToken,
        //        ExpiryTime = expiryTime,
        //        IsUsed = false
        //    };

        //    PasswordResetToken tokenResponse = new PasswordResetToken();
        //    var saveTokenResult = ac.PostItem("PasswordResetTokenAPI", tokenRequest, tokenResponse);

        //    // 3️⃣ Prepare reset link using the token
        //    string resetUrl = portalUrl + "Account/ResetForgotPassword?token=" + resetToken;

        //    // 4️⃣ Send email
        //    EmailService emailService = new EmailService();
        //    StringBuilder sbMailBody = new StringBuilder();
        //    sbMailBody.Append("<p>Hi,</p>");
        //    sbMailBody.Append("<p>&nbsp;</p>");
        //    sbMailBody.Append("<p>Welcome to the <strong>Prakash CRM Portal</strong>.</p>");
        //    sbMailBody.Append("<p>User Email: " + email + "</p>");
        //    sbMailBody.Append("<p>User Type: Salesperson</p>");
        //    sbMailBody.Append("<p>&nbsp;</p>");
        //    sbMailBody.Append("<p>Reset Password Link : </p><a target='_self' href='" + resetUrl + "'>" + resetUrl + "</a>");
        //    sbMailBody.Append("<p>&nbsp;</p>");
        //    sbMailBody.Append("<p>Warm Regards,</p>");
        //    sbMailBody.Append("<p>Support Team</p>");

        //    emailService.SendEmailTo(email, sbMailBody.ToString(), "Reset Password - PrakashCRM");

        //    // 5️⃣ Optional: Update user info in EmployeesDotNetAPI (if needed)
        //    Salesperson requestUser = new Salesperson()
        //    {
        //        No = userNo,
        //        Company_E_Mail = email,
        //        Password = "" // password blank because reset link sent
        //    };

        //    var result = ac.PatchItem("EmployeesDotNetAPI", requestUser, responseUser, "No='" + userNo + "'");

        //    if (result.Result.Item1.No != null)
        //        responseUser = result.Result.Item1;

        //    if (result.Result.Item2.message != null)
        //        ed = result.Result.Item2;

        //    return responseUser;
        //}

        [HttpPost]
        [Route("ChangePassword")]
        public bool ChangePassword(string email, string userNo, string role, string currentPassword, string newPassword)
        {
            bool flag = false;
            var ac = new API();
            errorDetails ed = new errorDetails();
            var result = (dynamic)null;
            Salesperson responseUser = new Salesperson();

            // First, validate the current password
            var userResult = ac.GetData<Salesperson>("EmployeesDotNetAPI", "Company_E_Mail eq '" + email + "'");

            if (userResult.Result.Item1.value.Count > 0)
            {
                string storedPassword = userResult.Result.Item1.value[0].Password;
                if (!PasswordSecurity.VerifyPassword(currentPassword, storedPassword))
                {
                    return false;
                }
            }
            else
            {
                // User not found
                return false;
            }

            Salesperson requestUser = new Salesperson()
            {
                No = userNo,
                Company_E_Mail = email,
                Password = PasswordSecurity.ProtectPasswordForStorage(newPassword),
                Role = role
            };

            result = ac.PatchItem("EmployeesDotNetAPI", requestUser, responseUser, "No='" + userNo + "'");

            if (result.Result.Item1.No != null)
            {
                flag = true;
                responseUser = result.Result.Item1;
            }

            if (result.Result.Item2.message != null)
                ed = result.Result.Item2;

            string reportingToPersonNo = "", reportingToPersonEmail = "", adminPersonEmail = "", userFName = "", userLName = "";

            var result1 = ac.GetData<SPEmailToPerson>("EmployeesDotNetAPI", "No eq '" + userNo + "'");

            if (result1.Result.Item1.value.Count > 0)
            {
                reportingToPersonNo = result1.Result.Item1.value[0].Reporting_Person_No;
                userFName = result1.Result.Item1.value[0].First_Name;
                userLName = result1.Result.Item1.value[0].Last_Name;
            }

            var result2 = ac.GetData<SPEmailToPerson>("EmployeesDotNetAPI", "No eq '" + reportingToPersonNo + "'");

            if (result2.Result.Item1.value.Count > 0)
                reportingToPersonEmail = result2.Result.Item1.value[0].Company_E_Mail;

            var result3 = ac.GetData<SPEmailToPerson>("EmployeesDotNetAPI", "Role eq 'Admin'");

            if (result3.Result.Item1.value.Count > 0)
                adminPersonEmail = result3.Result.Item1.value[0].Company_E_Mail;



            string ccPersonEmail = "";

            //ccPersonEmail = reportingToPersonEmail + ";" + adminPersonEmail;

            if (reportingToPersonEmail != "")
                ccPersonEmail += reportingToPersonEmail + ";";

            if (adminPersonEmail != "")
                ccPersonEmail += adminPersonEmail;

            //if(ccPersonEmail != "")
            //{
            //    if (ccPersonEmail.Contains(";"))
            //    {
            //        string[] ccPersonEmail_ = ccPersonEmail.Split(';');
            //        ccPersonEmail = ccPersonEmail_[0];
            //        ccPersonEmail += adminPersonEmail != "" ? ";" + ccPersonEmail[1] : "";
            //    }
            //    else
            //        ccPersonEmail = adminPersonEmail;
            //}

            if (flag)
            {
                EmailService emailService = new EmailService();
                StringBuilder sbMailBody = new StringBuilder();
                sbMailBody.Append("");
                sbMailBody.Append("<p>Hi,</p>");
                sbMailBody.Append("<p>&nbsp;</p>");
                sbMailBody.Append("<p>Welcome to the <strong>Prakash CRM Portal</strong>.</p>");
                sbMailBody.Append("<p>User Email:" + role + " " + email + "</p>");
                sbMailBody.Append("<p>User Role: " + role + "</p>");
                sbMailBody.Append("<p>&nbsp;</p>");
                sbMailBody.Append("<p>You have changed your password</p>");
                sbMailBody.Append("<p>Now you can login with new password</p>");
                sbMailBody.Append("<p>&nbsp;</p>");
                sbMailBody.Append("<p>Warm Regards,</p>");
                sbMailBody.Append("<p>Support Team</p>");

                //emailService.SendEmailTo(email, sbMailBody.ToString(), "Change Password - PrakashCRM");

                if (ccPersonEmail == "")
                    emailService.SendEmailTo(email, sbMailBody.ToString(), "Change Password - PrakashCRM");
                else
                    emailService.SendEmail(email, ccPersonEmail.TrimStart(';').TrimEnd(';'), "Change Password - PrakashCRM", sbMailBody.ToString());

            }

            return flag;
        }


        [Route("isEmailExist")]
        public string isEmailExist(string email)
        {
            API ac = new API();
            SPProfile user = new SPProfile();
            string isError = "";

            var result = ac.GetData<SPProfile>("EmployeesDotNetAPI", "Company_E_Mail eq '" + email + "'");

            if (result.Result.Item1.value.Count > 0)
                isError = "Error";
            else
                isError = "NoError";

            return isError;
        }

        [HttpGet]
        [Route("CheckIsEmpCodeExist")]
        public bool CheckIsEmpCodeExist(string EmpCode)
        {
            bool flag = false;
            API ac = new API();
            UserInfo userInfo = new UserInfo();

            var result = ac.GetData<UserInfo>("EmployeesDotNetAPI", "PCPL_Employee_Code eq '" + EmpCode + "'");

            if (result.Result.Item1.value.Count > 0)
                flag = true;

            return flag;
        }

        [Route("GetUserDetailsForProfile")]
        public UserProfileDetails GetUserDetailsForProfile(string email)
        {
            API ac = new API();
            //UserProfilePost user = new UserProfilePost();
            UserProfileDetails userProfile = new UserProfileDetails();

            var result = ac.GetData<UserProfileDetails>("EmployeesDotNetAPI", "Company_E_Mail eq '" + email + "'");

            if (result.Result.Item1.value.Count > 0)
                userProfile = result.Result.Item1.value[0];

            return userProfile;
        }

        [Route("GetAllUsers")]
        public List<SPProfile> GetAllUsers(int skip, int top, string orderby, string filter, bool isExport = false)
        {
            API ac = new API();
            List<SPProfile> users = new List<SPProfile>();
            if (filter == null)
                filter = "";

            var result = (dynamic)null;

            if (isExport)
                result = ac.GetData<SPProfile>("EmployeesDotNetAPI", filter);
            else
                result = ac.GetData1<SPProfile>("EmployeesDotNetAPI", filter, skip, top, orderby);

            if (result.Result.Item1?.value?.Count > 0)
                users = result.Result.Item1.value;

            return users;
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

        [Route("GetUserFromNo")]
        public SPProfile GetUserFromNo(string No)
        {
            API ac = new API();
            SPProfile user = new SPProfile();

            var result = ac.GetData<SPProfile>("EmployeesDotNetAPI", "No eq '" + No + "'");

            if (result.Result.Item1.value.Count > 0)
            {
                user = result.Result.Item1.value[0];

            }

            return user;
        }

        [Route("GetAdminContactNo")]
        public string GetAdminContactNo()
        {
            string adminContactNo_ = "";
            API ac = new API();
            SPAdminContactNo adminContactNo = new SPAdminContactNo();

            var result = ac.GetData<SPAdminContactNo>("EmployeesDotNetAPI", "Role eq 'Admin'");

            if (result.Result.Item1.value.Count > 0)
            {
                adminContactNo = result.Result.Item1.value[0];
                adminContactNo_ = adminContactNo.Phone_No.ToString();
            }

            return adminContactNo_;
        }

        [Route("SalesPersonCard")]
        public SPProfile SalesPersonCard(SPProfilePost requestUser, bool isEdit, string SPNo, string Password, string portalUrl)
        {
            int length = 10;
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            StringBuilder res = new StringBuilder();
            Random rnd = new Random();
            while (0 < length--)
            {
                res.Append(valid[rnd.Next(valid.Length)]);
            }
            string generatedPassword = res.ToString();

            if (isEdit)
                requestUser.Password = Password;
            else
                requestUser.Password = PasswordSecurity.ProtectPasswordForStorage(generatedPassword);

            bool flagEmail = false;

            var ac = new API();
            errorDetails ed = new errorDetails();
            SPProfile responseUser = new SPProfile();
            var result = (dynamic)null;

            if (isEdit)
                result = PatchItemSP("EmployeesDotNetAPI", requestUser, responseUser, "No='" + SPNo + "'");
            else
                result = PostItemSP("EmployeesDotNetAPI", requestUser, responseUser);

            if (result.Result.Item1.No != null && isEdit == true)
                responseUser = result.Result.Item1;

            if (result.Result.Item1.No != null && isEdit == false)
            {
                flagEmail = true;
                responseUser = result.Result.Item1;
            }

            ed = result.Result.Item2;
            responseUser.errorDetails = ed;

            if (!responseUser.errorDetails.isSuccess)
                flagEmail = false;

            //if (result.Result.Item2.message != null)
            //    ed = result.Result.Item2;

            if (flagEmail)
            {
                EmailService emailService = new EmailService();
                StringBuilder sbMailBody = new StringBuilder();

                string LoginURL = "<a title='Prakash Chemicals' href='" + portalUrl + "' target='_blank'>Log In</a>";
                sbMailBody.Append("<table cellpadding='8' cellspacing='5' border='0' width='100%' style='box-shadow: 0 0 4px rgba(0, 0, 0, 0.3);'>");
                sbMailBody.Append("<tr><td>Welcome " + requestUser.First_Name.Trim() + " " + requestUser.Last_Name.Trim() + "</td></tr>");
                sbMailBody.Append("<tr><td>Thanks For Registration With Us.</td></tr>");
                sbMailBody.Append("<tr><td>Email : " + requestUser.Company_E_Mail.Trim() + "</td></tr>");
                sbMailBody.Append("<tr><td>Password : " + generatedPassword.Trim() + "</td></tr>");
                sbMailBody.Append("<tr><td>You have to login by using below link</td></tr>");
                sbMailBody.Append("<tr><td>Login Link : " + LoginURL + "</td></tr>");
                sbMailBody.Append("<tr><td>Thanks</td></tr>");
                sbMailBody.Append("</table>");

                emailService.SendEmailTo(requestUser.Company_E_Mail, sbMailBody.ToString(), "Prakash CRM Registration");
            }

            return responseUser;
        }

        [Route("ResetForgotPassword")]
        [HttpPost]
        public PasswordResetResult ResetForgotPassword(string token, string newPassword)
        {
            PasswordResetResult resetResult = new PasswordResetResult
            {
                Success = false,
                Status = "Invalid",
                Message = "Invalid link"
            };

            PasswordResetValidationResult validation = PasswordResetTokenStore.ValidateToken(token);
            if (!validation.IsValid)
            {
                resetResult.Status = validation.Status;
                resetResult.Message = validation.Message;
                resetResult.UserNo = validation.UserNo;
                resetResult.Email = validation.Email;
                return resetResult;
            }

            DateTime usedUtc = DateTime.UtcNow;
            PasswordResetTokenRecord consumedToken = PasswordResetTokenStore.TryMarkTokenUsed(token.Trim(), usedUtc);
            if (consumedToken == null)
            {
                validation = PasswordResetTokenStore.ValidateToken(token);
                resetResult.Status = validation.Status;
                resetResult.Message = validation.Message;
                resetResult.UserNo = validation.UserNo;
                resetResult.Email = validation.Email;
                return resetResult;
            }

            var ac = new API();
            errorDetails ed = new errorDetails();
            Salesperson responseUser = new Salesperson();

            Salesperson requestUser = new Salesperson()
            {
                No = consumedToken.UserNo,
                Company_E_Mail = consumedToken.Email,
                Role = consumedToken.Role,
                Password = PasswordSecurity.ProtectPasswordForStorage(newPassword)
            };

            var result = ac.PatchItem("EmployeesDotNetAPI", requestUser, responseUser, "No='" + consumedToken.UserNo + "'");

            if (result.Result.Item1.No != null)
            {
                responseUser = result.Result.Item1;
            }

            if (result.Result.Item2.message != null)
                ed = result.Result.Item2;

            if (responseUser.No != null)
            {
                EmailService emailService = new EmailService();
                StringBuilder sbMailBody = new StringBuilder();
                sbMailBody.Append("");
                sbMailBody.Append("<p>Hi,</p>");
                sbMailBody.Append("<p>&nbsp;</p>");
                sbMailBody.Append("<p>Welcome to the <strong>Prakash CRM Portal</strong>.</p>");
                sbMailBody.Append("<p>User Email: " + consumedToken.Email + "</p>");
                sbMailBody.Append("<p>User Role: " + consumedToken.Role + "</p>");
                sbMailBody.Append("<p>&nbsp;</p>");
                sbMailBody.Append("<p>You have reset your password</p>");
                sbMailBody.Append("<p>Now you can login with new password</p>");
                sbMailBody.Append("<p>&nbsp;</p>");
                sbMailBody.Append("<p>Warm Regards,</p>");
                sbMailBody.Append("<p>Support Team</p>");

                emailService.SendEmailTo(consumedToken.Email, sbMailBody.ToString(), "New Password - PrakashCRM");

                resetResult.Success = true;
                resetResult.Status = "Success";
                resetResult.Message = "Password updated successfully.";
                resetResult.UserNo = consumedToken.UserNo;
                resetResult.Email = consumedToken.Email;
                return resetResult;
            }

            PasswordResetTokenStore.RevertTokenUsage(consumedToken.Id);
            resetResult.Status = "ResetFailed";
            resetResult.Message = ed != null && !string.IsNullOrWhiteSpace(ed.message)
                ? ed.message
                : "Unable to reset password.";
            resetResult.UserNo = consumedToken.UserNo;
            resetResult.Email = consumedToken.Email;
            return resetResult;
        }

        [Route("Profile")]
        public UserProfile Profile(UserProfilePost requestUser, string SPNo)
        {

            var ac = new API();
            errorDetails ed = new errorDetails();
            UserProfile responseUser = new UserProfile();

            var result = PatchItemUserProfile("EmployeesDotNetAPI", requestUser, responseUser, "No='" + SPNo + "'");

            if (result.Result.Item1.First_Name != null)
                responseUser = result.Result.Item1;

            if (result.Result.Item2.message != null)
                ed = result.Result.Item2;

            return responseUser;

        }

        [Route("GetAllPostCodesForDDL")]
        public List<PostCodes> GetAllCityForDDL()
        {
            API ac = new API();
            List<PostCodes> postcodes = new List<PostCodes>();

            var result = ac.GetData<PostCodes>("PostCodesDotNetAPI", "");

            if (result != null && result.Result.Item1.value.Count > 0)
                postcodes = result.Result.Item1.value;

            return postcodes;
        }

        [Route("GetAllCountryForDDL")]
        public List<Country> GetAllCountryForDDL()
        {
            API ac = new API();
            List<Country> country = new List<Country>();

            var result = ac.GetData<Country>("CountriesRegionsListDotNetAPI", "");

            if (result != null && result.Result.Item1.value.Count > 0)
                country = result.Result.Item1.value;

            return country;
        }

        [Route("GetAllBranchForDDL")]
        public List<Branch> GetAllBranchForDDL()
        {
            API ac = new API();
            List<Branch> branch = new List<Branch>();

            var result = ac.GetData<Branch>("DimensionValuesDotNetAPI", "Dimension_Code eq 'BRANCH'");

            if (result != null && result.Result.Item1.value.Count > 0)
                branch = result.Result.Item1.value;

            return branch;
        }

        [Route("GetAllRoleForDDL")]
        public List<Role> GetAllRoleForDDL()
        {
            API ac = new API();
            List<Role> role = new List<Role>();

            var result = ac.GetData<Role>("RolesListDotNetAPI", "");

            if (result != null && result.Result.Item1.value.Count > 0)
                role = result.Result.Item1.value;

            return role;
        }

        [Route("GetAllViewTransactionForDDL")]
        public List<ViewTransaction> GetAllViewTransactionForDDL()
        {
            API ac = new API();
            List<ViewTransaction> viewtransaction = new List<ViewTransaction>();

            var result = ac.GetData<ViewTransaction>("ViewTransactionOptsListDotNetAPI", "IsActive eq true");

            if (result != null && result.Result.Item1.value.Count > 0)
                viewtransaction = result.Result.Item1.value;

            return viewtransaction;
        }

        [Route("GetAllReportingPersonForDDL")]
        public List<ReportingPerson> GetAllReportingPersonForDDL()
        {
            API ac = new API();
            var list = new List<ReportingPerson>();
            var result1 = ac.GetData<ReportingPerson>("EmployeesDotNetAPI", "Is_Reporting_Person eq true");

            if (result1?.Result.Item1.value != null)
                list.AddRange(result1.Result.Item1.value);

            var result2 = ac.GetData<ReportingPerson>("EmployeesDotNetAPI", "PCPL_Is_HOD eq true");

            if (result2?.Result.Item1.value != null)
                list.AddRange(result2.Result.Item1.value);
            list = list.GroupBy(x => x.No).Select(g => g.First()).ToList();

            return list;
        }

        [Route("GetAllSalespersonForDDL")]
        public List<SPSalespeoplePurchaser> GetAllSalespersonForDDL()
        {
            API ac = new API();
            List<SPSalespeoplePurchaser> salesperson = new List<SPSalespeoplePurchaser>();

            var result = ac.GetData<SPSalespeoplePurchaser>("SalespersonPurchaserDotNetAPI", "");

            if (result != null && result.Result.Item1.value.Count > 0)
                salesperson = result.Result.Item1.value;

            return salesperson;
        }

        [HttpPost]
        [Route("UpdatePassword")]
        public IHttpActionResult UpdatePassword(UpdatePasswordRequest model)
        {
            if (model == null || string.IsNullOrEmpty(model.UserNo))
                return BadRequest("Invalid request");

            var ac = new API();
            bool flag = false;
            Salesperson1 requestUser = new Salesperson1()
            {
                No = model.UserNo,
                Company_E_Mail = model.Email,
                Password = PasswordSecurity.ProtectPasswordForStorage(model.NewPassword)
            };

            Salesperson1 responseUser = new Salesperson1();

            var result = ac.PatchItem("EmployeesDotNetAPI", requestUser, responseUser, "No='" + model.UserNo + "'"
            );

            if (result?.Result.Item1 != null && result.Result.Item1.No != null)
                flag = true;

            return Ok(flag);
        }


        [Route("GetDetailsByCode")]
        public List<PostCodes> GetDetailsByCode(string Code)
        {
            API ac = new API();
            List<PostCodes> postcodes = new List<PostCodes>();

            var result = ac.GetData<PostCodes>("PostCodesDotNetAPI", "Code eq '" + Code + "'");

            if (result != null && result.Result.Item1.value.Count > 0)
                postcodes = result.Result.Item1.value;

            return postcodes;
        }


        [Route("GetSPCodesOfReportingPersonUser")]
        public List<SPCodesOfReportingPersonUser> GetSPCodesOfReportingPersonUser(string LoggedInUserNo)
        {
            API ac = new API();
            List<SPCodesOfReportingPersonUser> spCodes = new List<SPCodesOfReportingPersonUser>();

            var result = ac.GetData<SPCodesOfReportingPersonUser>("EmployeesDotNetAPI", "Reporting_Person_No eq '" + LoggedInUserNo + "'");

            if (result != null && result.Result.Item1.value.Count > 0)
                spCodes = result.Result.Item1.value;

            return spCodes;
        }

        public async Task<(SPProfile, errorDetails)> PostItemSP<SPProfile>(string apiendpoint, SPProfilePost requestModel, SPProfile responseModel)
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
            catch (Exception ex)
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
                    responseModel = res.ToObject<SPProfile>();

                    errordetail.code = response.StatusCode.ToString();
                    errordetail.message = response.ReasonPhrase;
                }
                catch (Exception ex1)
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
                catch (Exception ex1)
                {
                }
            }
            return (responseModel, errordetail);
        }

        public async Task<(SPProfile, errorDetails)> PatchItemSP<SPProfile>(string apiendpoint, SPProfilePost requestModel, SPProfile responseModel, string fieldWithValue)
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
                response = _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).Result;
            }
            catch (Exception ex)
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
                    responseModel = res.ToObject<SPProfile>();


                    errordetail.code = response.StatusCode.ToString();
                    errordetail.message = response.ReasonPhrase;
                }
                catch (Exception ex1)
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
                catch (Exception ex1)
                {
                }
            }

            return (responseModel, errordetail);
        }

        public async Task<(ContactNoOTPForLogin, errorDetails)> PatchItemOTPUpdate<ContactNoOTPForLogin>(string apiendpoint, ContactNoOTPForLoginUpdate requestModel, ContactNoOTPForLogin responseModel, string fieldWithValue)
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
                response = _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).Result;
            }
            catch (Exception ex)
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
                    responseModel = res.ToObject<ContactNoOTPForLogin>();


                    errordetail.code = response.StatusCode.ToString();
                    errordetail.message = response.ReasonPhrase;
                }
                catch (Exception ex1)
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
                catch (Exception ex1)
                {
                }
            }

            return (responseModel, errordetail);
        }

        public async Task<(UserProfile, errorDetails)> PatchItemUserProfile<UserProfile>(string apiendpoint, UserProfilePost requestModel, UserProfile responseModel, string fieldWithValue)
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
                response = _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).Result;
            }
            catch (Exception ex)
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
                    responseModel = res.ToObject<UserProfile>();


                    errordetail.code = response.StatusCode.ToString();
                    errordetail.message = response.ReasonPhrase;
                }
                catch (Exception ex1)
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
                catch (Exception ex1)
                {
                }
            }

            return (responseModel, errordetail);
        }

        [Route("GetDepartmentName")]
        public List<Departments> GetDepartmentName()
        {
            API ac = new API();
            List<Departments> departments = new List<Departments>();

            var result = ac.GetData<Departments>("DepartmentsDotNetAPI", "");

            if (result != null && result.Result.Item1.value.Count > 0)
                departments = result.Result.Item1.value;

            return departments;
        }

    }
}
