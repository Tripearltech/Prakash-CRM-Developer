using ClosedXML.Excel;
using DocumentFormat.OpenXml.Presentation;
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
    [RedirectingAction]
    public class SPGRNController : Controller
    {
        // GET: GRN
        public ActionResult Index()
        {
            return View();
        }

        public async Task<JsonResult> ExportListData(int orderBy, string orderDir, string filter, int skip, int top)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPGRN/";

            string orderByField = "";

            switch (orderBy)
            {
                case 1:
                    orderByField = "DocumentNo " + orderDir;
                    break;
                case 2:
                    orderByField = "Order_Date " + orderDir;
                    break;
                case 3:
                    orderByField = "Name " + orderDir;
                    break;
                case 4:
                    orderByField = "Description_Product_Name " + orderDir;
                    break;
                case 5:
                    orderByField = "Packing_Style " + orderDir;
                    break;
                default:
                    orderByField = "Order_Date asc";
                    break;
            }

            orderByField = orderByField + ",DocumentNo asc";
            apiUrl = apiUrl + "GetGRNTaskAll?orderby=" + orderByField + "&filter=" + filter;


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

            DataTable dt = ToDataTable(warehouseSales);

            //Name of File  
            string fileName = "GRNList.xlsx";
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

        public ActionResult GRNList()
        {
            return View();
        }

        public async Task<JsonResult> GetGRNListData(int orderBy, string orderDir, string filter, string documenttype = "")
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPGRN/";

            string orderByField = "";

            switch (orderBy)
            {
                case 1:
                    orderByField = "DocumentNo " + orderDir;
                    break;
                case 2:
                    orderByField = "Order_Date " + orderDir;
                    break;
                case 3:
                    orderByField = "Name " + orderDir;
                    break;
                case 4:
                    orderByField = "Description_Product_Name " + orderDir;
                    break;
                case 5:
                    orderByField = "Packing_Style " + orderDir;
                    break;
                default:
                    orderByField = "Order_Date asc";
                    break;
            }

            orderByField = orderByField + ",DocumentNo asc";
            apiUrl = apiUrl + "GetGRNTaskAll?orderby=" + orderByField + "&filter=" + filter;

            HttpClient client = new HttpClient();
            List<SPGRNList> grnlist = new List<SPGRNList>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                grnlist = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPGRNList>>(data);
            }

            return Json(grnlist, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GRNCard(string No, string DocumentType)
        {
            SPGRNCard grnCard = new SPGRNCard();

            if (No != "" && DocumentType != "")
            {
                Task<SPGRNCard> task = Task.Run<SPGRNCard>(async () => await GetGRNFromNo(No, DocumentType));
                grnCard = task.Result;
            }
            ViewBag.importDoc = grnCard.EXM_Import_Document;
            if (grnCard != null)
                return View(grnCard);
            else
                return View(new SPGRNCard());
        }

        public async Task<SPGRNCard> GetGRNFromNo(string No, string DocumentType)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPGRN/";

            apiUrl = apiUrl + "GetGRNFromNo?No=" + No + "&DocumentType=" + DocumentType;

            HttpClient client = new HttpClient();
            SPGRNCard grnCard = new SPGRNCard();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                grnCard = Newtonsoft.Json.JsonConvert.DeserializeObject<SPGRNCard>(data);
            }
            else
            {
                var JsonData = response.Content.ReadAsStringAsync().Result;
            }
            return grnCard;
        }

        [HttpPost]
        public async Task<bool> SaveSPGRNCard(SPGRNCardRequest grncard)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPGRN/SaveSPGRNCard";
            bool flag = false;

            try
            {
                HttpClient client = new HttpClient();

                client.BaseAddress = new Uri(apiUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                string UserObjString = JsonConvert.SerializeObject(grncard);
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
                    flag = Newtonsoft.Json.JsonConvert.DeserializeObject<bool>(data);
                }
            }
            catch (Exception)
            {
                flag = false;
            }

            return flag;
        }

        [HttpPost]
        public async Task<bool> SaveSPGRNLine(SPGRNCardLineRequest grnLine)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPGRN/SaveSPGRNLine";
            bool flag = false;

            try
            {
                HttpClient client = new HttpClient();

                client.BaseAddress = new Uri(apiUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                string userObjString = JsonConvert.SerializeObject(grnLine);
                var content = new StringContent(userObjString, Encoding.UTF8, "application/json");

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
                    flag = Newtonsoft.Json.JsonConvert.DeserializeObject<bool>(data);
                }
            }
            catch (Exception)
            {
                flag = false;
            }

            return flag;
        }

        public async Task<JsonResult> GetGRNLineItemTrackingForPopup(string DocumentType, string DocumentNo, string LineNo, string sourceType = "", string sourceSubtype = "")
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPGRN/";

            apiUrl = apiUrl + "GetGRNLineItemTrackingForPopup?DocumentType=" + HttpUtility.UrlEncode(DocumentType)
                + "&DocumentNo=" + HttpUtility.UrlEncode(DocumentNo)
                + "&LineNo=" + HttpUtility.UrlEncode(LineNo)
                + "&sourceType=" + HttpUtility.UrlEncode(sourceType)
                + "&sourceSubtype=" + HttpUtility.UrlEncode(sourceSubtype);

            HttpClient client = new HttpClient();
            List<SPGRNLineItemTracking> spGRNLineItemTracking = new List<SPGRNLineItemTracking>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                spGRNLineItemTracking = JsonConvert.DeserializeObject<List<SPGRNLineItemTracking>>(data);
            }

            return Json(spGRNLineItemTracking, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<bool> SaveGRNLineItemTracking(List<ReservationEntryForGRN> reservationEntryForGRNs)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPGRN/SaveGRNLineItemTracking";
            bool flag = false;



            HttpClient client = new HttpClient();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            string UserObjString = JsonConvert.SerializeObject(reservationEntryForGRNs);
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
                flag = Newtonsoft.Json.JsonConvert.DeserializeObject<bool>(data);
            }

            return flag;
        }

        [HttpPost]
        public async Task<JsonResult> GetMakeMfgCodeAndName(string prefix)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPGRN/GetMakeMfgCodeAndName?prefix=" + prefix;

            HttpClient client = new HttpClient();
            List<SPGRNVendors> makemfgcode = new List<SPGRNVendors>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                makemfgcode = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPGRNVendors>>(data);
            }

            return Json(makemfgcode, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<string> DeleteGRNLineItemTracking(DeleteReservationEntryForGRNRequest request)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPGRN/DeleteGRNLineItemTracking";
            string msg = "";

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            string reqJson = JsonConvert.SerializeObject(request);
            var content = new StringContent(reqJson, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync(apiUrl, content);
            if (response.IsSuccessStatusCode)
            {
                msg = await response.Content.ReadAsStringAsync();
            }

            return msg;
        }

        [HttpPost]
        public async Task<bool> UploadGRNItemTrackingAttachments(UploadGRNItemTrackingAttachmentsRequest request)
        {
            request = request ?? ReadJsonRequestBody<UploadGRNItemTrackingAttachmentsRequest>();

            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPGRN/UploadGRNItemTrackingAttachments";
            bool flag = false;

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            string requestJson = JsonConvert.SerializeObject(request);
            var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync(apiUrl, content);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                flag = JsonConvert.DeserializeObject<bool>(data);
            }

            return flag;
         }

        private T ReadJsonRequestBody<T>() where T : class
        {
            if (Request == null || Request.InputStream == null)
            {
                return null;
            }

            try
            {
                Request.InputStream.Position = 0;
                using (var reader = new StreamReader(Request.InputStream, Encoding.UTF8, true, 1024, true))
                {
                    var requestBody = reader.ReadToEnd();
                    if (string.IsNullOrWhiteSpace(requestBody))
                    {
                        return null;
                    }

                    Request.InputStream.Position = 0;
                    return JsonConvert.DeserializeObject<T>(requestBody);
                }
            }
            catch
            {
                return null;
            }
        }

        [HttpPost]
        public async Task<JsonResult> GetGRNItemTrackingAttachments(GetGRNItemTrackingAttachmentsRequest request)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPGRN/GetGRNItemTrackingAttachments";
            List<GRNDocumentAttachmentPayload> attachments = new List<GRNDocumentAttachmentPayload>();

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            string requestJson = JsonConvert.SerializeObject(request);
            var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync(apiUrl, content);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                attachments = JsonConvert.DeserializeObject<List<GRNDocumentAttachmentPayload>>(data);
            }

            return Json(attachments, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<bool> DeleteGRNItemTrackingAttachment(DeleteGRNItemTrackingAttachmentRequest request)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPGRN/DeleteGRNItemTrackingAttachment";
            bool flag = false;

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            string requestJson = JsonConvert.SerializeObject(request);
            var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync(apiUrl, content);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                flag = JsonConvert.DeserializeObject<bool>(data);
            }

            return flag;
        }
    }
}