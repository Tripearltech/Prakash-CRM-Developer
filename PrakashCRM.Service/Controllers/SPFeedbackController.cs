using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using PrakashCRM.Data.Models;
using PrakashCRM.Service.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Threading.Tasks;
using Microsoft.Ajax.Utilities;

namespace PrakashCRM.Service.Controllers
{
    [RoutePrefix("api/SPFeedback")]
    public class SPFeedbackController : ApiController
    {
        #region FeedbackList

        [Route("GetAllFeedbackHeaderDetails")]
        public List<SPFeedBackHeaderList> GetAllFeedbackHeaderDetails(string SPNo, int skip, int top, string orderby, string filter, bool isExport = false)
        {
            API ac = new API();
            List<SPFeedBackHeaderList> feedbackHeaderList = new List<SPFeedBackHeaderList>();

            if (filter == null)
                filter = "";

            //if (filter == "" || filter == null)
            //    filter = "Salesperson_Code eq '" + SPNo + "'";
            //else
            //    filter = filter + " and Salesperson_Code eq '" + SPNo + "'";

            var result = (dynamic)null;

            if (isExport)
                result = ac.GetData<SPFeedBackHeaderList>("FeedbackHeadersListDotNetAPI", filter);
            else
                result = ac.GetData1<SPFeedBackHeaderList>("FeedbackHeadersListDotNetAPI", filter, skip, top, orderby);

            if (result.Result.Item1.value.Count > 0)
                feedbackHeaderList = result.Result.Item1.value;

            return feedbackHeaderList;
        }

        [Route("GetApiRecordsCount")]
        public int GetApiRecordsCount(string SPNo, string apiEndPointName, string filter)
        {
            API ac = new API();

            if (filter == null)
                filter = "";

            //if (filter == "" || filter == null)
            //    filter = "Salesperson_Code eq '" + SPNo + "'";
            //else
            //    filter = filter + " and Salesperson_Code eq '" + SPNo + "'";

            var count = ac.CalculateCount(apiEndPointName, filter);

            return Convert.ToInt32(count.Result);
        }

        [Route("GetAllFeedbackLines")]
        public List<SPFeedBackLineList> GetAllFeedbackLines(string FeedbackHeaderNo)
        {
            API ac = new API();
            List<SPFeedBackLineList> feedbackLineList = new List<SPFeedBackLineList>();

            var result = ac.GetData<SPFeedBackLineList>("FeedbackLinesListDotNetAPI", "Feedback_Header_No eq '" + FeedbackHeaderNo + "'");

            if (result.Result.Item1.value.Count > 0)
                feedbackLineList = result.Result.Item1.value;

            return feedbackLineList;
        }

        #endregion

        #region FeedbackChart

        [HttpGet]
        [Route("BindBarChart")]
        public List<SPFeedBacksForBarChart> BindBarChart(string filter)
        {
            API ac = new API();
            List<SPFeedBacksForBarChart> feedBackForBarChart = new List<SPFeedBacksForBarChart>();

            if (filter == null)
                filter = "";

            var result = (dynamic)null;

            if (filter != "" && filter != null)
            {
                result = ac.GetData<SPFeedBacksForBarChart>("Feedback", filter);
            }
            else
            {
                result = ac.GetData<SPFeedBacksForBarChart>("Feedback", "");
            }

            if (result.Result.Item1.value.Count > 0)
                feedBackForBarChart = result.Result.Item1.value;

            return feedBackForBarChart;
        }

        [Route("GetAllSalesPersonForDDL")]
        public List<SPSalespeoplePurchaser> GetAllSalesPersonForDDL(string SalesPerson, string No)
        {
            API ac = new API();
            List<SPSalespeoplePurchaser> salesperson = new List<SPSalespeoplePurchaser>();
            var filter = "Code eq '" + SalesPerson + "'";
            var result = ac.GetData<SPSalespeoplePurchaser>("SalespersonPurchaserDotNetAPI",filter);

            if (result != null && result.Result.Item1.value.Count > 0)
                salesperson = result.Result.Item1.value;

            string supportsalesfilter = "PCPL_Reporting_Person_No eq '" + No + "'";
            List<SPPCPLEmployeeList> sPPCPLEmployeeLists = new List<SPPCPLEmployeeList>();
            var SPPCPLEmployeeLists = ac.GetData<SPPCPLEmployeeList>("PcplEmployeeList", supportsalesfilter);
            if (SPPCPLEmployeeLists.Result.Item1.value.Count > 0)
            {
                sPPCPLEmployeeLists = SPPCPLEmployeeLists.Result.Item1.value;
            }
            var reportingsalesperson = new List<SPSalespeoplePurchaser>();
            foreach(var employelist in sPPCPLEmployeeLists)
            {
                var PCPL_SP = employelist.PCPL_Salespers_Purch_Name;
                var filteremp = "Name eq '" + PCPL_SP + "'";
                var report = ac.GetData<SPSalespeoplePurchaser>("SalespersonPurchaserDotNetAPI", filteremp);
                if (report?.Result.Item1?.value != null && report.Result.Item1.value.Count > 0)
                {
                    reportingsalesperson.AddRange(report.Result.Item1.value);
                }
            }
            salesperson = salesperson.Concat(reportingsalesperson).ToList();
            return salesperson;
        }

        [Route("GetAllCustomerRating")]
        public List<SPCustomerRating> GetAllCustomerRating(string QuestionNo, string FromDate, string ToDate, string SPCode)
        {
            API ac = new API();
            List<SPCustomerRating> customerRating = new List<SPCustomerRating>();

            var filter = "";
            if (SPCode != "" && SPCode != null)
            {
                filter = "Feedback_Question_No_ eq '" + QuestionNo + "' and Submitted_On_Filter_FilterOnly ge " + FromDate + " and Submitted_On_Filter_FilterOnly le " + ToDate + " and Employee_Filter_FilterOnly eq '" + SPCode + "'";
            }
            else
            {
                filter = "Feedback_Question_No_ eq '" + QuestionNo + "' and Submitted_On_Filter_FilterOnly ge " + FromDate + " and Submitted_On_Filter_FilterOnly le " + ToDate;
            }

            var result = ac.GetData<SPCustomerRating>("CustomerWiseRatingdetails", filter);

            if (result.Result.Item1.value.Count > 0)
                customerRating = result.Result.Item1.value;

            return customerRating;
        }

        #endregion

        #region CustomerSatisfaction

        [HttpPost]
        [Route("BindPieChart")]
        public async Task<List<PieChartResponseModel>> BindPieChart(PieChartRequestModel pieChartRequestModel)
        {
            List<PieChartResponseModel> pieChartResponseModel = new List<PieChartResponseModel>();

            string _codeUnitBaseUrl = System.Configuration.ConfigurationManager.AppSettings["CodeUnitBaseURL"];
            string _tenantId = System.Configuration.ConfigurationManager.AppSettings["TenantID"];
            string _environment = System.Configuration.ConfigurationManager.AppSettings["Environment"];
            string _companyName = System.Configuration.ConfigurationManager.AppSettings["CompanyName"];

            API ac = new API();
            var accessToken = await ac.GetAccessToken();

            HttpClient _httpClient = new HttpClient();
            string encodeurl = Uri.EscapeUriString(_codeUnitBaseUrl.Replace("{TenantID}", _tenantId).Replace("{Environment}", _environment).Replace("{CompanyName}", _companyName).Replace("{Endpoint}", "CodeunitAPIMgmt_GetOverAllRatingPercentage"));

            Uri baseuri = new Uri(encodeurl);
            _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken.Token);


            string ItemCardObjString = JsonConvert.SerializeObject(pieChartRequestModel);
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
                    // pieChartResponseModel = res.ToObject<List<PieChartResponseModel>>();
                    pieChartResponseModel = ConvertJsonToList(JsonData);
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


            return pieChartResponseModel;
        }

        public static List<PieChartResponseModel> ConvertJsonToList(string jsonData)
        {
            // Parse the JSON string into a JObject
            JObject res = JObject.Parse(jsonData);

            // Extract the "value" property, which contains the actual JSON array (as a string)
            string valueJson = res["value"]?.ToString();

            // Deserialize the JSON array string into a List<PieChartResponseModel>
            return JsonConvert.DeserializeObject<List<PieChartResponseModel>>(valueJson) ?? new List<PieChartResponseModel>();
        }

        [Route("GetAllCustomerList")]
        public List<SPCustomerOverallRating> GetAllCustomerList(string Rating, string FromDate, string ToDate, string SPCode)
        {
            API ac = new API();
            List<SPCustomerOverallRating> customerRating = new List<SPCustomerOverallRating>();

            var filter = "";
            if (SPCode != "" && SPCode != null)
            {
                filter = "Overall_Rating_Filter eq " + Rating + " and Submitted_On_Filter_FilterOnly ge " + FromDate + " and Submitted_On_Filter_FilterOnly le " + ToDate + " and Employee_Filter_FilterOnly eq '" + SPCode + "'";
            }
            else
            {
                filter = "Overall_Rating_Filter eq " + Rating + " and Submitted_On_Filter_FilterOnly ge " + FromDate + " and Submitted_On_Filter_FilterOnly le " + ToDate;
            }

            var result = ac.GetData<SPCustomerOverallRating>("CustomerWiseOverAllRatings", filter);

            if (result.Result.Item1.value.Count > 0)
                customerRating = result.Result.Item1.value;

            return customerRating;
        }

        #endregion

        // feedback report
        [HttpGet]
        [Route("GetFeedBackList")]

        public List<FeedBackList> GetFeedBackList(string Employee, string FDate, string TDate, string TXTSp)
        {
            API ac = new API();
            List<FeedBackList> feedBackQuestions = new List<FeedBackList>();

            var filter = "";

            if (FDate != null && TDate != null && TXTSp == null)
            {
                filter = "Employee eq '" + Employee + "' and Submitted_On ge " + FDate + " and Submitted_On le " + TDate;
            }
            else if (FDate != null && TDate != null && TXTSp != null)
            {
                filter = "Employee eq '" + Employee + "' and Submitted_On ge " + FDate + " and Submitted_On le " + TDate + " and Company_Name eq '" + TXTSp + "'";

            }
            else if (FDate == null && TDate == null && TXTSp != null)
            {

                filter = "Employee eq '" + Employee + "' and Company_Name eq '" + TXTSp + "'";
            }
            if (filter == "")
            {
                filter = "Employee eq '" + Employee + "'";
            }
            var result = ac.GetData<FeedBackList>("FeedbackHeadersListDotNetAPI", filter);

            if (result != null && result.Result.Item1.value.Count > 0)
            {
                feedBackQuestions = result.Result.Item1.value;
            }
            return feedBackQuestions;
        }


        [HttpGet]
        [Route("GetFeedBackLineList")]

        public List<FeedbBackLines> GetFeedBackLineList(string FeedbackId)
        {
            API ac = new API();
            List<FeedbBackLines> feedBackQuestions = new List<FeedbBackLines>();
            var result = (dynamic)null;
            var filter = "";
            filter += "Feedback_Header_No eq '" + FeedbackId + "'";
            result = ac.GetData<FeedbBackLines>("FeedbackLinesListDotNetAPI", filter);
            if (result != null && result.Result.Item1.value.Count > 0)
            {
                feedBackQuestions = result.Result.Item1.value;
            }
            return feedBackQuestions;
        }
        [HttpGet]
        [Route("GetFeedBackQuestionList")]

        public List<FeedbBackQuestion> GetFeedBackQuestionList()
        {
            API ac = new API();
            List<FeedbBackQuestion> feedBackQuestions = new List<FeedbBackQuestion>();
            var result = (dynamic)null;
            result = ac.GetData<FeedbBackQuestion>("FeedbackQuestionListDotNetAPI", "");
            if (result != null && result.Result.Item1.value.Count > 0)
            {
                feedBackQuestions = result.Result.Item1.value;//.DistinctBy(a => a.Code).ToList();
                feedBackQuestions = feedBackQuestions.DistinctBy(a => a.No).ToList();
            }
            return feedBackQuestions;
        }

        [HttpPost]
        [Route("AddFeedBackQuestion")]
        public async Task<IHttpActionResult> AddFeedBackQuestion(FeedbBackQuestion request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Feedback_Question))
                return BadRequest("Feedback_Question is required.");

            if (string.IsNullOrWhiteSpace(request.Order_No))
                return BadRequest("Order_No is required.");

            API ac = new API();
            FeedbBackQuestion model = new FeedbBackQuestion
            {
                No = string.IsNullOrWhiteSpace(request.No) ? string.Empty : request.No.Trim(),
                Feedback_Question = request.Feedback_Question.Trim(),
                Order_No = request.Order_No.Trim(),
                IsActive = request.IsActive
            };

            FeedbBackQuestion responseModel = new FeedbBackQuestion();
            var result = await ac.PostItem("FeedbackQuestionListDotNetAPI", model, responseModel);

            if (result.Item2 != null && !result.Item2.isSuccess)
                return Content(HttpStatusCode.BadRequest, result.Item2);

            return Ok(result.Item1 ?? responseModel);
        }

        [AcceptVerbs("POST", "PATCH")]
        [Route("UpdateFeedBackQuestion")]
        public async Task<IHttpActionResult> UpdateFeedBackQuestion(FeedbBackQuestion request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.No))
                    return BadRequest("No is required.");
                if (string.IsNullOrWhiteSpace(request.Feedback_Question))
                    return BadRequest("Feedback_Question is required.");
                if (string.IsNullOrWhiteSpace(request.Order_No))
                    return BadRequest("Order_No is required.");

                API ac = new API();
                FeedbBackQuestion model = new FeedbBackQuestion
                {
                    No = request.No.Trim(),
                    Feedback_Question = request.Feedback_Question.Trim(),
                    Order_No = request.Order_No.Trim(),
                    IsActive = request.IsActive
                };
                FeedbBackQuestion responseModel = new FeedbBackQuestion();
                string noEscaped = request.No.Trim().Replace("'", "''");
                var result = await ac.PatchItem("FeedbackQuestionListDotNetAPI", model, responseModel, "No='" + noEscaped + "'");

                if (result.Item2 != null && !result.Item2.isSuccess)
                    return Content(HttpStatusCode.BadRequest, result.Item2);

                return Ok(result.Item1 ?? responseModel);
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, new { isSuccess = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("GetCustomerDropdown")]
        public List<CustomerDropdown> GetCustomerDropdown(string prefix)
        {
            API ac = new API();
            List<CustomerDropdown> customerReports = new List<CustomerDropdown>();
            var result = ac.GetData<CustomerDropdown>("ContactDotNetAPI", "startswith(Company_Name,'" + prefix + "')");

            if (result != null && result.Result.Item1.value.Count > 0)

                customerReports = result.Result.Item1.value;
            return customerReports;

        }


    }
}
