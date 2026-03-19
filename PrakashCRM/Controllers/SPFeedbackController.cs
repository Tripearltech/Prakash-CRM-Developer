using ClosedXML.Excel;
using DocumentFormat.OpenXml.ExtendedProperties;
using Newtonsoft.Json;
using PrakashCRM.Data.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace PrakashCRM.Controllers
{
    public class SPFeedbackController : Controller
    {
        // GET: SPFeedback
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult FeedBackQuestion()
        {
            return View();
        }

        public ActionResult FeedbackList()
        {
            var Model = new CustomerDropdown
            {
                Company_Name = "",
                Company_No = "",
            };

            return View(Model);
        }

        public async Task<JsonResult> GetFeedbackListData(int orderBy, string orderDir, string filter, int skip, int top)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "Feedback/";

            string orderByField = "";

            switch (orderBy)
            {
                case 1:
                    orderByField = "No " + orderDir;
                    break;
                case 2:
                    orderByField = "Company_No " + orderDir;
                    break;
                case 3:
                    orderByField = "Company_Name " + orderDir;
                    break;
                case 4:
                    orderByField = "Products " + orderDir;
                    break;
                case 5:
                    orderByField = "Overall_Rating " + orderDir;
                    break;
                case 6:
                    orderByField = "Suggestion " + orderDir;
                    break;
                default:
                    orderByField = "No asc";
                    break;
            }

            apiUrl = apiUrl + "GetAllFeedbackHeaderDetails?SPNo=" + Session["loggedInUserNo"].ToString() + "&skip=" + skip + "&top=" + top + "&orderby=" + orderByField + "&filter=" + filter;

            HttpClient client = new HttpClient();
            List<SPFeedBackHeaderList> feedbackHeaderList = new List<SPFeedBackHeaderList>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                feedbackHeaderList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPFeedBackHeaderList>>(data);
            }

            return Json(feedbackHeaderList, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> ExportListData(int orderBy, string orderDir, string filter, int skip, int top)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPFeedback/";

            string orderByField = "";

            switch (orderBy)
            {
                case 1:
                    orderByField = "No " + orderDir;
                    break;
                case 2:
                    orderByField = "Customer_No " + orderDir;
                    break;
                case 3:
                    orderByField = "Customer_Name " + orderDir;
                    break;
                case 4:
                    orderByField = "Products " + orderDir;
                    break;
                case 5:
                    orderByField = "Overall_Rating " + orderDir;
                    break;
                case 6:
                    orderByField = "Suggestion " + orderDir;
                    break;
                case 7:
                    orderByField = "IsActive " + orderDir;
                    break;
                default:
                    orderByField = "No asc";
                    break;
            }

            apiUrl = apiUrl + "GetAllFeedbackHeaderDetails?SPNo=" + Session["loggedInUserNo"].ToString() + "&skip=" + skip + "&top=" + top + "&orderby=" + orderByField + "&filter=" + filter + "&isExport=true";

            HttpClient client = new HttpClient();
            List<SPFeedBackHeaderList> feedbackHeaderList = new List<SPFeedBackHeaderList>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                feedbackHeaderList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPFeedBackHeaderList>>(data);
            }

            DataTable dt = ToDataTable(feedbackHeaderList);

            //Name of File  
            string fileName = "FeedbackList.xlsx";
            string fullPath = Path.Combine(Server.MapPath("~/temp"), fileName);
            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(dt);

                using (var exportData = new MemoryStream())
                {
                    wb.SaveAs(exportData);
                    FileStream file = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
                    exportData.WriteTo(file);
                    file.Close();
                }
            }

            return Json(new { fileName = fileName, errorMessage = "" }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Download(string file)
        {
            //get the temp folder and file path in server
            string fullPath = Path.Combine(Server.MapPath("~/temp"), file);

            //return the file for download, this is an Excel 
            //so I set the file content type to "application/vnd.ms-excel"
            return File(fullPath, "application/vnd.ms-excel", file);
        }

        public static DataTable ToDataTable<T>(List<T> items)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);

            //Get all the properties
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Defining type of data column gives proper data table 
                var type = (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) ? Nullable.GetUnderlyingType(prop.PropertyType) : prop.PropertyType);
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name, type);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            //put a breakpoint here and check datatable
            return dataTable;
        }

        public async Task<JsonResult> GetAllFeedbackLinesForPopup(string FeedbackHeaderNo)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPFeedback/";

            apiUrl = apiUrl + "GetAllFeedbackLines?FeedbackHeaderNo=" + FeedbackHeaderNo;

            HttpClient client = new HttpClient();
            List<SPFeedBackLineList> feedbackLineList = new List<SPFeedBackLineList>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                feedbackLineList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPFeedBackLineList>>(data);
            }

            return Json(feedbackLineList, JsonRequestBehavior.AllowGet);
        }

        #region FeedbackBarChart
        public ActionResult FeedbackChart()
        {
            return View();
        }

        public async Task<JsonResult> BindBarChart(string filter)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPFeedback/";

            apiUrl = apiUrl + "BindBarChart?filter=" + filter;

            HttpClient client = new HttpClient();
            List<SPFeedBacksForBarChart> feedBackForBarChart = new List<SPFeedBacksForBarChart>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                feedBackForBarChart = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPFeedBacksForBarChart>>(data);
            }

            return Json(feedBackForBarChart, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetAllSalesPersonForDDL()
        {
            string SalesPerson = Session["loggedInUserSPCode"].ToString();
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPFeedback/GetAllSalesPersonForDDL?SalesPerson=" + SalesPerson + "&No=" + Session["loggedInUserNo"].ToString();

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
            }

            return Json(salesperson, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetAllCustomerRatingForPopup(string QuestionNo, string FromDate, string ToDate, string SPCode)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPFeedback/";

            apiUrl = apiUrl + "GetAllCustomerRating?QuestionNo=" + QuestionNo + "&FromDate=" + FromDate + "&ToDate=" + ToDate + "&SPCode=" + SPCode;

            HttpClient client = new HttpClient();
            List<SPCustomerRating> customerRating = new List<SPCustomerRating>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                customerRating = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPCustomerRating>>(data);
            }

            return Json(customerRating, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region CustomerSatisfaction

        public ActionResult CustomerSatisfaction()
        {
            return View();
        }

        public async Task<JsonResult> BindPieChart(string fromdate,string todate,string spcode)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPFeedback/";

            PieChartRequestModel reqModel = new PieChartRequestModel();
            reqModel.fromdate = fromdate;
            reqModel.todate = todate;   
            reqModel.employeeid= spcode;

            List<PieChartResponseModel> responseModel = new List<PieChartResponseModel>();
            HttpClient client = new HttpClient();
            apiUrl = apiUrl + "BindPieChart";
            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            string UserObjString = JsonConvert.SerializeObject(reqModel);
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
                responseModel = Newtonsoft.Json.JsonConvert.DeserializeObject<List<PieChartResponseModel>>(data);
            }

            return Json(responseModel, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetAllCustomerListForPopup(string Rating, string FromDate, string ToDate, string SPCode)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPFeedback/";

            apiUrl = apiUrl + "GetAllCustomerList?Rating=" + Rating + "&FromDate=" + FromDate + "&ToDate=" + ToDate + "&SPCode=" + SPCode;

            HttpClient client = new HttpClient();
            List<SPCustomerOverallRating> customerRating = new List<SPCustomerOverallRating>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                customerRating = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPCustomerOverallRating>>(data);
            }

            return Json(customerRating, JsonRequestBehavior.AllowGet);
        }

        #endregion

        public async Task<JsonResult> GetFeedBackList(string FDate, string TDate, string TXTSp)
        {
            var Employee = Session["loggedInUserSPCode"].ToString();
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPFeedback/GetFeedBackList?Employee=" + Employee +"&FDate=" +FDate + "&TDate=" + TDate + "&TXTSp=" + TXTSp;
            HttpClient client = new HttpClient();
            List<FeedBackList> FeedBackQuestions = new List<FeedBackList>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                FeedBackQuestions = Newtonsoft.Json.JsonConvert.DeserializeObject<List<FeedBackList>>(data);

            }
            return Json(FeedBackQuestions, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<JsonResult> GetFeedBackLineList(string FeedbackId)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPFeedback/GetFeedBackLineList?FeedbackId=" + FeedbackId;
            HttpClient client = new HttpClient();
            List<FeedbBackLines> feedbBackLines = new List<FeedbBackLines>();
            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                feedbBackLines = Newtonsoft.Json.JsonConvert.DeserializeObject<List<FeedbBackLines>>(data);
            }
            return Json(feedbBackLines, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public async Task<JsonResult> GetFeedBackQuestionList()
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPFeedback/GetFeedBackQuestionList";
            HttpClient client = new HttpClient();
            List<FeedbBackQuestion> feedbBackLines = new List<FeedbBackQuestion>();
            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                feedbBackLines = Newtonsoft.Json.JsonConvert.DeserializeObject<List<FeedbBackQuestion>>(data);
            }
            return Json(feedbBackLines, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<JsonResult> AddFeedBackQuestion(FeedbBackQuestion model)
        {
            try
            {
                if (model == null || string.IsNullOrWhiteSpace(model.Feedback_Question))
                    return Json(new { success = false, message = "Feedback question is required." }, JsonRequestBehavior.AllowGet);

                if (string.IsNullOrWhiteSpace(model.Order_No))
                    return Json(new { success = false, message = "Order No is required." }, JsonRequestBehavior.AllowGet);

                string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPFeedback/AddFeedBackQuestion";

                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri(apiUrl);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    string json = JsonConvert.SerializeObject(model);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var response = await client.PostAsync(apiUrl, content);

                    if (!response.IsSuccessStatusCode)
                    {
                        var err = await response.Content.ReadAsStringAsync();
                        return Json(new { success = false, message = string.IsNullOrWhiteSpace(err) ? "Save failed." : err }, JsonRequestBehavior.AllowGet);
                    }
                }
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [AcceptVerbs(HttpVerbs.Post | HttpVerbs.Patch)]
        public async Task<JsonResult> UpdateFeedBackQuestion(FeedbBackQuestion model)
        {
            try
            {
                if (model == null || string.IsNullOrWhiteSpace(model.No))
                    return Json(new { success = false, message = "No is required." }, JsonRequestBehavior.AllowGet);
                if (string.IsNullOrWhiteSpace(model.Feedback_Question))
                    return Json(new { success = false, message = "Feedback question is required." }, JsonRequestBehavior.AllowGet);
                if (string.IsNullOrWhiteSpace(model.Order_No))
                    return Json(new { success = false, message = "Order No is required." }, JsonRequestBehavior.AllowGet);

                string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPFeedback/UpdateFeedBackQuestion";

                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri(apiUrl);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    string json = JsonConvert.SerializeObject(model);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var request = new HttpRequestMessage(new HttpMethod("PATCH"), apiUrl)
                    {
                        Content = content
                    };
                    var response = await client.SendAsync(request);

                    if (!response.IsSuccessStatusCode)
                    {
                        var err = await response.Content.ReadAsStringAsync();
                        return Json(new { success = false, message = string.IsNullOrWhiteSpace(err) ? "Update failed." : err }, JsonRequestBehavior.AllowGet);
                    }
                }
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public async Task<JsonResult> GetCustomerDropdown(string prefix)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPFeedback/GetCustomerDropdown?prefix=" + prefix;

            HttpClient client = new HttpClient();
            List<CustomerDropdown> customerReports = new List<CustomerDropdown>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                customerReports = Newtonsoft.Json.JsonConvert.DeserializeObject<List<CustomerDropdown>>(data);
            }

            return Json(customerReports, JsonRequestBehavior.AllowGet);
        }

    }
}
