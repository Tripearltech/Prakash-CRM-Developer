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
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

using System.Web.Http.Cors;

namespace PrakashCRM.Service.Controllers
{
    [RoutePrefix("api/SPItems")]
    public class SPItemsController : ApiController
    {        

        [Route("GetAllItems")]
        public List<SPItemList> GetAllItems()
        {
            API ac = new API();
            List<SPItemList> items = new List<SPItemList>();

            var result = ac.GetData<SPItemList>("ItemDotNetAPI", ""); // and Contact_Business_Relation eq 'Customer'

            if (result.Result.Item1.value.Count > 0)
                items = result.Result.Item1.value;

            return items;
        }

        [Route("GetItemsFromItemPackingStyle")]
        public List<SPItemPackingStyleDetails> GetItemsFromItemPackingStyle()
        {
            API ac = new API();

            List<SPItemPackingStyleDetails> items = new List<SPItemPackingStyleDetails>();

            var result = ac.GetData<SPItemPackingStyleDetails>("ItemPackingStyleDotNetAPI", ""); // and Contact_Business_Relation eq 'Customer'

            if (result.Result.Item1.value.Count > 0)
                items = result.Result.Item1.value;

            return items;
        }


        [HttpPost]
        [Route("UpdateItemPackingStylePrice")]
        public async Task<IHttpActionResult> UpdateItemPackingStylePrice([FromBody] SPitemUpdateModel model)
        {
            if (model == null)
                return BadRequest("Invalid payload.");

            if (string.IsNullOrWhiteSpace(model.Item_No))
                return BadRequest("Item_No is required.");
            if (string.IsNullOrWhiteSpace(model.Packing_Style_Code))
                return BadRequest("Packing_Style_Code is required.");

            var mrpPriceToUpdate = model.PCPL_MRP_Price;
            if (!mrpPriceToUpdate.HasValue && model.PCPL_Purchase_Cost.HasValue)
                mrpPriceToUpdate = model.PCPL_Purchase_Cost;

            // update the zero value ar empty filed.
            var purchaseDaysToUpdate = model.PCPL_Purchase_Days;
            var discountToUpdate = model.PCPL_Discount ?? 0;
            var mrpPriceValueToUpdate = mrpPriceToUpdate;

            var requestMU = new SPitemUpdateModel
            {
                PCPL_Purchase_Days = purchaseDaysToUpdate,
                PCPL_Discount = discountToUpdate,
                PCPL_MRP_Price = mrpPriceValueToUpdate,
                PCPL_IsDiscUpdate = model.PCPL_IsDiscUpdate,
                //PCPL_Purchase_Cost = null,
                //PCPL_Previous_Price = model.PCPL_Previous_Price
            };

            var responseMU = new SPItemPackingStyleDetails();
            var result = await PatchPackingStyleFields("ItemPackingStyleDotNetAPI", requestMU, responseMU, $"Item_No='{model.Item_No}',Packing_Style_Code='{model.Packing_Style_Code}'");

            if (!result.Item2.isSuccess)
                return Content(HttpStatusCode.BadRequest, result.Item2);

            if (!string.IsNullOrWhiteSpace(model.SalesPerson_Code) && mrpPriceValueToUpdate.HasValue)
            {
                NotificationService notificationService = new NotificationService();
                notificationService.RecordItemPriceUpdated(
                    model.SalesPerson_Code,
                    model.Item_No,
                    result.Item1 == null ? string.Empty : result.Item1.Item_Description,
                    model.Packing_Style_Code,
                    result.Item1 == null ? string.Empty : result.Item1.Packing_Style_Description,
                    mrpPriceValueToUpdate);
            }

            return Ok(result.Item1);
        }

        // PATCH only required fields for ItemPackingStyleDotNetAPI
        public async Task<(SPItemPackingStyleDetails, errorDetails)> PatchPackingStyleFields(string apiendpoint, SPitemUpdateModel requestModel, SPItemPackingStyleDetails responseModel, string fieldWithValue)
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

            // Only send the fields to update
            var patchObj = new JObject();
            if (requestModel.PCPL_Purchase_Days.HasValue)
                patchObj["PCPL_Purchase_Days"] = requestModel.PCPL_Purchase_Days.Value;
            if (requestModel.PCPL_Discount.HasValue)
                patchObj["PCPL_Discount"] = requestModel.PCPL_Discount.Value;
            //if (requestModel.PCPL_Purchase_Cost.HasValue)
            //    patchObj["PCPL_Purchase_Cost"] = requestModel.PCPL_Purchase_Cost.Value;
            if (requestModel.PCPL_MRP_Price.HasValue)
                patchObj["PCPL_MRP_Price"] = requestModel.PCPL_MRP_Price.Value;
            //if (requestModel.PCPL_Previous_Price.HasValue)
            //    patchObj["PCPL_Previous_Price"] = requestModel.PCPL_Previous_Price.Value;
            if (requestModel.PCPL_IsDiscUpdate.HasValue)
                patchObj["PCPL_IsDiscUpdate"] = requestModel.PCPL_IsDiscUpdate.Value;
            //if (requestModel.PCPL_Purchase_Cost.HasValue)
            //    patchObj["PCPL_Purchase_Cost"] = requestModel.PCPL_Purchase_Cost.Value;
            //if (requestModel.PCPL_Previous_Price.HasValue)
            //    patchObj["PCPL_Previous_Price"] = requestModel.PCPL_Previous_Price.Value;

            request.Content = new StringContent(patchObj.ToString(), Encoding.UTF8, "application/json");

            HttpResponseMessage response = null;
            try
            {
                response = _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).Result;
            }
            catch (Exception ex)
            {
            }

            errorDetails errordetail = new errorDetails();
            errordetail.isSuccess = response != null && response.IsSuccessStatusCode;
            if (response != null && response.IsSuccessStatusCode)
            {
                var JsonData = response.Content.ReadAsStringAsync().Result;
                try
                {
                    JObject res = JObject.Parse(JsonData);
                    responseModel = res.ToObject<SPItemPackingStyleDetails>();
                    errordetail.code = response.StatusCode.ToString();
                    errordetail.message = response.ReasonPhrase;
                }
                catch (Exception ex1)
                {
                }
            }
            else if (response != null)
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
    }
}
