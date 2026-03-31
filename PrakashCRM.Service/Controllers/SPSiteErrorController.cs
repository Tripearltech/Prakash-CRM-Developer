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
                UserID = string.IsNullOrWhiteSpace(siteError.UserID) ? "System" : siteError.UserID,
                CurrentDateTime = string.IsNullOrWhiteSpace(siteError.CurrentDateTime) ? DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss") : siteError.CurrentDateTime,
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

            var result = ac.SaveSiteError(requestModel).Result;
            if (result.Item1 != null)
                requestModel = result.Item1;

            if (result.Item2 != null && !result.Item2.isSuccess)
            {
                var errorMessage = string.IsNullOrWhiteSpace(result.Item2.message)
                    ? "Site error save failed."
                    : result.Item2.message;

                return Content(HttpStatusCode.BadRequest, new { success = false, message = errorMessage });
            }

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

            if ((result.Result.Item2 == null || !result.Result.Item2.isSuccess) &&
                !string.IsNullOrWhiteSpace(orderby) &&
                (orderby.IndexOf("CurrentDateTime", StringComparison.OrdinalIgnoreCase) >= 0 ||
                 orderby.IndexOf("UserID", StringComparison.OrdinalIgnoreCase) >= 0))
            {
                result = ac.GetData1<SPSiteError>("SiteErrorsListDotNetAPI", filter, skip, top, "Error_Code desc");
            }

            if (result.Result.Item1.value.Count > 0)
                siteerror = result.Result.Item1.value;

            foreach (var item in siteerror)
            {
                if (item == null)
                    continue;

                item.CurrentDateTime = NormalizeErrorDateTime(item.CurrentDateTime);
            }
            
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

        private static string NormalizeErrorDateTime(string currentDateTime)
        {
            if (string.IsNullOrWhiteSpace(currentDateTime) ||
                currentDateTime.StartsWith("0001-01-01", StringComparison.OrdinalIgnoreCase) ||
                currentDateTime.StartsWith("01-01-0001", StringComparison.OrdinalIgnoreCase))
            {
                return string.Empty;
            }

            DateTime parsedDateTime;
            if (DateTime.TryParse(currentDateTime, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out parsedDateTime) ||
                DateTime.TryParse(currentDateTime, CultureInfo.CurrentCulture, DateTimeStyles.AllowWhiteSpaces, out parsedDateTime))
            {
                if (parsedDateTime == DateTime.MinValue)
                    return string.Empty;

                return parsedDateTime.ToString("dd-MM-yyyy HH:mm:ss");
            }

            return currentDateTime;
        }

    }
}
