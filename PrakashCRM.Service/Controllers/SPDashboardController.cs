using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using PrakashCRM.Data.Models;
using PrakashCRM.Service.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Ajax.Utilities;

namespace PrakashCRM.Service.Controllers
{
    [RoutePrefix("api/SPDashboard")]
    public class SPDashboardController : ApiController
    {
        [Route("GetDashboardDetails")]
        public SPDashboardDetails GetDashboardDetails(string SPCode)
        {
            API ac = new API();
            SPDashboardDetails sdbDetails = new SPDashboardDetails();
            sdbDetails.OrdersCount = 0;
            sdbDetails.InvoicesCount = 0;
            sdbDetails.ContactsCount = 0;
            sdbDetails.CustomersCount = 0;
            sdbDetails.QuotesCount = 0;
            sdbDetails.InquiryCount = 0;
            sdbDetails.ContactCompanyList = 0;

            var resultOrders = ac.GetData<SPSalesOrdersList>("SalesOrdersListDotNetAPI", "Salesperson_Code eq '" + SPCode + "'");

            if (resultOrders.Result.Item1.value != null)
            {
                if (resultOrders.Result.Item1.value.Count > 0)
                {
                    sdbDetails.OrdersCount = resultOrders.Result.Item1.value.Count;
                }
            }

            var resultInvoices = ac.GetData<SPPostedSalesInvoiceList>("PostedSalesInvoicesDotNetAPI", "Salesperson_Code eq '" + SPCode + "'");

            if (resultInvoices.Result.Item1.value != null)
            {
                if (resultInvoices.Result.Item1.value.Count > 0)
                    sdbDetails.InvoicesCount = resultInvoices.Result.Item1.value.Count;
            }

            var resultContacts = ac.GetData<SPCompanyList>("ContactDotNetAPI", "Type eq 'Company' and Salesperson_Code eq '" + SPCode + "'");

            if (resultContacts.Result.Item1.value != null)
            {
                if (resultContacts.Result.Item1.value.Count > 0)
                    sdbDetails.ContactsCount = resultContacts.Result.Item1.value.Count;
            }
            var resultManufactors = ac.GetData<SPCompanyList>("ContactDotNetAPI", "Salesperson_Code eq '" + SPCode + "' and Business_Type_No eq '" + "MANUFACTURER'");
            if (resultManufactors.Result.Item1.value != null)
            {
                if (resultManufactors.Result.Item1.value.Count > 0)
                    sdbDetails.ContactManufactorCount = resultManufactors.Result.Item1.value.Count;
            }
            var resultTraders = ac.GetData<SPCompanyList>("ContactDotNetAPI", "Salesperson_Code eq '" + SPCode + "' and Business_Type_No eq '" + "TRADER'");
            if (resultTraders.Result.Item1.value != null)
            {
                if (resultTraders.Result.Item1.value.Count > 0)
                    sdbDetails.ContactTraderCount = resultTraders.Result.Item1.value.Count;
            }

            var resultCustomers = ac.GetData<SPCustomersList>("CustomerCardDotNetAPI", "Salesperson_Code eq '" + SPCode + "'");

            if (resultCustomers.Result.Item1.value != null)
            {
                if (resultCustomers.Result.Item1.value.Count > 0)
                    sdbDetails.CustomersCount = resultCustomers.Result.Item1.value.Count;
            }

            var resultQuotes = ac.GetData<SPSalesQuotesList>("SalesQuotesListDotNetAPI", "Salesperson_Code eq '" + SPCode + "'");

            if (resultQuotes.Result.Item1.value != null)
            {
                if (resultQuotes.Result.Item1.value.Count > 0)
                    sdbDetails.QuotesCount = resultQuotes.Result.Item1.value.Count;
            }

            if (resultQuotes.Result.Item1.value != null)
            {
                if (resultQuotes.Result.Item1.value.Count > 0)
                    sdbDetails.QuotesCount = resultQuotes.Result.Item1.value.Count;
            }

            var resultInquiries = ac.GetData<SPInquiry>("InquiryDotNetAPI", "Document_Type eq 'Quote' and PCPL_IsInquiry eq true and Salesperson_Code eq '" + SPCode + "'");

            if (resultInquiries.Result.Item1?.value?.Count > 0)
                sdbDetails.InquiryCount = resultInquiries.Result.Item1.value.Count;

            return sdbDetails;
        }

        [HttpGet]
        [Route("DailyVisitsDetails")]
        public List<SPDailyVisitDetails> DailyVisitsDetails(string SPCode)
        {
            string today = DateTime.Now.Date.ToString("yyyy-MM-dd");

            API ac = new API();
            List<SPDailyVisitDetails> dailyVisitData = new List<SPDailyVisitDetails>();

            var DailyVisitsResult = ac.GetData<SPDailyVisitDetails>("DailyVisitsDotNetAPI", "Entry_Type eq 'ENTRY' and Salesperson_Code eq '" + SPCode + "' and Date eq " + today);

            if (DailyVisitsResult != null && DailyVisitsResult.Result.Item1.value.Count > 0)
                dailyVisitData = DailyVisitsResult.Result.Item1.value;

            return dailyVisitData;
        }

        [Route("GetAllFeedback")]
        public List<SPFeedBacksForDashboard> GetAllFeedback()
        {
            string today = DateTime.Now.Date.ToString("yyyy-MM-dd");

            API ac = new API();
            List<SPFeedBacksForDashboard> feedBacksForDashboard = new List<SPFeedBacksForDashboard>();
            var filter = "Submitted_On eq " + today;
            var result = ac.GetData<SPFeedBacksForDashboard>("FeedbackHeadersListDotNetAPI", filter);

            if (result != null && result.Result.Item1.value != null && result.Result.Item1.value.Count > 0)
                feedBacksForDashboard = result.Result.Item1.value;

            return feedBacksForDashboard;
        }

        [Route("GetAllMarketUpdateDetails")]
        public List<SPMarketUpdateList> GetAllMarketUpdateDetails()
        {
            API ac = new API();
            List<SPMarketUpdateList> marketUpdateList = new List<SPMarketUpdateList>();
            string today = DateTime.Now.Date.ToString("yyyy-MM-dd");

            //if (filter == "" || filter == null)
            //    filter = "Salesperson_Code eq '" + SPNo + "'";
            //else
            //    filter = filter + " and Salesperson_Code eq '" + SPNo + "'";

            var result = (dynamic)null;

            result = ac.GetData1<SPMarketUpdateList>("MarketUpdateDotNetAPI", "Update_Date eq " + today, 0, 0, "SystemCreatedAt desc");

            if (result.Result.Item1.value.Count > 0)
                marketUpdateList = result.Result.Item1.value;

            for (int i = 0; i < marketUpdateList.Count; i++)
            {
                string[] strDate = marketUpdateList[i].Update_Date.Split('-');
                marketUpdateList[i].Update_Date = strDate[2] + '-' + strDate[1] + '-' + strDate[0];
            }

            return marketUpdateList;
        }

        [HttpPost]
        [Route("AddMarketUpdate")]
        public SPMarketUpdateResponse AddMarketUpdate(int Entry_No, string Update, string Update_Date, string Employee_Code)//SPMarketUpdate MarketUpdate, bool isEdit, string EntryNo
        {

            DateTime UpdateDate = Convert.ToDateTime(Update_Date);
            Update_Date = UpdateDate.ToString("yyyy-MM-dd");

            SPMarketUpdate requestMU = new SPMarketUpdate
            {
                Update_Date = Update_Date,
                Update = Update,
                Employee_Code = Employee_Code
            };
            var responseMU = new SPMarketUpdateResponse();
            dynamic result = null;

            if (Entry_No == 0)
            {
                result = PostItemMarketUpdate("MarketUpdateDotNetAPI", requestMU, responseMU);
            }
            else
            {
                result = PatchItemMarketUpdate("MarketUpdateDotNetAPI", requestMU, responseMU, "Entry_No=" + Entry_No);
            }

            if (result.Result.Item1 != null)
            {
                responseMU = result.Result.Item1;
            }

            return responseMU;
        }

        public async Task<(SPMarketUpdateResponse, errorDetails)> PostItemMarketUpdate<SPMarketUpdateResponse>(string apiendpoint, SPMarketUpdate requestModel, SPMarketUpdateResponse responseModel)
        {
            string _baseURL = System.Configuration.ConfigurationManager.AppSettings["BaseURL"];
            string _tenantId = System.Configuration.ConfigurationManager.AppSettings["TenantID"];
            string _environment = System.Configuration.ConfigurationManager.AppSettings["Environment"];
            string _companyName = System.Configuration.ConfigurationManager.AppSettings["CompanyName"];

            API ac = new API();
            var accessToken = await ac.GetAccessToken();

            HttpClient _httpClient = new HttpClient();
            string encodeurl = Uri.EscapeUriString(_baseURL.Replace("{TenantID}", _tenantId).Replace("{Environment}", _environment).Replace("{CompanyName}", _companyName) + apiendpoint);
            Uri baseuri = new Uri(encodeurl);
            _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken.Token);


            string ItemCardObjString = JsonConvert.SerializeObject(requestModel);
            var content = new StringContent(ItemCardObjString, Encoding.UTF8, "application/json");
            HttpResponseMessage response = null;
            try
            {
                response = _httpClient.PostAsync(baseuri, content).Result;
            }
            catch (Exception ex)
            {

            }
            errorDetails errordetail = new errorDetails();
            errordetail.isSuccess = response.IsSuccessStatusCode;
            if (response.IsSuccessStatusCode)
            {
                var JsonData = response.Content.ReadAsStringAsync().Result;
                try
                {
                    JObject res = JObject.Parse(JsonData);
                    responseModel = res.ToObject<SPMarketUpdateResponse>();

                    errordetail.code = response.StatusCode.ToString();
                    errordetail.message = response.ReasonPhrase;
                }
                catch (Exception ex1)
                {
                }
            }
            else
            {
                var JsonData = response.Content.ReadAsStringAsync().Result;

                try
                {
                    JObject res = JObject.Parse(JsonData);
                    errorMaster<errorDetails> emd = res.ToObject<errorMaster<errorDetails>>();
                    errordetail = emd.error;
                }
                catch (Exception ex1)
                {
                }
            }
            return (responseModel, errordetail);
        }

        public async Task<(SPMarketUpdateResponse, errorDetails)> PatchItemMarketUpdate<SPMarketUpdateResponse>(string apiendpoint, SPMarketUpdate requestModel, SPMarketUpdateResponse responseModel, string fieldWithValue)
        {
            string _baseURL = System.Configuration.ConfigurationManager.AppSettings["BaseURL"];
            string _tenantId = System.Configuration.ConfigurationManager.AppSettings["TenantID"];
            string _environment = System.Configuration.ConfigurationManager.AppSettings["Environment"];
            string _companyName = System.Configuration.ConfigurationManager.AppSettings["CompanyName"];

            API ac = new API();
            var accessToken = await ac.GetAccessToken();

            HttpClient _httpClient = new HttpClient();
            string encodeurl = Uri.EscapeUriString(_baseURL.Replace("{TenantID}", _tenantId).Replace("{Environment}", _environment).Replace("{CompanyName}", _companyName) + apiendpoint);
            Uri baseuri = new Uri(encodeurl);
            var request = new HttpRequestMessage(new HttpMethod("PATCH"), baseuri + "(" + fieldWithValue + ")");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Token);
            _httpClient.DefaultRequestHeaders.Add("If-Match", "*");

            string ItemCardObjString = JsonConvert.SerializeObject(requestModel);
            request.Content = new StringContent(ItemCardObjString, Encoding.UTF8, "application/json");

            HttpResponseMessage response = null;
            try
            {
                response = _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).Result;
            }
            catch (Exception ex)
            {
            }

            errorDetails errordetail = new errorDetails();
            errordetail.isSuccess = response.IsSuccessStatusCode;
            if (response.IsSuccessStatusCode)
            {
                var JsonData = response.Content.ReadAsStringAsync().Result;
                try
                {
                    JObject res = JObject.Parse(JsonData);
                    responseModel = res.ToObject<SPMarketUpdateResponse>();


                    errordetail.code = response.StatusCode.ToString();
                    errordetail.message = response.ReasonPhrase;
                }
                catch (Exception ex1)
                {
                }
            }
            else
            {
                var JsonData = response.Content.ReadAsStringAsync().Result;

                try
                {
                    JObject res = JObject.Parse(JsonData);
                    errorMaster<errorDetails> emd = res.ToObject<errorMaster<errorDetails>>();
                    errordetail = emd.error;

                }
                catch (Exception ex1)
                {
                }
            }

            return (responseModel, errordetail);
        }
        // Non Performing Customers list
        [Route("GetNonPerfomingCuslist")]
        public List<SPNonPerfomingCuslist> GetNonPerfomingCuslist(string Salesperson_Code)
        {
            API ac = new API();
            List<SPNonPerfomingCuslist> nonperfoming = new List<SPNonPerfomingCuslist>();

            var result = ac.GetData<SPNonPerfomingCuslist>("NonPerformingCustomer", "Salesperson_Code eq '" + Salesperson_Code + "'");

            if (result.Result.Item1.value.Count > 0)
                nonperfoming = result.Result.Item1.value;

            return nonperfoming;
        }
        [HttpGet]
        [Route("GetCombinedSalesData")]
        public CombinedSalesData GetCombinedSalesData(string Salesperson_Code, string No, string fromdate, string enddate, string Year)
        {
            API ac = new API();
            CombinedSalesData combinedData = new CombinedSalesData();

            // Salespersons
            string filter = $"SalesPerson eq '{Salesperson_Code}' and IsSalesPerson eq true and IsProduct eq true and IsIncludTop10Product eq true and " + " Plan_Year eq '" + Year + "'";
            if (fromdate != null && enddate != null)
            {
                filter += " and Busiess_Plan_Date ge " + fromdate + " and Busiess_Plan_Date le " + enddate;
            }
            var salespersonResult = ac.GetData<SPSelaspersonlist>("TargetvsSalesReport", filter);
            List<SPSelaspersonlist> salespersons = new List<SPSelaspersonlist>();

            if (salespersonResult.Result.Item1.value.Count > 0)
                salespersons = salespersonResult.Result.Item1.value.GroupBy(x => x.SalesPerson_Name).Select(g => new SPSelaspersonlist
                {
                    SalesPerson_Name = g.Key,
                    Demand_Qty = g.Sum(x => x.Demand_Qty),
                    Target_Qty = g.Sum(x => x.Target_Qty),
                    Sales_Qty = g.Sum(x => x.Sales_Qty),
                    Sales_Percentage_Qty = g.Sum(x => x.Sales_Percentage_Qty)
                }).ToList();

            // Support SP list (Reporting persons)
            string supportsalesfilter = "PCPL_Reporting_Person_No eq '" + No + "'";
            List<SPPCPLEmployeeList> sPPCPLEmployeeLists = new List<SPPCPLEmployeeList>();
            var SPPCPLEmployeeLists = ac.GetData<SPPCPLEmployeeList>("PcplEmployeeList", supportsalesfilter);

            if (SPPCPLEmployeeLists.Result.Item1.value.Count > 0)
                sPPCPLEmployeeLists = SPPCPLEmployeeLists.Result.Item1.value;

            var reportingSalesData = new List<SPSelaspersonlist>();

            foreach (var employeeList in sPPCPLEmployeeLists)
            {
                var PCPL_SP = employeeList.PCPL_Salespers_Purch_Name;
                var filteremp = "SalesPerson_Name eq '" + PCPL_SP + "' and " + " Plan_Year eq '" + Year + "'";
                if (fromdate != null && enddate != null)
                {
                    filteremp += " and Busiess_Plan_Date ge " + fromdate + " and Busiess_Plan_Date le " + enddate;
                }
                var report = ac.GetData<SPSelaspersonlist>("TargetvsSalesReport", filteremp);

                if (report?.Result.Item1?.value != null && report.Result.Item1.value.Count > 0)
                    reportingSalesData.AddRange(report.Result.Item1.value);
            }

            // Make Support SP data UNIQUE + merge values
            reportingSalesData = reportingSalesData.GroupBy(x => x.SalesPerson_Name).Select(g => new SPSelaspersonlist
            {
                SalesPerson_Name = g.Key,
                Demand_Qty = g.Sum(x => x.Demand_Qty),
                Target_Qty = g.Sum(x => x.Target_Qty),
                Sales_Qty = g.Sum(x => x.Sales_Qty),
                Sales_Percentage_Qty = g.Sum(x => x.Sales_Percentage_Qty)
            }).ToList();

            // Products
            string productFilter = "IsSalesPerson eq true and IsProduct eq true and IsIncludTop10Product eq true and SalesPerson eq '" + Salesperson_Code + "' and " + " Plan_Year eq '" + Year + "'";
            if (fromdate != null && enddate != null)
            {
                productFilter += " and Busiess_Plan_Date ge " + fromdate + " and Busiess_Plan_Date le " + enddate;
            }
            var productResult = ac.GetData<SPProductlist>("TargetvsSalesReport", productFilter);
            List<SPProductlist> products = new List<SPProductlist>();

            if (productResult.Result.Item1.value.Count > 0)
                products = productResult.Result.Item1.value;

            // Product totals
            string producttotalfilter = "IsSalesPerson eq true and IsProduct eq false and IsIncludTop10Product eq false and SalesPerson eq '" + Salesperson_Code + "' and " + " Plan_Year eq '" + Year + "'";
            if (fromdate != null && enddate != null)
            {
                producttotalfilter += " and Busiess_Plan_Date ge " + fromdate + " and Busiess_Plan_Date le " + enddate;
            }
            List<SPSelaspersonlist> producttotal = new List<SPSelaspersonlist>();
            var producttotalResult = ac.GetData<SPSelaspersonlist>("TargetvsSalesReport", producttotalfilter);

            if (producttotalResult.Result.Item1.value.Count > 0)
                producttotal = producttotalResult.Result.Item1.value;

            combinedData.Salespersons = salespersons;
            combinedData.SupportSPs = reportingSalesData;
            combinedData.Products = products;
            combinedData.ProductsTotalList = producttotal;

            return combinedData;
        }



        [HttpGet]
        [Route("GetTodayVisit")]
        public List<SPTodayVisitlist> GetTodayVisit(string Week_Plan_Date, string Salesperson_Code)
        {
            API ac = new API();
            List<SPTodayVisitlist> todayVisit = new List<SPTodayVisitlist>();
            var result = ac.GetData<SPTodayVisitlist>("WeeklySalesPlanListDotNetAPI", "Week_Plan_Date eq " + Week_Plan_Date + " and Salesperson_Code eq '" + Salesperson_Code + "'" + " and Entry_Type eq 'Entry'");

            if (result.Result.Item1 != null && result.Result.Item1.value != null && result.Result.Item1.value.Count > 0)
                todayVisit = result.Result.Item1.value;

            return todayVisit;
        }

        [HttpGet]
        [Route("GetWeeklytask")]
        public List<SPWeeklytasklist> GetWeeklytask(string Salesperson_Code)
        {
            API ac = new API();
            List<SPWeeklytasklist> weeklydyvisit = new List<SPWeeklytasklist>();

            var result = ac.GetData<SPWeeklytasklist>("WeeklyTaskDotNetAPI", "Salesperson_Code eq '" + Salesperson_Code + "'");

            if (result.Result.Item1 != null && result.Result.Item1.value != null && result.Result.Item1.value.Count > 0)
                weeklydyvisit = result.Result.Item1.value;

            return weeklydyvisit;
        }

        [HttpGet]
        [Route("GetMonthlyTask")]
        public List<SPMonthlylist> GetMonthlyTask(string Visit_Month, string Salesperson_Code)
        {
            API ac = new API();
            List<SPMonthlylist> monthlyVisit = new List<SPMonthlylist>();

            var result = ac.GetData<SPMonthlylist>("MonthlyTask", "Visit_Month eq '" + Visit_Month + "' and Salesperson_Code eq '" + Salesperson_Code + "'");

            if (result.Result.Item1 != null && result.Result.Item1.value != null && result.Result.Item1.value.Count > 0)
                monthlyVisit = result.Result.Item1.value;

            return monthlyVisit;
        }
        [HttpGet]
        [Route("GetInquiryList")]
        public List<DBInquiry> GetInquiryList(string Salesperson_Code)
        {
            API ac = new API();
            List<DBInquiry> dBInquiries = new List<DBInquiry>();

            var result = ac.GetData<DBInquiry>("filteredinquires", "Salesperson_Code eq '" + Salesperson_Code + "'");
            if (result.Result.Item1 != null && result.Result.Item1.value != null && result.Result.Item1.value.Count > 0)
                dBInquiries = result.Result.Item1.value;

            return dBInquiries;
        }
        [HttpGet]
        [Route("GetSaleOrderList")]
        public List<DBSalesOrder> GetSaleOrderList(string Salesperson_Code)
        {
            API ac = new API();
            List<DBSalesOrder> dBSalesOrders = new List<DBSalesOrder>();

            var result = ac.GetData<DBSalesOrder>("filteredsalesorder", "Salesperson_Code eq '" + Salesperson_Code + "'");
            if (result.Result.Item1 != null && result.Result.Item1.value != null && result.Result.Item1.value.Count > 0)
                dBSalesOrders = result.Result.Item1.value;

            return dBSalesOrders;
        }
        [HttpGet]
        [Route("GetSaleQuoteList")]
        public List<DBSaleQuote> GetSaleQuoteList(string Salesperson_Code)
        {
            API ac = new API();
            List<DBSaleQuote> dBSaleQuotes = new List<DBSaleQuote>();

            var result = ac.GetData<DBSaleQuote>("filteredsalequote", "Salesperson_Code eq '" + Salesperson_Code + "'");
            if (result.Result.Item1 != null && result.Result.Item1.value != null && result.Result.Item1.value.Count > 0)
                dBSaleQuotes = result.Result.Item1.value;

            return dBSaleQuotes;
        }
        [HttpGet]
        [Route("GetSalesInvoiceList")]
        public List<DBSalesInvoice> GetSalesInvoiceList(string Salesperson_Code)
        {
            API ac = new API();
            List<DBSalesInvoice> dBSalesInvoices = new List<DBSalesInvoice>();

            var result = ac.GetData<DBSalesInvoice>("filteredsaleinoice", "Salesperson_Code eq '" + Salesperson_Code + "'");
            if (result.Result.Item1 != null && result.Result.Item1.value != null && result.Result.Item1.value.Count > 0)
                dBSalesInvoices = result.Result.Item1.value;

            return dBSalesInvoices;
        }
        [HttpGet]
        [Route("GetitemwisetotalqtyList")]
        public List<DBitemwisetotalqty> GetitemwisetotalqtyList(string Salesperson_Code)
        {
            API ac = new API();
            List<DBitemwisetotalqty> dBitemwisetotalqties = new List<DBitemwisetotalqty>();

            var result = ac.GetData<DBitemwisetotalqty>("itemwisetotalqty", "SalesPerson_Code eq '" + Salesperson_Code + "'");
            if (result.Result.Item1 != null && result.Result.Item1.value != null && result.Result.Item1.value.Count > 0)
                dBitemwisetotalqties = result.Result.Item1.value;

            return dBitemwisetotalqties;
        }
        [HttpGet]
        [Route("GetPendingWarehouseSales")]
        public List<PendingWarehouseSales> GetPendingWarehouseSales()
        {
            API ac = new API();
            List<PendingWarehouseSales> pendingwarehousesales = new List<PendingWarehouseSales>();

            var result = ac.GetData<PendingWarehouseSales>("PendingWarehouseSales", "Document_Type eq 'Order' or Document_Type eq 'Return Order'");
            if (result.Result.Item1 != null && result.Result.Item1.value != null && result.Result.Item1.value.Count > 0)
                pendingwarehousesales = result.Result.Item1.value;

            return pendingwarehousesales;
        }
        [HttpGet]
        [Route("GetPendingWarehousePurchese")]
        public List<PendingWarehousePurchese> GetPendingWarehousePurchese()
        {
            API ac = new API();
            List<PendingWarehousePurchese> pendingwarehousepurches = new List<PendingWarehousePurchese>();

            var result = ac.GetData<PendingWarehousePurchese>("PendingWarehousePurchase", "Document_Type eq 'Order'");

            if (result.Result.Item1 != null && result.Result.Item1.value != null && result.Result.Item1.value.Count > 0)
                pendingwarehousepurches = result.Result.Item1.value;

            return pendingwarehousepurches;
        }
    }
}