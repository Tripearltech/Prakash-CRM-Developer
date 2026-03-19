using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrakashCRM.Data.Models
{
    
    public class SPInquiryList
    {
        public string Inquiry_No { get; set; }
        public string Inquiry_Date { get; set; }
        public string Inquiry_Customer_Contact { get; set; }
        public string Contact_Company_Name { get; set; }
        public string PCPL_Contact_Person { get; set; }
        public string PCPL_Contact_Person_Name { get; set; }
        //public string Delivery_Date { get; set; }
        public string Inquiry_Status_Remarks { get; set; }
        public string Payment_Terms { get; set; }
        public string Inquiry_Status { get; set; }
        public string Salesperson_Code { get; set; }
        //public string Consignee_Code { get; set; }
        public string Sell_to_Address { get; set; }
        public string Sell_to_Address_2 { get; set; }
        public string Sell_to_City { get; set; }
        public string Sell_to_Post_Code { get; set; }
        public string Ship_to_Code { get; set; }
        public string Ship_to_Name { get; set; }
        public string Ship_to_Address { get; set; }
        public string Ship_to_Address_2 { get; set; }
        public string Ship_to_City { get; set; }
        public string Ship_to_Post_Code { get; set; }
        public string PCPL_Job_to_Code { get; set; }
        public string PCPL_Job_to_Name { get; set; }
        public string PCPL_Job_to_Address { get; set; }
        public string PCPL_Job_to_Address_2 { get; set; }
        public string PCPL_Job_to_City { get; set; }
        public string PCPL_Job_to_Post_Code { get; set; }
    }

    public class SPInqHeaderDetails
    {
        public string IsInqEdit { get; set; }

        public string InquiryDate { get; set; }

        public string InquiryNo { get; set; }

        public string CustomerName { get; set; }

        public string ContactCompanyNo { get; set; }

        public string ContactPersonNo { get; set; }

        public string CustomerNo { get; set; }

        public string ContactNo { get; set; }

        public string ConsigneeAddress { get; set; }

        public string ShiptoCode { get; set; }

        public string JobtoCode { get; set; }

        public string ConsigneeCode { get; set; }

        public string DeliveryDate { get; set; }

        public string PaymentTerms { get; set; }

        public string Inquiry_Status_Remarks { get; set; }

        public string CustomerTemplateCode { get; set; }

        public string SPCode { get; set; }

        public List<SPInqLinesDetails> Products { get; set; }

    }

    public class SPInquiry
    {
        public string Inquiry_No { get; set; }
        public string Inquiry_Customer_No { get; set; }
        public string Inquiry_Customer_Contact { get; set; }
        public string Contact_Company_Name { get; set; }
        public string PCPL_Contact_Person { get; set; }
        [Column(TypeName = "date")]
        public DateTime Inquiry_Date { get; set; }
        //public string Consignee_Code { get; set; }
        //[Column(TypeName = "date")]
        //public DateTime Delivery_Date { get; set; }
        public string Payment_Terms { get; set; }
        public string Inquiry_Status { get; set; }
        public string Sell_to_Address { get; set; }
        public string Sell_to_Address_2 { get; set; }
        public string Sell_to_City { get; set; }
        public string PCPL_Job_to_Code { get; set; }
        public string Ship_to_Code { get; set; }
        public string Inquiry_Status_Remarks { get; set; }
        public string Salesperson_Code { get; set; }
        public bool IsActive { get; set; }
        public errorDetails errorDetails { get; set; } = null;
    }

    public class SPInquiryPost
    {
        public string Inquiry_No { get; set; }
        public string Inquiry_Customer_Contact { get; set; }
        public string Contact_Company_Name { get; set; }
        public string PCPL_Contact_Person { get; set; }
        public string Inquiry_Customer_No { get; set; }
        public string Inquiry_Date { get; set; }
        //public string Sell_to_Address { get; set; }
        //public string Sell_to_Address_2 { get; set; }
        //public string Sell_to_City { get; set; }
        //public string Sell_to_Post_Code { get; set; }
        //public string Sell_to_Country_Region_Code { get; set; }
        //public string Sell_to_Contact_No { get; set; }
        //public string SellToPhoneNo { get; set; }
        //public string SellToMobilePhoneNo { get; set; }
        //public string SellToEmail { get; set; }
        public string PCPL_Job_to_Code { get; set; }
        public string Ship_to_Code { get; set; }
        public string Payment_Terms { get; set; }
        public string Inquiry_Status_Remarks { get; set; }
        public string Inquiry_Status { get; set; }
        public string Salesperson_Code { get; set; }
        public bool PCPL_IsInquiry { get; set; }
        public bool IsActive { get; set; }
    }

    public class SPInquiryPostWithCustTemplateCode
    {
        public string Inquiry_No { get; set; }
        public string Inquiry_Customer_Contact { get; set; }
        public string Contact_Company_Name { get; set; }
        public string PCPL_Contact_Person { get; set; }
        public string Inquiry_Customer_No { get; set; }
        public string Inquiry_Date { get; set; }
        public string PCPL_Job_to_Code { get; set; }
        public string Ship_to_Code { get; set; }
        public string Payment_Terms { get; set; }
        public string Inquiry_Status_Remarks { get; set; }
        public string Inquiry_Status { get; set; }
        public string Sell_to_Customer_Templ_Code { get; set; }
        public string Bill_to_Customer_Templ_Code { get; set; }
        public string Salesperson_Code { get; set; }
        public bool PCPL_IsInquiry { get; set; }
        public bool IsActive { get; set; }
    }

    public class SPInquiryUpdate
    {
        public string PCPL_Contact_Person { get; set; }
        public string PCPL_Job_to_Code { get; set; }
        public string Ship_to_Code { get; set; }
        public string Payment_Terms { get; set; }
        public string Inquiry_Status_Remarks { get; set; }
        public string Inquiry_Status { get; set; }
    }

    public class SPInqCustDetails
    {
        public string CompanyNo { get; set; }

        public string CompanyName { get; set; }

        public string CustNo { get; set; }

        public string CustName { get; set; }

        public string OutstandingDue { get; set; }

        public string Address { get; set; }

        public string Address_2 { get; set; }

        public string City { get; set; }

        public string Post_Code { get; set; }

        public string PANNo { get; set; }

        public string Country_Region_Code { get; set; }

        public List<SPInqShiptoAddressRes> ShiptoAddress { get; set; }

        public List<SPInqJobtoAddressRes> JobtoAddress { get; set; }

    }

    public class SPInqAddressOData
    {
        [JsonProperty("@odata.context")]
        public string Metadata { get; set; }
        public string value { get; set; }
    }

    public class SPInqNextNoOData
    {
        [JsonProperty("@odata.context")]
        public string Metadata { get; set; }
        public string value { get; set; }
    }

    public class SPInqShiptoAddress
    {
        public string customerno { get; set; }
    }

    public class SPInqShiptoAddressRes
    {
        public string Code { get; set; }

        public string Address { get; set; }

    }

    public class SPInqJobtoAddress
    {
        public string customerno { get; set; }
    }

    public class SPInqJobtoAddressRes
    {
        public string Code { get; set; }

        public string Address { get; set; }

    }

    public class SPInqProductDetails
    {
        public string No { get; set; }

        public string Description { get; set; }

        public string Base_Unit_of_Measure { get; set; }

        public List<SPInqProductPackingStyle> ProductPackingStyle { get; set; }
    }

    public class SPInqProductPackingStyle
    {
        public string Packing_Style_Code { get; set; }

        public string Packing_Style_Description { get; set; }
    }

    public class SPInqLines
    {
        public string Document_No { get; set; }

        public string Type { get; set; }

        public string Line_No { get; set; }

        public string Product_No { get; set; }

        public string Product_Name { get; set; }

        public string Unit_of_Measure { get; set; }

        public decimal Quantity { get; set; }

        public string PCPL_Packing_Style_Code { get; set; }

        public bool PCPL_Convert_Quote { get; set; }

        public string Delivery_Date { get; set; }
    }

    public class SPInqLinesDetails
    {
        public string Document_No { get; set; }

        public string Line_No { get; set; }

        public string Type { get; set; }

        public string Product_No { get; set; }

        public decimal Quantity { get; set; }

        public string PCPL_Packing_Style_Code { get; set; }

        public bool PCPL_Convert_Quote { get; set; }

        public string Delivery_Date { get; set; }
    }

    public class SPInqLinesPost
    {
        public string Document_No { get; set; }

        public string Type { get; set; }

        public string Product_No { get; set; }

        public decimal Quantity { get; set; }

        public string PCPL_Packing_Style_Code { get; set; }

        public bool PCPL_Convert_Quote { get; set; }

        public string Delivery_Date { get; set; }
    }

    public class SPInqLinesUpdate
    {
        public decimal Quantity { get; set; }

        public string PCPL_Packing_Style_Code { get; set; }

        public string Delivery_Date { get; set; }
    }

    public class SPInqAssignTo
    {
        public string Salesperson_Code { get; set; }
    }

    public class SPInquiryProducts
    {
        public int Line_No { get; set; }
        public string Product_No { get; set; }
        public string Product_Name { get; set; }
        public string Unit_of_Measure { get; set; }
        public string PCPL_Packing_Style_Code { get; set; }
        public decimal Quantity { get; set; }
        public string Delivery_Date { get; set; }
        public string PCPL_Payment_Terms { get; set; }
    }

    public class SPInqLinesDelete
    {
        public string Document_No { get; set; }

        public string Line_No { get; set; }
    }

    public class SPSalesReceivableSetup
    {
        public string PCPL_Inquiry_Customer_Template { get; set; }

        public double PCPL_Interest_Rate_Percent { get; set; }
    }

    public class SPInqNewShiptoAddress
    {
        public string Customer_No { get; set; }
      // public string Code { get; set; }
        //public string Name { get; set; }
        public string Address { get; set; }
        public string Address_2 { get; set; }
        public string Post_Code { get; set; }
        public string PCPL_Area { get; set; }
        public string State { get; set; }
        public string GST_Registration_No { get; set; }
        public string Ship_to_GST_Customer_Type { get; set; }
    }

    public class SPInqNewShiptoAddressRes
    {
        public string Customer_No { get; set; }
        public string Code { get; set; }
        //public string Name { get; set; }
        public string Address { get; set; }
        public string Address_2 { get; set; }
        public string Post_Code { get; set; }
        public string PCPL_Area { get; set; }
        public string State { get; set; }
        public string GST_Registration_No { get; set; }
        public string Ship_to_GST_Customer_Type { get; set; }
        public errorDetails errorDetails { get; set; } = null;
    }

    public class SPInqNewJobtoAddress
    {
        public string Customer_No { get; set; }
        public string Code { get; set; }
        //public string Name { get; set; }
        public string Address { get; set; }
        public string Address_2 { get; set; }
        public string Post_Code { get; set; }
        public string PCPL_Area { get; set; }
        public string State { get; set; }
        public string GST_Registration_No { get; set; }
        public string Job_to_GST_Customer_Type { get; set; }
    }

    public class SPInqNewJobtoAddressRes
    {
        public string Customer_No { get; set; }
        public string Code { get; set; }
        //public string Name { get; set; }
        public string Address { get; set; }
        public string Address_2 { get; set; }
        public string Post_Code { get; set; }
        public string PCPL_Area { get; set; }
        public string State { get; set; }
        public string GST_Registration_No { get; set; }
        public string Job_to_GST_Customer_Type { get; set; }
        public errorDetails errorDetails { get; set; } = null;
    }
}
