using PrakashCRM.Data.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace PrakashCRM.Controllers
{
    [RedirectingAction]
    public class SPBranchesController : Controller
    {
        // GET: SPBranches
        public ActionResult Index()
        {
            return View();
        }

        public async Task<JsonResult> GetBranchesListData()
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPBranches/";

            apiUrl = apiUrl + "GetAllBranches";

            HttpClient client = new HttpClient();
            List<SPBranches> branches = new List<SPBranches>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                branches = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPBranches>>(data);
            }

            return Json(branches, JsonRequestBehavior.AllowGet);
        }
    }
}