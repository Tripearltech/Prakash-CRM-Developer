using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrakashCRM.Data.Models
{
    public class SPTransporter
    {
        public string PCPL_Transporter_No { get; set; }
        public bool PCPL_Apply_Freight_Charges { get; set; }
        public decimal PCPL_Fraight_Charges { get; set; }
        public bool PCPL_Apply_Loading_Charges { get; set; }
        public bool PCPL_Apply_Unloading_Charges { get; set; }
        public string LR_RR_No { get; set; }
        public string LR_RR_Date { get; set; }
        public string PCPL_Driver_Name { get; set; }
        public string PCPL_Driver_Mobile_No { get; set; }
        public string PCPL_Driver_License_No { get; set; }
        public string Vehicle_No { get; set; }
        public string PCPL_Remarks { get; set; }

    }
}
