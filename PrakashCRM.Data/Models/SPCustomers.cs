using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrakashCRM.Data.Models
{
    public class SPCustomersList
    {
        public string No { get; set; }

        public string Name { get; set; }

        public string City { get; set; }

        public string Post_Code { get; set; }

        public string Phone_No { get; set; }

        public string E_Mail { get; set; }

        public string Salesperson_Code { get; set; }

        public string ContactName { get; set; }

        public string MobilePhoneNo { get; set; }

        public double Balance_Due_LCY { get; set; }

        public bool IsActive { get; set; }
        public string PCPL_Secondary_SP_Code { get; set; }
        public string PCPL_Class { get; set; }
    }
    public class SPCustomer
    {
        public string No { get; set; }

        public string Name { get; set; }

        public double Balance_LCY { get; set; }

        public double Balance_Due_LCY { get; set; }

        public double Credit_Limit_LCY { get; set; }

        public double PCPL_Credit_Limit_LCY { get; set; }

        public string Address { get; set; }

        public string Address_2 { get; set; }

        public string City { get; set; }

        public string Post_Code { get; set; }

        public string Country_Region_Code { get; set; }

        public string Bill_to_Customer_No { get; set; }

        public string P_A_N_No { get; set; }
        public object PCPL_Class { get; set; }
        public object PCPL_ADD_Average_Delay_Days { get; set; }
    }
}
