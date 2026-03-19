using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PrakashCRM.Data.Models
{
    public class SPAdminBusinessPlan
    {
        public string No { get; set; }
        public string Description { get; set; }
        public double AVGPrice1 { get; set; } = new Random().Next(10, 500);
        public double AVGPrice2 { get; set; }
        public double GP1 { get; set; } = new Random().Next(1, 100);
        public double GP2 { get; set; } = 0;
    }
    public class AdminBusinessPlan
    {
        //public string Plan_Year { get; set; }
        //public string Product_No { get; set; }
        //public double Avg_Price { get; set; }
        //public double GP_Percentage { get; set; }
        public string PlanYear { get; set; }
        public string ProductNo { get; set; }
        public double AvgPrice { get; set; }
        public double GPPercentage { get; set; }

    }
    public class AdminBusinessPlanData
    {
        public string Plan_Year { get; set; }
        public string Product_No { get; set; }
        public string Product_Name { get; set; }
        public double Avg_Price { get; set; }
        public double GP_Percentage { get; set; }
        public double Pre_Avg_Price { get; set; }
        public double Pre_GP_Percentage { get; set; }
        public double Actual_Avg_Price { get; set; }
        public double Actual_GP_Percent { get; set; }

    }
    public class AdminBusinessPlanPost
    {
        public string text { get; set; }
    }
    public class AdminBusinessPlanOData
    {
        [JsonProperty("@odata.context")]
        public string Metadata { get; set; }

        public bool value { get; set; }

        public errorDetails errorDetails { get; set; } = null;
    }
    public class AdminBusinessPlanResponse
    {
        public string Plan_Year { get; set; }
        public string Product_No { get; set; }
        public string Product_Name { get; set; }
        public double Avg_Price { get; set; }
        public double GP_Percentage { get; set; }
    }
}
