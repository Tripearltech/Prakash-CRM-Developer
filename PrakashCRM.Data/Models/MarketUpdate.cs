using System.Collections.Generic;
//using System.Web.Mvc;

namespace PrakashCRM.Data.Models
{
    public class SPMarketUpdateList
    {
        public string Entry_No { get; set; }
        public string Update_Date { get; set; }
        public string Update { get; set; }
        public string Employee_Code { get; set; }
        public string Employee_Name { get; set; }
    }

    public class SPMarketUpdateRequest
    {
        public string Update_Date { get; set; }
        public string Update { get; set; }
        public string Employee_Code { get; set; }
    }
    public class SPMarketUpdateResponse
    {
        public string Entry_No { get; set; }
        public string Update_Date { get; set; }
        public string Update { get; set; }

    }

    public class SPMarketUpdate
    {
        public string Update_Date { get; set; }
        public string Update { get; set; }
        public string Employee_Code { get; set; }
    }
}
