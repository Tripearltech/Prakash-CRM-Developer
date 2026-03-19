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
    [RoutePrefix("api/SPActivities")]
    public class SPActivitiesController : ApiController
    {
        [Route("GetAllActivities")]
        public List<SPActivities> GetAllActivities()
        {
            API ac = new API();
            List<SPActivities> customers = new List<SPActivities>();

            var result = ac.GetData<SPActivities>("CustomerCardDotNetAPI", "");

            if (result.Result.Item1.value.Count > 0)
                customers = result.Result.Item1.value;
            
            return customers;
        }
    }
}
