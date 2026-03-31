using PrakashCRM.Data.Models;
using PrakashCRM.Service.Classes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace PrakashCRM.Service.Controllers
{
    [RoutePrefix("api/SPSiteActivity")]
    public class SPSiteActivityController : ApiController
    {
        [HttpPost]
        [Route("LogActivity")]
        public IHttpActionResult LogActivity([FromBody] SPSiteActivity siteActivity)
        {
            if (siteActivity == null)
                return BadRequest("Activity payload is required.");

            API ac = new API();
            string incomingIp = siteActivity.IP_Address;
            bool useServiceIp = string.IsNullOrWhiteSpace(incomingIp) || incomingIp == "::1" || incomingIp == "127.0.0.1" || incomingIp == "localhost";

            SPSiteActivity requestModel = new SPSiteActivity
            {
                Activity_User_Name = string.IsNullOrWhiteSpace(siteActivity.Activity_User_Name) ? "System" : siteActivity.Activity_User_Name,
                Activity_Date = PrepareActivityDateForSave(siteActivity.Activity_Date),
                Module_Name = string.IsNullOrWhiteSpace(siteActivity.Module_Name) ? "Unknown" : siteActivity.Module_Name,
                Trace_Id = string.IsNullOrWhiteSpace(siteActivity.Trace_Id) ? "ACT" : siteActivity.Trace_Id,
                IP_Address = useServiceIp ? ac.getIPAddress() : siteActivity.IP_Address,
                Browser = string.IsNullOrWhiteSpace(siteActivity.Browser) ? ac.getBrowser() : siteActivity.Browser,
                Description = string.IsNullOrWhiteSpace(siteActivity.Description) ? "Viewed" : siteActivity.Description,
                Web_URL = string.IsNullOrWhiteSpace(siteActivity.Web_URL) ? ac.getWebURL() : siteActivity.Web_URL,
                Company_Code = string.IsNullOrWhiteSpace(siteActivity.Company_Code) ? "System" : siteActivity.Company_Code,
                MAC_Address = siteActivity.MAC_Address ?? "",
                Device_Name = string.IsNullOrWhiteSpace(siteActivity.Device_Name) ? Environment.MachineName : siteActivity.Device_Name
            };

            if (requestModel.Description.Length > 100)
                requestModel.Description = requestModel.Description.Substring(0, 100);

            var result = ac.SaveSiteActivity(requestModel).Result;
            if (result.Item1 != null)
                requestModel = result.Item1;

            if (result.Item2 != null && !result.Item2.isSuccess)
            {
                var errorMessage = string.IsNullOrWhiteSpace(result.Item2.message)
                    ? "Site activity save failed."
                    : result.Item2.message;

                return Content(HttpStatusCode.BadRequest, new { success = false, message = errorMessage });
            }

            return Ok(requestModel);
        }

        [Route("GetSiteActivity")]
        public List<SPSiteActivity> GetSiteActivity(string SPCode, int skip, int top, string orderby, string filter)
        {
            API ac = new API();
            List<SPSiteActivity> siteactivity = new List<SPSiteActivity>();
            var result = ac.GetData1<SPSiteActivity>("SiteActivitiesListDotNetAPI", filter, skip, top, orderby);

            if ((result.Result.Item2 == null || !result.Result.Item2.isSuccess) &&
                !string.IsNullOrWhiteSpace(orderby) &&
                (orderby.IndexOf("Activity_Date", StringComparison.OrdinalIgnoreCase) >= 0 ||
                 orderby.IndexOf("Activity_User_Name", StringComparison.OrdinalIgnoreCase) >= 0))
            {
                result = ac.GetData1<SPSiteActivity>("SiteActivitiesListDotNetAPI", filter, skip, top, "Module_Name desc");
            }

            if (result.Result.Item1.value.Count > 0)
                siteactivity = result.Result.Item1.value;

            foreach (var item in siteactivity)
            {
                if (item == null)
                    continue;

                item.Activity_Date = NormalizeActivityDate(item.Activity_Date);
            }
            
            return siteactivity;
        }

        [Route("GetApiRecordsCount")]
        public int GetApiRecordsCount(string apiEndPointName, string filter)
        {
            API ac = new API();
            if (filter == null)
                filter = "";

            var count = ac.CalculateCount(apiEndPointName, filter);

            return Convert.ToInt32(count.Result);
        }

        private static string NormalizeActivityDate(string activityDate)
        {
            if (string.IsNullOrWhiteSpace(activityDate) ||
                activityDate.StartsWith("0001-01-01", StringComparison.OrdinalIgnoreCase) ||
                activityDate.StartsWith("01-01-0001", StringComparison.OrdinalIgnoreCase))
            {
                return string.Empty;
            }

            DateTime parsedDate;
            if (DateTime.TryParse(activityDate, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out parsedDate) ||
                DateTime.TryParse(activityDate, CultureInfo.CurrentCulture, DateTimeStyles.AllowWhiteSpaces, out parsedDate))
            {
                if (parsedDate == DateTime.MinValue)
                    return string.Empty;

                return parsedDate.ToString("dd-MM-yyyy");
            }

            return activityDate;
        }

        private static string PrepareActivityDateForSave(string activityDate)
        {
            DateTime parsedDate;
            if (string.IsNullOrWhiteSpace(activityDate))
                parsedDate = DateTime.Now;
            else if (!DateTime.TryParse(activityDate, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out parsedDate) &&
                     !DateTime.TryParse(activityDate, CultureInfo.CurrentCulture, DateTimeStyles.AllowWhiteSpaces, out parsedDate))
                parsedDate = DateTime.Now;

            if (parsedDate == DateTime.MinValue)
                parsedDate = DateTime.Now;

            return parsedDate.ToString("yyyy-MM-dd");
        }
    }
}
