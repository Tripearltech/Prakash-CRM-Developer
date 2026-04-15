using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrakashCRM.Data.Models
{
    public class OData<T>
    {
        [JsonProperty("@odata.context")]
        public string Metadata { get; set; }
        public List<T> value { get; set; }
    }
    public class Salesperson
    {
        public string No { get; set; }

        public string Company_E_Mail { get; set; }

        public string Password { get; set; }
        public string Role { get; set; }
    }
    public class Salesperson1
    {
        public string No { get; set; }

        public string Company_E_Mail { get; set; }

        public string Password { get; set; }
       // public string Role { get; set; }
    }
    public class UpdatePasswordRequest
{
    public string Email { get; set; }
    public string UserNo { get; set; }
    //public string Role { get; set; }
    public string NewPassword { get; set; }
}

    public class PasswordResetTokenRecord
    {
        public int Id { get; set; }
        public string UserNo { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string Token { get; set; }
        public DateTime ExpiryUtc { get; set; }
        public bool IsUsed { get; set; }
        public DateTime CreatedUtc { get; set; }
        public DateTime? UsedUtc { get; set; }
    }

    public class PasswordResetValidationResult
    {
        public bool IsValid { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public string Token { get; set; }
        public string UserNo { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
    }

    public class PasswordResetResult
    {
        public bool Success { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public string UserNo { get; set; }
        public string Email { get; set; }
        public string ResetLinkText { get; set; }
    }


    //public class PasswordResetToken
    //{
    //    public string UserNo { get; set; }
    //    public string Email { get; set; }
    //    public string Token { get; set; }
    //    public DateTime ExpiryTime { get; set; }
    //    public bool IsUsed { get; set; }
    //}

    public class SPPass
    {
        public string Password { get; set; }
    }

    public class SPEmailToPerson
    {
        public string First_Name { get; set; }

        public string Last_Name { get; set; }

        public string Company_E_Mail { get; set; }

        public string Reporting_Person_No { get; set; }
    }

    public class SPProfile
    {
        /*public static explicit operator SPProfilePost(SPProfile obj)
        {
            return JsonConvert.DeserializeObject<SPProfilePost>(JsonConvert.SerializeObject(obj));
        }*/

        public string No { get; set; }

        public string PCPL_Employee_Code { get; set; }

        public string First_Name { get; set; }


        public string Last_Name { get; set; }


        public string Company_E_Mail { get; set; }


        public string Job_Title { get; set; }


        public string Address { get; set; }


        public string Mobile_Phone_No { get; set; }

        public string Address_2 { get; set; }

        public string Password { get; set; }

        public string Gender { get; set; }
        public string Post_Code { get; set; }
        //public string Alt_Address_Code { get; set; }
        //public string Country_Region_Code { get; set; }
        public string County { get; set; }
        public string Salespers_Purch_Code { get; set; }
        [Column(TypeName = "date")]
        public DateTime Birth_Date { get; set; }
        public string Phone_No { get; set; }
        public string E_Mail { get; set; }
        public DateTime Employment_Date { get; set; }
        public string Global_Dimension_1_Code { get; set; }
        //public string Branch_Code { get; set; }
        //public string Branch { get; set; }
        public string Role_No { get; set; }
        public string PCPL_Department_Code { get; set; }
        public string PCPL_Department_Name { get; set; }
        public string Middle_Name { get; set; }
        public string Role { get; set; }

        public string View_Transaction_No { get; set; }

        public string Reporting_Person_No { get; set; }
        public bool Is_Reporting_Person { get; set; }
        public string Status { get; set; }
        public bool PCPL_Enable_OTP_On_Login { get; set; }
        public bool PCPL_Is_HOD { get; set; }
        public errorDetails errorDetails { get; set; } = null;
    }

    public class SPProfilePost
    {

        [Required(ErrorMessage = "Employee Code is required")]
        public string PCPL_Employee_Code { get; set; }

        [Required(ErrorMessage = "First Name is required")]
        public string First_Name { get; set; }
        public string Middle_Name { get; set; }

        [Required(ErrorMessage = "Last Name is required")]
        public string Last_Name { get; set; }

        [EmailAddress(ErrorMessage = "Invalid Email")]
        [Required(ErrorMessage = "Email is required")]
        public string Company_E_Mail { get; set; }

        [Required(ErrorMessage = "Job Title is required")]
        public string Job_Title { get; set; }

        [Required(ErrorMessage = "Address is required")]
        public string Address { get; set; }

        [RegularExpression(@"^(\d{10})$", ErrorMessage = "Contact No must be in 10 digit")]
        [Required(ErrorMessage = "Contact No is required")]
        public string Mobile_Phone_No { get; set; }

        public string Address_2 { get; set; }

        [Required(ErrorMessage = "Department is required")]
        public string PCPL_Department_Code { get; set; }

        public string Password { get; set; }

        public string Gender { get; set; }

        [Required(ErrorMessage = "Post Code is required")]
        public string Post_Code { get; set; }
        //public string Alt_Address_Code { get; set; }
        //public string City { get; set; }
        //public string Country_Region_Code { get; set; }
        //public string County { get; set; }
        public string Salespers_Purch_Code { get; set; }
        public string Birth_Date { get; set; }
        public string Phone_No { get; set; }

        [EmailAddress(ErrorMessage = "Invalid Email")]
        public string E_Mail { get; set; }

        public string Employment_Date { get; set; }
        public string Global_Dimension_1_Code { get; set; }
        //public string Branch_Code { get; set; }
        //public string Branch { get; set; }
        public string Role_No { get; set; }

        public string View_Transaction_No { get; set; }

        public string Reporting_Person_No { get; set; }
        public bool Is_Reporting_Person { get; set; }
        public string Status { get; set; }
        public bool PCPL_Enable_OTP_On_Login { get; set; }
        public bool PCPL_Is_HOD { get; set; }
    }

    public class UserProfile
    {
        public string First_Name { get; set; }

        public string Last_Name { get; set; }

        public string Company_E_Mail { get; set; }

        public string Job_Title { get; set; }

        public string Address { get; set; }

        public string Mobile_Phone_No { get; set; }

        public string Address_2 { get; set; }

        [Column(TypeName = "date")]
        public DateTime Birth_Date { get; set; }

        [Column(TypeName = "date")]
        public DateTime Employment_Date { get; set; }

        public string Global_Dimension_1_Code { get; set; }

        public string Role_No { get; set; }

        public string View_Transaction_No { get; set; }

        public string Status { get; set; }
    }

    public class UserProfilePost
    {
        [Required(ErrorMessage = "First Name is required")]
        public string First_Name { get; set; }

        [Required(ErrorMessage = "Last Name is required")]
        public string Last_Name { get; set; }

        [Required(ErrorMessage = "Email is required")]
        public string Company_E_Mail { get; set; }

        [Required(ErrorMessage = "Job Title is required")]
        public string Job_Title { get; set; }

        [Required(ErrorMessage = "Address is required")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Contact No is required")]
        public string Mobile_Phone_No { get; set; }

        public string Phone_No { get; set; }

        public string Address_2 { get; set; }

        public string Birth_Date { get; set; }

        public string Employment_Date { get; set; }

        public string Global_Dimension_1_Code { get; set; }

        public string Role_No { get; set; }

        public string View_Transaction_No { get; set; }

        public string Status { get; set; }
    }

    public class UserProfileDetails
    {
        [Required(ErrorMessage = "First Name is required")]
        public string First_Name { get; set; }

        [Required(ErrorMessage = "Last Name is required")]
        public string Last_Name { get; set; }

        [Required(ErrorMessage = "Email is required")]
        public string Company_E_Mail { get; set; }

        [Required(ErrorMessage = "Job Title is required")]
        public string Job_Title { get; set; }

        [Required(ErrorMessage = "Address is required")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Contact No is required")]
        public string Mobile_Phone_No { get; set; }

        public string Phone_No { get; set; }

        public string Address_2 { get; set; }

        public string Birth_Date { get; set; }

        public string Employment_Date { get; set; }

        public string Global_Dimension_1_Code { get; set; }

        public string Role_No { get; set; }

        public string Role { get; set; }

        public string View_Transaction_No { get; set; }

        public string View_Transaction { get; set; }

        public string Status { get; set; }
    }

    public class SPUploadProfileImage
    {
        public string ImagePath { get; set; }

        public HttpPostedFileBase ImageFile { get; set; }
    }

    public class CustomerVendorProfile
    {
        public string No { get; set; }

        public string Name { get; set; }

        public string E_Mail { get; set; }

        public string Job_Title { get; set; }

        public string Address { get; set; }

        public string Mobile_Phone_No { get; set; }

        public string Address_2 { get; set; }
    }
    public class LoggedInUserProfile
    {
        public string No { get; set; }

        public string First_Name { get; set; }

        public string Last_Name { get; set; }

        public string Company_E_Mail { get; set; }

        public string Job_Title { get; set; }

        public string Address { get; set; }

        public string Mobile_Phone_No { get; set; }

        public string Address_2 { get; set; }

        public string Role { get; set; }

        public string Login_Type { get; set; }

        public string Salespers_Purch_Code { get; set; }

        public string Global_Dimension_1_Code { get; set; }
    }

    public class ContactNoOTPForLogin
    {
        public string No { get; set; }

        public string Phone_No_2 { get; set; }

        public string PCPL_OTP { get; set; }

        public string First_Name { get; set; }

        public string Last_Name { get; set; }

        public string Company_E_Mail { get; set; }

        public string Job_Title { get; set; }

        public string Mobile_Phone_No { get; set; }

        public string Role { get; set; }

        public string Salespers_Purch_Code { get; set; }
        public string Global_Dimension_1_Code { get; set; }
        public string Status { get; set; }

        public bool PCPL_Enable_OTP_On_Login { get; set; }
        public bool OTPEmailSent { get; set; }

        public bool RequireOtpVerification { get; set; }

        public bool TrustedDeviceRecognized { get; set; }

        public bool ClearTrustedDeviceState { get; set; }

        public string TrustedDeviceToken { get; set; }

        public string TrustedDeviceExpiresUtc { get; set; }

        public string RedirectUrl { get; set; }
    }

    public class ContactNoOTPForLoginProfile : ContactNoOTPForLogin
    {
        public string Password { get; set; }
    }

    public class ContactNoOTPForLoginUpdate
    {
        public string PCPL_OTP { get; set; }
    }

    public class SPUpdateOTP
    {
        public string No { get; set; }

        public string Phone_No_2 { get; set; }

        public string PCPL_OTP { get; set; }
    }

    public class UserCustVendor
    {
        public string No { get; set; }

        public string Company_E_Mail { get; set; }

        public string Password { get; set; }

        public string Role { get; set; }
    }

    public class LoginOtpVerificationResult
    {
        public bool IsSuccess { get; set; }

        public string LoggedInUser { get; set; }

        public bool ClearTrustedDeviceState { get; set; }

        public string TrustedDeviceToken { get; set; }

        public string TrustedDeviceExpiresUtc { get; set; }

        public string RedirectUrl { get; set; }
    }

    public class SPSalespeoplePurchaser
    {
        public string Code { get; set; }

        public string Name { get; set; }
    }

    public class SPAdminContactNo
    {
        public string Phone_No { get; set; }
    }

    public class UserInfo
    {
        public string No { get; set; }

        public string First_Name { get; set; }

        public string Last_Name { get; set; }

        public string Company_E_Mail { get; set; }

        public string Salespers_Purch_Code { get; set; }
    }

    public class SPUserReportingPersonDetails
    {
        public string Reporting_Person_No { get; set; }

        public string PCPL_Reporting_Person_Email { get; set; }
    }

    public class SPFinanceUserDetails
    {
        public string No { get; set; }

        public string Company_E_Mail { get; set; }
    }

    public class SPCodesOfReportingPersonUser
    {
        public string Salespers_Purch_Code { get; set; }
    }

    public class errorMaster<T>
    {
        public T error { get; set; }
    }

    public class errorDetails
    {
        public bool isSuccess { get; set; }

        public string code { get; set; }

        public string message { get; set; }
    }

}
