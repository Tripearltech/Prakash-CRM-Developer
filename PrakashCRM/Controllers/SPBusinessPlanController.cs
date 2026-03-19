using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.ExtendedProperties;
using Newtonsoft.Json;
using PrakashCRM.Data.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace PrakashCRM.Controllers
{
    [RedirectingAction]
    public class SPBusinessPlanController : Controller
    {
        // GET: SPBusinessPlan
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult BusinessPlanContactList()
        {
            return View();
        }
        public ActionResult BusinessPlanReport()
        {
            var model = new SPSalespersonDropDwon
            {
                SalesPerson_Name = "",
                Sales_Person = ""
            };

            return View(model);
        }


        public async Task<JsonResult> GetBusinessPlanCustWiseListData(string page, string SPCode, int orderBy, string orderDir, string filter, int skip, int top)
        {

            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPBusinessPlan/";

            string orderByField = "";

            switch (orderBy)
            {
                case 1:
                    orderByField = "Customer_Name " + orderDir;
                    break;
                default:
                    orderByField = "Customer_Name asc";
                    break;
            }

            apiUrl = apiUrl + "GetBusinessPlanCustWise?Page=" + page + "&SPCode=" + SPCode + "&LoggedInUserNo=" + Session["loggedInUserNo"].ToString() + "&skip=" + skip + "&top=" + top + "&orderby=" + orderByField + "&filter=" + filter;

            HttpClient client = new HttpClient();
            List<SPBusinessPlanDetails> businessPlanDetails = new List<SPBusinessPlanDetails>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                businessPlanDetails = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPBusinessPlanDetails>>(data);
            }

            return Json(businessPlanDetails, JsonRequestBehavior.AllowGet);
        }

        public ActionResult BusinessPlan()
        {
            var model = new SPBusinessProductsPost
            {
                Customer_No = ""
            };

            return View(model);
        }

        [HttpPost]
        public async Task<string> PostBusinessPlan(SPBusinessPlan businessPlan)
        {
            bool flag = false;
            string resMsg = "";
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPBusinessPlan/";
            string SPName = Session["loggedInUserFName"] + " " + Session["loggedInUserLName"];
            apiUrl += "BusinessPlan?SPNo=" + Session["loggedInUserSPCode"] + "&SPName=" + SPName;

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            string UserObjString = JsonConvert.SerializeObject(businessPlan);
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
                flag = true;
                SPBusinessPlanDetails businessPlanDetails = new SPBusinessPlanDetails();
                var data = await response.Content.ReadAsStringAsync();
                businessPlanDetails = Newtonsoft.Json.JsonConvert.DeserializeObject<SPBusinessPlanDetails>(data);

                if (businessPlanDetails.errorDetails.isSuccess)
                    resMsg = "True";
                else
                    resMsg = "Error:" + businessPlanDetails.errorDetails.message;

            }

            return resMsg;
        }
        public ActionResult BusinessPlanStatus()
        {
            return View();
        }

        public ActionResult BusinessPlanStatusSPWise()
        {
            return View();
        }

        public ActionResult AssignBusinessPlan()
        {
            return View();
        }

        public async Task<JsonResult> GetAllSalespersonForDDL()
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPBusinessPlan/GetAllSalespersonForDDL";

            HttpClient client = new HttpClient();
            List<SPSalespeoplePurchaser> salesperson = new List<SPSalespeoplePurchaser>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                salesperson = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPSalespeoplePurchaser>>(data);
                salesperson = salesperson.OrderBy(a => a.Name).ToList();
            }

            return Json(salesperson, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetBusinessPlanListDataAllForAssign(int orderBy, string orderDir, string filter)
        {

            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPBusinessPlan/";

            string orderByField = "";

            switch (orderBy)
            {
                case 2:
                    orderByField = "PCPL_Contact_Company_Name " + orderDir;
                    break;
                default:
                    orderByField = "PCPL_Contact_Company_Name asc";
                    break;
            }

            apiUrl = apiUrl + "GetBusinessPlanListDataAllForAssign?orderby=" + orderByField + "&filter=" + filter;

            HttpClient client = new HttpClient();
            List<SPBusinessPlanAssignCustList> businessPlanDetails = new List<SPBusinessPlanAssignCustList>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                businessPlanDetails = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPBusinessPlanAssignCustList>>(data);
            }

            return Json(businessPlanDetails, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetAllProductsForDDL()
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPBusinessPlan/";

            apiUrl = apiUrl + "GetAllProductsForDDL";

            HttpClient client = new HttpClient();
            List<SPItemList> prods = new List<SPItemList>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                prods = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPItemList>>(data);
            }

            return Json(prods, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetBusinessPlanSPList(string filter)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPBusinessPlan/";

            apiUrl += "GetBusinessPlanSPList?LoggedInUserNo=" + Session["loggedInUserNo"].ToString() + "&filter=" + filter;

            HttpClient client = new HttpClient();
            List<SPBusinessPlanSPList> salesperson = new List<SPBusinessPlanSPList>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                salesperson = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPBusinessPlanSPList>>(data);
            }

            return Json(salesperson, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetTotalDemandAndTargetQtyOfAllCust(string SPCode, string filter)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPBusinessPlan/";

            apiUrl += "GetTotalDemandAndTargetQtyOfAllCust?SPCode=" + SPCode + "&filter=" + filter;

            HttpClient client = new HttpClient();
            SPBusinessPlanTotalQtyDetails qtyDetails = new SPBusinessPlanTotalQtyDetails();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                qtyDetails = Newtonsoft.Json.JsonConvert.DeserializeObject<SPBusinessPlanTotalQtyDetails>(data);
            }

            return Json(qtyDetails, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetBusinessReport(string SerachSP, string Year)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPBusinessPlan/";

            apiUrl += "GetBusinessReport?Sales_Person=" + Session["loggedInUserSPCode"].ToString() + "&Year=" + Year + "&No=" + Session["loggedInUserNo"].ToString() + "&SerachSP=" + SerachSP;

            HttpClient client = new HttpClient();
            CombineSPBusinessPlanReport businessreport = new CombineSPBusinessPlanReport();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                businessreport = Newtonsoft.Json.JsonConvert.DeserializeObject<CombineSPBusinessPlanReport>(data);
            }

            return Json(businessreport, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetBusinessReportSP(string SalesPerson, string Year = null)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPBusinessPlan/GetBusinessReportSP?SalesPerson=" + SalesPerson + "&Year=" + Year;

            HttpClient client = new HttpClient();
            List<SPBusinessReportSP> businessreportsp = new List<SPBusinessReportSP>();

            HttpResponseMessage response = await client.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                businessreportsp = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPBusinessReportSP>>(data);
            }

            return Json(businessreportsp, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetSalespersonDropDwon()
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPBusinessPlan/";

            apiUrl += "GetSalespersonDropDwon";

            HttpClient client = new HttpClient();
            List<SPSalespersonDropDwon> salespersondropdwon = new List<SPSalespersonDropDwon>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                salespersondropdwon = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPSalespersonDropDwon>>(data);
            }

            return Json(salespersondropdwon, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetBusinessReportYears()
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPBusinessPlan/GetBusinessReportYears";

            HttpClient client = new HttpClient();
            List<string> years = new List<string>();

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                years = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(data);
            }

            return Json(years, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetItemDropDwon(string Search)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPBusinessPlan/GetItemDropDwon?Search=" + Search;

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