using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrakashCRM.Data.Models
{
    public class SPMeeting
    {
        public string Subject { get; set; }
        public DateTime Meeting_To { get; set; }
        public DateTime From_Date { get; set; }
        public DateTime To_Date { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public string Regarding { get; set; }
        public string Meeting_With { get; set; }
        public string Notes { get; set; }

    }
}
