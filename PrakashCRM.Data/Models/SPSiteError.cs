using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrakashCRM.Data.Models
{
    public class SPSiteError
    {
        public string Error_Code { get; set; }
        public string Exception_Message { get; set; }
        public string Exception_Stack_Trace { get; set; }
        public string Source { get; set; }
        public string IP_Address { get; set; }
        public string Browser { get; set; }
        public string Description { get; set; }
        public string Web_URL { get; set; }
    }
}
