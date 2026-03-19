using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrakashCRM.Data.Models
{
    public class SPRoles
    {
        public string No { get; set; }

        [Required(ErrorMessage = "Role Name is required")]
        public string Role_Name { get; set; }

        public bool IsActive { get; set; }
    }
    public class SPRolesResponse
    {
        public string No { get; set; }

        public string Role_Name { get; set; }

        public bool IsActive { get; set; }
    }
    public class SPRoleList
    {
        public string No { get; set; }

        public string Role_Name { get; set; }

        public bool IsActive { get; set; }
    }
    public class SPUserRoleRelationList
    {
        [Required(ErrorMessage = "User Relation Role ID is required")]
        public int User_Relation_Role_ID { get; set; }

        [Required(ErrorMessage = "User Security ID is required")]
        public string User_Security_ID { get; set; }

        [Required(ErrorMessage = "User Name is required")]
        public string User_Name { get; set; }

        [Required(ErrorMessage = "Role ID is required")]
        public string Role_ID { get; set; }

        [Required(ErrorMessage = "Role Name is required")]
        public string Role_Name { get; set; }

        [Required(ErrorMessage = "Full Name is required")]
        public string Full_Name { get; set; }
    }
}
