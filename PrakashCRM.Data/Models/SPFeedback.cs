using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace PrakashCRM.Data.Models
{
    public class SPFeedBackHeaderOnSendLink
    {
        //public string Customer_No { get; set; }

        //public string Company_No { get; set; }

        //public bool IsActive { get; set; }

        //public string Employee { get; set; }

        //public string Customer_Name { get; set; }

        //public string Customer_Address { get; set; }

        //public string Contact_No { get; set; }

        public string companyno { get; set; }
        public string employee { get; set; }
        public bool isactive { get; set; }
    }
    public class SPFeedBackHeaderOnSendLinkRes
    {
        //public string No { get; set; }

        //public string Company_No { get; set; }

        //public string Company_Name { get; set; }

        //public string Customer_No { get; set; }

        //public string Customer_Name { get; set; }

        //public string Customer_Address { get; set; }

        //public string Employee { get; set; }

        //public string Contact_No { get; set; }

        //public bool IsActive { get; set; }

        public string systemid { get; set; }
        public string no { get; set; }
        public string companyno { get; set; }
        public string companyname { get; set; }
        public string companyaddress { get; set; }
        public string contactno { get; set; }
        public string contactperson { get; set; }
        public bool isfill { get; set; }

    }

    public class SPFeedBackHeaderList
    {
        public string No { get; set; }

        public string Company_No { get; set; }

        public string Company_Name { get; set; }

        public string Products { get; set; }

        public string Overall_Rating { get; set; }

        public string Suggestion { get; set; }

        public bool IsActive { get; set; }
    }
    public class SPFeedBackLineList
    {
        public string Feedback_Question_No { get; set; }

        public string Feedback_Question { get; set; }

        public string Rating { get; set; }

        public string Comments { get; set; }

    }

    public class SPFeedBacksForDashboard
    {
        public string Company_Name { get; set; }

        public string Overall_Rating { get; set; }

    }

    public class SPFeedBacksForBarChart
    {
        public string Feedback_Question_No_ { get; set; }
        public string Feedback_Question { get; set; }
        public string RatingSum { get; set; }
        public string RatingAvreage { get; set; }
        public string Submitted_On_FilterOnly { get; set; }
        public string Employee_FilterOnly { get; set; }

    }

    public class SPCustomerRating
    {
        public string Feedback_Question_No_ { get; set; }
        public string Feedback_Question { get; set; }
        public string Company_No_ { get; set; }
        public string Company_Name { get; set; }
        public string Rating { get; set; }
        public string Comments { get; set; }
        public string Submitted_On_Filter_FilterOnly { get; set; }
        public string Employee_FilterOnly { get; set; }
        public string Feedback_Question_No_Filter_FilterOnly { get; set; }
        public string Submitted_On { get; set; }
        public string EmployeeName { get; set; }

    }
    public class PieChartRequestModel
    {
        public string fromdate { get; set; }
        public string todate { get; set; }
        public string employeeid { get; set; }
    }
    public class PieChartResponseModel
    {
        public decimal Rating { get; set; }
        public decimal Percentage { get; set; }
        public string CustomerFilledRating { get; set; }
    }

    public class SPCustomerOverallRating
    {
        public string Overall_Rating_Filter { get; set; }
        //public string Customer_No_ { get; set; }
        //public string Customer_Name { get; set; }
        public string Company_No_ { get; set; }
        public string Company_Name { get; set; }
        public string EmployeeName { get; set; }
        public string Submitted_On { get; set; }
        public string Overall_Rating_ { get; set; }
        public string OverallRatingComments { get; set; }
        public string Employee_Filter_FilterOnly { get; set; }
        public string Submitted_On_Filter_FilterOnly { get; set; }

    }
    public class FeedBackList
    {
        public string No { get; set; }
        public string Contact_No { get; set; }
        public string Company_Name { get; set; }
        public string Contact_Person { get; set; }
        public string Products { get; set; }
        public string Overall_Rating { get; set; }
        public string Overall_Rating_Comments { get; set; }
        public string Suggestion { get; set; }
        public string Submitted_On { get; set; }
        public string Employee_Name { get; set; }
        public string Employee { get; set; }
        public string Feedback_Question { get; set; }
        public string Rating { get; set; }
        public string Comments { get; set; }


    }
    public class FeedbBackLines
    {
        public string Feedback_Question_No { get; set; }
        public string Feedback_Question { get; set; }
        public string Rating { get; set; }
        public string Comments { get; set; }

    }
    public class FeedbBackQuestion
    {
        public string No { get; set; }
        public string Feedback_Question { get; set; }
        public string Order_No { get; set; }
        public bool IsActive { get; set; }

    }

    public class CustomerDropdown 
    {
        public string Company_No { get; set; }
        public string Company_Name { get; set; }
        public List<CustomerDropdown> customerlists { get; set; }
    }
}
