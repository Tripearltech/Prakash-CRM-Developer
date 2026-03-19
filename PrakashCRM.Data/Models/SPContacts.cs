using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Data;
using System.ComponentModel.DataAnnotations;

namespace PrakashCRM.Data.Models
{
    public class SPContactCompanyList
    {
        public string No { get; set; }

        public string Company_Name { get; set; }

        public string City { get; set; }

        public string Post_Code { get; set; }

        public string Phone_No { get; set; }

        public string E_Mail { get; set; }

        public string Name { get; set; }

        public string Mobile_Phone_No { get; set; }

        public string Salesperson_Code { get; set; }

        public string Type { get; set; }
    }

    public class SPCompanyList
    {
        public string No { get; set; }

        public string Name { get; set; }

        public string Industry { get; set; }

        public string Source_of_Contact { get; set; }
        public string Company_Name { get; set; }

        public string Business_Type { get; set; }

        public string Area { get; set; }

        public string Mobile_Phone_No { get; set; }

        public string City { get; set; }

        public string Post_Code { get; set; }

        public string Phone_No { get; set; }

        public string E_Mail { get; set; }

        public string Salesperson_Code { get; set; }

        public string Type { get; set; }

        public string Address { get; set; }

        public string Address_2 { get; set; }

        public string PCPL_Primary_Contact_Name { get; set; }

        public string PCPL_Primary_Contact_Email { get; set; }

        public int Credit_Limit { get; set; }

        public string GST_Registration_No { get; set; }

        public string P_A_N_No { get; set; }

        public string Country_Region_Code { get; set; }

        public string PCPL_Feedback_Status { get; set; }
    }

    public class SPCompanyContact
    {
        [Required(ErrorMessage = "Company Name is required")]
        public string Company_Name { get; set; }
        //public string Pin_Code { get; set; }
        public string Area_Code { get; set; }
        public string Post_Code { get; set; } //City
        public string District { get; set; }
        public string City { get; set; }
        public string State_Code { get; set; }
        public string Country_Region_Code { get; set; }

        [Required(ErrorMessage = "Salesperson Code is required")]
        public string Salesperson_Code { get; set; }

        public string PCPL_Secondary_SP_Code { get; set; }

        [Required(ErrorMessage = "Address is required")]
        public string Address { get; set; }
        public string Address_2 { get; set; }
        public string PCPL_URL { get; set; }
        public string Industry_No { get; set; }
        public string Business_Type_No { get; set; }

        //[Required(ErrorMessage = "GST No is required")]
        //[RegularExpression("^\\d{15}$", ErrorMessage = "GST No. should be in 15 character")]
        [StringLength(15, MinimumLength = 15, ErrorMessage = "GST No. should be in 15 character")]
        public string GST_Registration_No { get; set; }

        //[Required(ErrorMessage = "PAN No is required")]
        //[RegularExpression("^\\d{10}$", ErrorMessage = "PAN No. should be in 15 character")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "PAN No. should be in 10 character")]
        public string P_A_N_No { get; set; }

        public string Assessee_Code { get; set; }

        public string Source_Of_Contact_No { get; set; }

        //[Required(ErrorMessage = "Phone No is required")]
        //[RegularExpression("^[0-9]*$", ErrorMessage = "Phone No must be in numeric")]
        //[RegularExpression(@"^(\d{10})$", ErrorMessage = "Phone No must be in 10 digit")]
        [RegularExpression("^[0-9]{10}$", ErrorMessage = "Phone No must be in numeric and in 10 digit")]
        public string Phone_No { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email")]
        public string E_Mail { get; set; }
        //public string Job_Title { get; set; }


        //[Required(ErrorMessage = "Contact Person Name is required")]
        public string Contact_Name { get; set; }

        //[Required(ErrorMessage = "Contact Person Mobile No is required")]
        //[RegularExpression("^[0-9]{10}$", ErrorMessage = "Mobile No must be in numeric and in 10 digit")]
        public string Mobile_Phone_No { get; set; }

        //[Required(ErrorMessage = "Contact Person Email is required")]
        //[EmailAddress(ErrorMessage = "Invalid Email")]
        public string Contact_EMail { get; set; }
        public string SQValue { get; set; }

        //[Required(ErrorMessage = "Job Responsibility is required")]
        public string PCPL_Job_Responsibility { get; set; }

        public string PCPL_Department_Code { get; set; }

        public bool PCPL_Allow_Login { get; set; }

        public bool PCPL_Enable_OTP_On_Login { get; set; }

        public bool Is_Primary { get; set; }

        public int Credit_Limit { get; set; }

        public string PCPL_Company_Customer_No { get; set; }
    }

    public class SPCompany
    {
        public string Name { get; set; }

        public string Type { get; set; }

        //public string Salesperson_Code { get; set; }

        //public string Address { get; set; }

        //public string Country_Region_Code { get; set; }

        ////public string Post_Code { get; set; }

        ////public string City { get; set; }

        ////public string Mobile_Phone_No { get; set; }

        //public string Phone_No { get; set; }

        //public string E_Mail { get; set; }
        //public string Company_Name { get; set; }
        //public string Pin_Code { get; set; }
        public string Area_Code { get; set; }
        public string Post_Code { get; set; } //City
        public string District { get; set; }
        public string State_Code { get; set; }
        public string City { get; set; }
        public string Country_Region_Code { get; set; }

        //[Required(ErrorMessage = "Salesperson Code is required")]
        public string Salesperson_Code { get; set; }
        public string PCPL_Secondary_SP_Code { get; set; }

        //[Required(ErrorMessage = "Address is required")]
        public string Address { get; set; }
        public string Address_2 { get; set; }
        public string PCPL_URL { get; set; }
        public string Industry_No { get; set; }
        public string Business_Type_No { get; set; }
        public string GST_Registration_No { get; set; }
        public string P_A_N_No { get; set; }
        public string Assessee_Code { get; set; }
        public string Source_Of_Contact_No { get; set; }

        //[Required(ErrorMessage = "Phone No is required")]
        public string Phone_No { get; set; }

        //[Required(ErrorMessage = "Email is required")]
        public string E_Mail { get; set; }

        public int Credit_Limit { get; set; }
        public string Mobile_Phone_No { get; set; }
    }

    public class SPCompanyResponse
    {
        public string No { get; set; }

        public string Name { get; set; }

        public string Type { get; set; }

        public string Salesperson_Code { get; set; }

        public string PCPL_Secondary_SP_Code { get; set; }

        public string Address { get; set; }

        public string Country_Region_Code { get; set; }

        //public string Post_Code { get; set; }

        //public string City { get; set; }

        public string Mobile_Phone_No { get; set; }

        //public string Phone_No { get; set; }

        public string E_Mail { get; set; }

        public int Credit_Limit { get; set; }

        public errorDetails errorDetails { get; set; } = null;

    }

    public class SPContactList
    {
        public string Name { get; set; }

        public string Type { get; set; }

        public string Company_No { get; set; }

        public string Company_Name { get; set; }

        public string Mobile_Phone_No { get; set; }

        public string E_Mail { get; set; }
    }

    public class SPContact
    {
        public string Name { get; set; }

        public string Type { get; set; }

        public string Company_No { get; set; }

        public string Salesperson_Code { get; set; }

        //public string Address { get; set; }

        //public string Country_Region_Code { get; set; }

        //public string Post_Code { get; set; }

        //public string City { get; set; }

        public string Mobile_Phone_No { get; set; }

        //public string Phone_No { get; set; }

        public string E_Mail { get; set; }

        public string PCPL_Job_Responsibility { get; set; }

        public string PCPL_Department_Code { get; set; }

        public bool PCPL_Allow_Login { get; set; }

        public bool PCPL_Enable_OTP_On_Login { get; set; }

        public bool Is_Primary { get; set; }
    }

    public class SPContactResponse
    {
        public string No { get; set; }

        public string Name { get; set; }

        public string Mobile_Phone_No { get; set; }

        public string E_Mail { get; set; }

        public string Type { get; set; }

        public string Company_No { get; set; }

        public string PCPL_Job_Responsibility { get; set; }

        public string PCPL_Department_Code { get; set; }

        public string PCPL_Department_Name { get; set; }

        public bool PCPL_Allow_Login { get; set; }

        public bool PCPL_Enable_OTP_On_Login { get; set; }

        public bool Is_Primary { get; set; }
    }

    public class SPContactProductsPost
    {
        public string Contact_No { get; set; }

        public string Item_No { get; set; }

        public string Customer_No { get; set; }

        public string SalesPerson_Code { get; set; }

        public bool IsActive { get; set; }
    }

    public class SPContactProducts
    {
        public string No { get; set; }

        public string Contact_No { get; set; }

        public string Customer_No { get; set; }

        public string Item_No { get; set; }

        public string Item_Name { get; set; }

        public string PCPL_Unit_Of_Measure { get; set; }

        public bool IsActive { get; set; }
        public bool Blocked { get; set; }

    }

    public class SPDeleteContactProd
    {
        public string contactno { get; set; }
        public string productno { get; set; }
    }

    public class SPDeleteContactProdOData
    {
        [JsonProperty("@odata.context")]
        public string Metadata { get; set; }

        public string value { get; set; }

        public errorDetails errorDetails { get; set; } = null;
    }

}
