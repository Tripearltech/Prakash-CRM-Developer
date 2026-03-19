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
    [RoutePrefix("api/SPBranches")]
    public class SPBranchesController : ApiController
    {
        [Route("GetAllBranches")]
        public List<SPBranches> GetAllBranches()
        {
            API ac = new API();
            List<SPBranches> branches = new List<SPBranches>();

            var result = ac.GetData<SPBranches>("BranchListDotNetAPI", ""); // and Contact_Business_Relation eq 'Customer'

            if (result.Result.Item1.value.Count > 0)
                branches = result.Result.Item1.value;

            return branches;
        }
    }
}
