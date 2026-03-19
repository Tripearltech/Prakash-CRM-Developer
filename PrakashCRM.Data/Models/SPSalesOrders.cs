using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrakashCRM.Data.Models
{
    public class SPSalesOrdersList
    {
        public string No { get; set; }

        public string Status { get; set; }

        public string Document_Date { get; set; }

        public double Amount { get; set; }

        public double PCPL_Amount_Including_GST { get; set; }
        public string Sell_to_Customer_No { get; set; }
        public string Sell_to_Customer_Name { get; set; }

    }
}
