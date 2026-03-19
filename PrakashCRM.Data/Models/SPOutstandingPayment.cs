using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrakashCRM.Data.Models
{
    public class SPOutstandingPaymentList
    {
        public string Posting_Date { get; set; }
        public string Document_No { get; set; }
        public string PCPL_Customer_Name { get; set; }
        public string Description { get; set; }
        public double Amount_LCY { get; set; }
        public double Remaining_Amt_LCY { get; set; }
        public string Due_Date { get; set; }

    }
    public class CustomerCollectionOut
    {
        public string Customer_Name { get; set; }
        public string Document_No { get; set; }
        public string Customer_No { get; set; }
        public double OverDays { get; set; }
        public double Original_Amount { get; set; }
        public double Remaining_Amount { get; set; }
        public bool IsReceivedAmount { get; set; }
        public double Received_Amount { get; set; }
        public bool IsTotalCustAmt { get; set; }
        public double Total_Customer_Amt { get; set; }
        public double ACD_Amt { get; set; }
        public double Total_Period_Amt { get; set; }
        public double Total_Collection_Amt { get; set; }
        public bool IsLastSixMonthsData { get; set; }
        public string LastSixMonths_Customer_No { get; set; }
        public string LastSixMonths_Document_No { get; set; }
        public string LastSixMonths_Posting_Date { get; set; }
        public string LastSixMonths_DueDate { get; set; }
        public double LastSixMonths_Original_Amt { get; set; }
        public string LastSixMonths_Received_Date { get; set; }
        public double LastSixMonths_Received_Amt { get; set; }
        public double LastSixMonths_ACD_Amt { get; set; }
        public double LastSixMonths_ADD_Amt { get; set; }
        public double LastSixMonths_No_of_Days { get; set; }
        public double LastSixMonths_Total_Coll_Amt { get; set; }
        public double LastSixMonths_Total_ACD_Amt { get; set; }
        public double LastSixMonths_Total_ADD_Amt { get; set; }
        public bool Is_Customer { get; set; }
    }

    public class SPCollGenerateDataOData 
    {
        [JsonProperty("@odata.context")]
        public string Metadata { get; set; }

        public bool value { get; set; }

        public errorDetails errorDetails { get; set; } = null;
    }
    public class SPCollGenerateDataPost
    {
        public bool value { get; set; }

        public string systemdate { get; set; }
    }
    public class SPCollGenerateDetails
    {
        public string systemdate { get; set; }
       // public string enddate { get; set; }
    }
}
