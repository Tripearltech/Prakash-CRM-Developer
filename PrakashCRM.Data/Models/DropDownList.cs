using System.Collections.Generic;
//using System.Web.Mvc;

namespace PrakashCRM.Data.Models
{

    public class PostCodes
    {
        public string Code { get; set; }

        public string City { get; set; }
        public string District_Code { get; set; }
        public string PCPL_State_Code { get; set; }
        public string Country_Region_Code { get; set; }
        public bool IsActive { get; set; }
    }

    public class Country
    {
        public string Code { get; set; }

        public string Name { get; set; }
    }
    public class Branch
    {
        public string Code { get; set; }

        public string Name { get; set; }
    }

    public class Role
    {
        public string No { get; set; }
        public string Role_Name { get; set; }
    }

    public class ViewTransaction
    {
        public string No { get; set; }
        public string Title { get; set; }
    }

    public class ReportingPerson //SalesPerson
    {
        public string No { get; set; }
        public string First_Name { get; set; }
        public string Last_Name { get; set; }
    }

    public class State
    {
        public string Code { get; set; }

        public string Description { get; set; }
    }

    public class Area
    {
        public string Code { get; set; }

        public string Text { get; set; }    
    }

    public class District
    {
        public string No { get; set; }

        public string District_Name { get; set; }
    }

    public class Pincode
    {
        public string Code { get; set; }

    }

    public class Industry
    {
        public string Code { get; set; }

        public string Name { get; set; }
    }

    public class BusinessType
    {
        public string No { get; set; }

        public string Type { get; set; }
    }

    public class SourceofContacts
    {
        public string No { get; set; }

        public string Title { get; set; }
    }

    public class Departments
    {
        public string No { get; set; }

        public string Department { get; set; }
    }

    public class Company
    {
        public string No { get; set; }

        public string Name { get; set; }

        public string PCPL_Primary_Contact_No { get; set; }

        public string Salesperson_Code { get; set; }

        public string Address { get; set; }

        public string Address_2 { get; set; }

        public string Area { get; set; }

        public string City { get; set; }

        public string Post_Code { get; set; }
    }

    public class Contact
    {
        public string No { get; set; }

        public string Name { get; set; }
    }

    public class PaymentTerms
    {
        public string Code { get; set; }

        public string Description { get; set; }
    }

    public class Product
    {
        public string No { get; set; }

        public string Description { get; set; }

        public string Base_Unit_of_Measure { get; set; }
    }

    public class ContactBusinessRel
    {
        public string No { get; set; }

    }

    public class ConsigneeAddress
    {
        public string Code { get; set; }
        public string Address { get; set; }
        public string CustomerNo { get; set; }
        public bool IsDummy { get; set; }
    }

    public class Transporter
    {
        public string No { get; set; }

        public string Name { get; set; }
        public string GST_Registration_No { get; set; }
        public string ARN_No { get; set; }
    }

}
