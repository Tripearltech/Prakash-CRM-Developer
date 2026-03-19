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
    [RoutePrefix("api/SPLocations")]
    public class SPLocationsController : ApiController
    {
        [Route("GetAllLocations")]
        public List<SPLocations> GetAllLocations()
        {
            API ac = new API();
            List<SPLocations> locations = new List<SPLocations>();

            var result = ac.GetData<SPLocations>("LocationCardDotNetAPI", ""); // and Contact_Business_Relation eq 'Customer'

            if (result.Result.Item1.value.Count > 0)
                locations = result.Result.Item1.value;

            return locations;
        }
    }
}
