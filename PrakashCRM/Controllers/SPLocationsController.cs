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
    public class SPLocationsController : Controller
    {
        // GET: SPLocations
        public ActionResult Index()
        {
            return View();
        }

        public async Task<JsonResult> GetLocationsListData()
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPLocations/";

            apiUrl = apiUrl + "GetAllLocations";

            HttpClient client = new HttpClient();
            List<SPLocations> locations = new List<SPLocations>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                locations = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPLocations>>(data);
                locations = locations.OrderBy(a => a.Name).ToList();
            }

            return Json(locations, JsonRequestBehavior.AllowGet);
        }
    }
}