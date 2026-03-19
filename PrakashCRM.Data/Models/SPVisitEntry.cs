using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrakashCRM.Data.Models
{
    public class SPWeekPlanNoDetails
    {
        public string Week_No { get; set; }

        public string Week_Start_Date { get; set; }

        public string Week_End_Date { get; set; }

        public int Total_Planned_Visits { get; set; }

        public int Total_Actual_Visits { get; set; }
    }

    public class SPWeekPlanDetailsTypeWise
    {
        public string Visit_Type { get; set; }

        public string Visit_Sub_Type { get; set; }

        public string Visit_Name { get; set; }

        public string Visit_SubType_Name { get; set; }

        public int DailyVisitPlanCount { get; set; }

        public int DailyVisitActualCount { get; set; }
    }

    public class WeekPlanTypeWiseTotal
    {
        public string Visit_Type { get; set; }

        public string Visit_Sub_Type { get; set; }

        public string Visit_Name { get; set; }

        public string Visit_SubType_Name { get; set; }

        public int DailyVisitPlanCount { get; set; }
    }

    public class WeekPlanTypeWiseTotalActual
    {
        public string Visit_Name { get; set; }

        public string Visit_SubType_Name { get; set; }

        public int DailyVisitPlanCount { get; set; }
    }

    public class SPVisitTypes
    {
        public string No { get; set; }

        public string Description { get; set; }
    }

    public class SPVisitSubTypes
    {
        public string No { get; set; }

        public string Description { get; set; }

        public string Visit_Type_Option { get; set; }
    }

    public class VisitEntrySP
    {
        public string Code { get; set; }

        public string Name { get; set; }
    }

    public class SPVEContactCompany
    {
        public string No { get; set; }

        public string Name { get; set; }

        public string E_Mail { get; set; }

        public string PCPL_Primary_Contact_No { get; set; }
    }

    public class SPVEContactPerson
    {
        public string No { get; set; }

        public string Name { get; set; }
    }

    public class SPVEYearMonthPlan
    {
        public string No { get; set; }
        public string Edate { get; set; }
        public string Year { get; set; }
        public string Visit_Type { get; set; }
        public string Visit_Type_Name { get; set; }
        public string Visit_Sub_Type { get; set; }
        public string Visit_Sub_Type_Name { get; set; }
        public string SalesPerson_Code { get; set; }
        public int No_of_Visit { get; set; }
        public int No_of_Actual_Visit { get; set; }
        public bool IsActive { get; set; }
        public errorDetails errorDetails { get; set; } = null;
    }

    public class SPVEYearMonthPlanPost
    {
        public string Edate { get; set; }
        public string Year { get; set; }
        public string Visit_Type { get; set; }
        public string Visit_Sub_Type { get; set; }
        public string SalesPerson_Code { get; set; }

        [Required(ErrorMessage = "No. Of Visit is required.")]
        [Range(0, int.MaxValue, ErrorMessage = "No. Of Visit should be greater than 0")]
        public int No_of_Visit { get; set; }

        public bool IsActive { get; set; }
    }

    public class SPVEMonthlyPlan
    {
        public string No { get; set; }
        public string Yearly_Plan_No { get; set; }
        public string Visit_Month { get; set; }
        public string Visit_Type_No { get; set; }
        public string Visit_Sub_Type_No { get; set; }
        public int No_of_Visit { get; set; }
        public string Visit_Year { get; set; }
        public string Salesperson_Code { get; set; }
        public bool IsActive { get; set; }
    }

    public class SPVEMonthlyPlanPost
    {
        public string Yearly_Plan_No { get; set; }
        public string Visit_Month { get; set; }
        public string Visit_Type_No { get; set; }
        public string Visit_Sub_Type_No { get; set; }
        public int No_of_Visit { get; set; }
        public string Visit_Year { get; set; }
        public string Salesperson_Code { get; set; }
        public bool IsActive { get; set; }
    }

    public class SPVEMonthlyPlanList
    {
        public string No { get; set; }
        public string Visit_Month { get; set; }
        public double No_of_Actual_visit { get; set; }
        public string Visit_Type { get; set; }
        public string Visit_Sub_Type_No { get; set; }
        public string Visit_Sub_Type_Name { get; set; }
        public int No_of_Visit { get; set; }
    }

    public class SPVEWeekSalesPlanDetails
    {
        public string IsWeekPlanEdit { get; set; }
        public string No { get; set; }
        public string Financial_Year { get; set; }
        public string Week_No { get; set; }
        public string Week_Start_Date { get; set; }
        public string Week_End_Date { get; set; }
        public string Week_Plan_Date { get; set; }
        public string Visit_Type { get; set; }
        public string Visit_Sub_Type { get; set; }
        public string Contact_Company_No { get; set; }
        public string Contact_Person_No { get; set; }
        public string Event_No { get; set; }
        public string Topic_Name { get; set; }
        public string Mode_of_Visit { get; set; }
        public string Pur_Visit { get; set; }
        public string Remarks { get; set; }
        public string SalesPerson_Code { get; set; }
        public List<SPVEWeeklyDailyPlanProds> Products { get; set; }

    }

    public class SPVEWeekSalesPlan
    {
        public string No { get; set; }
        public string Financial_Year { get; set; }
        public string Week_No { get; set; }
        public string Week_Start_Date { get; set; }
        public string Week_End_Date { get; set; }
        //public string Month_Detail_No { get; set; }
        public string Week_Plan_Date { get; set; }
        public string SalesPerson_Code { get; set; }
        public string Visit_Type { get; set; }
        public string Visit_Name { get; set; }
        public string Visit_Sub_Type { get; set; }
        public string Visit_Sub_Type_Name { get; set; }
        //public string Week_Day { get; set; }
        //public string Week_Date { get; set; }
        public string Contact_Company_No { get; set; }
        public string ContactCompanyName { get; set; }
        public string Contact_Person_No { get; set; }
        public string Contact_Person_Name { get; set; }
        public string Mode_of_Visit { get; set; }
        public string Pur_Visit { get; set; }
        public string Event_No { get; set; }
        public string Event_Name { get; set; }
        public string Topic_Name { get; set; }
        //public string Target { get; set; }
        public string Remarks { get; set; }
        public string Status { get; set; }
        public bool IsActive { get; set; }
        public string Entry_Type { get; set; }
        public bool IsDailyVisitCreated { get; set; }
        public errorDetails errorDetails { get; set; } = null;
    }

    public class SPVEWeekSalesPlanPost
    {

        public string Financial_Year { get; set; }
        public string Week_No { get; set; }
        public string Week_Start_Date { get; set; }
        public string Week_End_Date { get; set; }
        public string Week_Plan_Date { get; set; }
        public string Visit_Type { get; set; }
        public string Visit_Sub_Type { get; set; }
        public string Contact_Company_No { get; set; }
        public string Contact_Person_No { get; set; }
        public string Event_No { get; set; }
        public string Topic_Name { get; set; }
        public string Mode_of_Visit { get; set; }
        public string Remarks { get; set; }
        public string SalesPerson_Code { get; set; }
        public string Pur_Visit { get; set; }
        public bool IsActive { get; set; }

    }

    public class SPVEWeekSalesPlanUpdate
    {
        public string Week_Plan_Date { get; set; }
        public string Visit_Type { get; set; }
        public string Visit_Sub_Type { get; set; }
        public string Contact_Company_No { get; set; }
        public string Contact_Person_No { get; set; }
        public string Event_No { get; set; }
        public string Topic_Name { get; set; }
        public string Mode_of_Visit { get; set; }
        public string Pur_Visit { get; set; }
        public string Remarks { get; set; }
    }

    public class SPVEWeekPlanWeekNos
    {
        public string Week_No { get; set; }
        public string From_Start { get; set; }
        public string To_End { get; set; }
    }

    public class SPWeeklyDailyPlanEvents
    {
        public string Event_No { get; set; }
        public string Event_Name { get; set; }
    }

    public class SPVEWeeklyDailyPlanProds
    {
        public string No { get; set; }
        public string Daily_Visit_Plan_No { get; set; }
        public string Product_No { get; set; }
        public string Product_Name { get; set; }
        public double Quantity { get; set; }
        public string Unit_of_Measure { get; set; }
        public string Weekly_No { get; set; }
        public string Financial_Year { get; set; }
        public string Competitor { get; set; }
        public bool IsActive { get; set; }
    }

    public class SPVEWeeklyDailyPlanProdsPost
    {
        public string Daily_Visit_Plan_No { get; set; }
        public string Product_No { get; set; }
        public double Quantity { get; set; }
        public string Weekly_No { get; set; }
        public string Financial_Year { get; set; }
        public string Competitor { get; set; }
        public bool IsActive { get; set; }
    }

    public class SPVEWeeklyDailyPlanProdsUpdate
    {
        public double Quantity { get; set; }
    }

    public class SPVECompetitors
    {
        public string No { get; set; }
        public string competitor_Name { get; set; }
    }

    public class SPVECompetitorPost
    {
        public string competitor_Name { get; set; }
        public bool IsActive { get; set; }
    }

    public class SPVEContactProducts
    {
        public string Item_No { get; set; }
        public string Item_Name { get; set; }
    }

    public class SPVEInvoiceProducts
    {
        public string No { get; set; }
        public string Description { get; set; }
    }

    public class SPVEContactProductDetails
    {
        public string PCPL_Unit_Of_Measure { get; set; }

        public string Competitor { get; set; }
    }

    public class SPVEProductDetails
    {
        public string UOM { get; set; }

        public string Competitor { get; set; }
    }
    //public class SPVisitALLDataDDL
    //{
    //    public string No { get; set; }
    //    public string Entry_Type { get; set; }

    //    public string Date { get; set; }

    //    public string Contact_Company_No { get; set; }
    //    public string Contact_Company_Name { get; set; }

    //    public string Contact_Person_No { get; set; }
    //    public string Contact_Person_Name { get; set; }
    //    public string Contact_Per { get; set; }

    //    public string Visit_Type { get; set; }
    //    public string Visit_Name { get; set; }

    //    public string Visit_SubType_No { get; set; }
    //    public string Visit_SubType_Name { get; set; }

    //    public string Feedback { get; set; }
    //    public string Mode_of_Visit { get; set; }
    //    public string Pur_Visit { get; set; }

    //    public string Complain_Subject { get; set; }

    //    public string Corrective_Action { get; set; }
    //    public string Corrective_Action_Date { get; set; }

    //    public string Preventive_Action { get; set; }
    //    public string Preventive_Date { get; set; }

    //    public string Complain_Assign_To { get; set; }
    //    public string Complain_Invoice { get; set; }
    //    public string Complain_Products { get; set; }

    //    public string Market_Update { get; set; }
    //    public string Market_Update_Date { get; set; }

    //    public decimal Payment_Amt { get; set; }
    //    public string Payment_Date { get; set; }
    //    public string Payment_Remarks { get; set; }

    //    public string Remarks { get; set; }

    //    public string Salesperson_Code { get; set; }
    //    public string User_Code { get; set; }

    //    public string Appro_Date { get; set; }
    //    public string Reject_Date { get; set; }

    //    public string Suggestion { get; set; }
    //    public string Status { get; set; }

    //    public string Event_No { get; set; }
    //    public string Event_Name { get; set; }
    //    public string Topic_Name { get; set; }

    //    public string Customer_Code { get; set; }
    //    public string Contact_Code { get; set; }

    //    public string Week_No { get; set; }
    //    public string Week_Start_Date { get; set; }
    //    public string Week_End_Date { get; set; }

    //    public string Financial_Year { get; set; }

    //    public bool Is_PDC { get; set; }
    //    public bool IsActive { get; set; }

    //    public string Created_By { get; set; }
    //    public string CreatedOn { get; set; }

    //    public string Modified_By { get; set; }
    //    public string ModifiedOn { get; set; }

    //    public string Start_Time { get; set; }
    //    public string End_Time { get; set; }
    //    public string Total_Time { get; set; }

    //    public int Start_Km { get; set; }
    //    public int End_Km { get; set; }
    //    public int Total_Km { get; set; }
    //}
}
