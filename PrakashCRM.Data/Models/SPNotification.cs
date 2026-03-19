using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrakashCRM.Data.Models
{
    public class SPNotification
    {
        public string Type { get; set; }

        public string Employee_No { get; set; }

        public string From_Code { get; set; }

        [Required(ErrorMessage = "From Name is required")]
        public string From_Name { get; set; }

        [Required(ErrorMessage = "From Email is required")]
        public string From_E_mail { get; set; }

        [Required(ErrorMessage = "From Mobile is required")]
        public string From_Mobile_No { get; set; }

        [Required(ErrorMessage = "To Name is required")]
        public string To_Name { get; set; }

        [Required(ErrorMessage = "To Email is required")]
        public string To_E_mail { get; set; }

        public string To_Mobile_No { get; set; }

        public string CC_Name { get; set; }

        public string CC_E_mail { get; set; }

        public string CC_Mobile_No { get; set; }

        public string BCC_Name { get; set; }

        public string BCC_E_mail { get; set; }

        public string BCC_Mobile_No { get; set; }

        public bool IsActive { get; set; }
    }
    public class SPDetailsForNotif
    {
        public string First_Name { get; set; }

        public string Mobile_Phone_No { get; set;}

        public string Company_E_Mail { get; set; }
    }
    public class SPNoCodeForNotif
    {
        public string No { get; set; }

        public string PCPL_Employee_Code { get; set; }
    }
}
