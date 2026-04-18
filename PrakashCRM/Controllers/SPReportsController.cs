using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Wordprocessing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PrakashCRM.Data.Models;
using PrakashCRM.Service.Classes;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace PrakashCRM.Controllers
{
    public class SPReportsController : Controller
    {
        public string FromDate { get; private set; }
        public string ToDate { get; private set; }

        // GET: SPReports
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult CustomerOutsatnding()
        {
            return View();
        }
        //public ActionResult OutstandingDetails()
        //{
        //    return View();
        //}

        public ActionResult Complaint()
        {
            return View();
        }
        public ActionResult SupportSaleData()
        {
            return View();
        }
        public ActionResult InventoryView()
        {
            return View();
        }
        public ActionResult WebsiteLog()
        {
            return View();
        }
        public ActionResult BusinessTypeSalesPerformance()
        {
            return View();
        }
        public ActionResult ComplaintReportDailyVisitPlan()
        {
            return View();
        }
        public ActionResult IndustryWiseSalesPerformance()
        {
            return View();
        }
        public ActionResult TransporterDashboard()
        {
            return View();
        }
        public ActionResult InquiryManagement()
        {
            return View();
        }
        public ActionResult DailyVisitMonthWise()
        {
            return View();
        }
        public ActionResult TaskPerformance()
        {
            return View();
        }
        public ActionResult SalesPerformance()
        {
            return View();
        }
        public ActionResult StockManagement()
        {
            return View();
        }
        public ActionResult CustomerLedgerEntry()
        {
            var Model = new SPCustomerReport
            {
                Name = "",
                No = "",
            };

            return View(Model);
        }


        public async Task<JsonResult> GetBranchWiseTotal()
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPReports/GetBranchWiseTotal";

            HttpClient client = new HttpClient();
            List<GetBranchWiseTotalSum> InvBranchWiseTotals = new List<GetBranchWiseTotalSum>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                InvBranchWiseTotals = Newtonsoft.Json.JsonConvert.DeserializeObject<List<GetBranchWiseTotalSum>>(data);
            }

            return Json(InvBranchWiseTotals, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public async Task<JsonResult> GetInv_ProductGroupsWise(string branchCode)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPReports/GetInv_ProductGroupsWise?branchCode=" + branchCode;
            HttpClient Client = new HttpClient();
            List<ProductGroupsWise> Inv_ProductGroupsWise = new List<ProductGroupsWise>();
            Client.BaseAddress = new Uri(apiUrl);
            Client.DefaultRequestHeaders.Accept.Clear();
            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await Client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content?.ReadAsStringAsync();
                Inv_ProductGroupsWise = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ProductGroupsWise>>(data);
            }
            return Json(Inv_ProductGroupsWise, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public async Task<JsonResult> GetInv_ItemWise(string branchCode, string pgCode)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPReports/GetInv_ItemWise?branchCode=" + branchCode + "&pgCode=" + pgCode;

            HttpClient client = new HttpClient();
            List<ItemWise> Inv_ItemWise = new List<ItemWise>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                Inv_ItemWise = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ItemWise>>(data);
            }

            return Json(Inv_ItemWise, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetInv_Inward(string branchCode, string pgCode, string itemName, string FromDate, string ToDate, string Type, bool Positive)
        {
            string baseUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString();
            string apiUrl = baseUrl + "SPReports/GetInv_Inward?" + "branchCode=" + HttpUtility.UrlEncode(branchCode) + "&pgCode=" + HttpUtility.UrlEncode(pgCode) + "&itemName=" + HttpUtility.UrlEncode(itemName) + "&FromDate=" + HttpUtility.UrlEncode(FromDate) + "&ToDate=" + HttpUtility.UrlEncode(ToDate) + "&Type=" + HttpUtility.UrlEncode(Type) + "&Positive=" + HttpUtility.UrlEncode(Positive.ToString());

            HttpClient client = new HttpClient();
            List<SPInwardDetails> Inv_Inward = new List<SPInwardDetails>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                Inv_Inward = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPInwardDetails>>(data);
            }

            return Json(Inv_Inward, JsonRequestBehavior.AllowGet);
        }
        public async Task<JsonResult> GetReservedDetails(string branchCode, string pgCode, string itemName, string FromDate, string ToDate)
        {
            string baseUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString();
            string apiUrl = baseUrl + "SPReports/GetReservedDetails?" + "&branchCode=" + HttpUtility.UrlEncode(branchCode) + "&pgCode=" + HttpUtility.UrlEncode(pgCode) + "&itemName=" + HttpUtility.UrlEncode(itemName) + "&FromDate=" + HttpUtility.UrlEncode(FromDate) + "&ToDate=" + HttpUtility.UrlEncode(ToDate);

            HttpClient client = new HttpClient();
            List<SPReservedQtyDetails> Inv_ReservedDetails = new List<SPReservedQtyDetails>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                Inv_ReservedDetails = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPReservedQtyDetails>>(data);
            }

            return Json(Inv_ReservedDetails, JsonRequestBehavior.AllowGet);
        }

        // Customer Ledger Entry Report

        [HttpPost]
        public async Task<string> PrintCustomerLedgerEntryPostApi(string CustomerNo, string FromDate, string ToDate)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() +
                $"SPReports/PrintCustomerLedgerEntryPostApi?CustomerNo={CustomerNo}&FromDate={FromDate}&ToDate={ToDate}";

            HttpClient client = new HttpClient();
            string savedPath = "";

            // Generate unique file name using CustomerNo + FromDate + ToDate
            string expectedFileName = $"{CustomerNo}_{FromDate}_{ToDate}".Replace("/", "-").Replace(":", "-").Replace(" ", "_");

            string path = Server.MapPath("~/CustomerLedgerEntryPrint/");

            DirectoryInfo di = new DirectoryInfo(path);
            FileInfo[] smFiles = di.GetFiles("*.*");
            bool flag = false;

            // Check if file already exists
            foreach (FileInfo smFile in smFiles)
            {
                if (Path.GetFileNameWithoutExtension(smFile.Name).Equals(expectedFileName, StringComparison.OrdinalIgnoreCase))
                {
                    flag = true;
                    savedPath = smFile.Name;
                    break;
                }
            }

            if (flag)
            {
                return savedPath;
            }
            else
            {
                client.BaseAddress = new Uri(apiUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await client.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    var base64PDF = JsonConvert.DeserializeObject<string>(data);
                    savedPath = SaveBase64ToPdf(base64PDF, "CustomerLedgerEntryPrint", expectedFileName);
                }
                else
                {
                    var JsonData = await response.Content.ReadAsStringAsync(); // For debugging/logging if needed
                }

                return savedPath;
            }
        }

        public string SaveBase64ToPdf(string base64String, string relativeFolderPath, string fileNameWithoutExtension)
        {
            string projectRoot = Server.MapPath("~/");
            string fullFolderPath = Path.Combine(projectRoot, relativeFolderPath);

            if (!Directory.Exists(fullFolderPath))
            {
                Directory.CreateDirectory(fullFolderPath);
            }

            string filePath = Path.Combine(fullFolderPath, $"{fileNameWithoutExtension}.pdf");
            byte[] pdfBytes = Convert.FromBase64String(base64String);
            System.IO.File.WriteAllBytes(filePath, pdfBytes);

            return $"{fileNameWithoutExtension}.pdf";
        }


        // Customer Report DrowDown Api.

        [HttpPost]
        public async Task<JsonResult> GetCustomerReport(string prefix)
        {
            string SalesPerson = Session["loggedInUserSPCode"].ToString();
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPReports/GetCustomerReport?prefix=" + prefix + "&salesPerson=" + SalesPerson;
            HttpClient client = new HttpClient();
            List<SPCustomerReport> customerReports = new List<SPCustomerReport>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                customerReports = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPCustomerReport>>(data);
            }

            return Json(customerReports, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetBusinessTypeSalesPerfomanceList()
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPReports/GetBusinessTypeSalesPerfomanceList";

            HttpClient client = new HttpClient();
            List<BusinessTypeSalesPerfomance> BusinessTypeSalesPerfomanceTotals = new List<BusinessTypeSalesPerfomance>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                BusinessTypeSalesPerfomanceTotals = Newtonsoft.Json.JsonConvert.DeserializeObject<List<BusinessTypeSalesPerfomance>>(data);
            }

            return Json(BusinessTypeSalesPerfomanceTotals, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetIndustryWiseSalesPerfomanceList()
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPReports/GetIndustryWiseSalesPerfomanceList";

            HttpClient client = new HttpClient();
            List<IndustryWiseSalesPerfomance> IndustryWiseSalesPerfomanceTotals = new List<IndustryWiseSalesPerfomance>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                IndustryWiseSalesPerfomanceTotals = Newtonsoft.Json.JsonConvert.DeserializeObject<List<IndustryWiseSalesPerfomance>>(data);
            }

            return Json(IndustryWiseSalesPerfomanceTotals, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetComplaintList(int orderBy, string orderDir, int skip, int top)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPReports/";
            string orderByField = "";
            orderByField = "No asc";

            apiUrl = apiUrl + "GetComplaintList?&skip=" + skip + "&top=" + top + "&orderby=" + orderByField;
            HttpClient client = new HttpClient();
            List<Complaint> Complaint = new List<Complaint>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                Complaint = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Complaint>>(data);
            }

            return Json(Complaint, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetComplaintReportDailyVisitPlanList(string Fromdate, string Todate, string Search, int orderBy, string orderDir, int skip, int top)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPReports/";
            string orderByField = "";
            orderByField = "No asc";

            apiUrl = apiUrl + "GetComplaintReportDailyVisitPlanList?&Fromdate=" + Fromdate + "&Todate=" + Todate + "&Search=" + Search + "&skip=" + skip + "&top=" + top + "&orderby=" + orderByField;
            HttpClient client = new HttpClient();
            List<ComplaintReportDailyVisitPlan> Complaint = new List<ComplaintReportDailyVisitPlan>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                Complaint = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ComplaintReportDailyVisitPlan>>(data);
            }

            return Json(Complaint, JsonRequestBehavior.AllowGet);
        }
        public async Task<JsonResult> GetSupportSaleDataList(string FDate, string TDate)
        {
            string SalesPerson = Session["loggedInUserSPCode"].ToString();
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPReports/GetSupportSaleDataList?Sales_Person_Code=" + SalesPerson + "&No=" + Session["loggedInUserNo"].ToString() + "&FDate=" + FDate + "&TDate=" + TDate;

            HttpClient client = new HttpClient();
            CombineSupportSaleData combineSupportSaleData = new CombineSupportSaleData();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                combineSupportSaleData = Newtonsoft.Json.JsonConvert.DeserializeObject<CombineSupportSaleData>(data);
            }

            return Json(combineSupportSaleData, JsonRequestBehavior.AllowGet);
        }
        public async Task<JsonResult> GetWebsiteLog(string FromDate, string ToDate)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPReports/GetWebsiteLog?&FromDate=" + FromDate + "&ToDate=" + ToDate;

            HttpClient client = new HttpClient();
            List<WebsiteLog> WebsiteLog = new List<WebsiteLog>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                WebsiteLog = Newtonsoft.Json.JsonConvert.DeserializeObject<List<WebsiteLog>>(data);
            }

            return Json(WebsiteLog, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetCustomerOutStanding(int pageNumber = 1, int pageSize = 10, string orderby = "Location_Code asc")
        {
            if (pageNumber < 1)
                pageNumber = 1;

            if (pageSize <= 0)
                pageSize = 10;

            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPReports/GetCustomerOutStanding?pageNumber=" + pageNumber + "&pageSize=" + pageSize + "&orderby=" + HttpUtility.UrlEncode(string.IsNullOrWhiteSpace(orderby) ? "Location_Code asc" : orderby);

            HttpClient client = new HttpClient();
            PagedResult<SPCustomerOutstanding> outStandingDtails = new PagedResult<SPCustomerOutstanding>
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                outStandingDtails = Newtonsoft.Json.JsonConvert.DeserializeObject<PagedResult<SPCustomerOutstanding>>(data);
            }

            return Json(outStandingDtails, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public async Task<JsonResult> GetSalespersonData(string branchCode)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPReports/GetSalespersonData?branchCode=" + branchCode;
            HttpClient Client = new HttpClient();
            List<SPCustomerOutstanding> Inv_ProductGroupsWise = new List<SPCustomerOutstanding>();
            Client.BaseAddress = new Uri(apiUrl);
            Client.DefaultRequestHeaders.Accept.Clear();
            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await Client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content?.ReadAsStringAsync();
                Inv_ProductGroupsWise = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPCustomerOutstanding>>(data);
            }
            return Json(Inv_ProductGroupsWise, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<JsonResult> GetCustomerDataBySalesperson(string spCode)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPReports/GetCustomerDataBySalesperson?spCode=" + spCode;
            HttpClient Client = new HttpClient();
            List<SPCustomerOutstanding> Inv_ProductGroupsWise = new List<SPCustomerOutstanding>();
            Client.BaseAddress = new Uri(apiUrl);
            Client.DefaultRequestHeaders.Accept.Clear();
            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await Client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content?.ReadAsStringAsync();
                Inv_ProductGroupsWise = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPCustomerOutstanding>>(data);
            }
            return Json(Inv_ProductGroupsWise, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public async Task<JsonResult> GetCustomerwiseInvoice(string customerCode)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPReports/GetCustomerwiseInvoice?&customerCode=" + customerCode;

            HttpClient client = new HttpClient();
            List<SPCustomerOutstanding> Inv_ItemWise = new List<SPCustomerOutstanding>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                Inv_ItemWise = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPCustomerOutstanding>>(data);
            }

            return Json(Inv_ItemWise, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetComplaintReportDailyVisitPlanApproved(string DailyVisitoNo, string Status, string RowNo)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPReports/GetComplaintReportDailyVisitPlanApproved" + "?Daily_Visit_No=" + DailyVisitoNo + "&Status=" + Status + "&RowNo=" + RowNo;

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage response = await client.GetAsync(apiUrl);

                List<ComplaintReportDailyVisitPlan> complaintReportDailyVisitPlans = new List<ComplaintReportDailyVisitPlan>();

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    var token = Newtonsoft.Json.Linq.JToken.Parse(data);
                    if (token.Type == Newtonsoft.Json.Linq.JTokenType.Array)
                    {
                        complaintReportDailyVisitPlans = token.ToObject<List<ComplaintReportDailyVisitPlan>>();
                    }
                    else
                    {
                        var single = token.ToObject<ComplaintReportDailyVisitPlan>();
                        complaintReportDailyVisitPlans.Add(single);
                    }
                }

                return Json(complaintReportDailyVisitPlans, JsonRequestBehavior.AllowGet);
            }
        }

        public async Task<JsonResult> GetInquiryManagement(/*string FromDate, string ToDate*/)
        {
            string SalesPerson = Session["loggedInUserSPCode"].ToString();
            //string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPReports/GetInquiryManagement?&FromDate=" + FromDate + "&ToDate=" + ToDate;
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPReports/GetInquiryManagement?Sales_Person_Code=" + SalesPerson + "&No=" + Session["loggedInUserNo"].ToString();

            HttpClient client = new HttpClient();
            CombineInquiryManagement inquiryManagements = new CombineInquiryManagement();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                inquiryManagements = Newtonsoft.Json.JsonConvert.DeserializeObject<CombineInquiryManagement>(data);
            }

            return Json(inquiryManagements, JsonRequestBehavior.AllowGet);
        }
        public async Task<JsonResult> GetSalesPesonQuotes(string SalesPerson, string FromDate, string ToDate)
        {

            //string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPReports/GetInquiryManagement?&FromDate=" + FromDate + "&ToDate=" + ToDate;
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPReports/GetSalesPesonQuotes?Salesperson_Name=" + SalesPerson + "&FromDate=" + FromDate + "&ToDate=" + ToDate;

            HttpClient client = new HttpClient();
            List<InquirySalesPersonQuotes> inquirySalesPersonQuotes = new List<InquirySalesPersonQuotes>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                inquirySalesPersonQuotes = Newtonsoft.Json.JsonConvert.DeserializeObject<List<InquirySalesPersonQuotes>>(data);
            }

            return Json(inquirySalesPersonQuotes, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetSalesPesonInquiry(string SalesPerson, string FromDate, string ToDate)
        {

            //string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPReports/GetInquiryManagement?&FromDate=" + FromDate + "&ToDate=" + ToDate;
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPReports/GetSalesPesonInquiry?Salesperson_Name=" + SalesPerson + "&FromDate=" + FromDate + "&ToDate=" + ToDate;

            HttpClient client = new HttpClient();
            List<InquirySalesPersonInquiry> inquirySalesPersonInquiries = new List<InquirySalesPersonInquiry>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                inquirySalesPersonInquiries = Newtonsoft.Json.JsonConvert.DeserializeObject<List<InquirySalesPersonInquiry>>(data);
            }

            return Json(inquirySalesPersonInquiries, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetDailyVisitMonthWise()
        {
            string SalesPerson = Session["loggedInUserSPCode"].ToString();
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPReports/GetDailyVisitMonthWise?Salesperson_Code=" + SalesPerson + "&No=" + Session["loggedInUserNo"].ToString(); ;

            HttpClient client = new HttpClient();
            CombineDailyVisitMonthWise dailyVisitMonthWises = new CombineDailyVisitMonthWise();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                dailyVisitMonthWises = Newtonsoft.Json.JsonConvert.DeserializeObject<CombineDailyVisitMonthWise>(data);
            }

            return Json(dailyVisitMonthWises, JsonRequestBehavior.AllowGet);
        }
        public async Task<JsonResult> GetTransporterDashboardList(int orderBy, string orderDir, int skip, int top)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPReports/";
            string orderByField = "";
            orderByField = "Entry_No asc";
            apiUrl = apiUrl + "GetTransporterDashboardList?&skip=" + skip + "&top=" + top + "&orderby=" + orderByField;
            HttpClient client = new HttpClient();
            List<SPTransporterDashboard> dailyVisitMonthWises = new List<SPTransporterDashboard>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                dailyVisitMonthWises = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPTransporterDashboard>>(data);
            }

            return Json(dailyVisitMonthWises, JsonRequestBehavior.AllowGet);
        }
        public async Task<JsonResult> GetTaskPerformance()
        {
            string SalesPerson = Session["loggedInUserSPCode"].ToString();
            //string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPReports/GetInquiryManagement?&FromDate=" + FromDate + "&ToDate=" + ToDate;
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPReports/GetTaskPerformance?Sales_Person_Code=" + SalesPerson + "&No=" + Session["loggedInUserNo"].ToString();

            HttpClient client = new HttpClient();
            CombineTaskPerformance combineTaskPerformance = new CombineTaskPerformance();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                combineTaskPerformance = Newtonsoft.Json.JsonConvert.DeserializeObject<CombineTaskPerformance>(data);
            }

            return Json(combineTaskPerformance, JsonRequestBehavior.AllowGet);
        }
        public async Task<JsonResult> GetSalesWiseTaskPerformance(string SalesPersonName)
        {
            string SalesPerson = Session["loggedInUserSPCode"].ToString();
            //string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPReports/GetInquiryManagement?&FromDate=" + FromDate + "&ToDate=" + ToDate;
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPReports/GetSalesWiseTaskPerformance?Sales_Person_Code=" + SalesPersonName;
            HttpClient client = new HttpClient();
            List<TaskPerformance> taskPerformances = new List<TaskPerformance>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                taskPerformances = Newtonsoft.Json.JsonConvert.DeserializeObject<List<TaskPerformance>>(data);
            }

            return Json(taskPerformances, JsonRequestBehavior.AllowGet);
        }
        public async Task<JsonResult> GetSalesPerformance()
        {
            string SalesPerson = Session["loggedInUserSPCode"].ToString();
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPReports/GetSalesPerformance?Sales_Person_Code=" + SalesPerson;
            HttpClient client = new HttpClient();
            List<SalesPerformance> salesPerformances = new List<SalesPerformance>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                salesPerformances = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SalesPerformance>>(data);
            }

            return Json(salesPerformances, JsonRequestBehavior.AllowGet);
        }
        public async Task<JsonResult> GetBranchWiseProduct(string BranchName)
        {
            string SalesPerson = Session["loggedInUserSPCode"].ToString();
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPReports/GetBranchWiseProduct?BranchName=" + BranchName + "&SalesPerson=" + SalesPerson;
            HttpClient client = new HttpClient();
            List<SalesPerformance> branchWiseProducts = new List<SalesPerformance>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                branchWiseProducts = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SalesPerformance>>(data);
            }

            return Json(branchWiseProducts, JsonRequestBehavior.AllowGet);
        }
        public async Task<JsonResult> GetSalesPesonConfirmQuotes(string SalesPerson, string FromDate, string ToDate)
        {

            //string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPReports/GetInquiryManagement?&FromDate=" + FromDate + "&ToDate=" + ToDate;
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPReports/GetSalesPesonConfirmQuotes?Salesperson_Name=" + SalesPerson + "&FromDate=" + FromDate + "&ToDate=" + ToDate;

            HttpClient client = new HttpClient();
            List<InquirySalesPersonQuotes> inquirySalesPersonQuotes = new List<InquirySalesPersonQuotes>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                inquirySalesPersonQuotes = Newtonsoft.Json.JsonConvert.DeserializeObject<List<InquirySalesPersonQuotes>>(data);
            }

            return Json(inquirySalesPersonQuotes, JsonRequestBehavior.AllowGet);
        }
        public async Task<JsonResult> GetStockManagement()
        {

            //string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPReports/GetInquiryManagement?&FromDate=" + FromDate + "&ToDate=" + ToDate;
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPReports/GetStockManagement";

            HttpClient client = new HttpClient();
            List<StockManagement> stockManagements = new List<StockManagement>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                stockManagements = Newtonsoft.Json.JsonConvert.DeserializeObject<List<StockManagement>>(data);
            }

            return Json(stockManagements, JsonRequestBehavior.AllowGet);
        }
        public async Task<JsonResult> GetBranchWiseProducts(string BranchName)
        {

            //string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPReports/GetInquiryManagement?&FromDate=" + FromDate + "&ToDate=" + ToDate;
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPReports/GetBranchWiseProducts?BranchName=" + BranchName;

            HttpClient client = new HttpClient();
            CombineStockManagement stockManagements = new CombineStockManagement();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                stockManagements = Newtonsoft.Json.JsonConvert.DeserializeObject<CombineStockManagement>(data);
            }

            return Json(stockManagements, JsonRequestBehavior.AllowGet);
        }
        public async Task<JsonResult> GetProductPackingStyle(string BranchName, string Product)
        {

            //string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPReports/GetInquiryManagement?&FromDate=" + FromDate + "&ToDate=" + ToDate;
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPReports/GetProductPackingStyle?BranchName=" + BranchName + "&Product=" + Product;

            HttpClient client = new HttpClient();
            List<StockManagement> stockManagements = new List<StockManagement>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                stockManagements = Newtonsoft.Json.JsonConvert.DeserializeObject<List<StockManagement>>(data);
            }

            return Json(stockManagements, JsonRequestBehavior.AllowGet);
        }



    }
}