using PrakashCRM.Data.Models;
using PrakashCRM.Service.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;

namespace PrakashCRM.Service.Controllers
{
    [RoutePrefix("api/SPNotification")]
    public class SPNotificationController : ApiController
    {
        [Route("GetAllSPEmployeeCode")]
        public List<SPNoCodeForNotif> GetAllSPEmployeeCode()
        {
            API ac = new API();
            List<SPNoCodeForNotif> spNoCode = new List<SPNoCodeForNotif>();

            var result = ac.GetData<SPNoCodeForNotif>("EmployeesDotNetAPI", "Salespers_Purch_Code ne ''");

            if (result.Result.Item1.value.Count > 0)
                spNoCode = result.Result.Item1.value;

            return spNoCode;
        }

        [Route("GetSalespersonDetails")]
        public SPDetailsForNotif GetSalespersonDetails(string FromCode)
        {
            API ac = new API();
            SPDetailsForNotif user = new SPDetailsForNotif();

            var result = ac.GetData<SPDetailsForNotif>("EmployeesDotNetAPI", "No eq '" + FromCode + "'");

            if (result.Result.Item1.value.Count > 0)
                user = result.Result.Item1.value[0];
            
            return user;
        }

        [Route("Notification")]
        public SPNotification Notification(SPNotification requestNotification, bool isEdit, string NotifType, string NotifEmployee_No)
        {
            var ac = new API();
            errorDetails ed = new errorDetails();
            SPNotification responseNotification = new SPNotification();
            var result = (dynamic)null;

            if (isEdit)
            {
                requestNotification.Type = NotifType;
                result = ac.PatchItem("NotificationsListDotNetAPI", requestNotification, responseNotification, "Type='" + NotifType + "',Employee_No='" + NotifEmployee_No + "'");
            }
            else
                result = ac.PostItem("NotificationsListDotNetAPI", requestNotification, responseNotification);

            if (result.Result.Item1.Employee_No != null)
                responseNotification = result.Result.Item1;

            if (result.Result.Item2.message != null)
                ed = result.Result.Item2;

            return responseNotification;
        }

        [Route("GetApiRecordsCount")]
        public int GetApiRecordsCount(string filter, string apiEndPointName)
        {
            API ac = new API();

            //var filter = (dynamic)null;

            //if (FromCode == "All")
            //    filter = "";
            //else
            //    filter = "From_Code eq '" + FromCode + "'";

            var count = ac.CalculateCount(apiEndPointName, filter);

            return Convert.ToInt32(count.Result);
        }

        [Route("GetAllNotificationSetups")]
        public List<SPNotification> GetAllNotificationSetups(int skip, int top, string orderby, string filter, bool isExport = false)
        {
            API ac = new API();
            List<SPNotification> notifications = new List<SPNotification>();

            var result = (dynamic)null;

            filter = filter == null ? "" : filter;

            if (isExport)
                result = ac.GetData<SPNotification>("NotificationsListDotNetAPI", filter);
            else
                result = ac.GetData1<SPNotification>("NotificationsListDotNetAPI", filter, skip, top, orderby);

            if (result.Result.Item1.value.Count > 0)
                notifications = result.Result.Item1.value;

            return notifications;
        }

        [Route("GetNotificationFromTypeAndEmpNo")]
        public SPNotification GetNotificationFromTypeAndEmpNo(string Type, string Employee_No)
        {
            API ac = new API();
            SPNotification notification = new SPNotification();

            var result = ac.GetData<SPNotification>("NotificationsListDotNetAPI", "Type eq '" + Type + "' and Employee_No eq '" + Employee_No + "'");

            if (result.Result.Item1.value.Count > 0)
                notification = result.Result.Item1.value[0];
            
            return notification;
        }

        [Route("GetAllUsersForDDL")]
        public List<SPProfile> GetAllUsersForDDL()
        {
            API ac = new API();
            List<SPProfile> users = new List<SPProfile>();
            
            var result = ac.GetData<SPProfile>("EmployeesDotNetAPI", "");
            
            if (result.Result.Item1.value.Count > 0)
                users = result.Result.Item1.value;

            return users;
        }

        [Route("GetUserFromName")]
        public SPProfile GetUserFromName(string Name)
        {
            API ac = new API();
            SPProfile user = new SPProfile();
            if (Name != null || Name != "") {
                string[] name = Name.Split(' ');

                var result = ac.GetData<SPProfile>("EmployeesDotNetAPI", "First_Name eq '" + name[0] + "' and Last_Name eq '" + name[1] + "'");

                if (result.Result.Item1.value.Count > 0)
                    user = result.Result.Item1.value[0];
            }
            
            return user;
        }
    }
}
