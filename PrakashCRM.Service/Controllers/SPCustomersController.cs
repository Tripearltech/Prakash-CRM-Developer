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
    [RoutePrefix("api/SPCustomers")]
    public class SPCustomersController : ApiController
    {
        [Route("GetAllCustomers")]
        public List<SPCustomersList> GetAllCustomers(string SPCode, int skip, int top, string orderby, string filter, bool isExport = false)
        {
            API ac = new API();
            List<SPCustomersList> customers = new List<SPCustomersList>();

            if (filter == "" || filter == null)
                filter = "Salesperson_Code eq '" + SPCode + "'";
            else
                filter = filter + " and Salesperson_Code eq '" + SPCode + "'";

            var result = (dynamic)null;

            if(isExport)
                result = ac.GetData<SPCustomersList>("CustomerCardDotNetAPI", filter);
            else
                result = ac.GetData1<SPCustomersList>("CustomerCardDotNetAPI", filter, skip, top, orderby);

            if (result.Result.Item1.value != null && result.Result.Item1.value.Count > 0)
                customers = result.Result.Item1.value;

            return customers;
        }

        [Route("GetApiRecordsCount")]
        public int GetApiRecordsCount(string SPCode, string apiEndPointName, string filter)
        {
            API ac = new API();

            if (filter == "" || filter == null)
                filter = "Salesperson_Code eq '" + SPCode + "'";
            else
                filter = filter + " and Salesperson_Code eq '" + SPCode + "'";

            var count = ac.CalculateCount(apiEndPointName, filter);

            return Convert.ToInt32(count.Result);
        }
    }
}
