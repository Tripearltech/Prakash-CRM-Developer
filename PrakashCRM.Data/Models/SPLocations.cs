using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrakashCRM.Data.Models
{
    public class SPLocations
    {
        public string Code { get; set; }

        public string Name { get; set; }

        // Optional fields (may be provided by the API depending on NAV configuration)
        public string Address { get; set; }

        public string Address_2 { get; set; }

        public string City { get; set; }

        public string Post_Code { get; set; }

        // State/region (often returned as County in Business Central)
        public string County { get; set; }

        public string Country_Region_Code { get; set; }

        public string Phone_No { get; set; }

        public string E_Mail { get; set; }
    }
}
