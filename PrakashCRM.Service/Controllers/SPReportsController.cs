using Antlr.Runtime.Tree;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PrakashCRM.Data.Models;
using PrakashCRM.Service.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Services.Description;
using System.Xml.Linq;

namespace PrakashCRM.Service.Controllers
{
    [RoutePrefix("api/SPReports")]
    public class SPReportsController : ApiController
    {
        [HttpPost]
        [Route("GenerateInvData")]
        public string GenerateInvData(string FromDate, string ToDate)
        {
            bool response = false;
            SPInvGenerateDataPost invGeneratereq = new SPInvGenerateDataPost();
            SPInvGenerateDataOData invGenerateres = new SPInvGenerateDataOData();
            errorDetails ed = new errorDetails();

            invGeneratereq.startdate = FromDate;
            invGeneratereq.enddate = ToDate;

            var result = PostItemForGenerateInvData<SPInvGenerateDataOData>("", invGeneratereq, invGenerateres);

            if (result?.Result != null)
            {
                response = result.Result.Item1?.value ?? false;
                ed = result.Result.Item2;
                invGenerateres.errorDetails = ed;
            }
            else
            {
                response = true;
            }
            return response.ToString().ToLower();
        }

        [HttpGet]
        [Route("GetBranchWiseTotal")]
        public List<GetBranchWiseTotalSum> GetBranchWiseTotal()
        {
            API ac = new API();
            List<GetBranchWiseTotalSum> InvBranchWiseTotals = new List<GetBranchWiseTotalSum>();

            var BranchWiseTotalResult = ac.GetData<GetBranchWiseTotalSum>("LocationByQty", "");

            if (BranchWiseTotalResult != null && BranchWiseTotalResult.Result.Item1.value.Count > 0)
                InvBranchWiseTotals = BranchWiseTotalResult.Result.Item1.value;


            return InvBranchWiseTotals;
        }

        [HttpGet]
        [Route("GetInv_ProductGroupsWise")]
        public List<ProductGroupsWise> GetInv_ProductGroupsWise(string branchCode)
        {
            API ac = new API();
            List<ProductGroupsWise> Inv_ProductGroupsWise = new List<ProductGroupsWise>();

            var ProductGroupsWiseResult = ac.GetData<ProductGroupsWise>("IndustryWiseQty", "Location_Filter_FilterOnly eq '" + branchCode + "'");

            if (ProductGroupsWiseResult != null && ProductGroupsWiseResult.Result.Item1.value.Count > 0)
                Inv_ProductGroupsWise = ProductGroupsWiseResult.Result.Item1.value;

            Inv_ProductGroupsWise = Inv_ProductGroupsWise.DistinctBy(a => a.Code).ToList();

            return Inv_ProductGroupsWise;
        }

        [HttpGet]
        [Route("GetInv_ItemWise")]
        public List<ItemWise> GetInv_ItemWise(string branchCode, string pgCode)
        {
            API ac = new API();
            List<ItemWise> Inv_ItemWise = new List<ItemWise>();

            var ItemWiseResult = ac.GetData<ItemWise>("IndustryWiseQty", "Location_Filter_FilterOnly eq '" + branchCode + "' and Code eq '" + pgCode + "'");
            if (ItemWiseResult != null && ItemWiseResult.Result.Item1.value.Count > 0)
                Inv_ItemWise = ItemWiseResult.Result.Item1.value;

            return Inv_ItemWise;
        }
        public async Task<(SPInvGenerateDataOData, errorDetails)> PostItemForGenerateInvData<SPInvGenerateDataOData>(string apiendpoint, SPInvGenerateDataPost requestModel, SPInvGenerateDataOData responseModel)
        {
            string _baseURL = System.Configuration.ConfigurationManager.AppSettings["BaseURL"];
            string _tenantId = System.Configuration.ConfigurationManager.AppSettings["TenantID"];
            string _environment = System.Configuration.ConfigurationManager.AppSettings["Environment"];
            string _companyName = System.Configuration.ConfigurationManager.AppSettings["CompanyName"];

            API ac = new API();
            var accessToken = await ac.GetAccessToken();

            HttpClient _httpClient = new HttpClient();
            string encodeurl = Uri.EscapeUriString("https://api.businesscentral.dynamics.com/v2.0/e55ad508-ef1a-489f-afe3-ae21f856e440/Sandbox/ODataV4/InventoryView_GenerateInventoryViewReportDate?company=\'Prakash Company\'");
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
                    //SPSQScheduleOrderOData scheduleOrderOData = res.ToObject<SPSQScheduleOrderOData>();
                    responseModel = res.ToObject<SPInvGenerateDataOData>();

                    //string scheduleOrderData = "{\"value\":" + scheduleOrderOData.value + "}";
                    //responseModel = JsonConvert.DeserializeObject<SPSQScheduleOrder>(scheduleOrderData);

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

        [HttpGet]
        [Route("GetInv_Inward")]
        public List<SPInwardDetails> GetInv_Inward(string branchCode, string pgCode, string itemName, string FromDate, string ToDate, string Type, bool Positive)
        {
            API ac = new API();
            List<SPInwardDetails> Inv_Inward = new List<SPInwardDetails>();
            string filter1 = "";
            if (!string.IsNullOrWhiteSpace(branchCode) && !string.IsNullOrWhiteSpace(pgCode))
            {
                filter1 += $" Location_Code eq '{branchCode}' and Item_Category_Code eq '{pgCode}'";
            }
            if (!string.IsNullOrWhiteSpace(itemName))
            {
                filter1 += $" and Item_Description eq '{itemName}'";
            }
            if (Type == "Inward")
            {
                if (!string.IsNullOrWhiteSpace(FromDate) && !string.IsNullOrWhiteSpace(ToDate))
                {
                    if (DateTime.TryParse(FromDate, out DateTime fromDateParsed) &&
                        DateTime.TryParse(ToDate, out DateTime toDateParsed))
                    {
                        string from = fromDateParsed.ToString("yyyy-MM-dd");
                        string to = toDateParsed.ToString("yyyy-MM-dd");
                        filter1 += $" and Posting_Date ge {from} and Posting_Date le {to} and Positive eq true";
                    }
                }
            }
            else if (string.Equals(Type, "Outward", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrWhiteSpace(FromDate) && !string.IsNullOrWhiteSpace(ToDate))
                {
                    if (DateTime.TryParse(FromDate, out DateTime fromDateParsed) &&
                        DateTime.TryParse(ToDate, out DateTime toDateParsed))
                    {
                        string from = fromDateParsed.ToString("yyyy-MM-dd");
                        string to = toDateParsed.ToString("yyyy-MM-dd");
                        filter1 += $" and Posting_Date ge {from} and Posting_Date le {to} and Positive eq false";
                    }
                }
            }
            else if (Type == "CLStock")
            {
                FromDate = "";

                if (!string.IsNullOrWhiteSpace(ToDate))
                {
                    if (DateTime.TryParse(ToDate, out DateTime toDateParsed))
                    {
                        string to = toDateParsed.ToString("yyyy-MM-dd");
                        filter1 += $" and Posting_Date le {to}";
                    }
                }
            }

            var result1 = ac.GetData<SPInwardDetails>("ItemLedgerEntriesDotNetAPI", filter1);

            if (result1 != null && result1.Result.Item1.value.Count > 0)
            {
                Inv_Inward = result1.Result.Item1.value;
            }

            // Calculate No_of_days
            foreach (var item in Inv_Inward)
            {
                if (DateTime.TryParse(item.PCPL_Original_Buying_Date, out DateTime buyingDate) &&
                    DateTime.TryParse(item.Posting_Date, out DateTime postingDate))
                {
                    item.No_of_days = (buyingDate - postingDate).Days;
                }
            }

            return Inv_Inward;
        }


        [HttpGet]
        [Route("GetReservedDetails")]
        public List<SPReservedQtyDetails> GetReservedDetails(string branchCode, string pgCode, string itemName, string FromDate, string ToDate)
        {
            API ac = new API();
            List<SPReservedQtyDetails> Inv_ReservedDetails = new List<SPReservedQtyDetails>();
            string filter1 = "";

            if (!string.IsNullOrWhiteSpace(branchCode) && !string.IsNullOrWhiteSpace(pgCode))
            {
                filter1 += $" Location_Code eq '{branchCode}' and Item_Category_Code eq '{pgCode}'";
            }

            if (!string.IsNullOrWhiteSpace(itemName))
            {
                filter1 += $" and Description eq '{itemName}'";
            }

            if (!string.IsNullOrWhiteSpace(FromDate) && !string.IsNullOrWhiteSpace(ToDate))
            {
                if (DateTime.TryParse(FromDate, out DateTime fromDateParsed) &&
                    DateTime.TryParse(ToDate, out DateTime toDateParsed))
                {
                    string from = fromDateParsed.ToString("yyyy-MM-dd");
                    string to = toDateParsed.ToString("yyyy-MM-dd");
                    filter1 += $" and Posting_Date ge {from} and Posting_Date le {to}";
                }
            }

            var result1 = ac.GetData<SPReservedQtyDetails>("SalesLinesDotNetAPI", filter1);

            if (result1 != null && result1.Result.Item1.value.Count > 0)
            {
                Inv_ReservedDetails = result1.Result.Item1.value;
            }
            return Inv_ReservedDetails;
        }

        // Customer Ledger Entry Pdf Api
        [HttpGet]
        [Route("PrintCustomerLedgerEntryPostApi")]
        public string PrintCustomerLedgerEntryPostApi(string CustomerNo, string FromDate, string ToDate)
        {
            var PrintCustomerLedgerReportResponse = new PrintCustomerLedgerReportResponse();

            PrintCustomerLedgerReportRequest PrintCustomerLedgerReportRequest = new PrintCustomerLedgerReportRequest
            {
                customerno = CustomerNo,
                fromdate = FromDate,
                todate = ToDate,
            };

            var result = (dynamic)null;
            result = PostCustomerLegerEntryPrint("ReportAPIMngtDotNetAPI_CustomerLedgerReportPrint", PrintCustomerLedgerReportRequest, PrintCustomerLedgerReportResponse);

            var base64PDF = "";
            if (result.Result.Item1 != null)
            {
                base64PDF = result.Result.Item1.value;

            }
            return base64PDF;
        }

        public async Task<(PrintCustomerLedgerReportResponse, errorDetails)> PostCustomerLegerEntryPrint<PrintCustomerLedgerReportRequest>(string apiendpoint, PrintCustomerLedgerReportRequest requestModel, PrintCustomerLedgerReportResponse responseModel)
        {
            string _codeUnitBaseUrl = System.Configuration.ConfigurationManager.AppSettings["CodeUnitBaseURL"];
            string _tenantId = System.Configuration.ConfigurationManager.AppSettings["TenantID"];
            string _environment = System.Configuration.ConfigurationManager.AppSettings["Environment"];
            string _companyName = System.Configuration.ConfigurationManager.AppSettings["CompanyName"];

            API ac = new API();
            var accessToken = await ac.GetAccessToken();

            HttpClient _httpClient = new HttpClient();
            //string encodeurl = Uri.EscapeUriString(_baseURL.Replace("{TenantID}", _tenantId).Replace("{Environment}", _environment).Replace("{CompanyName}", _companyName) + apiendpoint);
            string encodeurl = Uri.EscapeUriString(_codeUnitBaseUrl.Replace("{TenantID}", _tenantId).Replace("{Environment}", _environment).Replace("{CompanyName}", _companyName).Replace("{Endpoint}", apiendpoint));
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
                    responseModel = res.ToObject<PrintCustomerLedgerReportResponse>();

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


        // Customer Report DrowDown Api.
        [HttpGet]
        [Route("GetCustomerReport")]
        public List<SPCustomerReport> GetCustomerReport(string prefix)
        {
            API ac = new API();
            List<SPCustomerReport> customerReports = new List<SPCustomerReport>();
            var result = ac.GetData<SPCustomerReport>("CustomerCardDotNetAPI", "startswith(Name,'" + prefix + "')");

            if (result != null && result.Result.Item1.value.Count > 0)

                customerReports = result.Result.Item1.value;
            return customerReports;

        }


        [HttpGet]
        [Route("GetBusinessTypeSalesPerfomanceList")]
        public List<BusinessTypeSalesPerfomance> GetBusinessTypeSalesPerfomanceList()
        {
            API ac = new API();
            List<BusinessTypeSalesPerfomance> BusinessTypeSalesPerfomanceTotals = new List<BusinessTypeSalesPerfomance>();

            var Result = ac.GetData<BusinessTypeSalesPerfomance>("businesstypeindiviual", "");

            if (Result.Result.Item1.value.Count > 0)
                BusinessTypeSalesPerfomanceTotals = Result.Result.Item1.value;


            return BusinessTypeSalesPerfomanceTotals;
        }

        [HttpGet]
        [Route("GetIndustryWiseSalesPerfomanceList")]
        public List<IndustryWiseSalesPerfomance> GetIndustryWiseSalesPerfomanceList()
        {
            API ac = new API();
            List<IndustryWiseSalesPerfomance> IndustryWiseSalesPerfomanceTotals = new List<IndustryWiseSalesPerfomance>();

            var Result = ac.GetData<IndustryWiseSalesPerfomance>("industrytypeindiviual", "");

            if (Result.Result.Item1.value.Count > 0)
                IndustryWiseSalesPerfomanceTotals = Result.Result.Item1.value;


            return IndustryWiseSalesPerfomanceTotals;
        }

        [HttpGet]
        [Route("GetComplaintList")]
        public List<Complaint> GetComplaintList(int skip, int top, string orderby)

        {
            string filter = null;
            API ac = new API();
            List<Complaint> Complaint = new List<Complaint>();

            var Result = ac.GetData1<Complaint>("DailyVisitsDotNetAPI", filter, skip, top, orderby);

            if (Result.Result.Item1.value.Count > 0)
                Complaint = Result.Result.Item1.value;


            return Complaint;
        }


        [HttpGet]
        [Route("GetComplaintReportDailyVisitPlanList")]
        public List<ComplaintReportDailyVisitPlan> GetComplaintReportDailyVisitPlanList(string Fromdate, string Todate, string Search, int skip, int top, string orderby)
        {
            string filter = "";
            if (Fromdate != null && Todate != null && Search == null)
            {
                filter = "Com_Date ge " + Fromdate + " and Com_Date le " + Todate;
            }
            else if (Fromdate != null && Todate != null && Search != null)
            {
                filter = "Com_Date ge " + Fromdate + " and Com_Date le " + Todate + " and Contact_Company_Name eq '" + Search + "'";
            }
            else if (Fromdate == null && Todate == null && Search != null)
            {
                filter = "Contact_Company_Name eq '" + Search + "'";
            }



            API ac = new API();
            List<ComplaintReportDailyVisitPlan> ComplaintReportDailyVisitPlan = new List<ComplaintReportDailyVisitPlan>();

            var Result = ac.GetData1<ComplaintReportDailyVisitPlan>("DailyVisitsDotNetAPI", filter, skip, top, orderby);
            //var Result = ac.GetData1<Complaint>("ComplaintReport", filter,skip,top,orderby);

            if (Result.Result.Item1.value.Count > 0)
                ComplaintReportDailyVisitPlan = Result.Result.Item1.value;


            return ComplaintReportDailyVisitPlan;
        }
        [HttpGet]
        [Route("GetSupportSaleDataList")]
        public CombineSupportSaleData GetSupportSaleDataList(string Sales_Person_Code, string No, string FDate, string TDate)
        {
            API ac = new API();
            CombineSupportSaleData combineSupportSaleData = new CombineSupportSaleData();
            List<SupportSaleData> SupportSaleData = new List<SupportSaleData>();

            var filter = "Salesperson_Code eq '" + Sales_Person_Code + "'";
            if (FDate != null && TDate != null)
            {

                filter += "and Date ge " + FDate + " and Date le " + TDate;
                //filter += $" and startswith(Customer_Name,'{TXTSp}')";
            }
            var Result = ac.GetData<SupportSaleData>("SupportSaleData", filter);

            if (Result.Result.Item1.value.Count > 0)
                SupportSaleData = Result.Result.Item1.value;

            string supportsalesfilter = "PCPL_Reporting_Person_No eq '" + No + "'";
            List<SPPCPLEmployeeList> sPPCPLEmployeeLists = new List<SPPCPLEmployeeList>();
            var SPPCPLEmployeeLists = ac.GetData<SPPCPLEmployeeList>("PcplEmployeeList", supportsalesfilter);
            if (SPPCPLEmployeeLists.Result.Item1.value.Count > 0)
            {
                sPPCPLEmployeeLists = SPPCPLEmployeeLists.Result.Item1.value;
            }
            var reportingSalesData = new List<SupportSaleData>();

            foreach (var employeeList in sPPCPLEmployeeLists)
            {
                var PCPL_SP = employeeList.PCPL_Salespers_Purch_Name;
                var filterEMP = "Primary_Salesperson_Name eq '" + PCPL_SP + "'";

                if (FDate != null && TDate != null)
                {

                    filterEMP += "and Date ge " + FDate + " and Date le " + TDate;
                    //filter += $" and startswith(Customer_Name,'{TXTSp}')";
                }
                var report = ac.GetData<SupportSaleData>("SupportSaleData", filterEMP);
                if (report?.Result.Item1?.value != null && report.Result.Item1.value.Count > 0)
                {
                    reportingSalesData.AddRange(report.Result.Item1.value);
                }

            }

            combineSupportSaleData.SupportSaleDatas = SupportSaleData;
            combineSupportSaleData.SupportReportingSaleDatas = reportingSalesData;
            return combineSupportSaleData;
        }
        [HttpGet]
        [Route("GetWebsiteLog")]
        public List<WebsiteLog> GetWebsiteLog(string Fromdate, string Todate)
        {
            string filter = "";
            if (Fromdate != null && Todate != null)
            {
                filter = "Last_Modified_At ge " + Fromdate + " and Last_Modified_At le " + Todate;
            }
            API ac = new API();
            List<WebsiteLog> WebsiteLog = new List<WebsiteLog>();

            var Result = ac.GetData<WebsiteLog>("weblogreport", filter);
            //var Result = ac.GetData1<Complaint>("ComplaintReport", filter,skip,top,orderby);

            if (Result.Result.Item1.value.Count > 0)
                WebsiteLog = Result.Result.Item1.value;


            return WebsiteLog;
        }


        [Route("GetApiRecordsCount")]
        public int GetApiRecordsCount(string SPCode, string apiEndPointName, string fdate, string tdate, string text)
        {
            API ac = new API();
            string filter = "";
            if (fdate != null && tdate != null && text == null)
            {
                filter = "Com_Date ge " + fdate + " and Com_Date le " + tdate;
            }
            else if (fdate != null && tdate != null && text != null)
            {
                filter = "Com_Date ge " + fdate + " and Com_Date le " + tdate + " and Contact_Company_Name eq '" + text + "'";
            }
            else if (fdate == null && tdate == null && text != null)
            {
                filter = "Contact_Company_Name eq '" + text + "'";
            }
            var count = ac.CalculateCount(apiEndPointName, filter);

            return Convert.ToInt32(count.Result);
        }
        [HttpGet]
        [Route("GetCustomerOutStanding")]
        public List<SPCustomerOutstanding> GetCustomerOutStanding()
        {
            API ac = new API();
            List<SPCustomerOutstanding> customerOutStanding = new List<SPCustomerOutstanding>();
            var result = ac.GetData<SPCustomerOutstanding>("CollectionSummaryView", "");

            if (result != null && result.Result.Item1.value.Count > 0)
                customerOutStanding = result.Result.Item1.value;
            return customerOutStanding;
        }
        [HttpGet]
        [Route("GetTransporterDashboardList")]
        public List<SPTransporterDashboard> GetTransporterDashboardList(int skip, int top, string orderby)
        {
            API ac = new API();
            List<SPTransporterDashboard> transporterDashboards = new List<SPTransporterDashboard>();

            var Result = ac.GetData1<SPTransporterDashboard>("Transporterdashboard", "", skip, top, orderby);

            if (Result.Result.Item1.value.Count > 0)
                transporterDashboards = Result.Result.Item1.value;


            return transporterDashboards;
        }

        [HttpGet]
        [Route("GetSalespersonData")]
        public List<SPCustomerOutstanding> GetSalespersonData(string branchCode)
        {
            API ac = new API();
            List<SPCustomerOutstanding> Inv_ProductGroupsWise = new List<SPCustomerOutstanding>();
            var filter = "";
            filter += "LocationWise eq true and ISSalesPersonData eq true";

            var ProductGroupsWiseResult = ac.GetData<SPCustomerOutstanding>("CollectionSummaryView", filter);

            if (ProductGroupsWiseResult != null && ProductGroupsWiseResult.Result.Item1.value.Count > 0)
                Inv_ProductGroupsWise = ProductGroupsWiseResult.Result.Item1.value;

            return Inv_ProductGroupsWise;
        }

        [HttpGet]
        [Route("GetCustomerDataBySalesperson")]
        public List<SPCustomerOutstanding> GetCustomerDataBySalesperson(string spCode)
        {
            API ac = new API();
            List<SPCustomerOutstanding> Inv_ItemWise = new List<SPCustomerOutstanding>();

            var ItemWiseResult = ac.GetData<SPCustomerOutstanding>("CollectionSummaryView", "Salesperson_Code eq '" + spCode + "'");
            if (ItemWiseResult != null && ItemWiseResult.Result.Item1.value.Count > 0)
                Inv_ItemWise = ItemWiseResult.Result.Item1.value;

            return Inv_ItemWise;
        }

        [HttpGet]
        [Route("GetCustomerwiseInvoice")]
        public List<SPCustomerOutstanding> GetCustomerwiseInvoice(string customerCode)
        {
            API ac = new API();
            List<SPCustomerOutstanding> Inv_ItemWise = new List<SPCustomerOutstanding>();

            var ItemWiseResult = ac.GetData<SPCustomerOutstanding>("CollectionSummaryView", "Customer_Name eq '" + customerCode + "'");
            if (ItemWiseResult != null && ItemWiseResult.Result.Item1.value.Count > 0)
                Inv_ItemWise = ItemWiseResult.Result.Item1.value;

            return Inv_ItemWise;
        }


        [HttpGet]
        [Route("GetComplaintReportDailyVisitPlanApproved")]

        public SPUpdateComplaintStatusResponse GetComplaintReportDailyVisitPlanApproved(string Daily_Visit_No, string Status, string RowNo)
        {
            SPComplainstatusUpdate requestMU = new SPComplainstatusUpdate
            {
                No = Daily_Visit_No,
                Status = Status
            };

            var responseMU = new SPUpdateComplaintStatusResponse();
            dynamic result = null;
            result = PatchItemDailyVisit("DailyVisitsDotNetAPI", requestMU, responseMU, "No='" + Daily_Visit_No + "',Entry_Type='" + RowNo + "'");

            if (result.Result.Item1 != null)
            {
                responseMU = result.Result.Item1;
            }

            return responseMU;
        }


        public async Task<(SPUpdateComplaintStatusResponse, errorDetails)> PatchItemDailyVisit<SPUpdateComplaintStatusResponse>(string apiendpoint, SPComplainstatusUpdate requestModel, SPUpdateComplaintStatusResponse responseModel, string fieldWithValue)
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
                    responseModel = res.ToObject<SPUpdateComplaintStatusResponse>();


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

        [HttpGet]
        [Route("GetInquiryManagement")]
        public CombineInquiryManagement GetInquiryManagement(string Sales_Person_Code, string No)
        {
            string filter = "Sales_Person_Code eq '" + Sales_Person_Code + "'";
            API ac = new API();
            CombineInquiryManagement CombineInquiryManagement = new CombineInquiryManagement();
            List<InquiryManagement> inquiryManagements = new List<InquiryManagement>();
            var Result = ac.GetData<InquiryManagement>("Inquirymanagement", filter);

            if (Result.Result.Item1.value.Count > 0)
                inquiryManagements = Result.Result.Item1.value;

            //reporting Salesperson 

            string inquiryfilter = "PCPL_Reporting_Person_No eq '" + No + "'";
            List<SPPCPLEmployeeList> sPPCPLEmployeeLists = new List<SPPCPLEmployeeList>();
            var SPPCPLEmployeeLists = ac.GetData<SPPCPLEmployeeList>("PcplEmployeeList", inquiryfilter);
            if (SPPCPLEmployeeLists.Result.Item1.value.Count > 0)
            {
                sPPCPLEmployeeLists = SPPCPLEmployeeLists.Result.Item1.value;
            }

            var EmployeeWiseInquiry = new List<InquiryManagement>();

            foreach (var employeeList in sPPCPLEmployeeLists)
            {
                var PCPL_SP = employeeList.PCPL_Salespers_Purch_Name;
                var filteremployee = "Sales_Person_Name eq '" + PCPL_SP + "'";
                var report = ac.GetData<InquiryManagement>("Inquirymanagement", filteremployee);
                if (report?.Result.Item1?.value != null && report.Result.Item1.value.Count > 0)
                {
                    EmployeeWiseInquiry.AddRange(report.Result.Item1.value);
                }
            }

            CombineInquiryManagement.InquiryManagements = inquiryManagements;
            CombineInquiryManagement.EmployeeWiseInquiryManagements = EmployeeWiseInquiry.Where(x => !inquiryManagements.Any(y => y.Sales_Person_Name == x.Sales_Person_Name)).ToList();
            //CombineInquiryManagement.EmployeeWiseInquiryManagements
            return CombineInquiryManagement;
        }

        [HttpGet]
        [Route("GetSalesPesonQuotes")]
        public List<InquirySalesPersonQuotes> GetSalesPesonQuotes(string Salesperson_Name, string FromDate, string ToDate)
        {
            string filter = "Salesperson_Name eq '" + Salesperson_Name + "'";
            if (FromDate != null && ToDate != null)
            {
                filter += " and Document_Date ge " + FromDate + " and Document_Date le " + ToDate;
            }
            API ac = new API();
            List<InquirySalesPersonQuotes> inquirySalesPersonQuotes = new List<InquirySalesPersonQuotes>();

            var Result = ac.GetData<InquirySalesPersonQuotes>("salesersonsalequote", filter);
            //var Result = ac.GetData1<Complaint>("ComplaintReport", filter,skip,top,orderby);

            if (Result.Result.Item1.value.Count > 0)
                inquirySalesPersonQuotes = Result.Result.Item1.value;


            return inquirySalesPersonQuotes;
        }

        [HttpGet]
        [Route("GetSalesPesonInquiry")]
        public List<InquirySalesPersonInquiry> GetSalesPesonInquiry(string Salesperson_Name, string FromDate, string ToDate)
        {
            string filter = "Salesperson_Name eq '" + Salesperson_Name + "'";
            if (FromDate != null && ToDate != null)
            {
                filter += " and Document_Date ge " + FromDate + " and Document_Date le " + ToDate;
            }
            API ac = new API();
            List<InquirySalesPersonInquiry> inquirySalesPersonInquiries = new List<InquirySalesPersonInquiry>();

            var Result = ac.GetData<InquirySalesPersonInquiry>("Salepersoninquiry", filter);
            //var Result = ac.GetData1<Complaint>("ComplaintReport", filter,skip,top,orderby);

            if (Result.Result.Item1.value.Count > 0)
                inquirySalesPersonInquiries = Result.Result.Item1.value;


            return inquirySalesPersonInquiries;
        }

        [HttpGet]
        [Route("GetDailyVisitMonthWise")]
        public CombineDailyVisitMonthWise GetDailyVisitMonthWise(string Salesperson_Code, string No)
        {
            string filter = "";
            filter = "SalesPerson eq '" + Salesperson_Code + "'";
            //if (Fromdate != null && Todate != null)
            //{
            //    filter += " and Visit_Date ge " + Fromdate + " and Visit_Date le " + Todate;
            //}
            API ac = new API();
            CombineDailyVisitMonthWise CombineDailyVisitMonthWise = new CombineDailyVisitMonthWise();
            List<DailyVisitMonthWise> dailyVisitMonthWises = new List<DailyVisitMonthWise>();

            ComplaintReportDailyVisitPlan responseComplaintReportDailyVisitPlan = new ComplaintReportDailyVisitPlan();
            var Result = ac.GetData<DailyVisitMonthWise>("Dailyvisitmonthly", filter);
            //var Result = ac.GetData1<Complaint>("ComplaintReport", filter,skip,top,orderby);

            if (Result.Result.Item1.value.Count > 0)
                dailyVisitMonthWises = Result.Result.Item1.value;

            string reportingPerson = "PCPL_Reporting_Person_No eq '" + No + "'";
            List<SPPCPLEmployeeList> sPPCPLEmployeeLists = new List<SPPCPLEmployeeList>();
            var SPPCPLEmployeeLists = ac.GetData<SPPCPLEmployeeList>("PcplEmployeeList", reportingPerson);
            if (SPPCPLEmployeeLists.Result.Item1.value.Count > 0)
            {
                sPPCPLEmployeeLists = SPPCPLEmployeeLists.Result.Item1.value;
            }

            var EmployeeReport = new List<DailyVisitMonthWise>();
            foreach (var employeereport in sPPCPLEmployeeLists)
            {
                var PCPL_SP = employeereport.PCPL_Salespers_Purch_Name;
                var filterEmployeeList = "SalesPerson_Name eq '" + PCPL_SP + "'";
                var report = ac.GetData<DailyVisitMonthWise>("Dailyvisitmonthly", filterEmployeeList);

                if (report?.Result.Item1?.value != null && report.Result.Item1.value.Count > 0)
                {
                    EmployeeReport.AddRange(report.Result.Item1.value);
                }
            }

            CombineDailyVisitMonthWise.DailyVisitMonthWises = dailyVisitMonthWises;
            CombineDailyVisitMonthWise.EmployeeDailyVisitMonthWise = EmployeeReport.DistinctBy(c => c.SalesPerson_Name).Where(x => !dailyVisitMonthWises.Any(y => y.SalesPerson_Name == x.SalesPerson_Name)).ToList();
            return CombineDailyVisitMonthWise;
        }


        public async Task<(ComplaintReportDailyVisitPlan, errorDetails)> PatchItemDailyVisit<ComplaintReportDailyVisitPlan>(string apiendpoint, ComplaintReportDailyVisitPlan requestModel, ComplaintReportDailyVisitPlan responseModel, string fieldWithValue)
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
                    responseModel = res.ToObject<ComplaintReportDailyVisitPlan>();


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
        [HttpGet]
        [Route("GetTaskPerformance")]
        public CombineTaskPerformance GetTaskPerformance(string Sales_Person_Code, string No)
        {

            API ac = new API();
            CombineTaskPerformance combineTaskPerformance = new CombineTaskPerformance();
            List<TaskPerformance> taskPerformances = new List<TaskPerformance>();
            var filter = "SalesPerson_Code eq '" + Sales_Person_Code + "'";
            //ComplaintReportDailyVisitPlan responseComplaintReportDailyVisitPlan = new ComplaintReportDailyVisitPlan();
            var Result = ac.GetData<TaskPerformance>("TaskPerformance", filter);
            //var Result = ac.GetData1<Complaint>("ComplaintReport", filter,skip,top,orderby);

            if (Result.Result.Item1.value.Count > 0)
                taskPerformances = Result.Result.Item1.value;
            taskPerformances = taskPerformances.DistinctBy(a => a.SalesPerson_Code).ToList();

            string reportingPerson = "PCPL_Reporting_Person_No eq '" + No + "'";
            List<SPPCPLEmployeeList> sPPCPLEmployeeLists = new List<SPPCPLEmployeeList>();
            var SPPCPLEmployeeLists = ac.GetData<SPPCPLEmployeeList>("PcplEmployeeList", reportingPerson);
            if (SPPCPLEmployeeLists.Result.Item1.value.Count > 0)
            {
                sPPCPLEmployeeLists = SPPCPLEmployeeLists.Result.Item1.value;
            }

            var EmployeeReport = new List<TaskPerformance>();
            foreach (var employeereport in sPPCPLEmployeeLists)
            {
                var PCPL_SP = employeereport.PCPL_Salespers_Purch_Name;
                var filterEmployeeList = "SalesPerson_Name eq '" + PCPL_SP + "'";
                var report = ac.GetData<TaskPerformance>("TaskPerformance", filterEmployeeList);

                if (report?.Result.Item1?.value != null && report.Result.Item1.value.Count > 0)
                {
                    EmployeeReport.AddRange(report.Result.Item1.value);

                }
            }

            combineTaskPerformance.TaskPerformancesList = taskPerformances;
            combineTaskPerformance.TaskPerformanceReportingList = EmployeeReport.DistinctBy(a => a.SalesPerson_Name).Where(x => !taskPerformances.Any(y => y.SalesPerson_Code == x.SalesPerson_Code)).ToList();
            return combineTaskPerformance;
        }
        [HttpGet]
        [Route("GetSalesWiseTaskPerformance")]
        public List<TaskPerformance> GetSalesWiseTaskPerformance(string Sales_Person_Code)
        {

            API ac = new API();

            List<TaskPerformance> taskPerformances = new List<TaskPerformance>();
            var filter = "SalesPerson_Name eq '" + Sales_Person_Code + "'";

            var Result = ac.GetData<TaskPerformance>("TaskPerformance", filter);

            if (Result.Result.Item1.value.Count > 0)
                taskPerformances = Result.Result.Item1.value;

            return taskPerformances;


        }
        [HttpGet]
        [Route("GetSalesPerformance")]
        public List<SalesPerformance> GetSalesPerformance(string Sales_Person_Code)
        {
            API ac = new API();
            var filter = "Salesperson_Code eq ' " + Sales_Person_Code + " ' and IsTotalBranchWise eq true";

            List<SalesPerformance> salesPerformances = new List<SalesPerformance>();
            var Result = ac.GetData<SalesPerformance>("Salesperfrmnacebranch", filter);
            if (Result.Result.Item1.value.Count > 0)
                salesPerformances = Result.Result.Item1.value;
            salesPerformances = salesPerformances.DistinctBy(x => x.Branch_Name).ToList();
            return salesPerformances;
        }

        [HttpGet]
        [Route("GetBranchWiseProduct")]
        public List<SalesPerformance> GetBranchWiseProduct(string BranchName, string SalesPerson)
        {
            var filter = "Branch_Name eq '" + BranchName + "' and IsTotalBranchWise eq false and Salesperson_Code eq '" + SalesPerson + "'";
            API ac = new API();
            List<SalesPerformance> branchWiseProducts = new List<SalesPerformance>();
            var Result = ac.GetData<SalesPerformance>("Salesperfrmnacebranch", filter);

            if (Result.Result.Item1.value.Count > 0)
                branchWiseProducts = Result.Result.Item1.value;

            return branchWiseProducts;
        }
        [Route("GetApiRecordsCounts")]
        public int GetApiRecordsCounts(string SPCode, string apiEndPointName)
        {
            API ac = new API();

            var count = ac.CalculateCount(apiEndPointName, "");

            return Convert.ToInt32(count.Result);
        }


        [HttpGet]
        [Route("GetSalesPesonConfirmQuotes")]
        public List<InquirySalesPersonQuotes> GetSalesPesonConfirmQuotes(string Salesperson_Name, string FromDate, string ToDate)
        {
            string filter = "Salesperson_Name eq '" + Salesperson_Name + "'";
            if (FromDate != null && ToDate != null)
            {
                filter += " and Document_Date ge " + FromDate + " and Document_Date le " + ToDate;
            }
            API ac = new API();
            List<InquirySalesPersonQuotes> inquirySalesPersonQuotes = new List<InquirySalesPersonQuotes>();

            var Result = ac.GetData<InquirySalesPersonQuotes>("confirmsalesquote", filter);
            //var Result = ac.GetData1<Complaint>("ComplaintReport", filter,skip,top,orderby);

            if (Result.Result.Item1.value.Count > 0)
                inquirySalesPersonQuotes = Result.Result.Item1.value;


            return inquirySalesPersonQuotes;
        }

        [HttpGet]
        [Route("GetStockManagement")]
        public List<StockManagement> GetStockManagement()
        {
            string filter = "Branch_Total eq true and Branch ne ''";
            API ac = new API();
            List<StockManagement> stockManagements = new List<StockManagement>();

            var Result = ac.GetData<StockManagement>("StockManagement", filter);
            if (Result.Result.Item1.value.Count > 0)
                stockManagements = Result.Result.Item1.value;


            return stockManagements;
        }
        [HttpGet]
        [Route("GetBranchWiseProducts")]
        public CombineStockManagement GetBranchWiseProducts(string BranchName)
        {
            string filter = "Branch eq '" + BranchName + "' and Item_Location_Total eq true";
            API ac = new API();
            List<StockManagement> stockManagements = new List<StockManagement>();
            List<StockManagement> branchWiseTotal = new List<StockManagement>();
            CombineStockManagement combineStockManagement = new CombineStockManagement();

            var Result = ac.GetData<StockManagement>("StockManagement", filter);
            if (Result.Result.Item1.value.Count > 0)
            stockManagements = Result.Result.Item1.value;
            stockManagements = stockManagements.OrderBy(c => c.Location_Code).ToList();

            string filter1 = "Branch eq '" + BranchName + "' and Branch_Total eq true";
            var Result1 = ac.GetData<StockManagement>("StockManagement", filter1);
            if (Result1.Result.Item1.value.Count > 0)
                branchWiseTotal = Result1.Result.Item1.value;

            List<StockManagement> locationname = new List<StockManagement>();
            List<StockManagement> locationtotal = new List<StockManagement>();
            locationname = stockManagements.DistinctBy(c => c.Location_Code).ToList();
           foreach(var loc in locationname)
            {
                string locfilter = "Location_Total eq true and Location_Code eq '"+ loc.Location_Code + "'";
                var locationwisetotal = ac.GetData<StockManagement>("StockManagement", locfilter); 
                if(locationwisetotal.Result.items.value.Count > 0)
                {
                    locationtotal.AddRange(locationwisetotal.Result.items.value);
                }
            } 
             stockManagements = stockManagements.Concat(locationtotal).ToList();

            combineStockManagement.LocationWiseTotalList= locationtotal;
            combineStockManagement.BranchProductWise = stockManagements;
            combineStockManagement.BranchWiseTotalList = branchWiseTotal;
            return combineStockManagement;
        }
        [HttpGet]
        [Route("GetProductPackingStyle")]
        public List<StockManagement> GetProductPackingStyle(string BranchName, string Product)
        {
            string filter = "Location_Code eq '" + BranchName + "' and Description eq '" + Product + "' and  Item_Location_Total eq false";
            API ac = new API();
            List<StockManagement> stockManagements = new List<StockManagement>();

            var Result = ac.GetData<StockManagement>("StockManagement", filter);
            if (Result.Result.Item1.value.Count > 0)
                stockManagements = Result.Result.Item1.value;


            return stockManagements;
        }

    }

}

