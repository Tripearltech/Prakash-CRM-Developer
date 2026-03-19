using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrakashCRM.Data.Models
{
    public class SPTask
    {
        public string Subject { get; set; }
        public DateTime Due_Date  { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public string Regarding { get; set; }
        public string Cusromer { get; set; }
        public string Task_Team { get; set; }
        public string Person_In_Task { get; set; }
        public string Notes { get; set; }

    }
}
