using PrakashCRM.Data.Models;
using PrakashCRM.Service.Classes;
using System;
using System.Collections.Generic;
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

            var result = ac.PostItem("SiteActivitiesListDotNetAPI", requestModel, requestModel);
            if (result.Result.Item1 != null)
                requestModel = result.Result.Item1;

            return Ok(requestModel);
        }

        [Route("GetSiteActivity")]
        public List<SPSiteActivity> GetSiteActivity(string SPCode, int skip, int top, string orderby, string filter)
        {
            API ac = new API();
            List<SPSiteActivity> siteactivity = new List<SPSiteActivity>();
            var result = ac.GetData1<SPSiteActivity>("SiteActivitiesListDotNetAPI", filter, skip, top, orderby); 

            if (result.Result.Item1.value.Count > 0)
                siteactivity = result.Result.Item1.value;
            
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
    }
}
