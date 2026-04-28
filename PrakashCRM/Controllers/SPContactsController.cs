using Newtonsoft.Json;
using PrakashCRM.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using ClosedXML.Excel;
using System.IO;
using System.Reflection;
using System.Data;
using System.Net.Http.Headers;

namespace PrakashCRM.Controllers
{
    [RedirectingAction]
    public class SPContactsController : Controller
    {

        // GET: SPContacts
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ContactCompanyList()
        {
            return View();
        }

        public async Task<JsonResult> GetContactCompanyListData(
         int orderBy,
         string orderDir,
         string filter,
         int skip,
         int top)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPContacts/";
            string orderByField;

            switch (orderBy)
            {
                case 4: orderByField = "No " + orderDir; break;
                case 5: orderByField = "Name " + orderDir; break;
                case 6: orderByField = "Industry " + orderDir; break;
                case 7: orderByField = "Source_of_Contact " + orderDir; break;
                case 8: orderByField = "Business_Type " + orderDir; break;
                case 9: orderByField = "City " + orderDir; break;
                case 10: orderByField = "Area " + orderDir; break;
                case 11: orderByField = "Post_Code " + orderDir; break;
                case 12: orderByField = "Phone_No " + orderDir; break;
                case 13: orderByField = "E_Mail " + orderDir; break;
                default: orderByField = "No asc"; break;
            }

            // 👉 IMPORTANT FIXES 👇

            string type = "Company";
            string spCode = Session["loggedInUserSPCode"].ToString();

            if (string.IsNullOrWhiteSpace(filter))
                filter = $"Type eq '{type}' and Salesperson_Code eq '{spCode}'";
            else
                filter = $"({filter}) and Type eq '{type}' and Salesperson_Code eq '{spCode}'";

            // ✔ encode filter before sending
            string encodedFilter = Uri.EscapeDataString(filter);

            apiUrl = apiUrl +
                "GetAllCompanies?" +
                "SPCode=" + spCode +
                "&skip=" + skip +
                "&top=" + top +
                "&orderby=" + Uri.EscapeDataString(orderByField) +
                "&filter=" + encodedFilter;

            HttpClient client = new HttpClient();
            List<SPCompanyList> Companies = new List<SPCompanyList>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json")
            );

            HttpResponseMessage response = await client.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                Companies = Newtonsoft.Json.JsonConvert
                             .DeserializeObject<List<SPCompanyList>>(data);
            }

            Session["CompanyNo"] = "";
            Session["isCompanyContactEdit"] = false;
            Session["CompanyContactAction"] = "";

            if (ViewBag.Contacts != null)
                ViewBag.Contacts = null;

            return Json(Companies, JsonRequestBehavior.AllowGet);
        }


        public ActionResult ContactPersonList()
        {
            return View();
        }

        private static string BuildReturnUrlWithCustomerId(string returnUrl, string customerId)
        {
            string normalizedReturnUrl = LoginRedirectHelper.NormalizeReturnUrl(returnUrl);
            if (string.IsNullOrWhiteSpace(normalizedReturnUrl))
                return string.Empty;

            string[] urlParts = normalizedReturnUrl.Split(new[] { '?' }, 2);
            var query = HttpUtility.ParseQueryString(urlParts.Length > 1 ? urlParts[1] : string.Empty);

            if (!string.IsNullOrWhiteSpace(customerId))
                query["customerId"] = customerId;

            string queryString = query.ToString();
            return string.IsNullOrWhiteSpace(queryString)
                ? urlParts[0]
                : urlParts[0] + "?" + queryString;
        }

        public async Task<JsonResult> GetContactPersonListData(int orderBy, string orderDir, string filter, int skip, int top)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPContacts/";

            string orderByField = "";

            switch (orderBy)
            {
                case 2:
                    orderByField = "Name " + orderDir;
                    break;
                case 3:
                    orderByField = "Company_Name " + orderDir;
                    break;
                case 4:
                    orderByField = "E_Mail " + orderDir;
                    break;
                case 5:
                    orderByField = "Mobile_Phone_No " + orderDir;
                    break;
                default:
                    orderByField = "Name asc";
                    break;
            }

            apiUrl = apiUrl + "GetAllContacts?SPCode=" + Session["loggedInUserSPCode"].ToString() + "&skip=" + skip + "&top=" + top + "&orderby=" + orderByField + "&filter=" + filter;

            HttpClient client = new HttpClient();
            List<SPContactList> contacts = new List<SPContactList>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                contacts = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPContactList>>(data);
            }

            Session["CompanyNo"] = "";
            Session["isCompanyContactEdit"] = false;
            Session["CompanyContactAction"] = "";

            if (ViewBag.Contacts != null)
                ViewBag.Contacts = null;

            return Json(contacts, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CompanyContactCard(string No = "", string returnUrl = "")
        {
            string normalizedReturnUrl = LoginRedirectHelper.NormalizeReturnUrl(returnUrl);

            if (No == "inquiry")
            {
                ViewData["No"] = "inquiry";
                No = "";
            }
            SPCompanyContact companycontact = new SPCompanyContact();
            List<SPContactResponse> contacts = new List<SPContactResponse>();
            List<SPContactProducts> contactProducts = new List<SPContactProducts>();
            ViewBag.Contactscount = 0;

            if (No != "" || (Session["CompanyNo"] != null && Session["CompanyNo"].ToString() != ""))
            {

                if (Session["CompanyNo"].ToString() == "")
                    Session["CompanyNo"] = No;

                Task<SPCompanyContact> task = Task.Run<SPCompanyContact>(async () => await GetCompanyContactForEdit(Session["CompanyNo"].ToString()));
                companycontact = task.Result;


                Task<List<SPContactResponse>> task1 = Task.Run<List<SPContactResponse>>(async () => await GetAllContactsOfCompany(Session["CompanyNo"].ToString()));
                contacts = task1.Result;
                ViewBag.Contactscount = contacts.Count;
                ViewBag.Contacts = contacts;

                Task<List<SPContactProducts>> task2 = Task.Run<List<SPContactProducts>>(async () => await GetAllProductsOfCompany(Session["CompanyNo"].ToString()));
                contactProducts = task2.Result;
                ViewBag.ContactProductsCnt = contactProducts.Count;
                ViewBag.ContactProducts = contactProducts;

                Session["isCompanyContactEdit"] = true;

            }

            ViewBag.Salesperson_Code = Session["loggedInUserSPCode"].ToString();
            ViewBag.ReturnUrl = normalizedReturnUrl;
            if (companycontact.Company_Name != null)
            {
                companycontact.PCPL_Allow_Login = false;
                companycontact.PCPL_Enable_OTP_On_Login = true;
                return View(companycontact);
            }
            else
            {
                companycontact.PCPL_Enable_OTP_On_Login = true;
                Session["CompanyNo"] = "";
                Session["isCompanyContactEdit"] = false;
            }

            return View(companycontact);

        }

        [HttpPost]
        public async Task<ActionResult> CompanyContactCard(SPCompanyContact companycontact, string returnUrl = "")
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPContacts/";
            string normalizedReturnUrl = LoginRedirectHelper.NormalizeReturnUrl(returnUrl);

            string CompanyNo = "";
            if (Convert.ToBoolean(Session["isCompanyContactEdit"]) == true)
            {
                CompanyNo = Session["CompanyNo"].ToString();
                apiUrl = apiUrl + "CompanyContactCard?isEdit=true&CompanyNo=" + CompanyNo;
            }
            else
                apiUrl = apiUrl + "CompanyContactCard?isEdit=false&CompanyNo=" + CompanyNo;

            companycontact.Contact_Name = Request.Form["txtContactName"];
            companycontact.Mobile_Phone_No = Request.Form["txtMobilePhoneNo"];
            companycontact.Contact_EMail = Request.Form["txtContactEmail"];
            companycontact.PCPL_Department_Code = Request.Form["ddlDepartment"];
            companycontact.PCPL_Job_Responsibility = Request.Form["txtJobResponsibility"];
            //companycontact.PCPL_Allow_Login = Convert.ToBoolean(Request.Form["chkAllowLogin"]);
            //companycontact.PCPL_Enable_OTP_On_Login = Convert.ToBoolean(Request.Form["chkEnableOTPOnLogin"]);
            //companycontact.Is_Primary = Convert.ToBoolean(Request.Form["chkIsPrimary"]);

            SPCompanyResponse responseCompany = new SPCompanyResponse();

            HttpClient client = new HttpClient();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            string UserObjString = JsonConvert.SerializeObject(companycontact);
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
                responseCompany = Newtonsoft.Json.JsonConvert.DeserializeObject<SPCompanyResponse>(data);

                if (responseCompany.errorDetails.isSuccess)
                {
                    if (Convert.ToBoolean(Session["isCompanyContactEdit"]) == true)
                        Session["CompanyContactAction"] = "Updated";
                    else
                        Session["CompanyContactAction"] = "Added";
                }
                else
                    Session["CompanyContactActionErr"] = responseCompany.errorDetails.message;

            }

            //if (Convert.ToBoolean(Session["isCompanyContactEdit"]) == true)
            //    Session["CompanyContactAction"] = "Updated";
            //else
            //    Session["CompanyContactAction"] = "Added";
            if (companycontact.SQValue == "inquiry")
            {
                return RedirectToAction("Inquiry", "SPInquiry");
            }

            if (responseCompany != null
                && responseCompany.errorDetails != null
                && responseCompany.errorDetails.isSuccess
                && !string.IsNullOrWhiteSpace(normalizedReturnUrl)
                && !string.IsNullOrWhiteSpace(responseCompany.No))
            {
                return Redirect(BuildReturnUrlWithCustomerId(normalizedReturnUrl, responseCompany.No));
            }

            return RedirectToAction("CompanyContactCard", "SPContacts", new { No = responseCompany.No, returnUrl = normalizedReturnUrl });
        }

        public bool NullContactSession()
        {
            bool isSessionNull = false;

            Session["CompanyContactAction"] = "";
            Session["CompanyContactActionErr"] = "";
            isSessionNull = true;

            return isSessionNull;
        }

        public async Task<SPCompanyContact> GetCompanyContactForEdit(string No)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPContacts/";

            apiUrl = apiUrl + "GetCompanyContactFromNo?No=" + No;

            HttpClient client = new HttpClient();
            SPCompanyContact companycontact = new SPCompanyContact();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                companycontact = Newtonsoft.Json.JsonConvert.DeserializeObject<SPCompanyContact>(data);

                ViewBag.Post_Code = companycontact.Post_Code;
                ViewBag.Area_Code = companycontact.Area_Code;
                ViewBag.District = companycontact.District;
                ViewBag.State_Code = companycontact.State_Code;
                ViewBag.Country_Region_Code = companycontact.Country_Region_Code;
                ViewBag.Industry_No = companycontact.Industry_No;
                ViewBag.Business_Type_No = companycontact.Business_Type_No;
                ViewBag.Salesperson_Code = companycontact.Salesperson_Code;
                ViewBag.PCPL_Secondary_SP_Code = companycontact.PCPL_Secondary_SP_Code;
                ViewBag.Source_Of_Contact_No = companycontact.Source_Of_Contact_No;
                ViewBag.CustomerNo = companycontact.PCPL_Company_Customer_No;
            }

            return companycontact;
        }

        public async Task<List<SPContactResponse>> GetAllContactsOfCompany(string No)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPContacts/";

            apiUrl = apiUrl + "GetAllContactsOfCompany?SPCode=" + Session["loggedInUserSPCode"].ToString() + "&No=" + No;

            HttpClient client = new HttpClient();
            List<SPContactResponse> contacts = new List<SPContactResponse>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                contacts = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPContactResponse>>(data);
            }

            return contacts;
        }

        public async Task<List<SPContactProducts>> GetAllProductsOfCompany(string No)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPContacts/";

            apiUrl = apiUrl + "GetAllProductsOfCompany?SPCode=" + Session["loggedInUserSPCode"].ToString() + "&No=" + No;

            HttpClient client = new HttpClient();
            List<SPContactProducts> contactsProducts = new List<SPContactProducts>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                contactsProducts = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPContactProducts>>(data);
            }

            return contactsProducts;
        }

        //

        public async Task<JsonResult> ExportCompanyListData(int orderBy, string orderDir, string filter, int skip, int top)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPContacts/";

            string orderByField = "";

            switch (orderBy)
            {
                case 3:
                    orderByField = "No " + orderDir;
                    break;
                case 4:
                    orderByField = "Name " + orderDir;
                    break;
                case 5:
                    orderByField = "City " + orderDir;
                    break;
                case 6:
                    orderByField = "Post_Code " + orderDir;
                    break;
                case 7:
                    orderByField = "Phone_No " + orderDir;
                    break;
                case 8:
                    orderByField = "E_Mail " + orderDir;
                    break;
                case 9:
                    orderByField = "Salesperson_Code " + orderDir;
                    break;
                default:
                    orderByField = "No asc";
                    break;
            }

            apiUrl = apiUrl + "GetAllCompanies?SPCode=" + Session["loggedInUserSPCode"].ToString() + "&skip=" + skip + "&top=" + top + "&orderby=" + orderByField + "&filter=" + filter + "&isExport=true";

            HttpClient client = new HttpClient();
            List<SPCompanyList> Companies = new List<SPCompanyList>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                Companies = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPCompanyList>>(data);
            }
            DataTable dt = ToDataTable(Companies);

            //Name of File  
            string fileName = "ContactCompanyList.xlsx";
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

        public async Task<JsonResult> ExportContactListData(int orderBy, string orderDir, string filter, int skip, int top)
        {

            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPContacts/";

            string orderByField = "";

            switch (orderBy)
            {
                case 2:
                    orderByField = "Name " + orderDir;
                    break;
                case 3:
                    orderByField = "Company_Name " + orderDir;
                    break;
                case 4:
                    orderByField = "E_Mail " + orderDir;
                    break;
                case 5:
                    orderByField = "Mobile_Phone_No " + orderDir;
                    break;
                default:
                    orderByField = "Name asc";
                    break;
            }

            apiUrl = apiUrl + "GetAllContacts?SPCode=" + Session["loggedInUserSPCode"].ToString() + "&skip=" + skip + "&top=" + top + "&orderby=" + orderByField + "&filter=" + filter + "&isExport=true";

            HttpClient client = new HttpClient();
            List<SPContactList> contacts = new List<SPContactList>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                contacts = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPContactList>>(data);
            }
            DataTable dt = ToDataTable(contacts);

            string fileName = "ContactPersonList.xlsx";
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

        //

        [HttpPost]
        public async Task<JsonResult> GetPincodeForDDL(string prefix)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPContacts/GetPincodeForDDL?prefix=" + prefix;

            HttpClient client = new HttpClient();
            List<PostCodes> pincode = new List<PostCodes>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                pincode = Newtonsoft.Json.JsonConvert.DeserializeObject<List<PostCodes>>(data);
            }

            return Json(pincode, JsonRequestBehavior.AllowGet);
        }

        public async Task<int> GetQtyForCountStockFromILE(string LocationCode, string ItemNo)
        {

            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPContacts/";

            apiUrl = apiUrl + "GetQtyForCountStockFromILE?LocationCode=" + LocationCode + "&ItemNo=" + ItemNo;

            HttpClient client = new HttpClient();
            List<SPILEQtyForCountStock> ILEList = new List<SPILEQtyForCountStock>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                ILEList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPILEQtyForCountStock>>(data);
            }

            int qtySum = 0;
            for (int i = 0; i < ILEList.Count; i++)
            {
                qtySum += ILEList[i].Quantity;
            }

            return qtySum;
        }

        public async Task<JsonResult> GetAllContactsOfCompanyForPopup(string No)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPContacts/";

            apiUrl = apiUrl + "GetAllContactsOfCompany?SPCode=" + Session["loggedInUserSPCode"].ToString() + "&No=" + No;

            HttpClient client = new HttpClient();
            List<SPContactResponse> contacts = new List<SPContactResponse>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                contacts = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPContactResponse>>(data);
            }

            return Json(contacts, JsonRequestBehavior.AllowGet);
        }
        //
        public async Task<JsonResult> GetAllCountryForDDL()
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPContacts/GetAllCountryForDDL";

            HttpClient client = new HttpClient();
            List<Country> country = new List<Country>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                country = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Country>>(data);
                country = country.OrderBy(a => a.Name).ToList();
            }

            return Json(country, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetAllStateForDDL()
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPContacts/GetAllStateForDDL";

            HttpClient client = new HttpClient();
            List<State> state = new List<State>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                state = Newtonsoft.Json.JsonConvert.DeserializeObject<List<State>>(data);
                state = state.OrderBy(a => a.Description).ToList();
            }

            return Json(state, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetAllPostCodesForDDL()
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPContacts/GetAllPostCodesForDDL";

            HttpClient client = new HttpClient();
            List<PostCodes> postcodes = new List<PostCodes>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                postcodes = Newtonsoft.Json.JsonConvert.DeserializeObject<List<PostCodes>>(data);
                postcodes = postcodes.OrderBy(a => a.City).ToList();
            }

            return Json(postcodes, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetAllAreasForDDL()
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPContacts/GetAllAreasForDDL";

            HttpClient client = new HttpClient();
            List<Area> areas = new List<Area>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                areas = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Area>>(data);
            }

            return Json(areas, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetAllDistrictForDDL()
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPContacts/GetAllDistrictForDDL";

            HttpClient client = new HttpClient();
            List<District> district = new List<District>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                district = Newtonsoft.Json.JsonConvert.DeserializeObject<List<District>>(data);
                district = district.OrderBy(a => a.District_Name).ToList();
            }

            return Json(district, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetAllIndustryForDDL()
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPContacts/GetAllIndustryForDDL";

            HttpClient client = new HttpClient();
            List<Industry> industry = new List<Industry>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                industry = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Industry>>(data);
                industry = industry.OrderBy(a => a.Name).ToList();
            }

            return Json(industry, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetAllBusinessTypeForDDL()
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPContacts/GetAllBusinessTypeForDDL";

            HttpClient client = new HttpClient();
            List<BusinessType> businesstype = new List<BusinessType>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                businesstype = Newtonsoft.Json.JsonConvert.DeserializeObject<List<BusinessType>>(data);
                businesstype = businesstype.OrderBy(a => a.Type).ToList();
            }

            return Json(businesstype, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetAllSalesPersonForDDL()
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPContacts/GetAllSalesPersonForDDL";

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
                salesperson = salesperson.OrderBy(a => a.Name).ToList();
            }

            return Json(salesperson, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetAllSourceofContactsForDDL()
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPContacts/GetAllSourceofContactsForDDL";

            HttpClient client = new HttpClient();
            List<SourceofContacts> sourceofcontacts = new List<SourceofContacts>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                sourceofcontacts = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SourceofContacts>>(data);
                sourceofcontacts = sourceofcontacts.OrderBy(a => a.Title).ToList();
            }

            return Json(sourceofcontacts, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetAllDepartmentForDDL()
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPContacts/GetAllDepartmentForDDL";

            HttpClient client = new HttpClient();
            List<Departments> departments = new List<Departments>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                departments = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Departments>>(data);
                departments = departments.OrderBy(a => a.Department).ToList();
            }

            return Json(departments, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetAllPincodeForDDL()
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPContacts/GetAllPincodeForDDL";

            HttpClient client = new HttpClient();
            List<Pincode> pincode = new List<Pincode>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                pincode = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Pincode>>(data);
            }

            return Json(pincode, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<JsonResult> GetDetailsByCode(string Code)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPContacts/GetDetailsByCode?Code=" + Code;

            HttpClient client = new HttpClient();
            List<PostCodes> postcodes = new List<PostCodes>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                postcodes = Newtonsoft.Json.JsonConvert.DeserializeObject<List<PostCodes>>(data);
            }

            return Json(postcodes, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetAreasByPincodeForDDL(string Pincode)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPContacts/GetAreasByPincodeForDDL?Pincode=" + Pincode;

            HttpClient client = new HttpClient();
            List<Area> areas = new List<Area>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                areas = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Area>>(data);
                areas = areas.OrderBy(a => a.Text).ToList();
            }

            return Json(areas, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetContactBusinessPlan(string SPCode, string CCompanyNo, string PlanYear)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPContacts/";

            apiUrl = apiUrl + "GetContactBusinessPlan?SPCode=" + SPCode + "&CCompanyNo=" + CCompanyNo + "&PlanYear=" + PlanYear;

            HttpClient client = new HttpClient();
            List<SPCustBusinessPlan> contactBusinessPlan = new List<SPCustBusinessPlan>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                contactBusinessPlan = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPCustBusinessPlan>>(data);
            }

            return Json(contactBusinessPlan, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetContactDailyVisits(string SPCode, string FromDate, string ToDate, string CCompanyNo)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPContacts/";
            apiUrl = apiUrl + "GetContactDailyVisits?SPCode=" + SPCode + "&FromDate=" + FromDate + "&ToDate=" + ToDate + "&CCompanyNo=" + CCompanyNo;

            HttpClient client = new HttpClient();
            List<SPDailyVisit> dailyVisitData = new List<SPDailyVisit>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                dailyVisitData = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPDailyVisit>>(data);

                for (int i = 0; i < dailyVisitData.Count; i++)
                {
                    string[] date_ = dailyVisitData[i].Date.ToString().Split('-');
                    dailyVisitData[i].Date = date_[2] + '-' + date_[1] + '-' + date_[0];
                }
            }

            return Json(dailyVisitData, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<JsonResult> GetAllProducts()
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPContacts/GetAllProducts";

            HttpClient client = new HttpClient();
            List<SPItemList> items = new List<SPItemList>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                items = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPItemList>>(data);
            }

            return Json(items, JsonRequestBehavior.AllowGet);
        }

    }
}