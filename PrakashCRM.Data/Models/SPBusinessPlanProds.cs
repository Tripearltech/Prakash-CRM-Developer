using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace PrakashCRM.Data.Models
{
    public class SPBusinessPlanCustWisePost
    {
        public string Status { get; set; }
    }

    public class SPBusinessPlanCustWiseSendApproval
    {
        public string Status { get; set; }

        public DateTime Submitted_On { get; set; }

        public string Approver { get; set; }
    }

    public class SPBusinessPlanSPWiseForApproveReject
    {
        public string planyear { get; set; }
        public string spcode { get; set; }
        public string approved_by_rejected_by { get; set; }
        public string approvedorrejected { get; set; }
        public string rejectedreason { get; set; }
        public DateTime approved_rejected_on { get; set; }
    }

    public class SPBusinessPlanSPWiseOData
    {
        [JsonProperty("@odata.context")]
        public string Metadata { get; set; }
        public string value { get; set; }

        public errorDetails errorDetails { get; set; } = null;
    }

    public class SPBusinessPlanCustWiseForApprove
    {
        public string Approved_By_Rejected_By { get; set; }

        public string Status { get; set; }

        public DateTime Approved_Rejected_On { get; set; }
    }

    public class SPBusinessPlanCustWiseForReject
    {
        public string Approved_By_Rejected_By { get; set; }

        public string Status { get; set; }

        public DateTime Approved_Rejected_On { get; set; }

        public string Rejected_Reason { get; set; }
    }

    public class SPBusinessPlanDetails
    {
        public string Plan_Year { get; set; }

        public string Customer_No { get; set; }
        public string Contact_No { get; set; }

        public string Customer_Name { get; set; }

        public double Prev_Year_Demand_Qty { get; set; }

        public double Prev_Year_Target_Qty { get; set; }

        public double Prev_Year_Achieved_Qty { get; set; }

        public double Total_Demand_Qty { get; set; }

        public double Targeted_Qty { get; set; }

        public string Status { get; set; }

        public string Salesperson_Purchaser { get; set; }

        public string Salesperson_Purchaser_Name { get; set; }

        public string Rejected_Reason { get; set; }
        public double Total_Target_Qty_SP { get; set; }
        public double Total_Demand_Qty_SP { get; set; }
        public double Prev_Year_Total_Demand_Qty_SP { get; set; }
        public double Prev_Year_Total_Target_Qty_SP { get; set; }
        public double Prev_Year_Total_Achieved_Qty { get; set; }

        public errorDetails errorDetails { get; set; } = null;
    }

    public class SPContactProdsBusinessPlan
    {
        public string No { get; set; }
        public string Contact_No { get; set; }
        public string Item_No { get; set; }
        public string Item_Name { get; set; }
        public decimal PCPL_Budget_Price { get; set; }
        public string PCPL_Unit_Of_Measure { get; set; }
        public decimal LastOneYearSaleQty { get; set; }
        public decimal LastOneYearSaleAmt { get; set; }
    }

    public class SPCustBusinessPlan
    {
        public string Product_No { get; set; }
        public string Product_Name { get; set; }
        public string Unit_of_Measure { get; set; }
        public double Demand { get; set; }
        public double Target { get; set; }
        public double PCPL_Target_Revenue { get; set; }
        public double Average_Price { get; set; }
        public double Pre_Year_Demand { get; set; }
        public double Pre_Year_Target { get; set; }
        public double Last_year_Sale_Qty { get; set; }
        public double Last_year_Sale_Amount { get; set; }
        public bool IsActive { get; set; }
        public bool Approved { get; set; }
        public string PCPL_Contact_Company_No { get; set; }
    }

    public class SPBusinessPlanLastYearSale
    {
        public decimal Quantity { get; set; }

        public decimal Sales_Amount_Actual { get; set; }
    }

    public class SPBusinessPlan
    {
        public string Customer_No { get; set; }
        public string PCPL_Plan_Year { get; set; }
        public List<SPBusinessPlanProds> Products { get; set; }
        public bool IsActive { get; set; }
    }

    public class SPBusinessPlanProds
    {
        public string Product_No { get; set; }
        public double Demand { get; set; }
        public double Target { get; set; }
        public double PCPL_Target_Revenue { get; set; }
    }

    public class SPBusinessPlanPost
    {
        //public string Customer_No { get; set; }
        //public string Product_No { get; set; }
        //public string PCPL_Plan_Year { get; set; }
        public double Demand { get; set; }
        public double Target { get; set; }
        public double PCPL_Target_Revenue { get; set; }
        public bool IsActive { get; set; }
    }

    public class SPBusinessPlanRes
    {
        public string Customer_No { get; set; }
        public string PCPL_Contact_Company_Name { get; set; }
        public string Product_No { get; set; }
        public string Product_Name { get; set; }
        public string PCPL_Plan_Year { get; set; }
        public double Demand { get; set; }
        public double Target { get; set; }
        public double PCPL_Target_Revenue { get; set; }
        public double Last_year_Sale_Qty { get; set; }
        public bool IsActive { get; set; }
        public bool Approved { get; set; }
    }

    public class SPBusinessPlanSPList
    {
        public string Salesperson_Purchaser { get; set; }

        public string Salesperson_Purchaser_Name { get; set; }
    }

    public class SPBusinessPlanAssignCustDetails
    {
        public string Customer_No { get; set; }
        public string PCPL_Contact_Company_Name { get; set; }
    }

    public class SPBusinessPlanAssignCustList
    {
        public string Customer_No { get; set; }
        public string PCPL_Contact_Company_Name { get; set; }
        public List<SPBusinessPlanAssignCustProdList> businessPlanAssignCustProds { get; set; }
    }

    public class SPBusinessPlanAssignCustProdList
    {
        public string Product_No { get; set; }
        public string Product_Name { get; set; }
        public string PCPL_Plan_Year { get; set; }
        public double Demand { get; set; }
        public double Target { get; set; }
        public double PCPL_Target_Revenue { get; set; }
        public double Last_year_Sale_Qty { get; set; }
    }

    public class SPBusinessPlanAssignPost
    {

        public string currentsalespersoncode { get; set; }
        public string newsalespersoncode { get; set; }
        public string customerno { get; set; }
        public string planyear { get; set; }

    }

    public class SPBusinessPlanAssignOData
    {
        [JsonProperty("@odata.context")]
        public string Metadata { get; set; }
        public string value { get; set; }

        public errorDetails errorDetails { get; set; } = null;
    }

    public class SPBusinessPlanTotalQtyDetails
    {
        public double totalDemandQty { get; set; }

        public double totalTargetQty { get; set; }
    }
    public class SPBusinessPlanReport
    {
        public string SalesPerson_Name { get; set; }
        public double Demand_Qty { get; set; }
        public double Target_Qty { get; set; }
        public double Sales_Qty { get; set; }
        public double Sales_Percentage_Qty { get; set; }
        public double Target_Amt { get; set; }
        public double Sales_Amt { get; set; }
        public double Sales_Percentage_Amt { get; set; }
        public string Sales_Person { get; set; }
        public string Year { get; set; }
        
    }
    public class CombineSPBusinessPlanReport
    {
        public List<SPBusinessPlanReport> BusinessPlanReports { get; set; }
        public List<SPBusinessPlanReport> EmployeePlanReports { get; set; }
        public List<SPPCPLEmployeeList> SPPCPLEmployeeLists { get; set; }
    }
     
    public class SPPCPLEmployeeList
    {
        public string No { get; set; }
        public string SalespersPurchCode { get; set; }
        public string PCPL_Is_Reporting_Person { get; set; }
        public string PCPL_Salespers_Purch_Name { get; set; }
        public string PCPL_Reporting_Person_No { get; set; }
        public string PCPL_Reporting_Person { get; set; }
    }
    public class SPEmployeeModel
    {
        public string Reporting_Person_No { get; set; }
        public string Reporting_Person { get; set; }
        public bool Is_Reporting_Person { get; set; }
        public bool PCPL_Is_HOD { get; set; }
        
    }
    public class SPBusinessPlanCombinedResponse
    {
        public List<SPBusinessPlanReport> BusinessReport { get; set; }
        public List<SPEmployeeModel> ReportingPersons { get; set; }
    }
    public class SPSalespersonDropDwon
    {
        public string SalesPerson_Name { get; set; }
        public string Sales_Person { get; set; }
    }
    public class SPBusinessProductsPost
    {
        public string Item_No { get; set; }
        public string Customer_No { get; set; }
        public string SalesPerson_Code { get; set; }
        public string Contact_No { get; set; }
    }

    public class SPBusinessReportSP
    {
        public string Year { get; set; }
        public string Sales_Person { get; set; }
        public string Item_No { get; set; }
        public string Customer_No { get; set; }
        public string Customer_Name { get; set; }
        public string Item_Name { get; set; }
        public double Demand_Qty { get; set; }
        public double Target_Qty { get; set; }
        public double Sales_Qty { get; set; }
        public double Sales_Percentage_Qty { get; set; }
        public double Target_Amt { get; set; }
        public double Sales_Amt { get; set; }
        public double Sales_Percentage_Amt { get; set; }
        public string SalesPerson_Name { get; set; }
    }

}
