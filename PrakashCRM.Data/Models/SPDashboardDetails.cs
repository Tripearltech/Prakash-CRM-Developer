using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrakashCRM.Data.Models
{
    public class SPDashboardDetails
    {
        public int OrdersCount { get; set; }

        public int InvoicesCount { get; set; }

        public int CustomersCount { get; set; }

        public int ContactsCount { get; set; }

        public int QuotesCount { get; set; }

        public int InquiryCount { get; set; }
        public int ContactManufactorCount { get; set; }
        public int ContactTraderCount { get; set; }
        public int ContactCompanyList { get; set; }
    }

    public class GetBranchWiseTotalSum
    {
        public string LocationCode { get; set; }
        public double Opening_LocationCode { get; set; }
        public double Inward_LocationCode { get; set; }
        public double Outward_LocationCode { get; set; }
        public double Reserved_LocationCode { get; set; }
        public double CLStock_LocationCode { get; set; }
    }
    public class ProductGroupsWise
    {
        public string Code { get; set; }
        public double Opening { get; set; }
        public double Inward { get; set; }
        public double Outward { get; set; }
        public double CLStock { get; set; }
        public double Reserved { get; set; }
        public string Location_Filter_FilterOnly { get; set; }
    }

    public class ItemWise
    {
        public string ItemName { get; set; }
        public double Opening_Item { get; set; }
        public double Inward_Item { get; set; }
        public double Outward_Item { get; set; }
        public double CLStock_Item { get; set; }
        public double Reserved_Item { get; set; }
        public string Location_Filter_FilterOnly { get; set; }
    }

    public class SPInvGenerateDataPost
    {
        public string startdate { get; set; }
        public string enddate { get; set; }
    }

    public class SPInvGenerateDataOData
    {
        [JsonProperty("@odata.context")]
        public string Metadata { get; set; }

        public Boolean value { get; set; }

        public errorDetails errorDetails { get; set; } = null;
    }

    public class SPInvGenerateDetails
    {
        public string startdate { get; set; }
        public string enddate { get; set; }
    }

    public class SPInwardDetails
    {
        public string PCPL_Vendor_Name { get; set; }
        public string PCPL_Mfg_Name { get; set; }
        public string Document_Type { get; set; }
        public string Lot_No { get; set; }
        public string PCPL_Remarks { get; set; }
        public string Document_No { get; set; }
        public string Posting_Date { get; set; }
        public double Remaining_Quantity { get; set; }
        public double Reserved_Quantity { get; set; }
        public string Entry_Type { get; set; }
        public double Quantity { get; set; }
        public string Source_Description { get; set; }
        public string PCPL_Salesperson_Code { get; set; }
        public string PCPL_Original_Buying_Date { get; set; }
        public int No_of_days { get; set; }
        public double Cost_Amount_Actual { get; set; }
        public string Item_Category_Code { get; set; }
        public string Item_Description { get; set; }
        public string Location_Code { get; set; }
        public string Item_No { get; set; }

        public double Outstanding_Quantity { get; set; }
        public bool Positive { get; set; }
    }
    public class SPReservedQtyDetails
    {
        public string Location_Code { get; set; }
        public string Item_Category_Code { get; set; }
        public string Description { get; set; }
        public string Posting_Date { get; set; }
        public string Sell_to_Customer_Name { get; set; }
        public double Outstanding_Quantity { get; set; }
        public string PCPL_Salesperson_Name { get; set; }
        public string PCPL_Remarks { get; set; }
        public bool Positive { get; set; }

    }

    public class SPNonPerfomingCuslist
    {
        public string Customer_No { get; set; }
        public string Customer_Name { get; set; }
        public string Salesperson_Code { get; set; }
    }



    public class SPSelaspersonlist
    {
        public string SalesPerson_Name { get; set; }
        public string SalesPerson { get; set; }

        public double Demand_Qty { get; set; }
        public double Target_Qty { get; set; }
        public double Sales_Qty { get; set; }
        public bool IsSalesPerson { get; set; }
        public double Sales_Percentage_Qty { get; set; }
    }
    public class SPSupportSPlist
    {
        public string SalesPerson { get; set; }
        public double Demand_Qty { get; set; }
        public double Target_Qty { get; set; }
        public double Sales_Qty { get; set; }
        public double Sales_Percentage_Qty { get; set; }
    }
    public class SPProductlist
    {
        public string Product_Name { get; set; }
        public string Customer_Name { get; set; }
        public double Product_Total_Target_Qty { get; set; }
        public double Product_Total_Sales_Qty { get; set; }
        public double Product_Sales_Percentage_Qty { get; set; }
        public bool IsSalesPerson { get; set; }
        public bool IsProduct { get; set; }
        public bool IsIncludTop10Product { get; set; }
    }

    public class CombinedSalesData
    {
        public List<SPSelaspersonlist> Salespersons { get; set; }
        public List<SPSelaspersonlist> SupportSPs { get; set; }
        public List<SPProductlist> Products { get; set; }
        public List<SPSelaspersonlist> ProductsTotalList { get; set; }
    }

    public class SPTodayVisitlist
    {
        public string Week_Plan_Date { get; set; }
        public string Visit_Name { get; set; }
        public string Visit_Sub_Type_Name { get; set; }
        public string Pur_Visit { get; set; }
        public string Salesperson_Code { get; set; }
        public string ContactCompanyName { get; set; }
    }
    public class SPWeeklytasklist
    {
        public string Date { get; set; }
        public string Visit_Name { get; set; }
        public string Visit_SubType_Name { get; set; }
        public string Purpose_Of_Visit { get; set; }
        public string Salesperson_Code { get; set; }
        public string Customer_Name { get; set; }
    }
    public class SPMonthlylist
    {
        public string Visit_Month { get; set; }
        public string Visit_Type { get; set; }
        public string Visit_SubType_Name { get; set; }
        public string No_of_Visit { get; set; }
        public string Salesperson_Code { get; set; }
    }
    public class BusinessTypeSalesPerfomance
    {
        public string Business_Type_Sales_Performance { get; set; }
        public double Number_of_Customer { get; set; }
        public double Sales_in_CR { get; set; }
        public double Percentage { get; set; }
    }
    public class IndustryWiseSalesPerfomance
    {
        public string Industrial_Type { get; set; }
        public double Number_of_Customer { get; set; }
        public double Sales_in_CR { get; set; }
        public double Percentage { get; set; }
    }
    public class Complaint
    {

        public string Entry_Type { get; set; }
        public string Complain_Invoice { get; set; }
        public string No { get; set; }
        public string Contact_Company_Name { get; set; }
        public string Complain_Subject { get; set; }
        public string Com_Date { get; set; }
        public string Root_Analysis { get; set; }
        public string Root_Analysis_date { get; set; }
        public string Corrective_Action { get; set; }
        public string Corrective_Action_Date { get; set; }
        public string Preventive_Action { get; set; }
        public string Preventive_Date { get; set; }
        public string Status { get; set; }
    }
    public class ComplaintReportDailyVisitPlan
    {
        public string Entry_Type { get; set; }
        public string No { get; set; }
        public string Complain_Invoice { get; set; }
        public string Contact_Company_Name { get; set; }
        public string Complain_Subject { get; set; }
        public string Com_Date { get; set; }
        public string Root_Analysis { get; set; }
        public string Root_Analysis_date { get; set; }
        public string Corrective_Action { get; set; }
        public string Corrective_Action_Date { get; set; }
        public string Preventive_Action { get; set; }
        public string Preventive_Date { get; set; }
        public string Status { get; set; }
        public string Appro_Date { get; set; }
        public string Reject_Date { get; set; }
        public string Suggestion { get; set; }
    }
    public class SupportSaleData
    {
        public string Date { get; set; }
        public string Customer_Name { get; set; }
        public string Contact_Name { get; set; }
        public string Item_Description { get; set; }
        public string Primary_Salesperson_Name { get; set; }
        public string Secondary_Salesperson_Name { get; set; }
        public string Primary { get; set; }
        public string Support { get; set; }
        public string Salesperson_Code { get; set; }
        public string Total_Quantity { get; set; }

    }
    public class CombineSupportSaleData
    {

        public List<SupportSaleData> SupportSaleDatas { get; set; }
        public List<SPPCPLEmployeeList> SPPCPLEmployeeLists { get; set; }
        public List<SupportSaleData> SupportReportingSaleDatas { get; set; }

    }
    public class WebsiteLog
    {
        public string First_Name { get; set; }
        public string Email { get; set; }
        public string Phone_No { get; set; }
        public string Last_Modified_At { get; set; }

    }

    public class SPCustomerOutstanding
    {
        public string Location_Code { get; set; }
        public double CollectionuptoMTD { get; set; }
        public decimal CollRecdforthePeriod { get; set; }
        public double TotalCollectionRecdtilltoday { get; set; }
        public double Overdueuptopreviousmonthdue { get; set; }
        public double _x0031_st10thdueofcurrentmonth { get; set; }
        public double _x0031_1th20thdueofcurrentmonth { get; set; }
        public double _x0032_1st30_31stdueofcurrentmonth { get; set; }
        public double AchivementinPercent { get; set; }
        public string Salesperson_Code { get; set; }
        public bool LocationWise { get; set; }
        public bool ISSalesPersonData { get; set; }
        public string Customer_Name { get; set; }
        public string Class { get; set; }
        public decimal Cust_Invoice_Amount { get; set; }
        public decimal ACD_Amt { get; set; }
        public decimal ADD_Amt { get; set; }
        public bool IsCustData { get; set; }
        public string Document_Type { get; set; }
        public string PO_Number { get; set; }
        public string Bill_No { get; set; }
        public string Bill_Date { get; set; }
        public string Product_Dimension { get; set; }
        public string TERMS { get; set; }
        public string Due_Date { get; set; }
        public decimal Invoice_Amt { get; set; }
        public decimal Remaining_Amt { get; set; }
        public int Total_Days { get; set; }
        public int Overdue_Days { get; set; }
        public bool IsInvoiceData { get; set; }
        public double Total_Due_in_Month { get; set; }

    }
    public class DBInquiry
    {
        public string No { get; set; }
        public string Document_Type { get; set; }
        public string PCPL_Sell_to_Customer_Name { get; set; }
        public string Requested_Delivery_Date { get; set; }
        public string Status { get; set; }
        public string Salesperson_Code { get; set; }

    }
    public class DBSalesOrder
    {
        public string No { get; set; }
        public string Document_Type { get; set; }
        public string PCPL_Sell_to_Customer_No { get; set; }
        public string Sell_to_Customer_Name { get; set; }
        public string Requested_Delivery_Date { get; set; }
        public string Salesperson_Code { get; set; }
        public string Document_Date { get; set; }
        public string PCPL_Total_Quantity { get; set; }
        public string Unit_Price { get; set; }
        public string Outstanding_Quantity { get; set; }

    }
    public class DBSaleQuote
    {
        public string Quote_No { get; set; }
        public string Document_Type { get; set; }
        public string Requested_Delivery_Date { get; set; }
        public string Status { get; set; }
        public string Salesperson_Code { get; set; }
        public string Document_Date { get; set; }
        public string Sell_to_Customer_Name { get; set; }

    }
    public class DBSalesInvoice
    {
        public string No { get; set; }
        public string Sell_to_Customer_Name { get; set; }
        public string Document_Date { get; set; }
        public string Salesperson_Code { get; set; }
        public string Total_Quantity { get; set; }
        public string Invoice_Quantity { get; set; }

    }

    public class SPUpdateComplaintStatusResponse
    {
        public string No { get; set; }
        public string Status { get; set; }
        public string Entry_Type { get; set; }

    }

    public class SPComplainstatusUpdate
    {
        public string Status { get; set; }
        public string No { get; set; }

    }
    public class CombineInquiryManagement
    {
        public List<InquiryManagement> InquiryManagements { get; set; }
        public List<InquiryManagement> EmployeeWiseInquiryManagements { get; set; }
        public List<SPPCPLEmployeeList> SPPCPLEmployeeLists { get; set; }
    }
    public class InquiryManagement
    {
        public string Sales_Person_Name { get; set; }
        public string Total_Inquiry { get; set; }
        public string Total_Sales_Quote { get; set; }
        public string Total_Send_SMS { get; set; }
        public string Total_Send_Email { get; set; }
        public string Total_Confirm_Quote { get; set; }
        public string Inquiry_Conversion_Ratio { get; set; }
        public string Quote_Conversion_Ratio { get; set; }
        public string Quote_to_SMS { get; set; }
        public string Quote_to_Email { get; set; }
    }
    public class InquirySalesPersonQuotes
    {
        public string No { get; set; }
        public string Document_Date { get; set; }
        public string Sell_to_Customer_Name { get; set; }
        public string Sell_to_Contact { get; set; }
        public string Due_Date { get; set; }
        public string Amount { get; set; }

    }
    public class InquirySalesPersonInquiry
    {
        public string No { get; set; }
        public string Document_Date { get; set; }
        public string Sell_to_Customer_Name { get; set; }
        public string Sell_to_Contact { get; set; }
        public string Payment_Terms_Code { get; set; }
        public string PCPL_Inquiry_Remarks { get; set; }
        public string Ship_to_Code { get; set; }
        public string Ship_to_Address { get; set; }
        public string Ship_to_Address_2 { get; set; }
        public string Ship_to_City { get; set; }
        public string Ship_to_Country_Region_Code { get; set; }

    }
    public class DBitemwisetotalqty
    {
        public string Item_No { get; set; }
        public string SalesPerson_Code { get; set; }
        public string Product_Manager { get; set; }
        public string Sales_Person_Qty { get; set; }
        public string Support_Person { get; set; }
        public string Salesperson_Name { get; set; }


    }
    public class SPTransporterDashboard
    {
        public string Posting_Date { get; set; }
        public string Document_No { get; set; }
        public string Description { get; set; }
        public string Quantity { get; set; }
        public string Location { get; set; }
        public string Destination_Location { get; set; }
        public string Transporter_Name { get; set; }
        public string Vehicle_No { get; set; }
        public string LR_RR_No { get; set; }
        public string Freight_Amount { get; set; }
        public string Loading_Amount { get; set; }
        public string Remarks { get; set; }
    }

    public class CombineDailyVisitMonthWise
    {
        public List<DailyVisitMonthWise> DailyVisitMonthWises { get; set; }
        public List<DailyVisitMonthWise> EmployeeDailyVisitMonthWise { get; set; }
        public List<SPPCPLEmployeeList> EmployeePartyList { get; set; }
    }
    public class DailyVisitMonthWise
    {
        public string SalesPerson { get; set; }
        public string SalesPerson_Name { get; set; }
        public string Total_Time_HH_MM { get; set; }
        public string No_of_Visit_Personal { get; set; }
        public string Total_Kilometers { get; set; }
        public string Month_Year { get; set; }
        //public string Visit_Date { get; set; }
        //public string No__of_Personal_Visit { get; set; }
        //public string SalespersonCodeFilter_FilterOnly { get; set; }
        //public string Date_FilterOnly { get; set; }
    }
    public class TaskPerformance
    {
        public string SalesPerson_Code { get; set; }
        public string SalesPerson_Name { get; set; }
        public string Daily_Visit_in_Last_Monthwise { get; set; }
        public string Targeted { get; set; }
        public string Achievement { get; set; }
        public string Percentage { get; set; }
        public string Pending { get; set; }


    }
    public class CombineTaskPerformance
    {
        public List<TaskPerformance> TaskPerformancesList { get; set; }
        public List<TaskPerformance> TaskPerformanceReportingList { get; set; }
        public List<SPPCPLEmployeeList> SPPCPLEmployeeLists { get; set; }

    }
    public class SalesPerformance
    {
        public string Product_Name { get; set; }
        public string Branch_Name { get; set; }
        public string Annual_target { get; set; }
        public string June_Target { get; set; }
        public string June_Sales { get; set; }
        public string June_Percent { get; set; }
        public string Up_to_June_Sales { get; set; }
        public string Annual_Percent { get; set; }

    }
    public class StockManagement
    {
        public string Item_No { get; set; }
        public string Description { get; set; }
        public string Branch { get; set; }
        public string Location_Code { get; set; }
        public string Packing_Style_Code { get; set; }
        public string Inventory { get; set; }
        public string Qty_on_Purch_Order { get; set; }
        public string Qty_on_Sales_Order { get; set; }
        public string Closing_Stock { get; set; }
        public string Packing_Unit { get; set; }
        public string Packing_MRP_Price { get; set; }
        public string Expected_Receipt_Qty { get; set; }
        public string Expected_Shipment_Qty { get; set; }
        public string Item_Location_Total { get; set; }
        public string Location_Total { get; set; }
        public string Total { get; set; }
        public string Final_Total { get; set; }
        public string Branch_Total { get; set; }

    }
    public class CombineStockManagement
    {
        public List<StockManagement> BranchProductWise { get; set; }
        public List<StockManagement> BranchWiseTotalList { get; set; }
        public List<StockManagement> LocationWiseTotalList { get; set; }

    }
    public class PendingWarehouseSales
    {
        public string Document_No { get; set; }
        public string Name_of_Customer { get; set; }
        public string Product { get; set; }
        public double Quantity { get; set; }
        public string Remarks { get; set; }
        public string Document_Type { get; set; }
    }
    public class PendingWarehousePurchese
    {
        public string Document_No { get; set; }
        public string Name_of_Vendor { get; set; }
        public string Product { get; set; }
        public double Quantity { get; set; }
        public string Remarks { get; set; }
        public string Document_Type { get; set; }
    }

}