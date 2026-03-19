using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrakashCRM.Data.Models
{
    public class SPMenus
    {
        public string No { get; set; }

        [Required(ErrorMessage = "Menu Name is required")]
        public string Menu_Name { get; set; }

        public string Parent_Menu_No { get; set; }

        public string Parent_Menu_Name { get; set; }

        [Required(ErrorMessage = "Serial No is required")]
        public string Serial_No { get; set; }

        public string Type { get; set; }

        [Required(ErrorMessage = "Class Name is required")]
        public string ClassName { get; set; }

        public bool IsActive { get; set; }
    }
    public class SPMenusResponse
    {
        public string No { get; set; }
        
        public string Menu_Name { get; set; }

        public string Parent_Menu_No { get; set; }
       
        public string Serial_No { get; set; }

        public string Type { get; set; }

        public string ClassName { get; set; }

        public bool IsActive { get; set; }
    }
    public class SPMenuList
    {
        public SPMenuList()
        {
            subMenuList = new HashSet<SPSubMenuList>();
        }

        public string No { get; set; }

        public string Menu_Name { get; set; }

        public string Parent_Menu_No { get; set; }

        public string Parent_Menu_Name { get; set; }

        public string Serial_No { get; set; }

        public string Type { get; set; }

        public string Path { get; set; }

        public string ClassName { get; set; }

        public bool IsActive { get; set; }

        public virtual ICollection<SPSubMenuList> subMenuList { get; set; }

        public int subSubListCnt { get; set; }

    }

    public class SPSubMenuList
    {
        public SPSubMenuList()
        {
            subSubMenuList = new HashSet<SPSubSubMenuList>();
        }

        public string No { get; set; }

        public string Menu_Name { get; set;}

        public virtual ICollection<SPSubSubMenuList> subSubMenuList { get; set; }

    }

    public class SPSubSubMenuList
    {
        public string No { get; set; }

        public string Menu_Name { get; set; }
    }

    public class SPParentMenuNo
    {
        public string No { get; set; }

        public string Menu_Name { get; set; }
    }
}
