using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrakashCRM.Data.Models
{
    public class SPSiteActivity
    {
        public string Module_Name { get; set; }
        public string Trace_Id { get; set; }
        public string IP_Address { get; set; }
        public string Browser { get; set; }
        public string Description { get; set; }
        public string Web_URL { get; set; }
        public string Company_Code { get; set; }
        public string MAC_Address { get; set; }
        public string Device_Name { get; set; }
    }
}
