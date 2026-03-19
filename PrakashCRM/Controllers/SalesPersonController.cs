using ClosedXML.Excel;
using DocumentFormat.OpenXml.EMMA;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using PrakashCRM.Data.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace PrakashCRM.Controllers
{
    [RedirectingAction]
    public class SalesPersonController : Controller
    {
        // GET: SalesPerson
        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> Profile()
        {
            string email = "";
            if (Session["loggedInUserEmail"].ToString() != "" || Session["loggedInUserEmail"].ToString() != null)
                email = Session["loggedInUserEmail"].ToString();

            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "Salesperson/";

            apiUrl = apiUrl + "GetUserDetailsForProfile?email=" + email;

            HttpClient client = new HttpClient();
            UserProfileDetails userProfile = new UserProfileDetails();
            //UserProfilePost userProfile = new UserProfilePost();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                userProfile = Newtonsoft.Json.JsonConvert.DeserializeObject<UserProfileDetails>(data);

                DateTime userBirthDate = Convert.ToDateTime(userProfile.Birth_Date);
                userProfile.Birth_Date = userBirthDate.ToString("dd-MM-yyyy");

                DateTime userEmploymentDate = Convert.ToDateTime(userProfile.Employment_Date);
                userProfile.Employment_Date = userEmploymentDate.ToString("dd-MM-yyyy");

                //ViewBag.Post_Code = userProfile.Post_Code;
                //ViewBag.Country_Region_Code = userProfile.Country_Region_Code;
                ViewBag.Company_E_Mail = userProfile.Company_E_Mail;
                ViewBag.Global_Dimension_1_Code = userProfile.Global_Dimension_1_Code;
                ViewBag.Role_No = userProfile.Role_No;
                ViewBag.Role = userProfile.Role;
                ViewBag.View_Transaction_No = userProfile.View_Transaction_No;
                ViewBag.View_Transaction = userProfile.View_Transaction;
                //ViewBag.Reporting_Person_No = userProfile.Reporting_Person_No;
                ViewBag.Birth_Date = userProfile.Birth_Date;
                ViewBag.Status = userProfile.Status;
            }

            string path = Server.MapPath("~/SPProfileImages/");

            DirectoryInfo di = new DirectoryInfo(path);
            FileInfo[] smFiles = di.GetFiles("*.*");
            bool flag = true;
            foreach (FileInfo smFile in smFiles)
            {
                if (Path.GetFileNameWithoutExtension(smFile.Name) == Session["loggedInUserNo"].ToString() + "_ProfileImage")
                {
                    flag = false;
                    Session["SPProfileImage"] = "";
                    Session["SPProfileImage"] = "../SPProfileImages/" + smFile.Name;
                }
            }

            if (flag)
                Session["SPProfileImage"] = null;

            return View(userProfile);
        }

        [HttpPost]
        public async Task<ActionResult> Profile(UserProfileDetails userProfileDetails)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "Salesperson/";
            UserProfilePost userProfilePost = new UserProfilePost();

            apiUrl = apiUrl + "Profile?SPNo=" + Session["loggedInUserNo"].ToString();

            userProfilePost.First_Name = userProfileDetails.First_Name;
            userProfilePost.Last_Name = userProfileDetails.Last_Name;
            userProfilePost.Company_E_Mail = userProfileDetails.Company_E_Mail;
            userProfilePost.Job_Title = userProfileDetails.Job_Title;
            userProfilePost.Address = userProfileDetails.Address;
            //userProfilePost.Mobile_Phone_No = userProfileDetails.Mobile_Phone_No;
            userProfilePost.Phone_No = userProfileDetails.Phone_No;
            userProfilePost.Address_2 = userProfileDetails.Address_2;
            userProfilePost.Birth_Date = userProfileDetails.Birth_Date;
            userProfilePost.Employment_Date = userProfileDetails.Employment_Date;
            userProfilePost.Global_Dimension_1_Code = userProfileDetails.Global_Dimension_1_Code;
            userProfilePost.Role_No = userProfileDetails.Role_No;
            userProfilePost.View_Transaction_No = userProfileDetails.View_Transaction_No;
            userProfilePost.Status = userProfileDetails.Status;

            DateTime userBirthDate = Convert.ToDateTime(userProfilePost.Birth_Date);
            userProfilePost.Birth_Date = userBirthDate.ToString("yyyy-MM-dd");

            DateTime userEmploymentDate = Convert.ToDateTime(userProfilePost.Employment_Date);
            userProfilePost.Employment_Date = userEmploymentDate.ToString("yyyy-MM-dd");

            UserProfile responseUser = new UserProfile();

            HttpClient client = new HttpClient();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            string UserObjString = JsonConvert.SerializeObject(userProfilePost);
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
                responseUser = Newtonsoft.Json.JsonConvert.DeserializeObject<UserProfile>(data);
                Session["loggedInUserFName"] = responseUser.First_Name;
                Session["loggedInUserLName"] = responseUser.Last_Name;
                Session["ProfileAction"] = "Updated";
            }

            return RedirectToAction("Profile");
        }

        public bool NullProfileSession()
        {
            bool isSessionNull = false;

            Session["ProfileAction"] = "";
            isSessionNull = true;

            return isSessionNull;
        }

        public ActionResult SalesPersonList()
        {
            return View();
        }

        public async Task<JsonResult> GetSalesPersonListData(int orderBy, string orderDir, string filter, int skip, int top)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "Salesperson/";

            string orderByField = "";

            switch (orderBy)
            {
                case 3:
                    orderByField = "No " + orderDir;
                    break;
                case 4:
                    orderByField = "PCPL_Employee_Code " + orderDir;
                    break;
                case 5:
                    orderByField = "First_Name " + orderDir;
                    break;
                case 6:
                    orderByField = "Last_Name " + orderDir;
                    break;
                case 7:
                    orderByField = "Company_E_Mail " + orderDir;
                    break;
                case 8:
                    orderByField = "Job_Title " + orderDir;
                    break;
                case 9:
                    orderByField = "Role " + orderDir;
                    break;
                case 10:
                    orderByField = "PCPL_Department_Name " + orderDir;
                    break;
                default:
                    orderByField = "No asc";
                    break;
            }

            apiUrl = apiUrl + "GetAllUsers?skip=" + skip + "&top=" + top + "&orderby=" + orderByField + "&filter=" + filter;

            HttpClient client = new HttpClient();
            List<SPProfile> users = new List<SPProfile>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                users = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPProfile>>(data);
            }

            Session["SalespersonNo"] = null;
            Session["isSalespersonEdit"] = false;
            Session["SalespersonAction"] = "";

            return Json(users, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SalesPersonCard(string No = "")
        {
            SPProfilePost user = new SPProfilePost();
            if (No != "" || Session["SalespersonNo"] != null)
            {
                if (Session["SalespersonNo"] == null)
                    Session["SalespersonNo"] = No;

                Task<SPProfilePost> task = Task.Run<SPProfilePost>(async () => await GetSalespersonForEdit(Session["SalespersonNo"].ToString()));
                user = task.Result;
                Session["isSalespersonEdit"] = true;
                Session["SPPassword"] = user.Password;

            }

            if (user != null)
                return View(user);
            else
                return View(new SPProfile());
        }

        [HttpPost]
        public async Task<ActionResult> SalesPersonCard(SPProfilePost userpost)
        {
            //SPProfilePost userpost;
            //userpost = (SPProfilePost)user;

            DateTime userBirthDate = Convert.ToDateTime(userpost.Birth_Date);
            userpost.Birth_Date = userBirthDate.ToString("yyyy-MM-dd");

            DateTime userEmploymentDate = Convert.ToDateTime(userpost.Employment_Date);
            userpost.Employment_Date = userEmploymentDate.ToString("yyyy-MM-dd");

            if (userpost.Salespers_Purch_Code == null)
                userpost.Salespers_Purch_Code = "";

            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "Salesperson/";
            string portalUrl = ConfigurationManager.AppSettings["SPPortalUrl"].ToString();

            userpost.Global_Dimension_1_Code = userpost.Global_Dimension_1_Code == "-1" ? "" : userpost.Global_Dimension_1_Code;
            userpost.Role_No = userpost.Role_No == "-1" ? "" : userpost.Role_No;
            userpost.PCPL_Department_Code = userpost.PCPL_Department_Code == "-1" ? "" : userpost.PCPL_Department_Code;
            userpost.View_Transaction_No = userpost.View_Transaction_No == "-1" ? "" : userpost.View_Transaction_No;
            userpost.Reporting_Person_No = userpost.Reporting_Person_No == "-1" ? "" : userpost.Reporting_Person_No;
            userpost.Address_2 = userpost.Address_2 == null ? "" : userpost.Address_2;
            userpost.E_Mail = userpost.E_Mail == null ? "" : userpost.E_Mail;

            string SPNo = "";
            string SPPass = "";
            if (Convert.ToBoolean(Session["isSalespersonEdit"]) == true)
            {
                SPNo = Session["SalespersonNo"].ToString();
                SPPass = Session["SPPassword"].ToString();
                apiUrl = apiUrl + "SalesPersonCard?isEdit=true&SPNo=" + SPNo + "&Password=" + SPPass + "&portalUrl=" + portalUrl;
            }
            else
                apiUrl = apiUrl + "SalesPersonCard?isEdit=false&SPNo=" + SPNo + "&Password=" + SPPass + "&portalUrl=" + portalUrl;

            SPProfile responseUser = new SPProfile();

            HttpClient client = new HttpClient();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            string UserObjString = JsonConvert.SerializeObject(userpost);
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
                responseUser = Newtonsoft.Json.JsonConvert.DeserializeObject<SPProfile>(data);

                if (responseUser.errorDetails.isSuccess)
                {
                    if (responseUser != null && Convert.ToBoolean(Session["isSalespersonEdit"]) == true)
                        Session["SalespersonAction"] = "Updated";
                    else if (responseUser != null)
                        Session["SalespersonAction"] = "Registered";
                    else
                        Session["SalespersonAction"] = "Error";
                }
                else
                    Session["SalespersonActionErr"] = responseUser.errorDetails.message;

            }

            return RedirectToAction("SalesPersonCard");
        }

        public bool NullSalespersonSession()
        {
            bool isSessionNull = false;

            Session["SalespersonAction"] = "";
            Session["SalespersonActionErr"] = "";
            isSessionNull = true;

            return isSessionNull;
        }

        public async Task<SPProfilePost> GetSalespersonForEdit(string No)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "Salesperson/";

            apiUrl = apiUrl + "GetUserFromNo?No=" + No;

            HttpClient client = new HttpClient();
            SPProfilePost userProfile = new SPProfilePost();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                userProfile = Newtonsoft.Json.JsonConvert.DeserializeObject<SPProfilePost>(data);

                DateTime userBirthDate = Convert.ToDateTime(userProfile.Birth_Date);
                userProfile.Birth_Date = userBirthDate.ToString("yyyy-MM-dd");

                DateTime userEmploymentDate = Convert.ToDateTime(userProfile.Employment_Date);
                userProfile.Employment_Date = userEmploymentDate.ToString("yyyy-MM-dd");

                ViewBag.Post_Code = userProfile.Post_Code;
                //ViewBag.Country_Region_Code = userProfile.Country_Region_Code;
                ViewBag.Global_Dimension_1_Code = userProfile.Global_Dimension_1_Code;
                ViewBag.Role_No = userProfile.Role_No;
                ViewBag.View_Transaction_No = userProfile.View_Transaction_No;
                ViewBag.Reporting_Person_No = userProfile.Reporting_Person_No;
                ViewBag.SalespersonCode = userProfile.Salespers_Purch_Code;
                ViewBag.PCPL_Department_Code = userProfile.PCPL_Department_Code;
                ViewBag.Birth_Date = userProfile.Birth_Date;
                ViewBag.Status = userProfile.Status;
            }

            return userProfile;
        }

        [HttpPost]
        public ActionResult SalespersonUploadDoc()
        {
            SPUploadProfileImage userImage = new SPUploadProfileImage();
            userImage.ImageFile = Request.Files["docFile"];
            string path = Server.MapPath("~/SalespersonDocs/");

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            var index = userImage.ImageFile.FileName.LastIndexOf(".") + 1;
            var extension = userImage.ImageFile.FileName.Substring(index).ToUpperInvariant();
            Guid guid = Guid.NewGuid();
            userImage.ImageFile.SaveAs(path + guid.ToString() + "." + extension);

            return RedirectToAction("SalesPersonCard");
        }

        [HttpPost]
        public ActionResult UploadProfileImage()
        {
            SPUploadProfileImage userImage = new SPUploadProfileImage();
            userImage.ImageFile = Request.Files["flProfileImage"];
            string path = Server.MapPath("~/SPProfileImages/");

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            var index = userImage.ImageFile.FileName.LastIndexOf(".") + 1;
            var extension = userImage.ImageFile.FileName.Substring(index).ToUpperInvariant();
            DirectoryInfo di = new DirectoryInfo(path);
            FileInfo[] smFiles = di.GetFiles("*.*");
            foreach (FileInfo smFile in smFiles)
            {
                if (Path.GetFileNameWithoutExtension(smFile.Name) == Session["loggedInUserNo"].ToString() + "_ProfileImage")
                {
                    System.IO.File.Delete(smFile.FullName);
                }
            }
            userImage.ImageFile.SaveAs(path + Session["loggedInUserNo"].ToString() + "_ProfileImage." + extension);

            //Session["SPProfileImage"] = "../SPProfileImages/" + Session["loggedInUserNo"].ToString() + "_ProfileImage." + extension;

            return RedirectToAction("Profile");
        }


        [HttpPost]
        public bool DeleteProfileImage()
        {
            bool flag = false;

            string path = Server.MapPath("~/SPProfileImages/");

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            DirectoryInfo di = new DirectoryInfo(path);
            FileInfo[] smFiles = di.GetFiles("*.*");
            foreach (FileInfo smFile in smFiles)
            {
                if (Path.GetFileNameWithoutExtension(smFile.Name) == Session["loggedInUserNo"].ToString() + "_ProfileImage")
                {
                    System.IO.File.Delete(path + smFile.Name);
                    Session["SPProfileImage"] = null;
                    flag = true;
                }
            }

            return flag;
        }


        public async Task<JsonResult> ExportListData(int orderBy, string orderDir, string filter, int skip, int top)
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "Salesperson/";

            string orderByField = "";

            switch (orderBy)
            {
                case 2:
                    orderByField = "No " + orderDir;
                    break;
                case 3:
                    orderByField = "PCPL_Employee_Code " + orderDir;
                    break;
                case 4:
                    orderByField = "First_Name " + orderDir;
                    break;
                case 5:
                    orderByField = "Last_Name " + orderDir;
                    break;
                case 6:
                    orderByField = "Company_E_Mail " + orderDir;
                    break;
                case 7:
                    orderByField = "Job_Title " + orderDir;
                    break;
                case 8:
                    orderByField = "Address " + orderDir;
                    break;
                default:
                    orderByField = "No asc";
                    break;
            }

            apiUrl = apiUrl + "GetAllUsers?skip=" + skip + "&top=" + top + "&orderby=" + orderByField + "&filter=" + filter;

            HttpClient client = new HttpClient();
            List<SPProfile> users = new List<SPProfile>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                users = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPProfile>>(data);
            }
            DataTable dt = ToDataTable(users);

            //Name of File  
            string fileName = "SalesPersonList.xlsx";
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

        public async Task<JsonResult> GetAllPostCodesForDDL()
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "Salesperson/GetAllPostCodesForDDL";

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

            List<PostCodes> postcodes1 = postcodes.DistinctBy(a => a.City).ToList();

            return Json(postcodes1, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetAllCountryForDDL()
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "Salesperson/GetAllCountryForDDL";

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
            }

            return Json(country, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetAllBranchForDDL()
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "Salesperson/GetAllBranchForDDL";

            HttpClient client = new HttpClient();
            List<Branch> branch = new List<Branch>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                branch = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Branch>>(data);
                branch = branch.OrderBy(a => a.Name).ToList();
            }

            return Json(branch, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetAllRoleForDDL()
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "Salesperson/GetAllRoleForDDL";

            HttpClient client = new HttpClient();
            List<Role> role = new List<Role>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                role = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Role>>(data);
                role = role.OrderBy(a => a.Role_Name).ToList();
            }

            return Json(role, JsonRequestBehavior.AllowGet);
        }


        public async Task<JsonResult> GetAllViewTransactionForDDL()
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "Salesperson/GetAllViewTransactionForDDL";

            HttpClient client = new HttpClient();
            List<ViewTransaction> viewtransaction = new List<ViewTransaction>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                viewtransaction = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ViewTransaction>>(data);
                viewtransaction = viewtransaction.OrderBy(a => a.Title).ToList();
            }

            return Json(viewtransaction, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetAllReportingPersonForDDL()
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "Salesperson/GetAllReportingPersonForDDL";

            HttpClient client = new HttpClient();
            List<ReportingPerson> reportingperson = new List<ReportingPerson>();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                reportingperson = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ReportingPerson>>(data);
                reportingperson = reportingperson.OrderBy(a => a.First_Name).ToList();
            }

            return Json(reportingperson, JsonRequestBehavior.AllowGet);
        }
        public async Task<JsonResult> GetDepartmentName()
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "Salesperson/GetDepartmentName";
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
            }

            return Json(departments, JsonRequestBehavior.AllowGet);
        }
        public async Task<JsonResult> GetAllSalespersonForDDL()
        {
            string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "Salesperson/GetAllSalespersonForDDL";

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
    }
}