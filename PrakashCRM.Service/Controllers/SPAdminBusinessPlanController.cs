using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using PrakashCRM.Data.Models;
using PrakashCRM.Service.Classes;

namespace PrakashCRM.Service.Controllers
{
    [System.Web.Http.RoutePrefix("api/SPAdminBusinessPlan")]
    public class SPAdminBusinessPlanController : ApiController
    {
        [System.Web.Http.Route("GetAdminBusinessPlan")]
        public List<AdminBusinessPlanData> GetAdminBusinessPlan(string previousFinancialYear, string currentFinancialYear)
        {
            API ac = new API();
            List<AdminBusinessPlanData> items = new List<AdminBusinessPlanData>();

           
            var result = ac.GetData<AdminBusinessPlanData>("AdminBusinessPlan", $"Plan_Year eq '{currentFinancialYear}'");
            if (result.Result.Item1.value.Count > 0)
                items = result.Result.Item1.value;

            return items;
        }

        [Route("PostAdminBusinessPlan")]
        public AdminBusinessPlanOData PostAdminBusinessPlan(List<AdminBusinessPlan> adminBusinessPlans)
        {
            //if (adminBusinessPlans == null)
            //    return "";

            string resMsg = "";
            AdminBusinessPlanOData adminBusinessPlanOData = new AdminBusinessPlanOData();
            var ac = new API();
            errorDetails ed = new errorDetails();

            bool flag = false;
            var result =  PostItemAdminBusinessPlan<AdminBusinessPlanOData>("", adminBusinessPlans, adminBusinessPlanOData);
            adminBusinessPlanOData = result.Result.Item1;
            ed = result.Result.Item2;
            flag = adminBusinessPlanOData.value;
            adminBusinessPlanOData.errorDetails = ed;

            if (!adminBusinessPlanOData.errorDetails.isSuccess)
                resMsg = adminBusinessPlanOData.errorDetails.message;

            return adminBusinessPlanOData;
        }

        public async Task<(AdminBusinessPlanOData, errorDetails)> PostItemAdminBusinessPlan<AdminBusinessPlanOData>(string apiendpoint, List<AdminBusinessPlan> requestModel, AdminBusinessPlanOData responseModel)
        {
            string _baseURL = System.Configuration.ConfigurationManager.AppSettings["BaseURL"];
            string _tenantId = System.Configuration.ConfigurationManager.AppSettings["TenantID"];
            string _environment = System.Configuration.ConfigurationManager.AppSettings["Environment"];
            string _companyName = System.Configuration.ConfigurationManager.AppSettings["CompanyName"];

            API ac = new API();
            var accessToken = await ac.GetAccessToken();

            HttpClient _httpClient = new HttpClient();
            string encodeurl = Uri.EscapeUriString("https://api.businesscentral.dynamics.com/v2.0/e55ad508-ef1a-489f-afe3-ae21f856e440/Sandbox/ODataV4/CodeunitAPIMgmt_ModifyAdminBusinessPlan?company=\'Prakash Company\'");
            Uri baseuri = new Uri(encodeurl);
            _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken.Token);

            AdminBusinessPlanPost adminBusinessPlanPost = new AdminBusinessPlanPost();
            //invQtyReservePost.text = requestModel;
            string ObjString_ = JsonConvert.SerializeObject(requestModel);
            string txtString = ObjString_.Replace("\"", "'");
            adminBusinessPlanPost.text = txtString;
            string txtString_ = JsonConvert.SerializeObject(adminBusinessPlanPost);
            //ObjString_ = ObjString_.Replace("\"text\"", '"' + "text" + '"');
            var content = new StringContent(txtString_, Encoding.UTF8, "application/json");

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
                    AdminBusinessPlanOData adminBusinessPlanOData = res.ToObject<AdminBusinessPlanOData>();
                    responseModel = adminBusinessPlanOData;

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
        [Route("GetItemDropDwon")]
        public List<SPItemList> GetItemDropDwon(string Search)
        {
            API ac = new API();
            List<SPItemList> itemdropdwon = new List<SPItemList>();

            if (string.IsNullOrWhiteSpace(Search))
            {
                Search = "";
            }
            string filter = $"contains(Description,'{Search}') and Blocked eq false";

            var result = ac.GetData<SPItemList>("ItemDotNetAPI", filter);

            if (result != null && result.Result.Item1.value.Count > 0)
            {
                itemdropdwon = result.Result.Item1.value;
            }

            return itemdropdwon;
        }


    }
}