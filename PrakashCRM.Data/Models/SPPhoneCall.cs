using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrakashCRM.Data.Models
{
    public class SPPhoneCall
    {
        public string Subject { get; set; }
        public DateTime Call_To  { get; set; }
        public string Phone_No { get; set; }
        public DateTime Due_Date { get; set; }
        public string Direction { get; set; }
        public string Category { get; set; }
        public string Regarding { get; set; }
        public string Description { get; set; }
        public string Notes { get; set; }

    }
}
