using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Data;
using System.ComponentModel.DataAnnotations;

namespace PrakashCRM.Data.Models
{
    public class SPPostedSalesInvoiceList
    {
        public string No { get; set; }

        public string Posting_Date { get; set; }

        public string Due_Date { get; set; }

        public decimal Amount { get; set; }

        public decimal PCPL_Amount_Including_GST { get; set; }

        public decimal Remaining_Amount { get; set; }

        public string Closed { get; set; }
        public string Sell_to_Customer_No { get; set; }
        public string Sell_to_Customer_Name { get; set; }

    }

}
