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
    public class SPItemsController : Controller
    {
        // GET: SPItems
        public ActionResult Index()
        {
            return View();
        }

        public async Task<JsonResult> GetItemsListData()
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPItems/";

            apiUrl = apiUrl + "GetAllItems";

            HttpClient client = new HttpClient();
            List<SPItemList> items = new List<SPItemList>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                items = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPItemList>>(data);
                items = items.OrderBy(a => a.Description).ToList();
            }

            return Json(items, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ItemPriceChange()
        {
            return View();
        }

    }
}