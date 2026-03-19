using Newtonsoft.Json;
using PrakashCRM.Data.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
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
    public class SPDashboardController : Controller
    {
        // GET: Dashboard
        public ActionResult Index()
        {
            string path = Server.MapPath("~/SPProfileImages/");
            string fname = Session["loggedInUserFName"].ToString();
            string lname = Session["loggedInUserLName"].ToString();
            ViewBag.Username = fname + " " + lname;
            DirectoryInfo di = new DirectoryInfo(path);
            FileInfo[] smFiles = di.GetFiles("*.*");
            foreach (FileInfo smFile in smFiles)
            {
                if (Path.GetFileNameWithoutExtension(smFile.Name) == Session["loggedInUserNo"].ToString() + "_ProfileImage")
                {
                    Session["SPProfileImage"] = "";
                    Session["SPProfileImage"] = "../SPProfileImages/" + smFile.Name;
                }
            }

            return View();
        }

        public ActionResult DBInquiry()
        {
            return View();
        }

        public ActionResult DBSalesOrder()
        {
            return View();
        }
        public ActionResult DBSalesInvoice()
        {
            return View();
        }

        public ActionResult DBSaleQuote()
        {
            return View();
        }

        #region Feedback
        public async Task<JsonResult> GetAllFeedback()
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPDashboard/GetAllFeedback";

            HttpClient client = new HttpClient();
            List<SPFeedBacksForDashboard> feedBacksForDashboard = new List<SPFeedBacksForDashboard>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                feedBacksForDashboard = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPFeedBacksForDashboard>>(data);
            }

            return Json(feedBacksForDashboard, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Market Update

        public async Task<JsonResult> GetMarketUpdateListData()
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPDashboard/GetAllMarketUpdateDetails";

            HttpClient client = new HttpClient();
            List<SPMarketUpdateList> marketUpdateList = new List<SPMarketUpdateList>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                marketUpdateList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPMarketUpdateList>>(data);
            }

            return Json(marketUpdateList, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Sales Warehouse

        public async Task<JsonResult> GetWarehouseSalesAcceptTaskLis()
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPDashboard/GetWarehouseSalesAcceptedTask?SPCode=" + Session["loggedInUserSPCode"].ToString();

            HttpClient client = new HttpClient();
            List<SPWarehouse> warehouseSales = new List<SPWarehouse>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                warehouseSales = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPWarehouse>>(data);
            }

            return Json(warehouseSales, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetAllSalesLineForPopup(string Document_No)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPDashboard/";

            apiUrl = apiUrl + "GetAllSalesLine?Document_No=" + Document_No;

            HttpClient client = new HttpClient();
            List<SPWarehouseSalesLine> warehouseSalesLine = new List<SPWarehouseSalesLine>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                warehouseSalesLine = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPWarehouseSalesLine>>(data);
            }

            return Json(warehouseSalesLine, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Purchase Warehouse

        public async Task<JsonResult> GetWarehousePurchaseAcceptTaskList()
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPDashboard/GetWarehousePurchaseAcceptedTask?SPCode=" + Session["loggedInUserSPCode"].ToString();

            HttpClient client = new HttpClient();
            List<SPWarehousePurchase> warehousePurchase = new List<SPWarehousePurchase>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                warehousePurchase = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPWarehousePurchase>>(data);
            }

            return Json(warehousePurchase, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetAllPurchaseLineForPopup(string Document_No)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPDashboard/";

            apiUrl = apiUrl + "GetAllPurchaseLine?Document_No=" + Document_No;

            HttpClient client = new HttpClient();
            List<SPWarehousePurchaseLine> warehousePurchaseLine = new List<SPWarehousePurchaseLine>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                warehousePurchaseLine = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPWarehousePurchaseLine>>(data);
            }

            return Json(warehousePurchaseLine, JsonRequestBehavior.AllowGet);
        }

        #endregion
        public async Task<JsonResult> DailyVisitsDetails()
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPDashboard/";

            apiUrl += "DailyVisitsDetails?SPCode=" + Session["loggedInUserSPCode"].ToString();

            HttpClient client = new HttpClient();
            List<SPDailyVisitDetails> dailyVisitData = new List<SPDailyVisitDetails>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                dailyVisitData = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPDailyVisitDetails>>(data);

                if (dailyVisitData.Count > 0)
                {
                    for (int a = 0; a < dailyVisitData.Count; a++)
                    {
                        string[] date_ = dailyVisitData[a].Date.Split('-');
                        dailyVisitData[a].Date = date_[2] + "-" + date_[1] + "-" + date_[0];
                    }
                }
            }

            return Json(dailyVisitData, JsonRequestBehavior.AllowGet);
        }

        // Non Performing Customers list
        public async Task<JsonResult> GetNonPerfomingCuslist()
        {
            string SalesPersonCode = Session["loggedInUserSPCode"].ToString();
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPDashboard/";
            apiUrl = apiUrl + "GetNonPerfomingCuslist?Salesperson_Code=" + SalesPersonCode;
            HttpClient client = new HttpClient();
            List<SPNonPerfomingCuslist> nonperfoming = new List<SPNonPerfomingCuslist>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                nonperfoming = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPNonPerfomingCuslist>>(data);
            }

            return Json(nonperfoming, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetCombinedSalesData(string fromdate, string enddate, string Year)
        {
            string SalesPersonCode = Session["loggedInUserSPCode"].ToString();
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPDashboard/GetCombinedSalesData?Salesperson_Code=" + HttpUtility.UrlEncode(SalesPersonCode) + "&No=" + Session["loggedInUserNo"].ToString() + "&fromdate=" + fromdate + "&enddate=" + enddate + "&Year=" + Year;

            HttpClient client = new HttpClient();
            CombinedSalesData combinedData = new CombinedSalesData();

            client.BaseAddress = new Uri(ConfigurationManager.AppSettings["ServiceApiUrl"].ToString());
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response = await client.GetAsync("SPDashboard/GetCombinedSalesData?Salesperson_Code=" + HttpUtility.UrlEncode(SalesPersonCode) + "&No=" + Session["loggedInUserNo"].ToString() + "&fromdate=" + fromdate + "&enddate=" + enddate + "&Year=" + Year);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                combinedData = Newtonsoft.Json.JsonConvert.DeserializeObject<CombinedSalesData>(data);
            }

            return Json(combinedData, JsonRequestBehavior.AllowGet);
        }


        public async Task<JsonResult> GetTodayVisit()
        {
            string SalesPersonCode = Session["loggedInUserSPCode"].ToString();
            string Week_Plan_Date = DateTime.Now.ToString("yyyy-MM-dd");
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPDashboard/";
            apiUrl = apiUrl + "GetTodayVisit?Week_Plan_Date=" + Week_Plan_Date + "&Salesperson_Code=" + SalesPersonCode;

            HttpClient client = new HttpClient();
            List<SPTodayVisitlist> toadyvisit = new List<SPTodayVisitlist>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                toadyvisit = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPTodayVisitlist>>(data);
            }

            return Json(toadyvisit, JsonRequestBehavior.AllowGet);

        }
        public async Task<JsonResult> GetWeeklytask()
        {

            string SalesPersonCode = Session["loggedInUserSPCode"].ToString();
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPDashboard/";
            apiUrl = apiUrl + "GetWeeklytask?Salesperson_Code=" + SalesPersonCode;

            HttpClient client = new HttpClient();
            List<SPWeeklytasklist> weeklydyvisit = new List<SPWeeklytasklist>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                weeklydyvisit = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPWeeklytasklist>>(data);
            }
            return Json(weeklydyvisit, JsonRequestBehavior.AllowGet);

        }
        public async Task<JsonResult> GetMonthlyTask()
        {
            string SalesPersonCode = Session["loggedInUserSPCode"].ToString();
            string Visit_Month = DateTime.Now.ToString("MMMM");
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPDashboard/";
            apiUrl = apiUrl + "GetMonthlyTask?Visit_Month=" + Visit_Month + "&Salesperson_Code=" + SalesPersonCode;

            HttpClient client = new HttpClient();
            List<SPMonthlylist> monthlyVisit = new List<SPMonthlylist>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                monthlyVisit = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPMonthlylist>>(data);
            }
            return Json(monthlyVisit, JsonRequestBehavior.AllowGet);

        }
        public async Task<JsonResult> GetInquiryList()
        {
            string SalesPersonCode = Session["loggedInUserSPCode"].ToString();
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPDashboard/";
            apiUrl = apiUrl + "GetInquiryList?Salesperson_Code=" + SalesPersonCode;
            HttpClient client = new HttpClient();
            List<DBInquiry> dBInquiries = new List<DBInquiry>();
            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                dBInquiries = Newtonsoft.Json.JsonConvert.DeserializeObject<List<DBInquiry>>(data);
            }
            return Json(dBInquiries, JsonRequestBehavior.AllowGet);

        }

        public async Task<JsonResult> GetSaleOrderList()
        {
            string SalesPersonCode = Session["loggedInUserSPCode"].ToString();
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPDashboard/";
            apiUrl = apiUrl + "GetSaleOrderList?Salesperson_Code=" + SalesPersonCode;
            HttpClient client = new HttpClient();
            List<DBSalesOrder> salesOrders = new List<DBSalesOrder>();
            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                salesOrders = Newtonsoft.Json.JsonConvert.DeserializeObject<List<DBSalesOrder>>(data);
            }
            return Json(salesOrders, JsonRequestBehavior.AllowGet);

        }
        public async Task<JsonResult> GetSaleQuoteList()
        {
            string SalesPersonCode = Session["loggedInUserSPCode"].ToString();
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPDashboard/";
            apiUrl = apiUrl + "GetSaleQuoteList?Salesperson_Code=" + SalesPersonCode;
            HttpClient client = new HttpClient();
            List<DBSaleQuote> saleQuotes = new List<DBSaleQuote>();
            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                saleQuotes = Newtonsoft.Json.JsonConvert.DeserializeObject<List<DBSaleQuote>>(data);
            }
            return Json(saleQuotes, JsonRequestBehavior.AllowGet);

        }
        public async Task<JsonResult> GetSalesInvoiceList()
        {
            string SalesPersonCode = Session["loggedInUserSPCode"].ToString();
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPDashboard/";
            apiUrl = apiUrl + "GetSalesInvoiceList?Salesperson_Code=" + SalesPersonCode;
            HttpClient client = new HttpClient();
            List<DBSalesInvoice> salesInvoices = new List<DBSalesInvoice>();
            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                salesInvoices = Newtonsoft.Json.JsonConvert.DeserializeObject<List<DBSalesInvoice>>(data);
            }
            return Json(salesInvoices, JsonRequestBehavior.AllowGet);

        }
        public async Task<JsonResult> GetitemwisetotalqtyList()
        {
            string SalesPersonCode = Session["loggedInUserSPCode"].ToString();
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPDashboard/";
            apiUrl = apiUrl + "GetitemwisetotalqtyList?Salesperson_Code=" + SalesPersonCode;
            HttpClient client = new HttpClient();
            List<DBitemwisetotalqty> itemwisetotalqties = new List<DBitemwisetotalqty>();
            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                itemwisetotalqties = Newtonsoft.Json.JsonConvert.DeserializeObject<List<DBitemwisetotalqty>>(data);
            }
            return Json(itemwisetotalqties, JsonRequestBehavior.AllowGet);

        }
        public async Task<JsonResult> GetPendingWarehouseSales()
        {
            //string SalesPersonCode = Session["loggedInUserSPCode"].ToString();
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPDashboard/";
            apiUrl = apiUrl + "GetPendingWarehouseSales";
            HttpClient client = new HttpClient();
            List<PendingWarehouseSales> pendingwarehousesales = new List<PendingWarehouseSales>();
            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                pendingwarehousesales = Newtonsoft.Json.JsonConvert.DeserializeObject<List<PendingWarehouseSales>>(data);
            }
            return Json(pendingwarehousesales, JsonRequestBehavior.AllowGet);
        }
        public async Task<JsonResult> GetPendingWarehousePurchese()
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPDashboard/";
            apiUrl = apiUrl + "GetPendingWarehousePurchese";
            HttpClient client = new HttpClient();
            List<PendingWarehousePurchese> pendingwarehousepurchase = new List<PendingWarehousePurchese>();
            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                pendingwarehousepurchase = Newtonsoft.Json.JsonConvert.DeserializeObject<List<PendingWarehousePurchese>>(data);
            }
            return Json(pendingwarehousepurchase, JsonRequestBehavior.AllowGet);
        }
    }
}