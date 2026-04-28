using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrakashCRM.Data.Models
{
    public class SPDailyVisitDetails
    {
        public string IsDailyVisitEdit { get; set; }
        public string No { get; set; }
        public string Financial_Year { get; set; }
        public string Date { get; set; }
        public string Contact_Company_No { get; set; }
        public string Contact_Person_No { get; set; }
        public string Primary_Contact_Company_Email { get; set; }
        public string Primary_Contact_Company_No { get; set; }
        public string Contact_Company_Name { get; set; }
        //public string Consumer_Name { get; set; }
        //public string Contact_Per { get; set; }
        public string Visit_Type { get; set; }
        public string Visit_Name { get; set; }
        public string Visit_SubType_No { get; set; }
        public string Visit_SubType_Name { get; set; }
        public string Entry_Type { get; set; }
        public string Feedback { get; set; }
        public string Competitor_Name { get; set; }
        public string Mode_of_Visit { get; set; }
        public string Complain_Subject { get; set; }
        public string Complain_Products { get; set; }
        public string Complain_Invoice { get; set; }
        public string Com_Date { get; set; }
        public string Complain_Assign_To { get; set; }
        public string Root_Analysis { get; set; }
        public string Root_Analysis_date { get; set; }
        public string Corrective_Action { get; set; }
        public string Corrective_Action_Date { get; set; }
        public string Preventive_Action { get; set; }
        public string Preventive_Date { get; set; }
        public string Market_Update { get; set; }
        public string Market_Update_Date { get; set; }
        public double Payment_Amt { get; set; }
        public string Payment_Date { get; set; }
        public string Payment_Remarks { get; set; }
        public string Salesperson_Code { get; set; }
        public string Remarks { get; set; }
        public string Suggestion { get; set; }
        public string Status { get; set; }
        public string Event_No { get; set; }
        public string Topic_Name { get; set; }
        public int Week_No { get; set; }
        public string Week_Start_Date { get; set; }
        public string Week_End_Date { get; set; }
        public bool Is_PDC { get; set; }
        public bool IsActive { get; set; }

        public SPDailyVisitExpanse dailyVisitExpense { get; set; }

        public List<SPVEWeeklyDailyPlanProds> dailyVisitProds { get; set; }
        public string Pur_Visit { get; set; }

        public int DailyVisitExpNo { get; set; }
        public int DailyVisitNo { get; set; }
        public string Visit_Date { get; set; }
        public string Start_Time { get; set; }
        public string Start_Minutes { get; set; }
        public string StartTimeAMPM { get; set; }
        public string End_Time { get; set; }
        public string End_Minutes { get; set; }
        public string EndTimeAMPM { get; set; }
        public string Total_Time { get; set; }
        public string Start_km { get; set; }
        public string End_km { get; set; }
        public string Total_km { get; set; }

        public int DailyVisitProdNo { get; set; }
        public string Product_No { get; set; }
        public int Quantity { get; set; }
        public string Unit_of_Measure { get; set; }
        public string Competitor { get; set; }
    }

    public class SPDailyVisit
    {
        public string No { get; set; }
        public string Financial_Year { get; set; }
        public string Date { get; set; }
        public string Contact_Company_No { get; set; }
        public string Contact_Company_Name { get; set; }
        public string Contact_Person_No { get; set; }
        public string Contact_Person_Name { get; set; }
        public string Visit_Type { get; set; }
        public string Visit_Name { get; set; }
        public string Visit_SubType_No { get; set; }
        public string Visit_SubType_Name { get; set; }
        public string Feedback { get; set; }
        //public string Competitor_Name { get; set; }
        public string Mode_of_Visit { get; set; }
        public string Complain_Subject { get; set; }
        public string Complain_Products { get; set; }
        public string Complain_Invoice { get; set; }
        public string Com_Date { get; set; }
        public string Complain_Assign_To { get; set; }
        public string Root_Analysis { get; set; }
        public string Root_Analysis_date { get; set; }
        public string Corrective_Action { get; set; }
        public string Corrective_Action_Date { get; set; }
        public string Preventive_Action { get; set; }
        public string Preventive_Date { get; set; }
        public string Market_Update { get; set; }
        public string Market_Update_Date { get; set; }
        public double Payment_Amt { get; set; }
        public string Payment_Date { get; set; }
        public string Payment_Remarks { get; set; }
        public string Salesperson_Code { get; set; }
        public string Remarks { get; set; }
        public string Status { get; set; }
        public string Event_No { get; set; }
        public string Event_Name { get; set; }
        public string Topic_Name { get; set; }
        public int Week_No { get; set; }
        public string Week_Start_Date { get; set; }
        public string Week_End_Date { get; set; }
        public string Entry_Type { get; set; }
        public bool Is_PDC { get; set; }
        public string Pur_Visit { get; set; }
        public bool IsActive { get; set; }
        public string DocAttachmentJson { get; set; }
        public errorDetails errorDetails { get; set; } = null;
    }

    public class SPDailyVisitPost
    {
        public string Financial_Year { get; set; }
        public string Date { get; set; }
        public string Contact_Company_No { get; set; }
        public string Suggestion { get; set; }
        public string Contact_Person_No { get; set; }
        public string Visit_Type { get; set; }
        public string Visit_SubType_No { get; set; }
        public string Feedback { get; set; }
        //public string Competitor_Name { get; set; }
        public string Mode_of_Visit { get; set; }
        public string Complain_Subject { get; set; }
        public string Complain_Products { get; set; }
        public string Complain_Invoice { get; set; }
        public string Com_Date { get; set; }
        public string Complain_Assign_To { get; set; }
        public string Root_Analysis { get; set; }
        public string Root_Analysis_date { get; set; }
        public string Corrective_Action { get; set; }
        public string Corrective_Action_Date { get; set; }
        public string Preventive_Action { get; set; }
        public string Preventive_Date { get; set; }
        public string Market_Update { get; set; }
        public string Market_Update_Date { get; set; }
        public double Payment_Amt { get; set; }
        public string Payment_Date { get; set; }
        public string Payment_Remarks { get; set; }
        public string Salesperson_Code { get; set; }
        public string Remarks { get; set; }
        public string Event_No { get; set; }
        public string Topic_Name { get; set; }
        public int Week_No { get; set; }
        public string Week_Start_Date { get; set; }
        public string Week_End_Date { get; set; }
        public string Entry_Type { get; set; }
        public bool Is_PDC { get; set; }
        public string Pur_Visit { get; set; }
        public string DocAttachmentJson { get; set; }
        public bool IsTrackingDocumentAttached { get; set; }
        public bool IsActive { get; set; }
    }

    public class SPDailyVisitExpanse
    {
        public string No { get; set; }
        public string Daily_Visit_Plan_No { get; set; }
        public string Visit_Date { get; set; }
        public string Start_Time { get; set; }
        public string End_Time { get; set; }
        public string Total_Time { get; set; }
        public int Start_km { get; set; }
        public int End_km { get; set; }
        public int Total_km { get; set; }
        public string User_Code { get; set; }
        public bool IsActive { get; set; }
    }
    public class SPDailyVisitExpansePost
    {
        public string Daily_Visit_Plan_No { get; set; }
        public string Visit_Date { get; set; }
        public string Start_Time { get; set; }
        public string End_Time { get; set; }
        public string Total_Time { get; set; }
        public int Start_km { get; set; }
        public int End_km { get; set; }
        public int Total_km { get; set; }
        public string User_Code { get; set; }
        public bool IsActive { get; set; }
    }

    public class SPDailyVisitProducts
    {
        public string No { get; set; }
        public string Product_No { get; set; }
        public string Product_Name { get; set; }
        public int Quantity { get; set; }
        public string Unit_of_Measure { get; set; }
        //public string Competitor { get; set; }
        public bool IsActive { get; set; }
    }

    public class SPDVComplainList
    {
        public string No { get; set; }
        public string Complain_Subject { get; set; }
        public string Complain_Product_No { get; set; }
        public string Complain_Product_Name { get; set; }
        public string Com_Date { get; set; }
        public string Complain_Assign_To { get; set; }
        public string Root_Analysis { get; set; }
        public string Root_Analysis_date { get; set; }
        public string Corrective_Action { get; set; }
        public string Corrective_Action_Date { get; set; }
        public string Preventive_Action { get; set; }
        public string Preventive_Date { get; set; }
    }

    public class SPDVPaymentList
    {
        public string No { get; set; }
        public int Payment_Amt { get; set; }
        public string Payment_Date { get; set; }
        public string Payment_Remarks { get; set; }
    }

    public class SPDailyVisitExpenseForReport
    {
        public string No { get; set; }
        public string Visit_Date { get; set; }
        public string User_Code { get; set; }
        public string SalesPerson_Name { get; set; }
        public string Daily_Visit_Plan_No { get; set; }
        public string Contact_Company_Name { get; set; }
        public string Start_Time { get; set; }
        public string End_Time { get; set; }
        public string Total_Time { get; set; }
        //public string Personal_Visit_No { get; set; }
        public string No_of_Personal_Visit { get; set; }
        public string Start_km { get; set; }
        public string End_km { get; set; }
        public string Total_km { get; set; }

    }

    public class SPDailyVisitInvoiceDetails
    {
        public string No { get; set; }
    }
}
