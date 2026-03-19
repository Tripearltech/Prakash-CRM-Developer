using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrakashCRM.Data.Models
{
    public class PrintCustomerLedgerReportResponse
    {
        public string value {  get; set; }
    }
    public class PrintCustomerLedgerReportRequest
    {
        public string customerno {  get; set; }
        public string fromdate {  get; set; }
        public string todate {  get; set; }
    }
    public class SPCustomerReport
    {
        public string No { get; set; }
        public string Name { get; set; }
        public List<SPCustomerReport> customerlist { get; set; }
    }

}
