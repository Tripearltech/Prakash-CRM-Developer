using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrakashCRM.Data.Models
{
    public class SPUserTask
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public string MultiLineTextControl { get; set; }
        public string Created_DateTime { get; set; }
        public string Assigned_To_User_Name { get; set; }
        public string Due_DateTime { get; set; }
        public string Start_DateTime { get; set; }
        public string Priority { get; set; }
    }
    public class SPUserTaskPost
    {
        public string Title { get; set; }
        public string MultiLineTextControl { get; set; }
        public string Created_DateTime { get; set; }
        public string Assigned_To_User_Name { get; set; }
        public string Due_DateTime { get; set; }
        public string Start_DateTime { get; set; }
        public string Priority { get; set; }
    }
}
