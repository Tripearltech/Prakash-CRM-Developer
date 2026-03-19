using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PrakashCRM.Data.Models;
using PrakashCRM.Service.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;

namespace PrakashCRM.Service.Controllers
{
    [RoutePrefix("api/SPOutstandingPayment")]
    public class SPOutstandingPaymentController : ApiController
    {
        [Route("GetOutstandingPaymentDetails")]
        public List<SPOutstandingPaymentList> GetOutstandingPaymentDetails(string SPCode, int skip, int top, string orderby, string filter)
        {
            API ac = new API();
            List<SPOutstandingPaymentList> osPaymentDetails = new List<SPOutstandingPaymentList>();

            if (filter == "" || filter == null)
                filter = "Document_Type eq 'Invoice' and Remaining_Amt_LCY gt 0 and Salesperson_Code eq '" + SPCode + "'";
            else
                filter = filter + " and Document_Type eq 'Invoice' and Remaining_Amt_LCY gt 0 and Salesperson_Code eq '" + SPCode + "'";

            var result = ac.GetData1<SPOutstandingPaymentList>("CustomerLedgerEntriesDotNetAPI", filter, skip, top, orderby);

            if (result.Result.Item1.value.Count > 0)
                osPaymentDetails = result.Result.Item1.value;

            for(int i = 0; i < osPaymentDetails.Count; i++)
            {
                string[] strDate = osPaymentDetails[i].Due_Date.Split('-');
                osPaymentDetails[i].Due_Date = strDate[2] + '-' + strDate[1] + '-' + strDate[0];

                string[] strDate1 = osPaymentDetails[i].Posting_Date.Split('-');
                osPaymentDetails[i].Posting_Date = strDate1[2] + '-' + strDate1[1] + '-' + strDate1[0];
            }

            return osPaymentDetails;
        }

        [Route("GetApiRecordsCount")]
        public int GetApiRecordsCount(string SPCode, string apiEndPointName, string filter)
        {
            API ac = new API();

            if (filter == "" || filter == null)
                filter = "Document_Type eq 'Invoice' and Remaining_Amt_LCY gt 0 and Salesperson_Code eq '" + SPCode + "'";
            else
                filter = filter + " and Document_Type eq 'Invoice' and Remaining_Amt_LCY gt 0 and Salesperson_Code eq '" + SPCode + "'";

            var count = ac.CalculateCount(apiEndPointName, filter);

            return Convert.ToInt32(count.Result);
        }

        //Collection Report Generate date Calling on Post API.

        [HttpPost]
        [Route("GenerateCollData")]
        public string GenerateCollData(string FromDate)
        {
            bool response = false;
            SPCollGenerateDataPost collGeneratereq = new SPCollGenerateDataPost();
            SPCollGenerateDataOData collGenerateres = new SPCollGenerateDataOData();
            errorDetails ed = new errorDetails();

            collGeneratereq.systemdate = FromDate;
            //invGeneratereq.enddate = ToDate;

            var result = PostItemForGenerateCollData<SPCollGenerateDataOData>("", collGeneratereq, collGenerateres);

            if (result?.Result != null)
            {
                response = result.Result.Item1?.value ?? false;
                ed = result.Result.Item2;
                collGenerateres.errorDetails = ed;
            }
            else
            {
                response = true;
            }
            return response.ToString().ToLower();
        }

        public async Task<(SPCollGenerateDataOData, errorDetails)> PostItemForGenerateCollData<SPCollGenerateDataOData>(string apiendpoint, SPCollGenerateDataPost requestModel, SPCollGenerateDataOData responseModel)
        {
            string _baseURL = System.Configuration.ConfigurationManager.AppSettings["BaseURL"];
            string _tenantId = System.Configuration.ConfigurationManager.AppSettings["TenantID"];
            string _environment = System.Configuration.ConfigurationManager.AppSettings["Environment"];
            string _companyName = System.Configuration.ConfigurationManager.AppSettings["CompanyName"];

            API ac = new API();
            var accessToken = await ac.GetAccessToken();

            HttpClient _httpClient = new HttpClient();
            string encodeurl = Uri.EscapeUriString("https://api.businesscentral.dynamics.com/v2.0/e55ad508-ef1a-489f-afe3-ae21f856e440/Sandbox/ODataV4/DailyCustCollectionMgmt_GenerateCustCollectionReportDate?company=\'Prakash Company\'");
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
                    responseModel = res.ToObject<SPCollGenerateDataOData>();
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
        [Route("GetCustomerCollectionOut")]
        public List<CustomerCollectionOut> GetCustomerCollectionOut()
        {
            API ac = new API();
            List<CustomerCollectionOut> data = new List<CustomerCollectionOut>();

            // Customer list
            string customerFilter = "Is_Customer eq true and IsTotalCustAmt eq false and IsReceivedAmount eq false";

            var customerResult = ac.GetData<CustomerCollectionOut>("Daily_Customer_Collection_View_Excel", customerFilter);
            if (customerResult.Result.Item1.value.Count > 0)
                data.AddRange(customerResult.Result.Item1.value);


            // Received Amount list
            string receivedFilter = "Is_Customer eq false and IsTotalCustAmt eq false and IsReceivedAmount eq true";

            var receivedResult = ac.GetData<CustomerCollectionOut>("Daily_Customer_Collection_View_Excel", receivedFilter);
            if (receivedResult.Result.Item1.value.Count > 0)
                data.AddRange(receivedResult.Result.Item1.value);


            // Total Customer Amount
            string totalFilter = "Is_Customer eq true and IsTotalCustAmt eq true and IsReceivedAmount eq false and IsLastSixMonthsData eq true";

            var totalResult = ac.GetData<CustomerCollectionOut>("Daily_Customer_Collection_View_Excel", totalFilter);
            if (totalResult.Result.Item1.value.Count > 0)
                data.AddRange(totalResult.Result.Item1.value);


            return data;
        }

        [HttpGet]
        [Route("GetCustomerSexMonthData")]
        public List<CustomerCollectionOut> GetCustomerSexMonthData(string customerNo)
        {
            API ac = new API();
            List<CustomerCollectionOut> list = new List<CustomerCollectionOut>();

            string filter = "";
            filter += $"IsLastSixMonthsData eq true and IsTotalCustAmt eq false and Is_Customer eq false and IsReceivedAmount eq false";
            filter += $" and LastSixMonths_Customer_No eq '{customerNo}'";

            var result = ac.GetData<CustomerCollectionOut>("Daily_Customer_Collection_View_Excel", filter);

            if (result.Result.Item1.value.Count > 0)
                list = result.Result.Item1.value;

            return list;
        }


    }
}
