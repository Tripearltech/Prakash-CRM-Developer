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
    [RoutePrefix("api/SPSiteError")]
    public class SPSPSiteErrorController : ApiController
    {
        [HttpPost]
        [Route("LogError")]
        public IHttpActionResult LogError([FromBody] SPSiteError siteError)
        {
            if (siteError == null)
                return BadRequest("Error payload is required.");

            API ac = new API();

            SPSiteError requestModel = new SPSiteError
            {
                Error_Code = string.IsNullOrWhiteSpace(siteError.Error_Code) ? "ERR" : siteError.Error_Code,
                Exception_Message = string.IsNullOrWhiteSpace(siteError.Exception_Message) ? "Unhandled exception" : siteError.Exception_Message,
                Exception_Stack_Trace = string.IsNullOrWhiteSpace(siteError.Exception_Stack_Trace) ? "" : siteError.Exception_Stack_Trace,
                Source = string.IsNullOrWhiteSpace(siteError.Source) ? "MVC" : siteError.Source,
                IP_Address = string.IsNullOrWhiteSpace(siteError.IP_Address) ? ac.getIPAddress() : siteError.IP_Address,
                Browser = string.IsNullOrWhiteSpace(siteError.Browser) ? ac.getBrowser() : siteError.Browser,
                Description = string.IsNullOrWhiteSpace(siteError.Description) ? "Severity: Warning" : siteError.Description,
                Web_URL = string.IsNullOrWhiteSpace(siteError.Web_URL) ? ac.getWebURL() : siteError.Web_URL
            };

            if (requestModel.Error_Code.Length > 20)
                requestModel.Error_Code = requestModel.Error_Code.Substring(0, 20);

            if (requestModel.Exception_Message.Length > 250)
                requestModel.Exception_Message = requestModel.Exception_Message.Substring(0, 250);

            var result = ac.PostItem("SiteErrorsListDotNetAPI", requestModel, requestModel);
            if (result.Result.Item1 != null)
                requestModel = result.Result.Item1;

            return Ok(requestModel);
        }

        [Route("GetSiteError")]
        public List<SPSiteError> GetSiteError(int skip, int top, string orderby, string filter)
        {
            API ac = new API();
            List<SPSiteError> siteerror = new List<SPSiteError>();
            
            if (filter == null)
                filter = "";

            var result = ac.GetData1<SPSiteError>("SiteErrorsListDotNetAPI", filter, skip, top, orderby); 

            if (result.Result.Item1.value.Count > 0)
                siteerror = result.Result.Item1.value;
            
            return siteerror;
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
