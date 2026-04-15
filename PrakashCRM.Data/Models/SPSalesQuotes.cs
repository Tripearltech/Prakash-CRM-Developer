//using System;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

namespace PrakashCRM.Data.Models
{
    public class SPSalesQuotesList
    {

        public string No { get; set; }

        public string Sell_to_Customer_Name { get; set; }

        public string Order_Date { get; set; }

        public string Due_Date { get; set; }

        public string Location_Code { get; set; }

        //public string Requested_Delivery_Date { get; set; }

        //public string Payment_Method_Code { get; set; }

        public string Payment_Terms_Code { get; set; }

        public double Amount { get; set; }

        public double PCPL_Total_Qty { get; set; }

        public double TPTPLTotal_Ordered_Qty { get; set; }

        public double TPTPLTotal_Invoiced_Qty { get; set; }

        public double TPTPL_Short_Closed_Qty { get; set; }

        public string TPTPL_Schedule_status { get; set; }

        public string SellToEmail { get; set; }

        public bool TPTPL_Short_Close { get; set; }

        public string TPTPL_SC_Reason_Setup_Value { get; set; }

        public string PCPL_Status { get; set; }

        public string PCPL_Rejected_Reason { get; set; }

        public string PCPL_Rejected_Reason_HOD { get; set; }

        public bool IsPriceUpdated { get; set; }

        public double TPTPL_Balance_Qty { get; set; }

    }


    public class SPSQListForApproveReject
    {
        public string No { get; set; }

        public string Sell_to_Customer_Name { get; set; }

        public string Order_Date { get; set; }

        public string Payment_Terms_Code { get; set; }

        public string PCPL_Status { get; set; }

        public string PCPL_ApprovalFor { get; set; }

        public string Salesperson_Code { get; set; }

        public string PCPL_SalesPerson_Email { get; set; }
    }

    //public class SPSalesQuotesListWithQuantites
    //{
    //    public string No { get; set; }

    //    public string Sell_to_Customer_Name { get; set; }

    //    public string Order_Date { get; set; }

    //    public string Due_Date { get; set; }

    //    //public string Requested_Delivery_Date { get; set; }

    //    public string Payment_Method_Code { get; set; }

    //    public double Amount { get; set; }

    //    public double TotalQuoteQty { get; set; }

    //    public double TotalScheduledQty { get; set; }

    //    public double TotalInvoicedQty { get; set; }

    //    public double TotalInProcessQty { get; set; }
    //}

    //public class SPSQQuantities
    //{
    //    public string Document_No { get; set; }

    //    public double Quantity { get; set; }

    //    public double TPTPL_Qty_to_Order { get; set; }

    //    public double Outstanding_Quantity { get; set; }
    //}

    public class SPSQLineItems
    {
        public string Description { get; set; }

        public double Quantity { get; set; }

        public double Line_Amount { get; set; }
    }

    //public class SPSalesQuote
    //{
    //    public string No { get; set; }

    //    public string Location_Code { get; set; }

    //    public string Inquiry_Type { get; set; }

    //    public string Sell_to_Customer_Name { get; set; }

    //    public string Sell_to_Contact { get; set; }

    //    [Column(TypeName = "date")]
    //    public DateTime Quote_Date { get; set; }

    //    public double Credit_Limit { get; set; }

    //    public double Used_Credit_Limit { get; set; }

    //    public double Available_Credit_Limit { get; set; }

    //    public string Product_Name { get; set; }

    //    public string Packing_Style { get; set; }

    //    public double Prod_Qty { get; set; }

    //    public double Prod_MRP { get; set; }

    //    public double Basic_Purchase_Cost { get; set; }

    //    public double Pur_Discount { get; set; }

    //    public string Inco_Term { get; set; }

    //    public double Sales_Discount { get; set; }

    //    public string Transport { get; set; }

    //    public string Commission_Payable { get; set; }

    //    public double Transport_Cost { get; set; }

    //    public double Commission { get; set; }

    //    public string Payment_Terms_Code { get; set; }

    //    public double Insurance { get; set; }

    //    public double Total_Cost { get; set; }

    //    [Column(TypeName = "date")]
    //    public DateTime Delivery_Date { get; set; }

    //    public double Sales_Price { get; set; }

    //    public double Credit_Days { get; set; }

    //    public double Margin { get; set; }

    //    public double Interest { get; set; }

    //    public string Consignee_Address { get; set; }

    //    public string GSTPlace_Of_Supply { get; set; }

    //}

    public class SPSQHeaderAndLines
    {
        public string GeneratedSQNo { get; set; }

        public string Location_Code { get; set; }

        //public string Inquiry_Type { get; set; }

        public string Sell_to_Customer_No { get; set; }

        //public string Sell_to_Customer_Name { get; set; }

        public string Sell_to_Contact { get; set; }

        [Column(TypeName = "date")]
        public DateTime Order_Date { get; set; }

        public string Credit_Limit { get; set; }

        public string Used_Credit_Limit { get; set; }

        public string Available_Credit_Limit { get; set; }

        public string Sell_to_Address { get; set; }

        public string Sell_to_City { get; set; }

        public string Sell_to_Post_Code { get; set; }

        public string Salesperson_Code { get; set; }

        public string Prod_No { get; set; }

        //public string Description { get; set; }

        public string Document_No { get; set; }

        public double Unit_Price { get; set; }

        public double Quantity { get; set; }

        public double Line_Amount { get; set; }

        public string Payment_Terms_Code { get; set; }

        public string Unit_of_Measure_Code { get; set; }

        public string GST_Place_Of_Supply { get; set; }
    }


    public class SPSQHeaderAndLinesPost
    {
        public string GeneratedSQNo { get; set; }

        public string Location_Code { get; set; }

        //public string Inquiry_Type { get; set; }

        public string Sell_to_Customer_No { get; set; }

        //public string Sell_to_Customer_Name { get; set; }

        public string Sell_to_Contact { get; set; }

        public string Order_Date { get; set; }

        public string Credit_Limit { get; set; }

        public string Used_Credit_Limit { get; set; }

        public string Available_Credit_Limit { get; set; }

        public string Sell_to_Address { get; set; }

        public string Sell_to_City { get; set; }

        public string Sell_to_Post_Code { get; set; }

        public string Salesperson_Code { get; set; }

        public string Prod_No { get; set; }

        //public string Description { get; set; }

        public string Document_No { get; set; }

        public double Unit_Price { get; set; }

        public double Quantity { get; set; }

        public double Line_Amount { get; set; }

        public string Payment_Terms_Code { get; set; }

        public string Unit_of_Measure_Code { get; set; }

        public string GST_Place_Of_Supply { get; set; }
    }

    public class SPSQHeader
    {
        public string No { get; set; }

        public string PCPL_Inquiry_No { get; set; }

        public string Location_Code { get; set; }

        public string Sell_to_Customer_No { get; set; }

        public string Sell_to_Contact_No { get; set; }

        public string Sell_to_Customer_Name { get; set; }

        public string Sell_to_Contact { get; set; }

        public string PCPL_Contact_Person { get; set; }

        public string PCPL_Contact_Person_Name { get; set; }

        public string Order_Date { get; set; }

        public string Quote_Valid_Until_Date { get; set; }

        public string Customer_Credit_Limit_LCY { get; set; }

        public string Customer_Balance_Due_LCY { get; set; }

        public string Customer_Balance_LCY { get; set; }

        public string Sell_to_Address { get; set; }

        public string Sell_to_City { get; set; }

        public string Sell_to_Post_Code { get; set; }

        public string Salesperson_Code { get; set; }

        //public string Payment_Method_Code { get; set; }

        public string Transport_Method { get; set; }

        public string PCPL_Location_Post_Code { get; set; }
        public string Payment_Terms_Code { get; set; }

        public string Shipment_Method_Code { get; set; }

        public string Ship_to_Code { get; set; }

        public string PCPL_Job_to_Code { get; set; }

        public string ItemLineNo { get; set; }

        public string WorkDescription { get; set; }

        public string PCPL_Target_Date { get; set; }

        public string PCPL_Status { get; set; }

        public string PCPL_Submitted_On { get; set; }

        public string TPTPL_Schedule_status { get; set; }

        public bool TPTPL_Short_Close { get; set; }

        public string TPTPL_SC_Reason_Setup_Value { get; set; }

        public string PCPL_ApprovalFor { get; set; }

        public string PCPL_SalesPerson_Email { get; set; }

        public errorDetails errorDetails { get; set; } = null;
        public string PCPL_ApproverHOD { get; set; }
        public string Address_2 { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string PCPL_State_Name { get; set; }
        public string Country_Region_Code { get; set; }
    }

    public class SPSQHeaderDetails
    {
        public string IsSQEdit { get; set; }

        public string NoSeriesCode { get; set; }

        public string QuoteNo { get; set; }

        public string InquiryNo { get; set; }

        public string LocationCode { get; set; }

        public string ContactCompanyNo { get; set; }

        public string ContactCompanyName { get; set; }

        public string ContactPersonNo { get; set; }

        public string ContactPersonName { get; set; }

        public string CustomerNo { get; set; }

        public string OrderDate { get; set; }

        public string ValidUntillDate { get; set; }

        public string TotalCreditLimit { get; set; }

        public string UsedCreditLimit { get; set; }

        public string AvailableCreditLimit { get; set; }

        public string OutstandingDue { get; set; }

        //public string PaymentMethodCode { get; set; }

        public string TransportMethodCode { get; set; }

        public string PCPL_Location_Post_Code { get; set; }
        public string PaymentTermsCode { get; set; }

        public string ConsigneeAddress { get; set; }

        public string ShipmentMethodCode { get; set; }

        public string ShiptoCode { get; set; }

        public string JobtoCode { get; set; }

        public string JobtoAddress { get; set; }

        public string SalespersonCode { get; set; }

        public string CustomerTemplateCode { get; set; }

        public string JustificationFor { get; set; }

        public string JustificationDetails { get; set; }

        public string ApprovalFor { get; set; }

        public string TargetDate { get; set; }

        public bool ShortcloseStatus { get; set; }

        public string SCRemarksSetupValue { get; set; }

        public byte[] zipApprovalFormatFile { get; set; }

        public string SQApprovalFormURL { get; set; }

        public string Status { get; set; }

        public string WorkDescription { get; set; }

        public string SalespersonEmail { get; set; }

        public List<SPSQLinesWithInvQty> Products { get; set; }

        public List<SPSQLines> ProductsRes { get; set; }

    }

    public class SPSQHeaderDetailsRes
    {
        public string LocationCode { get; set; }

        public string QuoteNo { get; set; }

        public string CustomerName { get; set; }

        public string ContactName { get; set; }

        public string QuoteDate { get; set; }

        public string TotalCreditLimit { get; set; }

        public string AvailableCreditLimit { get; set; }

        public string OutstandingDue { get; set; }

        public string PaymentTermsCode { get; set; }

        //public string ConsigneeAddress { get; set; }

        public string ShipmentMethodCode { get; set; }

        public string ShiptoCode { get; set; }

        public string JobtoCode { get; set; }
    }

    public class SPSQHeaderPost
    {
        //public string No { get; set; }

        public string PCPL_Inquiry_No { get; set; }

        public string Location_Code { get; set; }

        public string Sell_to_Customer_No { get; set; }

        public string Sell_to_Contact_No { get; set; }

        public string Sell_to_Contact { get; set; }

        public string PCPL_Contact_Person { get; set; }

        public string Order_Date { get; set; }

        public string Quote_Valid_Until_Date { get; set; }

        public string Salesperson_Code { get; set; }

        //public string Payment_Method_Code { get; set; }

        //public string Transport_Method { get; set; }

        public string Payment_Terms_Code { get; set; }

        public string Shipment_Method_Code { get; set; }

        public string Ship_to_Code { get; set; }

        public string PCPL_Job_to_Code { get; set; }

        public bool PCPL_IsInquiry { get; set; }

        public string WorkDescription { get; set; }

        public string PCPL_Target_Date { get; set; }

        public string PCPL_Status { get; set; }

        public string PCPL_Submitted_On { get; set; }

        public string PCPL_Approver { get; set; }

        public string PCPL_ApprovalFor { get; set; }

        public string PCPL_ApproverHOD { get; set; }

    }

    public class SPSQHeaderPostWithCustTemplateCode
    {
        //public string No { get; set; }

        public string PCPL_Inquiry_No { get; set; }

        public string Location_Code { get; set; }

        public string Sell_to_Customer_No { get; set; }

        public string Sell_to_Contact_No { get; set; }

        public string Sell_to_Contact { get; set; }

        public string PCPL_Contact_Person { get; set; }

        public string Order_Date { get; set; }

        public string Quote_Valid_Until_Date { get; set; }

        public string Salesperson_Code { get; set; }

        //public string Payment_Method_Code { get; set; }

        //public string Transport_Method { get; set; }

        public string Payment_Terms_Code { get; set; }

        public string Shipment_Method_Code { get; set; }

        public string Ship_to_Code { get; set; }

        public string PCPL_Job_to_Code { get; set; }

        public bool PCPL_IsInquiry { get; set; }

        public string Sell_to_Customer_Templ_Code { get; set; }

        public string WorkDescription { get; set; }

        public string PCPL_Target_Date { get; set; }

        public string PCPL_Status { get; set; }

        public string PCPL_Submitted_On { get; set; }

        public string PCPL_Approver { get; set; }

        public string PCPL_ApprovalFor { get; set; }

        public string PCPL_ApproverHOD { get; set; }

    }

    public class SPSQHeaderUpdate
    {
        public string Quote_Valid_Until_Date { get; set; }

        public string PCPL_Contact_Person { get; set; }

        //public string Payment_Method_Code { get; set; }

        //public string Transport_Method { get; set; }

        public string Payment_Terms_Code { get; set; }

        public string Shipment_Method_Code { get; set; }

        public string Ship_to_Code { get; set; }

        public string PCPL_Job_to_Code { get; set; }

        public string WorkDescription { get; set; }

        public string PCPL_Status { get; set; }

        public string PCPL_Submitted_On { get; set; }

        public string PCPL_Approver { get; set; }

        public string PCPL_ApprovalFor { get; set; }

        public string PCPL_ApproverHOD { get; set; }
    }

    public class SPSQLines
    {
        public errorDetails errorDetails;

        public string Line_No { get; set; }

        public string Document_No { get; set; }
        //public double PCPL_Amount_Including_GST { get; set; } = 0;

        //Product No
        public string No { get; set; }

        public string Description { get; set; }

        public string Location_Code { get; set; }

        public string Type { get; set; }

        public double PCPL_Concentration_Rate_Percent { get; set; }

        public double Net_Weight { get; set; }

        public double PCPL_Liquid_Rate { get; set; }

        public bool PCPL_Liquid { get; set; }

        public double Unit_Price { get; set; }

        public double Unit_Cost_LCY { get; set; }

        public double Quantity { get; set; }

        public double TPTPL_Qty_to_Order { get; set; }

        public double Outstanding_Quantity { get; set; }

        public string Unit_of_Measure_Code { get; set; }

        public double Line_Amount { get; set; }

        public string Delivery_Date { get; set; }

        public string GST_Place_Of_Supply { get; set; }

        public string PCPL_Packing_Style_Code { get; set; }

        public double PCPL_Sales_Discount { get; set; }

        public string PCPL_Commission_Payable { get; set; }

        public string PCPL_Commission_Payable_Name { get; set; }

        public string PCPL_Commission_Type { get; set; }

        public double PCPL_Commission { get; set; }

        public double PCPL_Commission_Amount { get; set; }

        public string PCPL_Transport_Method { get; set; }

        public double PCPL_Transport_Cost { get; set; }

        public double PCPL_Total_Cost { get; set; }

        public double PCPL_Margin { get; set; }

        public double PCPL_Margin_Percent { get; set; }

        public double New_Price { get; set; }

        public double New_Margin { get; set; }
        public bool Price_Updated { get; set; }

        public double PCPL_Sales_Price { get; set; }

        public int PCPL_Credit_Days { get; set; }

        public double PCPL_Interest { get; set; }

        public double PCPL_Interest_Rate { get; set; }

        public double PCPL_Basic_Price { get; set; }

        public double PCPL_Purchase_Cost { get; set; }

        public double PCPL_MRP { get; set; }

        public double Total_Amount_Excl_VAT { get; set; }

        public bool Drop_Shipment { get; set; }

        public string PCPL_Vendor_No { get; set; }

        public string PCPL_Vendor_Name { get; set; }

        public bool TPTPL_Short_Closed { get; set; }
        public double PCPL_Packing_MRP_Price { get; set; }
        public string PCPL_Packing_UOM { get; set; }
        public string TPTPL_External_Document_No { get; set; }
        public string PCPL_GST_Amt { get; set; }
        public List<SPSQInvQtyReserve> InvQuantities { get; set; }
    }

    public class SPSQLinesWithInvQty
    {

        public string Document_No { get; set; }

        public string ItemLineNo { get; set; }

        //Product No
        public string No { get; set; }

        public string ProductName { get; set; }

        public string Location_Code { get; set; }

        public string Type { get; set; }

        public double Unit_Price { get; set; }

        public bool IsLiquidProd { get; set; }

        public double PCPL_Concentration_Rate_Percent { get; set; }

        public double Net_Weight { get; set; }

        public double PCPL_Liquid_Rate { get; set; }

        public double Quantity { get; set; }

        //public string GST_Place_Of_Supply { get; set; }

        public string PCPL_Packing_Style_Code { get; set; }

        public string PCPL_Transport_Method { get; set; }

        public double PCPL_Transport_Cost { get; set; }

        public string PCPL_Commission_Payable { get; set; }

        public string PCPL_Commission_Type { get; set; }

        public double PCPL_Commission { get; set; }

        public double PCPL_Commission_Amount { get; set; }

        //public string PCPL_Discount_Type { get; set; }

        //public double PCPL_Discount { get; set; }

        public double PCPL_Sales_Discount { get; set; }

        public int PCPL_Credit_Days { get; set; }

        public double PCPL_Margin { get; set; }

        public double PCPL_Margin_Percent { get; set; }

        // Item price change support
        public double New_Price { get; set; }
        public double New_Margin { get; set; }
        public bool Price_Updated { get; set; }

        public double PCPL_Interest { get; set; }

        public double PCPL_Interest_Rate { get; set; }

        //public double PCPL_Basic_Price { get; set; }

        public double PCPL_Purchase_Cost { get; set; }

        public double PCPL_MRP { get; set; }

        public string Delivery_Date { get; set; }

        public double PCPL_Total_Cost { get; set; }

        public bool Drop_Shipment { get; set; }

        public string PCPL_Vendor_No { get; set; }

        public string InqProdLineNo { get; set; }
        //public double PCPL_Amount_Including_GST { get; set; } = 0;

        public List<SPSQInvQtyReserve> InvQuantities { get; set; }
        public double PCPL_GST_Amt { get; set; }
        public int Line_No { get; set; }
    }

    public class SPSQLinesPost
    {
        public string Document_No { get; set; }

        //Product No
        public string No { get; set; }
        public int Line_No { get; set; }

        //public double PCPL_Amount_Including_GST { get; set; } = 0;

        public string Location_Code { get; set; }

        public string Type { get; set; }

        public double Unit_Price { get; set; }

        public double Quantity { get; set; }

        public string GST_Place_Of_Supply { get; set; }

        public string PCPL_Packing_Style_Code { get; set; }

        public string PCPL_Transport_Method { get; set; }

        public double PCPL_Transport_Cost { get; set; }

        public string PCPL_Commission_Payable { get; set; }

        public string PCPL_Commission_Type { get; set; }

        public double PCPL_Commission { get; set; }

        public double PCPL_Commission_Amount { get; set; }

        //public string PCPL_Discount_Type { get; set; }

        //public double PCPL_Discount { get; set; }

        public double PCPL_Sales_Discount { get; set; }

        public int PCPL_Credit_Days { get; set; }

        public double PCPL_Margin { get; set; }

        public double PCPL_Margin_Percent { get; set; }

        // Item price change support (defaults to 0/false on create)
        public double New_Price { get; set; }
        public double New_Margin { get; set; }
        public bool Price_Updated { get; set; }

        public double PCPL_Interest { get; set; }

        public double PCPL_Interest_Rate { get; set; }

        //public double PCPL_Basic_Price { get; set; }

        public double PCPL_Purchase_Cost { get; set; }

        public double PCPL_MRP { get; set; }

        public string Delivery_Date { get; set; }
        public double Net_Weight { get; set; }

        public double PCPL_Total_Cost { get; set; }

        public bool Drop_Shipment { get; set; }

        public string PCPL_Vendor_No { get; set; }

        public string PCPL_Inquiry_No { get; set; }

        public int PCPL_Inquiry_Line_No { get; set; }
    }


    public class SPSQLiquidLinesPost
    {
        public string Document_No { get; set; }

        //Product No
        public int Line_No { get; set; }
        public string No { get; set; }

        public string Location_Code { get; set; }

        public string Type { get; set; }

        public double PCPL_Concentration_Rate_Percent { get; set; }

        public double Net_Weight { get; set; }

        public double PCPL_Liquid_Rate { get; set; }

        public bool PCPL_Liquid { get; set; }

        //public double Quantity { get; set; }

        public string GST_Place_Of_Supply { get; set; }

        public string PCPL_Packing_Style_Code { get; set; }

        public string PCPL_Transport_Method { get; set; }

        public double PCPL_Transport_Cost { get; set; }

        public string PCPL_Commission_Payable { get; set; }

        public string PCPL_Commission_Type { get; set; }

        public double PCPL_Commission { get; set; }

        public double PCPL_Commission_Amount { get; set; }

        //public string PCPL_Discount_Type { get; set; }

        //public double PCPL_Discount { get; set; }

        public double PCPL_Sales_Discount { get; set; }

        public int PCPL_Credit_Days { get; set; }

        public double PCPL_Margin { get; set; }
        //public double PCPL_Amount_Including_GST { get; set; } = 0;

        public double PCPL_Margin_Percent { get; set; }

        // Item price change support (defaults to 0/false on create)
        public double New_Price { get; set; }
        public double New_Margin { get; set; }
        public bool Price_Updated { get; set; }

        public double PCPL_Interest { get; set; }

        public double PCPL_Interest_Rate { get; set; }

        //public double PCPL_Basic_Price { get; set; }

        public double PCPL_Purchase_Cost { get; set; }

        public double PCPL_MRP { get; set; }

        public string Delivery_Date { get; set; }

        public double PCPL_Total_Cost { get; set; }

        public bool Drop_Shipment { get; set; }

        public string PCPL_Vendor_No { get; set; }

        public string PCPL_Inquiry_No { get; set; }

        public int PCPL_Inquiry_Line_No { get; set; }
    }

    public class SPSQLinesUpdate
    {
        public double Quantity { get; set; }
        public double Unit_Price { get; set; }
        public string PCPL_Packing_Style_Code { get; set; }
        public string Delivery_Date { get; set; }
        public bool Drop_Shipment { get; set; }
        public string PCPL_Vendor_No { get; set; }

        public string GST_Place_Of_Supply { get; set; }
        //
        public string Location_Code { get; set; }

        public string PCPL_Transport_Method { get; set; }

        public double PCPL_Transport_Cost { get; set; }

        public string PCPL_Commission_Payable { get; set; }

        public string PCPL_Commission_Type { get; set; }

        public double PCPL_Commission { get; set; }

        public double PCPL_Commission_Amount { get; set; }

        public double PCPL_Sales_Discount { get; set; }

        public int PCPL_Credit_Days { get; set; }

        public double PCPL_Margin { get; set; }

        public double PCPL_Margin_Percent { get; set; }
        public double New_Price { get; set; }
        public double New_Margin { get; set; }
        public bool Price_Updated { get; set; }

        public double PCPL_Interest { get; set; }

        public double PCPL_Interest_Rate { get; set; }

        public double PCPL_Purchase_Cost { get; set; }

        public double PCPL_MRP { get; set; }

        public double PCPL_Total_Cost { get; set; }

        //
    }

    public class SPSQLiquidLinesUpdate
    {
        public double PCPL_Concentration_Rate_Percent { get; set; }
        public double Net_Weight { get; set; }
        public double PCPL_Liquid_Rate { get; set; }
        //public double Quantity { get; set; }
        //public double Unit_Price { get; set; }
        public string PCPL_Packing_Style_Code { get; set; }
        public string Delivery_Date { get; set; }
        public bool Drop_Shipment { get; set; }
        public string PCPL_Vendor_No { get; set; }
        public string GST_Place_Of_Supply { get; set; }

        //
        public string Location_Code { get; set; }

        public bool PCPL_Liquid { get; set; }

        public string PCPL_Transport_Method { get; set; }

        public double PCPL_Transport_Cost { get; set; }

        public string PCPL_Commission_Payable { get; set; }

        public string PCPL_Commission_Type { get; set; }

        public double PCPL_Commission { get; set; }

        public double PCPL_Commission_Amount { get; set; }

        public double PCPL_Sales_Discount { get; set; }

        public int PCPL_Credit_Days { get; set; }

        public double PCPL_Margin { get; set; }

        public double PCPL_Margin_Percent { get; set; }
        public double New_Price { get; set; }
        public double New_Margin { get; set; }
        public bool Price_Updated { get; set; }

        public double PCPL_Interest { get; set; }

        public double PCPL_Interest_Rate { get; set; }

        public double PCPL_Purchase_Cost { get; set; }

        public double PCPL_MRP { get; set; }

        public double PCPL_Total_Cost { get; set; }

        //
    }

    public class SPSQLinePriceUpdatePatch
    {
        public double PCPL_MRP { get; set; }

        public double PCPL_Margin { get; set; }
        public double New_Price { get; set; }

        public double New_Margin { get; set; }

        public bool Price_Updated { get; set; }
    }

    public class SPSQUser
    {
        public string No { get; set; }
        public string First_Name { get; set; }
        public string Last_Name { get; set; }
        public string Company_E_Mail { get; set; }

        public string Mobile_Phone_No { get; set; }

        public string Phone_No { get; set; }
        public bool PCPL_Is_Documents_Team { get; set; }
    }

    public class SPSQScheduleQty
    {
        public string Document_No { get; set; }
        public string Line_No { get; set; }
        public double TPTPL_Qty_to_Order { get; set; }
        public errorDetails errorDetails { get; set; } = null;
    }

    public class SPSQScheduleQtyPost
    {
        public double TPTPL_Qty_to_Order { get; set; }
        public string PCPL_Remarks { get; set; }
    }

    public class SPSQScheduleOrderPost
    {
        public string scheduledate { get; set; }
        public string documentdate { get; set; }
        public string quoteNo { get; set; }
        public string externaldocumentno { get; set; }
        public string assignto { get; set; }
    }

    public class SPSQScheduleOrderOData
    {
        [JsonProperty("@odata.context")]
        public string Metadata { get; set; }

        public string value { get; set; }

        public errorDetails errorDetails { get; set; } = null;
    }

    public class SPSQScheduleOrder
    {
        public bool value { get; set; }
    }

    public class SPSQHeaderAndLinesFromAddQtyChange
    {
        public string Location_Code { get; set; }

        public string Inquiry_Type { get; set; }

        public string Sell_to_Customer_No { get; set; }

        //public string Sell_to_Customer_Name { get; set; }

        public string Sell_to_Contact { get; set; }

        public string Order_Date { get; set; }

        public string Credit_Limit { get; set; }

        public string Used_Credit_Limit { get; set; }

        public string Available_Credit_Limit { get; set; }

        public string ConsigneeAddress { get; set; }

        public string Salesperson_Code { get; set; }

        public string Prod_No { get; set; }

        //public string Description { get; set; }

        public string Document_No { get; set; }

        public double Unit_Price { get; set; }

        public double Quantity { get; set; }

        public double Line_Amount { get; set; }

        public string GST_Place_Of_Supply { get; set; }
    }



    public class SPSQGetNextNo
    {
        public string noseriescode { get; set; }

        public string usagedate { get; set; }
    }

    public class SPSQGetNextNoRes
    {
        public string value { get; set; }
    }

    public class SPSQPurPayableSetups
    {
        public int PCPL_Interest_Rate { get; set; }
    }
    //public class SPSalesQuotePost
    //{
    //    public string Location_Code { get; set; }

    //    public string Inquiry_Type { get; set; }

    //    public string Sell_to_Customer_No { get; set; }

    //    public string Sell_to_Customer_Name { get; set; }

    //    public string Sell_to_Contact { get; set; }

    //    public string Quote_Date { get; set; }

    //    public double Credit_Limit { get; set; }

    //    public double Used_Credit_Limit { get; set; }

    //    public double Available_Credit_Limit { get; set; }

    //    public string Sell_to_Address { get; set; }

    //    public string Sell_to_City { get; set; }

    //    public string Sell_to_Post_Code { get; set; }

    //    public string Salesperson_Code { get; set; }

    //    public string Description { get; set; }

    //    public string Document_No { get; set; }

    //    public double Quantity { get; set; }

    //    public double Line_Amount { get; set; }

    //    public string GST_Place_Of_Supply { get; set; }

    //    public string Packing_Style { get; set; }

    //    public double Prod_Qty { get; set; }

    //    public double Prod_MRP { get; set; }

    //    public double Basic_Purchase_Cost { get; set; }

    //    public double Pur_Discount { get; set; }

    //    public string Inco_Term { get; set; }

    //    public double Sales_Discount { get; set; }

    //    public string Transport { get; set; }

    //    public string Commission_Payable { get; set; }

    //    public double Transport_Cost { get; set; }

    //    public double Commission { get; set; }

    //    public string Payment_Terms_Code { get; set; }

    //    public double Insurance { get; set; }

    //    public double Total_Cost { get; set; }

    //    public string Delivery_Date { get; set; }

    //    public double Sales_Price { get; set; }

    //    public double Credit_Days { get; set; }

    //    public double Margin { get; set; }

    //    public double Interest { get; set; }

    //    public string Consignee_Address { get; set; }

    //    public string GSTPlace_Of_Supply { get; set; }

    //}

    public class SPSQPaymentMethods
    {
        public string Code { get; set; }

        public string Description { get; set; }
    }

    public class SPSQContacts
    {
        public string No { get; set; }

        public string Name { get; set; }

        public bool Is_Primary { get; set; }
    }

    public class SPSQCreditLimitAndCustDetails
    {
        public string Salesperson_Code;

        public string CompanyNo { get; set; }

        public string CompanyName { get; set; }

        public string CustNo { get; set; }

        public string CustName { get; set; }

        public string CreditLimit { get; set; }

        public string AccountBalance { get; set; }

        public string UsedCreditLimit { get; set; }

        public string AvailableCredit { get; set; }

        public string OutstandingDue { get; set; }

        public string Address { get; set; }

        public string Address_2 { get; set; }

        public string City { get; set; }

        public string Post_Code { get; set; }

        public string PANNo { get; set; }

        public List<SPSQShiptoAddressRes> ShiptoAddress { get; set; }

        public List<SPSQJobtoAddressRes> JobtoAddress { get; set; }
        public object PcplClass { get; set; }
        public object AverageDelayDays { get; set; }
        public int Credit_Limit { get; set; }
    }

    public class SPSQInquiryNos
    {
        public string Inquiry_No { get; set; }
    }

    public class SPSQInquiry
    {
        public string Inquiry_No { get; set; }
        public string Inquiry_Customer_No { get; set; }
        public string Inquiry_Customer_Name { get; set; }
        public string Inquiry_Customer_Contact { get; set; }
        public string Contact_Company_Name { get; set; }
        public string PCPL_Contact_Person { get; set; }
        public string PCPL_Contact_Person_Name { get; set; }
        public string Sell_to_Address { get; set; }
        public string Sell_to_Address_2 { get; set; }
        public string Sell_to_City { get; set; }
        public string Sell_to_Post_Code { get; set; }
        public string PCPL_Job_to_Code { get; set; }
        public string Ship_to_Code { get; set; }
    }

    public class SPSQPaymentTerms
    {
        public string Code { get; set; }

        public string Description { get; set; }

        public string Due_Date_Calculation { get; set; }
    }

    public class SPSQVendors
    {
        public string No { get; set; }

        public string Name { get; set; }
        public string PCPL_Broker { get; set; }
    }

    public class SPSQItemVendors
    {
        public string Vendor_No { get; set; }

        public string Vendor_Name { get; set; }
        public string PCPL_Vendor_Post_Code { get; set; }
    }

    public class SPSQShipmentMethods
    {
        public string Code { get; set; }

        public string Description { get; set; }
    }

    public class SPSQProductDetails
    {
        public string No { get; set; }

        public string Description { get; set; }

        public string Base_Unit_of_Measure { get; set; }

        public string Base_Packing_Style_Code { get; set; }

        public double PCPL_MRP_Price { get; set; }

        public double Unit_Cost { get; set; }

        public double Unit_Price { get; set; }

        public bool PCPL_liquid { get; set; }

        public double Net_Weight { get; set; }

        public double Gross_Weight { get; set; }
        public double PCPL_Purchase_Cost { get; set; }
        public string Packing_Unit { get; set; }

        public List<SPSQProductPackingStyle> ProductPackingStyle { get; set; }
    }

    public class SPSQProductPackingStyle
    {
        public string Packing_Style_Code { get; set; }

        public string Packing_Style_Description { get; set; }

        public double PCPL_MRP_Price { get; set; }

        public int PCPL_Purchase_Days { get; set; }
        public int PCPL_Purchase_Cost { get; set; }
        public string Packing_Unit { get; set; }
    }

    public class SPSQNoSeriesDetails
    {
        public string Series_Code { get; set; }

        public string PCPL_Location_Code { get; set; }
    }

    public class SPSQAddressOData
    {
        [JsonProperty("@odata.context")]
        public string Metadata { get; set; }
        public string value { get; set; }
    }

    public class SPSQShiptoAddress
    {
        public string customerno { get; set; }
    }

    public class SPSQShiptoAddressRes
    {
        public string Code { get; set; }

        public string Address { get; set; }

    }

    public class SPSQSchQtyProds
    {
        public string QuoteNo { get; set; }

        public int ProdLineNo { get; set; }

        public double ScheduleQty { get; set; }
        public string PCPL_Remarks { get; set; }

    }

    public class SPSQScheduleOrderDetails
    {
        public string QuoteNo { get; set; }

        public string ScheduleDate { get; set; }
        public string DocumentDate { get; set; }

        public string ExternalDocNo { get; set; }

        public string AssignTo { get; set; }

        public List<SPSQSchQtyProds> SchQtyProds { get; set; }

        public List<SPSQInvQtyReserve> InvQuantities { get; set; }
    }

    public class SPSQJobtoAddress
    {
        public string customerno { get; set; }
    }

    public class SPSQJobtoAddressRes
    {
        public string Code { get; set; }

        public string Address { get; set; }

    }

    public class SPSQInvDetails
    {
        public string itemno { get; set; }

        public string locationcode { get; set; }

    }

    public class SPSQInvDetailsOData
    {
        [JsonProperty("@odata.context")]
        public string Metadata { get; set; }
        public string value { get; set; }
    }

    public class SPSQInvDetailsRes
    {
        public string ItemNo { get; set; }

        public string ManufactureCode { get; set; }

        public string LotNo { get; set; }

        public double AvailableQty { get; set; }

        public double RequestedQty { get; set; }

        public double UnitCost { get; set; }
    }

    public class SPSQInvQtyReserveOData
    {
        [JsonProperty("@odata.context")]
        public string Metadata { get; set; }

        public string value { get; set; }
    }

    public class SPSQInvQtyReserve
    {
        public string LotNo { get; set; }
        public int Qty { get; set; }
        public string OrderNo { get; set; }
        public string ItemNo { get; set; }
        public int LineNo { get; set; }
        public string LocationCode { get; set; }
    }

    public class SPSQInvQtyReservePost
    {
        public string text { get; set; }
    }

    public class SPSQSalesInvoiceDetails
    {
        public string PCPL_Due_Date { get; set; }

        public string PCPL_Payment_Terms { get; set; }

        public string Posting_Date { get; set; }

        public string Document_No { get; set; }

        public string Description { get; set; }
        public string PCPL_Margin { get; set; }
        public string PCPL_Freight_Charges { get; set; }

        public double Quantity { get; set; }

        public double Unit_Price { get; set; }

        public double Amount { get; set; }
        public double PCPL_Amount_Including_GST { get; set; }
    }

    public class SPSQIncoTerm
    {
        public string Code { get; set; }

        public string Description { get; set; }
    }

    public class SPSQPurDiscountDetails
    {
        public string Vendor_Name { get; set; }

        public double Minimum_Quantity { get; set; }

        public double Maximum_Quantity { get; set; }

        public double Trade_Disount_Amount { get; set; }

        public double Qty_Discount_Amount { get; set; }

        public double Consignee_Discount_Amount { get; set; }

        public double Distance_Discount_Amount { get; set; }
    }

    public class SPSQCostSheet
    {
        public string salesdoctype { get; set; }
        public string salesdocno { get; set; }
        public int doclineno { get; set; }
        public bool frombc { get; set; }
    }

    public class SPSQCostSheetOData
    {
        [JsonProperty("@odata.context")]
        public string Metadata { get; set; }

        public string value { get; set; }
    }

    public class SPSQCostSheetDetails
    {
        public string TPTPL_Line_No { get; set; }
        public string TPTPL_Item_Charge { get; set; }
        public string TPTPL_Description { get; set; }
        public double TPTPL_Quantity { get; set; }
        public double TPTPL_Rate_per_Unit { get; set; }
        public double TPTPL_Amount { get; set; }
        public double SalesPrice { get; set; }
        public double TotalUnitPrice { get; set; }
        public double Margin { get; set; }
        public bool TPTPL_Revenue { get; set; }
    }

    public class SPSQUpdateCostSheet
    {
        public double TPTPL_Rate_per_Unit { get; set; }
    }

    public class SPSQUpdateInqToQuote
    {
        public bool PCPL_Convert_Quote { get; set; }
    }

    public class SPSQUpdateInqStatus
    {
        public string salesquoteno { get; set; }
    }

    public class SPSQUpdateInqStatusOData
    {
        [JsonProperty("@odata.context")]
        public string Metadata { get; set; }

        public string value { get; set; }
    }

    public class SPSQShortclose
    {
        public string salesheader { get; set; }

        public string shortclosereason { get; set; }

        public string shortcloseremarks { get; set; }
    }

    public class SPSQProdShortclose
    {
        public string salesheader { get; set; }

        public string lineno { get; set; }

        public string shortclosereason { get; set; }

        public string shortcloseremarks { get; set; }
    }

    public class SPSQShortcloseOData
    {
        [JsonProperty("@odata.context")]
        public string Metadata { get; set; }

        public string value { get; set; }

        public errorDetails errorDetails { get; set; } = null;
    }

    public class SPSQOrderedQtyDetails
    {
        public string orderNo { get; set; }
        public string OrderDate { get; set; }
        public double Quantity { get; set; }
        public double ScheduleQty { get; set; }
        public string ScheduledDate { get; set; }
        public string PCPL_Remarks { get; set; }
        public string TPTPL_Assign_to { get; set; }
        public string Description { get; set; }
        public string PCPL_Packing_Style_Code { get; set; }

    }

    public class SPSQInvoicedQtyDetails
    {
        public string InvoiceNo { get; set; }
        public string Posting_Date { get; set; }
        public string Vehicle_No_ { get; set; }
        public string PCPL_Dirver_Mobile_No_ { get; set; }
        public string LR_RR_No_ { get; set; }
        public double Quantity { get; set; }
        public string Description { get; set; }
        public string PCPL_Packing_Style_Code { get; set; }
    }

    public class SPSQInProcessQtyDetails
    {
        public string No { get; set; }
        public string PCPL_Transporter_Name { get; set; }
        public string LR_RR_No { get; set; }
        public string Vehicle_No { get; set; }
        public string PCPL_Driver_Mobile_No { get; set; }
        public string PCPL_Remarks { get; set; }
    }

    public class SPSQTransportMethods
    {
        public string Code { get; set; }
        public string Description { get; set; }
    }

    public class SPSQFileUpload
    {
        public string SQFilePath { get; set; }

        public HttpPostedFileBase SQFile { get; set; }
    }
    public class PrintQuoteRequest
    {
        public string docno { get; set; }
    }

    public class PrintQuoteResponse
    {
        public string value { get; set; }
    }

    public class SPSQForApprove
    {
        public string PCPL_Status { get; set; }
        public string PCPL_Approved_By_Rejected_By { get; set; }
        public string PCPL_Approved_Rejected_On { get; set; }
    }

    public class SPSQForReject
    {
        public string PCPL_Status { get; set; }
        public string PCPL_Approved_By_Rejected_By { get; set; }
        public string PCPL_Rejected_Reason { get; set; }
        public string PCPL_Approved_Rejected_On { get; set; }
    }

    public class SPSQForApproveHOD
    {
        public string PCPL_Status { get; set; }
        public string PCPL_ApprovedBy_RejectedBy_HOD { get; set; }
        public string PCPL_Approved_Rejected_On_HOD { get; set; }
    }

    public class SPSQForRejectHOD
    {
        public string PCPL_Status { get; set; }
        public string PCPL_ApprovedBy_RejectedBy_HOD { get; set; }
        public string PCPL_Rejected_Reason_HOD { get; set; }
        public string PCPL_Approved_Rejected_On_HOD { get; set; }
    }

    public class SPSQShortcloseReasons
    {
        public string Entry_No { get; set; }
        public string Short_Close_Reason { get; set; }
    }

    public class SPSQJustificationDetails
    {
        public string No { get; set; }
        public string PCPL_Target_Date { get; set; }
        public string WorkDescription { get; set; }
        public string PCPL_Status { get; set; }
    }

    public class SPSQCompanyIndustry
    {
        public string Business_Type { get; set; }
    }
    public class SPDispacthDetails
    {
        public string No { get; set; }
        public string Posting_Date { get; set; }
        public string Customer_No { get; set; }
        public string Customer_Name { get; set; }
        public string Vehicle_No { get; set; }
        public string LRNo { get; set; }
        public string Quote_No { get; set; }
        public string Order_No { get; set; }
        public string Item_No { get; set; }
        public string Invoice_Qty { get; set; }
        public string Total_Qty { get; set; }
        public string Price { get; set; }
    }

    public class CSOutstandingDuelist
    {
        public string Document_Type { get; set; }
        public string Bill_No { get; set; }
        public string Bill_Date { get; set; }
        public string Product_Name { get; set; }
        public string Terms { get; set; }
        public string Due_Date { get; set; }
        public decimal Invoice_Amount { get; set; }
        public double Remain_Amount { get; set; }
        public double Total_Days { get; set; }
        public double Overdue_Days { get; set; }
        public string Customer_Name { get; set; }
        public string Customer_No { get; set; }
    }

    public class TransporterRateMethods
    {
        public string Rate_Effective_Date { get; set; }

        public string From_Post_Code { get; set; }

        public string To_Post_Code { get; set; }

        public string From_Area { get; set; }

        public string To_Area { get; set; }

        public string UOM { get; set; }

        public double Standard_Weight { get; set; }

        public double Rate_for_Standard_Weight { get; set; }

        public double Rate_above_Standard_Weight { get; set; }

        public string Vehicle_Type { get; set; }

        public string Transporter_Type { get; set; }
        public bool Latest_Rate { get; set; }

    }

    public class SPAvailableQuantity
    {
        public string Lot_No { get; set; }
        public string PCPL_Mfg_Name { get; set; }
        public string Remaining_Quantity { get; set; }
        public string Quantity { get; set; }
        public string Item_No { get; set; }
        public string Location_Code { get; set; }
        public string PCPL_Packing_Style_Code { get; set; }
        public string Cost_Amount_Actual { get; set; }
    }
    public class ReopenSalesQuoteRequest
    {
        public string QuoteNo { get; set; }
    }

    public class QuoteLinePriceActionRequest
    {
        public string QuoteNo { get; set; }
        public int LineNo { get; set; }
        public string Action { get; set; } // "Update" | "NoUpdate"
        public double NewPrice { get; set; }
        public double NewMargin { get; set; }
    }
}