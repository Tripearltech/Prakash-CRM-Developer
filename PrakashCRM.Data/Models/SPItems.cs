using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrakashCRM.Data.Models
{
    public class SPItemList
    {
        public string No { get; set; }

        public string Description { get; set; }

        public string Base_Unit_of_Measure { get; set; }
        public bool Blocked { get; set; }
    }

    public class SPItemPackingStyleDetails
    {
        public string Item_No { get; set; }

        public string Packing_Style_Code { get; set; }

        public string Item_Description { get; set; }

        public string Packing_Style_Description { get; set; }

        public string Packing_Unit { get; set; }

        public double PCPL_MRP_Price { get; set; }

        public double PCPL_Discount { get; set; }

        public double PCPL_Purchase_Cost { get; set; }

        public double PCPL_Previous_Price { get; set; }

        public int PCPL_Purchase_Days { get; set; }

        public string Item_Category_Code { get; set; }

        public bool PCPL_Rate_Change_Update { get; set; }
    }

    public class SPItemRequest
    {
        public int PCPL_Purchase_Days { get; set; }
        public double PCPL_Discount { get; set; }
        public double PCPL_MRP_Price { get; set; }
    }

    public class SPitemUpdateModel
    {
        public int? PCPL_Purchase_Days { get; set; }
        public double? PCPL_Discount { get; set; }
        public double? PCPL_MRP_Price { get; set; }
        public double? PCPL_Purchase_Cost { get; set; }
        public double? PCPL_Previous_Price { get; set; }
        public string Item_No { get; set; }
        public string Packing_Style_Code { get; set; }
    }
}
