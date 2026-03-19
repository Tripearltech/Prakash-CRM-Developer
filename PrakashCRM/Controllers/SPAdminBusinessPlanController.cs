using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using PrakashCRM.Data.Models;

namespace PrakashCRM.Controllers
{
    public class SPAdminBusinessPlanController : Controller
    {
        // GET: SPAdminBusinessPlan
        public ActionResult AdminBusinessPlan()
        {
            return View();
        }
        public async Task<JsonResult> GetAdminBusinessPlanData(string previousFinancialYear, string currentFinancialYear)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPAdminBusinessPlan/";

            apiUrl = apiUrl + $"GetAdminBusinessPlan?previousFinancialYear={previousFinancialYear}&currentFinancialYear={currentFinancialYear}";

            HttpClient client = new HttpClient();
            List<AdminBusinessPlanData> items = new List<AdminBusinessPlanData>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                items = Newtonsoft.Json.JsonConvert.DeserializeObject<List<AdminBusinessPlanData>>(data);
            }

            return Json(items, JsonRequestBehavior.AllowGet);
        }

        public async Task<string> PostAdminBusinessPlan(List<AdminBusinessPlan> adminBusinessPlans)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPAdminBusinessPlan/";

            apiUrl += "PostAdminBusinessPlan";

            bool flag = false;
            string resMsg = "";
            HttpClient client = new HttpClient();
            AdminBusinessPlanResponse responseABPlan = new AdminBusinessPlanResponse();
            AdminBusinessPlanOData adminBusinessPlanOData = new AdminBusinessPlanOData();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            string UserObjString = JsonConvert.SerializeObject(adminBusinessPlans);
            var content = new StringContent(UserObjString, Encoding.UTF8, "application/json");

            HttpRequestMessage request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(apiUrl),
                Content = content
            };

            HttpResponseMessage response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                adminBusinessPlanOData = Newtonsoft.Json.JsonConvert.DeserializeObject<AdminBusinessPlanOData>(data);

                if (adminBusinessPlanOData.errorDetails.isSuccess)
                    resMsg = "True";
                else
                    resMsg = "Error:" + adminBusinessPlanOData.errorDetails.message;

                //flag = true;
            }

            return resMsg;
        }

        public async Task<JsonResult> GetItemDropDwon(string Search)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPAdminBusinessPlan/GetItemDropDwon?Search=" + Search;

            HttpClient client = new HttpClient();
            List<SPItemList> itemdropdwon = new List<SPItemList>();

            HttpResponseMessage response = await client.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                itemdropdwon = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPItemList>>(data);
            }

            return Json(itemdropdwon, JsonRequestBehavior.AllowGet);
        }
    }
}