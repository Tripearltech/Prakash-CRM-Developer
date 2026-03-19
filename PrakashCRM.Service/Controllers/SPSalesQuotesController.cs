using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
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
using System.Web.Routing;
using System.Web.Security;
using System.Net.Http.Headers;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.IO;
using System.Xml.Linq;
using System.Runtime.Remoting.Messaging;
using Microsoft.SqlServer.Server;
using System.IO.Compression;
using Microsoft.Ajax.Utilities;
using Xipton.Razor.Extension;

namespace PrakashCRM.Service.Controllers
{
    [RoutePrefix("api/SPSalesQuotes")]
    public class SPSalesQuotesController : ApiController
    {
        private static string FirstEmailOrEmpty(string emails)
        {
            if (string.IsNullOrWhiteSpace(emails))
                return "";

            var parts = emails
                .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(p => (p ?? "").Trim())
                .Where(p => !string.IsNullOrWhiteSpace(p));

            foreach (var part in parts)
            {
                if (part.Contains("@"))
                    return part;
            }

            return "";
        }

        private static string LoadApprovalEmailTemplate()
        {
            try
            {
                var templatePath = System.Web.Hosting.HostingEnvironment.MapPath("~/Files/CreditLimitEmail-1.html");
                if (!string.IsNullOrWhiteSpace(templatePath) && File.Exists(templatePath))
                    return File.ReadAllText(templatePath);
            }
            catch
            {
            }

            return "";
        }

        private static (string Name, string Email, string Phone) TryGetSalespersonContact(API ac, string salesPersonCode)
        {
            try
            {
                var spCode = (salesPersonCode ?? "").Trim();
                if (string.IsNullOrWhiteSpace(spCode))
                    return ("", "", "");

                var res = ac.GetData<SPSQUser>("EmployeesDotNetAPI", "Salespers_Purch_Code eq '" + spCode.Replace("'", "''") + "'");
                var user = res?.Result.Item1?.value?.FirstOrDefault();
                if (user == null)
                    return ("", "", "");

                var fullName = ((user.First_Name ?? "") + " " + (user.Last_Name ?? "")).Trim();
                var phone = (user.Mobile_Phone_No ?? "").Trim();
                if (string.IsNullOrWhiteSpace(phone))
                    phone = (user.Phone_No ?? "").Trim();
                return (fullName, FirstEmailOrEmpty(user.Company_E_Mail ?? ""), phone);
            }
            catch
            {
                return ("", "", "");
            }
        }

        private static SPLocations TryGetLocation(API ac, string locationCode)
        {
            try
            {
                var code = (locationCode ?? "").Trim();
                if (string.IsNullOrWhiteSpace(code))
                    return null;

                var res = ac.GetData<SPLocations>("LocationCardDotNetAPI", "Code eq '" + code.Replace("'", "''") + "'");
                return res?.Result.Item1?.value?.FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }

        private static SPCompanyInformation TryGetCompanyInformation(API ac)
        {
            try
            {
                var res = ac.GetData<SPCompanyInformation>("CompanyInformationDotNetAPI", "");
                return res?.Result.Item1?.value?.FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }

        private static string BuildWarehouseAddressHtml(API ac, string locationCode)
        {
            try
            {
                var code = (locationCode ?? "").Trim();
                if (string.IsNullOrWhiteSpace(code))
                    return "";

                var loc = TryGetLocation(ac, code);
                if (loc == null)
                    return System.Net.WebUtility.HtmlEncode(code);

                var parts = new List<string>();
                if (!string.IsNullOrWhiteSpace(loc.Name)) parts.Add(loc.Name.Trim());
                if (!string.IsNullOrWhiteSpace(loc.Address)) parts.Add(loc.Address.Trim());
                if (!string.IsNullOrWhiteSpace(loc.Address_2)) parts.Add(loc.Address_2.Trim());

                var city = (loc.City ?? "").Trim();
                var county = (loc.County ?? "").Trim();
                var country = (loc.Country_Region_Code ?? "").Trim();
                if (!string.IsNullOrWhiteSpace(city)) parts.Add(city);
                if (!string.IsNullOrWhiteSpace(county)) parts.Add(county);
                if (!string.IsNullOrWhiteSpace(country)) parts.Add(country);

                var cityPin = city;
                var pin = (loc.Post_Code ?? "").Trim();
                if (!string.IsNullOrWhiteSpace(cityPin) || !string.IsNullOrWhiteSpace(pin))
                    parts.Add((cityPin + (!string.IsNullOrWhiteSpace(pin) ? ("-" + pin) : "")).Trim());

                return System.Net.WebUtility.HtmlEncode(string.Join(", ", parts));
            }
            catch
            {
                return "";
            }
        }

        private static string HtmlEncodeInline(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "";

            return System.Net.WebUtility.HtmlEncode(value)
                .Replace("\r\n", " ")
                .Replace("\n", " ")
                .Replace("\r", " ")
                .Trim();
        }

        private static string BuildFooterDetailsHtml(
            string contactPersonName,
            string contactPersonPhone,
            string contactPersonEmail,
            string warehouseAddress,
            string warehouseManagerPhone,
            string dispatchTiming,
            string companyName,
            string bankName,
            string bankAccountNo,
            string ifscCode)
        {
            var cpName = (contactPersonName ?? "").Trim();
            var cpPhone = (contactPersonPhone ?? "").Trim();
            var cpEmail = (contactPersonEmail ?? "").Trim();
            var whAddr = (warehouseAddress ?? "").Trim();
            var whMgrPhone = (warehouseManagerPhone ?? "").Trim();
            var disp = (dispatchTiming ?? "").Trim();
            var comp = (companyName ?? "").Trim();
            var bank = (bankName ?? "").Trim();
            var acc = (bankAccountNo ?? "").Trim();
            var ifsc = (ifscCode ?? "").Trim();

            var anyContent = !string.IsNullOrWhiteSpace(cpName)
                || !string.IsNullOrWhiteSpace(cpPhone)
                || !string.IsNullOrWhiteSpace(cpEmail)
                || !string.IsNullOrWhiteSpace(whAddr)
                || !string.IsNullOrWhiteSpace(whMgrPhone)
                || !string.IsNullOrWhiteSpace(disp)
                || !string.IsNullOrWhiteSpace(comp)
                || !string.IsNullOrWhiteSpace(bank)
                || !string.IsNullOrWhiteSpace(acc)
                || !string.IsNullOrWhiteSpace(ifsc);

            if (!anyContent)
                return "";

            var cpNameHtml = string.IsNullOrWhiteSpace(cpName) ? "-" : System.Net.WebUtility.HtmlEncode(cpName);
            var cpPhoneHtml = string.IsNullOrWhiteSpace(cpPhone) ? "-" : System.Net.WebUtility.HtmlEncode(cpPhone);
            var cpEmailHtml = string.IsNullOrWhiteSpace(cpEmail) ? "-" : System.Net.WebUtility.HtmlEncode(cpEmail);
            var whMgrPhoneHtml = string.IsNullOrWhiteSpace(whMgrPhone) ? "-" : System.Net.WebUtility.HtmlEncode(whMgrPhone);
            var dispHtml = string.IsNullOrWhiteSpace(disp) ? "-" : System.Net.WebUtility.HtmlEncode(disp);

            var compHtml = string.IsNullOrWhiteSpace(comp) ? "" : System.Net.WebUtility.HtmlEncode(comp);
            var bankHtml = string.IsNullOrWhiteSpace(bank) ? "" : System.Net.WebUtility.HtmlEncode(bank);
            var accHtml = string.IsNullOrWhiteSpace(acc) ? "-" : System.Net.WebUtility.HtmlEncode(acc);
            var ifscHtml = string.IsNullOrWhiteSpace(ifsc) ? "-" : System.Net.WebUtility.HtmlEncode(ifsc);

            var bankLeft = string.Join(", ", new[] { compHtml, bankHtml }.Where(s => !string.IsNullOrWhiteSpace(s)));
            if (string.IsNullOrWhiteSpace(bankLeft)) bankLeft = "-";

            var whAddrHtml = string.IsNullOrWhiteSpace(whAddr) ? "-" : whAddr;

            return $@"
                    <div style=""border-top:1px solid #ccc;padding-top:12px;background:#f7f7f7;text-align:center;font-size:12px;line-height:20px;"">
                        <div>
                            <b>Contact Person :</b> {cpNameHtml} - <b>{cpPhoneHtml}</b> |
                            <b>Email :</b> {cpEmailHtml} |
                            <b>Warehouse Manager :</b> <b>{whMgrPhoneHtml}</b>
                        </div>
                        <div style=""margin-top:6px;"">
                            <b>Bank :</b> {bankLeft}
                            A/C No : <b>{accHtml}</b> | IFSC : <b>{ifscHtml}</b>
                        </div>
                        <div style=""margin-top:6px;"">
                            <b>Warehouse Address :</b> {whAddrHtml}
                        </div>
                        <div style=""margin-top:6px;"">
                            <b>Dispatch Timing :</b> {dispHtml}
                        </div>
                    </div>".Trim();
        }

        [Route("GetAllSalesQuotes")]
        public List<SPSalesQuotesList> GetAllSalesQuotes(string LoggedInUserRole, string SPCode, int skip, int top, string orderby, string filter)
        {
            API ac = new API();
            List<SPSalesQuotesList> salesquotes = new List<SPSalesQuotesList>();

            string SPCodes = "";
            if (SPCode != null)
            {
                if (SPCode.Contains(",") == true)
                {
                    string[] SPCode_ = SPCode.Split(',');
                    if (SPCode_[0] != "")
                        SPCodes = "(Salesperson_Code eq '" + SPCode_[0] + "'";
                    else
                        SPCodes = "(";

                    for (int a = 1; a < SPCode_.Length; a++)
                    {
                        if (SPCode_[a].Trim() != "")
                        {
                            if (SPCodes == "(")
                                SPCodes += "Salesperson_Code eq '" + SPCode_[a] + "'";
                            else
                                SPCodes += " OR Salesperson_Code eq '" + SPCode_[a] + "'";
                        }


                    }
                    SPCodes += ")";
                }
            }
            //string SPCode_ = SPCode.Contains(",") == true ? 

            if (filter == "" || filter == null)
            {
                if (LoggedInUserRole == "Finance")
                    filter = "PCPL_IsInquiry eq false";
                else
                {
                    if (SPCode.Contains(",") == true)
                        filter = SPCodes + " and PCPL_IsInquiry eq false";
                    else
                        filter = "Salesperson_Code eq '" + SPCode + "' and PCPL_IsInquiry eq false";
                }
            }
            else
            {
                if (LoggedInUserRole == "Finance")
                    filter = filter + " and PCPL_IsInquiry eq false";
                else
                {
                    if (SPCode.Contains(",") == true)
                        filter = filter + " and " + SPCodes + " and PCPL_IsInquiry eq false";
                    else
                        filter = filter + " and Salesperson_Code eq '" + SPCode + "' and PCPL_IsInquiry eq false";
                }
            }

            var result = ac.GetData1<SPSalesQuotesList>("SalesQuoteDotNetAPI", filter, skip, top, orderby); // and Contact_Business_Relation eq 'Customer'

            if (result.Result.Item1.value.Count > 0)
                salesquotes = result.Result.Item1.value;

            for (int i = 0; i < salesquotes.Count; i++)
            {
                string[] strDate = salesquotes[i].Order_Date.Split('-');
                salesquotes[i].Order_Date = strDate[2] + '-' + strDate[1] + '-' + strDate[0];

                //string[] strDate1 = salesquotes[i].Requested_Delivery_Date.Split('-');
                //salesquotes[i].Requested_Delivery_Date = strDate1[2] + '-' + strDate1[1] + '-' + strDate1[0];
            }

            return salesquotes;
        }

        [Route("GetApiRecordsCount")]
        public int GetApiRecordsCount(string Page, string LoggedInUserNo, string UserRoleORReportingPerson, string SPCode, string apiEndPointName, string filter)
        {
            API ac = new API();

            string SPCodes = "";
            if (SPCode != null && SPCode.Contains(",") == true)
            {
                string[] SPCode_ = SPCode.Split(',');
                SPCodes = "(Salesperson_Code eq '" + SPCode_[0] + "'";
                for (int a = 1; a < SPCode_.Length; a++)
                {
                    if (SPCode_[a].Trim() != "")
                        SPCodes += " OR Salesperson_Code eq '" + SPCode_[a] + "'";

                }
                SPCodes += ")";
            }

            if (Page == "SQListForApproveReject")
            {
                if (filter == "" || filter == null)
                {
                    if (UserRoleORReportingPerson == "Finance")
                        filter = "PCPL_Approver eq '" + LoggedInUserNo + "' and PCPL_IsInquiry eq false and PCPL_Status eq 'Approval pending from finance'";
                    else if (UserRoleORReportingPerson == "ReportingPerson")
                        filter = "PCPL_IsInquiry eq false and PCPL_ApproverHOD eq '" + LoggedInUserNo + "' and PCPL_Status eq 'Approval pending from HOD'";
                }
                else
                {
                    if (UserRoleORReportingPerson == "Finance")
                        filter = filter + " and PCPL_Approver eq '" + LoggedInUserNo + "' and PCPL_IsInquiry eq false and PCPL_Status eq 'Approval pending from finance'";
                    else if (UserRoleORReportingPerson == "ReportingPerson")
                        filter = filter + " and PCPL_IsInquiry eq false and PCPL_ApproverHOD eq '" + LoggedInUserNo + "' and PCPL_Status eq 'Approval pending from HOD'";
                }

            }
            else if (Page == "SQList")
            {
                if (filter == "" || filter == null)
                {
                    if (UserRoleORReportingPerson == "Finance")
                        filter = "PCPL_IsInquiry eq false";
                    else
                    {
                        if (SPCode.Contains(",") == true)
                            filter = SPCodes + " and PCPL_IsInquiry eq false";
                        else
                            filter = "Salesperson_Code eq '" + SPCode + "' and PCPL_IsInquiry eq false";
                    }
                }
                else
                {
                    if (UserRoleORReportingPerson == "Finance")
                        filter = filter + " and PCPL_IsInquiry eq false";
                    else
                    {
                        if (SPCode.Contains(",") == true)
                            filter = filter + " and " + SPCodes + " and PCPL_IsInquiry eq false";
                        else
                            filter = filter + " and Salesperson_Code eq '" + SPCode + "' and PCPL_IsInquiry eq false";
                    }
                }
            }

            //if (Page == "SQListForApproveReject")
            //   filter += " and PCPL_Status ne '0'";

            var count = ac.CalculateCount(apiEndPointName, filter);

            return Convert.ToInt32(count.Result);
        }

        [Route("GetSalesLineItems")]
        public List<SPSQLines> GetSalesLineItems(string DocumentNo)
        {
            API ac = new API();
            List<SPSQLines> SQLines = new List<SPSQLines>();

            var result = ac.GetData<SPSQLines>("SalesQuoteSubFormDotNetAPI", "Document_No eq '" + DocumentNo + "'"); // and Contact_Business_Relation eq 'Customer'

            if (result.Result.Item1.value.Count > 0)
                SQLines = result.Result.Item1.value;

            return SQLines;
        }

        [Route("GetOrderedQtyDetails")]
        public List<SPSQOrderedQtyDetails> GetOrderedQtyDetails(string SQNo)
        {
            API ac = new API();
            List<SPSQOrderedQtyDetails> orderedQtyDetails = new List<SPSQOrderedQtyDetails>();

            var result = ac.GetData<SPSQOrderedQtyDetails>("OrderedQty", "No__FilterOnly eq '" + SQNo + "'");

            if (result.Result.Item1.value.Count > 0)
                orderedQtyDetails = result.Result.Item1.value;

            return orderedQtyDetails;
        }


        [Route("GetInvoicedQtyDetails")]
        public List<SPSQInvoicedQtyDetails> GetInvoicedQtyDetails(string SQNo)
        {
            API ac = new API();
            List<SPSQInvoicedQtyDetails> invoicedQtyDetails = new List<SPSQInvoicedQtyDetails>();

            var result = ac.GetData<SPSQInvoicedQtyDetails>("InvoicedQty", "No__FilterOnly eq '" + SQNo + "'"); // and Contact_Business_Relation eq 'Customer'

            if (result.Result.Item1.value.Count > 0)
                invoicedQtyDetails = result.Result.Item1.value;

            return invoicedQtyDetails;
        }

        [Route("GetInProcessQtyDetails")]
        public List<SPSQInProcessQtyDetails> GetInProcessQtyDetails(string SQNo)
        {
            API ac = new API();
            List<SPSQInProcessQtyDetails> inProcessQtyDetails = new List<SPSQInProcessQtyDetails>();

            var result = ac.GetData<SPSQInProcessQtyDetails>("SalesOrderCardDotNetAPI", "Quote_No eq '" + SQNo + "'"); // and Contact_Business_Relation eq 'Customer'

            if (result.Result.Item1.value.Count > 0)
                inProcessQtyDetails = result.Result.Item1.value;

            return inProcessQtyDetails;
        }

        [Route("SalesQuote")]
        public SPSQHeader SalesQuote(SPSQHeaderDetails salesQuoteDetails, string LoggedInSPUserEmail, string SPName)
        {
            SPSQHeaderPost requestSQHeader = new SPSQHeaderPost();
            SPSQHeaderPostWithCustTemplateCode reqSQHeaderWithCustTemplateCode = new SPSQHeaderPostWithCustTemplateCode();
            SPSQHeaderUpdate requestSQHeaderUpdate = new SPSQHeaderUpdate();
            SPSQHeader responseSQHeader = new SPSQHeader();
            string LocationCode = "", CustomerName = "";
            var ac = new API();
            errorDetails ed = new errorDetails();
            string ApprovalFormatFile = Unzip(salesQuoteDetails.zipApprovalFormatFile);
            int QuoteValidityDays = 0;

            // Initialize finance and reporting person details
            SPFinanceUserDetails financeUserDetails = new SPFinanceUserDetails();
            financeUserDetails.No = "";
            var resFinanceUserDetails = ac.GetData<SPFinanceUserDetails>("EmployeesDotNetAPI", "Role eq 'Finance'");
            if (resFinanceUserDetails?.Result.Item1.value.Count > 0)
                financeUserDetails = resFinanceUserDetails.Result.Item1.value[0];
            else
            {
                responseSQHeader.errorDetails = new errorDetails { isSuccess = false, message = "Failed to retrieve finance user details." };
                return responseSQHeader;
            }

            SPUserReportingPersonDetails reportingPersonDetails = new SPUserReportingPersonDetails();
            reportingPersonDetails.Reporting_Person_No = "";
            var resultUserDetails = ac.GetData<SPUserReportingPersonDetails>("EmployeesDotNetAPI", "Salespers_Purch_Code eq '" + salesQuoteDetails.SalespersonCode + "'");
            if (resultUserDetails?.Result.Item1.value.Count > 0)
                reportingPersonDetails = resultUserDetails.Result.Item1.value[0];
            else
            {
                responseSQHeader.errorDetails = new errorDetails { isSuccess = false, message = "Failed to retrieve reporting person details." };
                return responseSQHeader;
            }

            var result = (dynamic)null;

            if (!Convert.ToBoolean(salesQuoteDetails.IsSQEdit))
            {
                // New Sales Quote
                if (salesQuoteDetails.CustomerNo != null)
                {
                    // Populate requestSQHeader
                    requestSQHeader.Order_Date = salesQuoteDetails.OrderDate;
                    requestSQHeader.Quote_Valid_Until_Date = salesQuoteDetails.ValidUntillDate;
                    requestSQHeader.PCPL_Inquiry_No = string.IsNullOrEmpty(salesQuoteDetails.InquiryNo) ? "" : salesQuoteDetails.InquiryNo;
                    requestSQHeader.Sell_to_Contact_No = salesQuoteDetails.ContactCompanyNo;
                    requestSQHeader.Sell_to_Contact = salesQuoteDetails.ContactCompanyName;
                    requestSQHeader.PCPL_Contact_Person = salesQuoteDetails.ContactPersonNo;
                    requestSQHeader.Sell_to_Customer_No = salesQuoteDetails.CustomerNo;
                    requestSQHeader.Salesperson_Code = salesQuoteDetails.SalespersonCode;
                    requestSQHeader.Payment_Terms_Code = salesQuoteDetails.PaymentTermsCode;
                    requestSQHeader.Shipment_Method_Code = salesQuoteDetails.ShipmentMethodCode;
                    requestSQHeader.Location_Code = salesQuoteDetails.LocationCode;
                    requestSQHeader.Ship_to_Code = salesQuoteDetails.ShiptoCode == "-1" ? "" : salesQuoteDetails.ShiptoCode;
                    requestSQHeader.PCPL_Job_to_Code = salesQuoteDetails.JobtoCode == "-1" ? "" : salesQuoteDetails.JobtoCode;
                    requestSQHeader.PCPL_IsInquiry = false;
                    requestSQHeader.WorkDescription = string.IsNullOrEmpty(salesQuoteDetails.JustificationDetails) ? "" : salesQuoteDetails.JustificationDetails;
                    requestSQHeader.PCPL_Target_Date = string.IsNullOrEmpty(salesQuoteDetails.TargetDate) ? "1900-01-01" : salesQuoteDetails.TargetDate;

                    // Set approval status
                    if (salesQuoteDetails.ApprovalFor == "Negative Credit Limit")
                    {
                        requestSQHeader.PCPL_Approver = financeUserDetails.No;
                        requestSQHeader.PCPL_Status = "Approval pending from finance";
                        requestSQHeader.PCPL_ApprovalFor = "Credit Limit";
                        requestSQHeader.PCPL_Submitted_On = DateTime.Now.ToString("yyyy-MM-dd");
                        requestSQHeader.PCPL_ApproverHOD = "";
                    }
                    else if (salesQuoteDetails.ApprovalFor == "Negative Margin")
                    {
                        requestSQHeader.PCPL_ApproverHOD = reportingPersonDetails.Reporting_Person_No;
                        requestSQHeader.PCPL_Status = "Approval pending from HOD";
                        requestSQHeader.PCPL_ApprovalFor = "Margin";
                        requestSQHeader.PCPL_Submitted_On = DateTime.Now.ToString("yyyy-MM-dd");
                        requestSQHeader.PCPL_Approver = "";
                    }
                    else if (salesQuoteDetails.ApprovalFor == "Both")
                    {
                        requestSQHeader.PCPL_Approver = financeUserDetails.No;
                        requestSQHeader.PCPL_Status = "Approval pending from finance";
                        requestSQHeader.PCPL_ApprovalFor = "Both";
                        requestSQHeader.PCPL_Submitted_On = DateTime.Now.ToString("yyyy-MM-dd");
                        requestSQHeader.PCPL_ApproverHOD = reportingPersonDetails.Reporting_Person_No;
                    }
                    else
                    {
                        requestSQHeader.PCPL_Approver = "";
                        requestSQHeader.PCPL_Status = "Approved";
                        requestSQHeader.PCPL_ApprovalFor = "";
                        requestSQHeader.PCPL_Submitted_On = "1900-01-01";
                        requestSQHeader.PCPL_ApproverHOD = "";
                    }

                    result = PostItemSQ("SalesQuoteDotNetAPI", requestSQHeader, responseSQHeader);
                }
                else
                {
                    // Populate reqSQHeaderWithCustTemplateCode
                    reqSQHeaderWithCustTemplateCode.Order_Date = salesQuoteDetails.OrderDate;
                    reqSQHeaderWithCustTemplateCode.Quote_Valid_Until_Date = salesQuoteDetails.ValidUntillDate;
                    reqSQHeaderWithCustTemplateCode.PCPL_Inquiry_No = string.IsNullOrEmpty(salesQuoteDetails.InquiryNo) ? "" : salesQuoteDetails.InquiryNo;
                    reqSQHeaderWithCustTemplateCode.Sell_to_Contact_No = salesQuoteDetails.ContactCompanyNo;
                    reqSQHeaderWithCustTemplateCode.Sell_to_Contact = salesQuoteDetails.ContactCompanyName;
                    reqSQHeaderWithCustTemplateCode.PCPL_Contact_Person = salesQuoteDetails.ContactPersonNo;
                    reqSQHeaderWithCustTemplateCode.Sell_to_Customer_No = string.IsNullOrEmpty(salesQuoteDetails.CustomerNo) ? "" : salesQuoteDetails.CustomerNo;
                    reqSQHeaderWithCustTemplateCode.Salesperson_Code = salesQuoteDetails.SalespersonCode;
                    reqSQHeaderWithCustTemplateCode.Payment_Terms_Code = salesQuoteDetails.PaymentTermsCode;
                    reqSQHeaderWithCustTemplateCode.Shipment_Method_Code = salesQuoteDetails.ShipmentMethodCode;
                    reqSQHeaderWithCustTemplateCode.Location_Code = salesQuoteDetails.LocationCode;
                    reqSQHeaderWithCustTemplateCode.Ship_to_Code = salesQuoteDetails.ShiptoCode == "-1" ? "" : salesQuoteDetails.ShiptoCode;
                    reqSQHeaderWithCustTemplateCode.PCPL_Job_to_Code = salesQuoteDetails.JobtoCode == "-1" ? "" : salesQuoteDetails.JobtoCode;
                    reqSQHeaderWithCustTemplateCode.PCPL_IsInquiry = false;
                    reqSQHeaderWithCustTemplateCode.Sell_to_Customer_Templ_Code = salesQuoteDetails.CustomerTemplateCode;
                    reqSQHeaderWithCustTemplateCode.WorkDescription = string.IsNullOrEmpty(salesQuoteDetails.JustificationDetails) ? "" : salesQuoteDetails.JustificationDetails;
                    reqSQHeaderWithCustTemplateCode.PCPL_Target_Date = string.IsNullOrEmpty(salesQuoteDetails.TargetDate) ? "1900-01-01" : salesQuoteDetails.TargetDate;

                    // Set approval status
                    if (salesQuoteDetails.ApprovalFor == "Negative Credit Limit")
                    {
                        reqSQHeaderWithCustTemplateCode.PCPL_Approver = financeUserDetails.No;
                        reqSQHeaderWithCustTemplateCode.PCPL_Status = "Approval pending from finance";
                        reqSQHeaderWithCustTemplateCode.PCPL_ApprovalFor = "Credit Limit";
                        reqSQHeaderWithCustTemplateCode.PCPL_Submitted_On = DateTime.Now.ToString("yyyy-MM-dd");
                        reqSQHeaderWithCustTemplateCode.PCPL_ApproverHOD = "";
                    }
                    else if (salesQuoteDetails.ApprovalFor == "Negative Margin")
                    {
                        reqSQHeaderWithCustTemplateCode.PCPL_ApproverHOD = reportingPersonDetails.Reporting_Person_No;
                        reqSQHeaderWithCustTemplateCode.PCPL_Status = "Approval pending from HOD";
                        reqSQHeaderWithCustTemplateCode.PCPL_ApprovalFor = "Margin";
                        reqSQHeaderWithCustTemplateCode.PCPL_Submitted_On = DateTime.Now.ToString("yyyy-MM-dd");
                        reqSQHeaderWithCustTemplateCode.PCPL_Approver = "";
                    }
                    else if (salesQuoteDetails.ApprovalFor == "Both")
                    {
                        reqSQHeaderWithCustTemplateCode.PCPL_Approver = financeUserDetails.No;
                        // First approval is from finance; HOD comes after finance approval.
                        reqSQHeaderWithCustTemplateCode.PCPL_Status = "Approval pending from finance";
                        reqSQHeaderWithCustTemplateCode.PCPL_ApprovalFor = "Both";
                        reqSQHeaderWithCustTemplateCode.PCPL_Submitted_On = DateTime.Now.ToString("yyyy-MM-dd");
                        reqSQHeaderWithCustTemplateCode.PCPL_ApproverHOD = reportingPersonDetails.Reporting_Person_No;
                    }
                    else
                    {
                        reqSQHeaderWithCustTemplateCode.PCPL_Approver = "";
                        reqSQHeaderWithCustTemplateCode.PCPL_Status = "Approved";
                        reqSQHeaderWithCustTemplateCode.PCPL_ApprovalFor = "";
                        reqSQHeaderWithCustTemplateCode.PCPL_Submitted_On = "1900-01-01";
                        reqSQHeaderWithCustTemplateCode.PCPL_ApproverHOD = "";
                    }

                    result = PostItemSQWithCustTemplateCode("SalesQuoteDotNetAPI", reqSQHeaderWithCustTemplateCode, responseSQHeader);
                }

                // Check if header creation was successful
                if (result?.Result.Item1 == null || !result.Result.Item2.isSuccess)
                {
                    responseSQHeader.errorDetails = result?.Result.Item2 ?? new errorDetails { isSuccess = false, message = "Failed to create sales quote header." };
                    return responseSQHeader;
                }

                responseSQHeader = result.Result.Item1;
                ed = result.Result.Item2;
                responseSQHeader.errorDetails = ed;
                CustomerName = responseSQHeader.Sell_to_Customer_Name ?? "";
                LocationCode = salesQuoteDetails.LocationCode;

                // Process line items
                SPSQLinesPost reqSQLine = new SPSQLinesPost();
                SPSQLiquidLinesPost reqSQLiquidLine = new SPSQLiquidLinesPost();
                SPSQLines resSQLine = new SPSQLines();
                errorDetails ed1 = new errorDetails();

                for (int a = 0; a < salesQuoteDetails.Products.Count; a++)
                {
                    var product = salesQuoteDetails.Products[a];

                    // Update inquiry to quote if applicable
                    if (!string.IsNullOrEmpty(salesQuoteDetails.InquiryNo))
                    {
                        SPSQUpdateInqToQuote inqToQuoteReq = new SPSQUpdateInqToQuote();
                        SPInqLines inqToQuoteRes = new SPInqLines();
                        errorDetails edInqToQuote = new errorDetails();
                        inqToQuoteReq.PCPL_Convert_Quote = true;

                        var resultInqToQuote = PatchItemInqToQuote("InquiryProductsDotNetAPI", inqToQuoteReq, inqToQuoteRes, $"Document_Type='Quote',Document_No='{salesQuoteDetails.InquiryNo}',Line_No={Convert.ToInt32(!string.IsNullOrWhiteSpace(product.InqProdLineNo) ? product.InqProdLineNo : product.Line_No.ToString())}");

                        if (resultInqToQuote?.Result.Item1 == null || !resultInqToQuote.Result.Item2.isSuccess)
                        {
                            responseSQHeader.errorDetails = resultInqToQuote?.Result.Item2 ?? new errorDetails { isSuccess = false, message = "Failed to update inquiry to quote." };
                            return responseSQHeader; // Abort if inquiry update fails
                        }
                    }

                    // Create line item
                    var result1 = (dynamic)null;
                    if (!Convert.ToBoolean(product.IsLiquidProd))
                    {
                        reqSQLine.Document_No = responseSQHeader.No;
                        reqSQLine.No = product.No;
                        reqSQLine.Type = "Item";
                        reqSQLine.PCPL_MRP = product.PCPL_MRP;
                        reqSQLine.Location_Code = LocationCode;
                        reqSQLine.Quantity = product.Quantity;
                        reqSQLine.Unit_Price = product.Unit_Price;
                        reqSQLine.PCPL_Packing_Style_Code = product.PCPL_Packing_Style_Code;
                        reqSQLine.PCPL_Transport_Method = product.PCPL_Transport_Method;
                        reqSQLine.PCPL_Transport_Cost = product.PCPL_Transport_Cost;
                        reqSQLine.PCPL_Commission_Payable = product.PCPL_Commission_Payable ?? "";
                        reqSQLine.PCPL_Commission_Type = product.PCPL_Commission_Type ?? "";
                        reqSQLine.PCPL_Commission = product.PCPL_Commission;
                        reqSQLine.PCPL_Commission_Amount = product.PCPL_Commission_Amount;
                        reqSQLine.PCPL_Sales_Discount = product.PCPL_Sales_Discount;
                        reqSQLine.PCPL_Credit_Days = product.PCPL_Credit_Days;
                        reqSQLine.PCPL_Margin = product.PCPL_Margin;
                        reqSQLine.PCPL_Margin_Percent = product.PCPL_Margin_Percent;
                        reqSQLine.PCPL_Interest = product.PCPL_Interest;
                        reqSQLine.PCPL_Interest_Rate = product.PCPL_Interest_Rate;
                        reqSQLine.PCPL_Total_Cost = product.PCPL_Total_Cost;
                        reqSQLine.Delivery_Date = product.Delivery_Date;
                        reqSQLine.Drop_Shipment = product.Drop_Shipment;
                        reqSQLine.Net_Weight = product.Net_Weight;
                        reqSQLine.PCPL_Vendor_No = product.PCPL_Vendor_No ?? "";
                        reqSQLine.GST_Place_Of_Supply = salesQuoteDetails.ShiptoCode == "-1" && salesQuoteDetails.JobtoCode == "-1" ? "Bill-to Address" : "Ship-to Address";
                        reqSQLine.PCPL_Inquiry_No = string.IsNullOrEmpty(salesQuoteDetails.InquiryNo) ? "" : salesQuoteDetails.InquiryNo;
                        reqSQLine.PCPL_Inquiry_Line_No = string.IsNullOrEmpty(product.InqProdLineNo) ? 0 : Convert.ToInt32(product.InqProdLineNo);
                        reqSQLine.Line_No = product.Line_No;
                        reqSQLine.New_Margin = product.New_Margin;
                        reqSQLine.New_Price = product.New_Price;
                        reqSQLine.Price_Updated = product.Price_Updated;

                        result1 = PostItemSQLines("SalesQuoteSubFormDotNetAPI", "SQLine", reqSQLine, reqSQLiquidLine, resSQLine);
                    }
                    else
                    {
                        reqSQLiquidLine.Document_No = responseSQHeader.No;
                        reqSQLiquidLine.No = product.No;
                        reqSQLiquidLine.Type = "Item";
                        reqSQLiquidLine.PCPL_MRP = product.PCPL_MRP;
                        reqSQLiquidLine.Location_Code = LocationCode;
                        reqSQLiquidLine.PCPL_Concentration_Rate_Percent = product.PCPL_Concentration_Rate_Percent;
                        reqSQLiquidLine.Net_Weight = product.Net_Weight;
                        reqSQLiquidLine.PCPL_Liquid_Rate = product.PCPL_Liquid_Rate;
                        reqSQLiquidLine.PCPL_Liquid = product.IsLiquidProd;
                        reqSQLiquidLine.PCPL_Packing_Style_Code = product.PCPL_Packing_Style_Code;
                        reqSQLiquidLine.PCPL_Transport_Method = product.PCPL_Transport_Method;
                        reqSQLiquidLine.PCPL_Transport_Cost = product.PCPL_Transport_Cost;
                        reqSQLiquidLine.PCPL_Commission_Payable = product.PCPL_Commission_Payable ?? "";
                        reqSQLiquidLine.PCPL_Commission_Type = product.PCPL_Commission_Type ?? "";
                        reqSQLiquidLine.PCPL_Commission = product.PCPL_Commission;
                        reqSQLiquidLine.PCPL_Commission_Amount = product.PCPL_Commission_Amount;
                        reqSQLiquidLine.PCPL_Sales_Discount = product.PCPL_Sales_Discount;
                        reqSQLiquidLine.PCPL_Credit_Days = product.PCPL_Credit_Days;
                        reqSQLiquidLine.PCPL_Margin = product.PCPL_Margin;
                        reqSQLiquidLine.PCPL_Margin_Percent = product.PCPL_Margin_Percent;
                        reqSQLiquidLine.PCPL_Interest = product.PCPL_Interest;
                        reqSQLiquidLine.PCPL_Interest_Rate = product.PCPL_Interest_Rate;
                        reqSQLiquidLine.PCPL_Total_Cost = product.PCPL_Total_Cost;
                        reqSQLiquidLine.Delivery_Date = product.Delivery_Date;
                        reqSQLiquidLine.Drop_Shipment = product.Drop_Shipment;
                        reqSQLiquidLine.PCPL_Vendor_No = product.PCPL_Vendor_No ?? "";
                        reqSQLiquidLine.GST_Place_Of_Supply = salesQuoteDetails.ShiptoCode == "-1" && salesQuoteDetails.JobtoCode == "-1" ? "Bill-to Address" : "Ship-to Address";
                        reqSQLiquidLine.PCPL_Inquiry_No = string.IsNullOrEmpty(salesQuoteDetails.InquiryNo) ? "" : salesQuoteDetails.InquiryNo;
                        reqSQLiquidLine.PCPL_Inquiry_Line_No = string.IsNullOrEmpty(product.InqProdLineNo) ? 0 : Convert.ToInt32(product.InqProdLineNo);
                        reqSQLiquidLine.Line_No = product.Line_No;
                        reqSQLiquidLine.New_Margin = product.New_Margin;
                        reqSQLiquidLine.New_Price = product.New_Price;
                        reqSQLiquidLine.Price_Updated = product.Price_Updated;

                        result1 = PostItemSQLines("SalesQuoteSubFormDotNetAPI", "SQLiquidLine", reqSQLine, reqSQLiquidLine, resSQLine);
                    }

                    // Check if line item creation was successful
                    if (result1?.Result.Item1 == null || !result1.Result.Item2.isSuccess)
                    {
                        responseSQHeader.errorDetails = result1?.Result.Item2 ?? new errorDetails { isSuccess = false, message = "Failed to create sales quote line item." };
                        return responseSQHeader; // Abort if line item creation fails
                    }

                    resSQLine = result1.Result.Item1;
                    ed1 = result1.Result.Item2;
                    responseSQHeader.errorDetails = ed1;
                    responseSQHeader.ItemLineNo += $"{resSQLine.No}_{resSQLine.Line_No},";
                }

                // Update inquiry status if applicable
                if (!string.IsNullOrEmpty(salesQuoteDetails.InquiryNo))
                {
                    SPSQUpdateInqStatus updateInqStatus = new SPSQUpdateInqStatus();
                    SPSQUpdateInqStatusOData updateInqStatusOData = new SPSQUpdateInqStatusOData();
                    errorDetails edUpdateInqStatus = new errorDetails();
                    updateInqStatus.salesquoteno = responseSQHeader.No;

                    var result2 = PostItemForUpdateInqStatus<SPSQUpdateInqStatusOData>("", updateInqStatus, updateInqStatusOData);
                    if (result2?.Result.Item1 == null || !result2.Result.Item2.isSuccess)
                    {
                        responseSQHeader.errorDetails = result2?.Result.Item2 ?? new errorDetails { isSuccess = false, message = "Failed to update inquiry status." };
                        return responseSQHeader; // Abort if inquiry status update fails
                    }
                    updateInqStatusOData = result2.Result.Item1;
                    edUpdateInqStatus = result2.Result.Item2;
                    responseSQHeader.errorDetails = edUpdateInqStatus;

                    API ac2 = new API();
                    string inquiryStatus = "Completed";
                    var remainingInquiryLines = ac2.GetData<SPInquiryProducts>("InquiryProductsDotNetAPI", "Document_No eq '" + salesQuoteDetails.InquiryNo + "' and PCPL_Convert_Quote eq false");

                    if (remainingInquiryLines.Result.Item1 != null && remainingInquiryLines.Result.Item1.value.Count > 0)
                        inquiryStatus = "Partial";

                    SPInquiryUpdate inquiryStatusUpdate = new SPInquiryUpdate();
                    SPInquiry inquiryStatusResponse = new SPInquiry();
                    inquiryStatusUpdate.Inquiry_Status = inquiryStatus;

                    var inquiryStatusPatchResult = PatchItemInquiryStatus("InquiryDotNetAPI", inquiryStatusUpdate, inquiryStatusResponse, "Document_Type='Quote',Inquiry_No='" + salesQuoteDetails.InquiryNo + "'");
                    if (inquiryStatusPatchResult?.Result.Item1 == null || !inquiryStatusPatchResult.Result.Item2.isSuccess)
                    {
                        responseSQHeader.errorDetails = inquiryStatusPatchResult?.Result.Item2 ?? new errorDetails { isSuccess = false, message = "Failed to update inquiry header status." };
                        return responseSQHeader;
                    }

                    responseSQHeader.errorDetails = inquiryStatusPatchResult.Result.Item2;
                }

                // Send email only if all operations succeeded and approval is required
                if (salesQuoteDetails.ApprovalFor != null)
                {
                    string toEmail = "";
                    string ccEmail = LoggedInSPUserEmail;
                    if (salesQuoteDetails.ApprovalFor == "Negative Credit Limit")
                        toEmail = FirstEmailOrEmpty(financeUserDetails.Company_E_Mail);
                    else if (salesQuoteDetails.ApprovalFor == "Negative Margin")
                        toEmail = FirstEmailOrEmpty(reportingPersonDetails.PCPL_Reporting_Person_Email);
                    else if (salesQuoteDetails.ApprovalFor == "Both")
                        // Sequential approvals: first finance, then HOD after finance approval.
                        toEmail = FirstEmailOrEmpty(financeUserDetails.Company_E_Mail);

                    string ApprovalForText = salesQuoteDetails.ApprovalFor == "Both" ? "Negative Credit Limit And Margin" : salesQuoteDetails.ApprovalFor;

                    DateTime quoteDate = Convert.ToDateTime(salesQuoteDetails.OrderDate);
                    DateTime quoteValidUntilDate = Convert.ToDateTime(salesQuoteDetails.ValidUntillDate);
                    QuoteValidityDays = (quoteValidUntilDate - quoteDate).Days;

                    string myString = ApprovalFormatFile;

                    // AprDetailsJustificationReason (shown in UI) maps to Sales Quote WorkDescription.
                    // For the initial approval mail, take it from the request payload.
                    var aprDetailsJustificationReason = (salesQuoteDetails?.JustificationDetails ?? "").Trim();
                    var justificationSuffix = "";
                    if (!string.IsNullOrWhiteSpace(aprDetailsJustificationReason))
                    {
                        var safeJustification = System.Net.WebUtility.HtmlEncode(aprDetailsJustificationReason)
                            .Replace("\r\n", "<br />")
                            .Replace("\n", "<br />")
                            .Replace("\r", "<br />");
                        justificationSuffix = "<br /><b>Justification : </b>" + safeJustification;
                    }

                    if (salesQuoteDetails.ApprovalFor == "Negative Credit Limit")
                    {
                        myString = myString.Replace("##pageheading##", " CREDIT LIMIT EXCEEDED ");
                        myString = myString.Replace("##heading##", $"The user '{SPName}' was trying to create the quote, but the Credit limit is exceeded.{justificationSuffix}");
                    }
                    else if (salesQuoteDetails.ApprovalFor == "Negative Margin")
                    {
                        myString = myString.Replace("##pageheading##", " MARGIN IS LESS THAN ZERO ");
                        myString = myString.Replace("##heading##", $"The user '{SPName}' was trying to create the quote, but the Margin is less than zero.{justificationSuffix}");
                    }
                    else if (salesQuoteDetails.ApprovalFor == "Both")
                    {
                        myString = myString.Replace("##pageheading##", " CREDIT LIMIT EXCEEDED ");
                        myString = myString.Replace("##heading##", $"The user '{SPName}' was trying to create the quote, but the Credit limit is exceeded and Margin is less than Zero.{justificationSuffix}");
                    }

                    var approvalLinkRole = "";
                    if (salesQuoteDetails.ApprovalFor == "Negative Credit Limit" || salesQuoteDetails.ApprovalFor == "Both")
                        approvalLinkRole = "Finance";
                    else if (salesQuoteDetails.ApprovalFor == "Negative Margin")
                        approvalLinkRole = "HOD";

                    salesQuoteDetails.SQApprovalFormURL += $"?SQNo={responseSQHeader.No}&ScheduleStatus=''&SQStatus={responseSQHeader.PCPL_Status}&SQFor=ApproveReject&LoggedInUserRole={approvalLinkRole}";
                    myString = myString.Replace("##SalesQuoteNo##", responseSQHeader.No);
                    myString = myString.Replace("##SalesQuoteDate##", responseSQHeader.Order_Date ?? "");
                    myString = myString.Replace("##ApprovalForText##", ApprovalForText);

                    string[] CustSellToAddress = salesQuoteDetails.ConsigneeAddress.Split('_');

                    string CustName = CustSellToAddress.ElementAtOrDefault(0) ?? "";
                    string line1 = CustSellToAddress.ElementAtOrDefault(1) ?? "";
                    string line2 = CustSellToAddress.ElementAtOrDefault(2) ?? "";
                    string line3 = CustSellToAddress.ElementAtOrDefault(3) ?? "";
                    string pin = CustSellToAddress.ElementAtOrDefault(4) ?? "";

                    string CustSellToAddress_ = $"{CustName}<br />{line1},<br />{line2},<br />{line3}-{pin}";

                    myString = myString.Replace("##CustomerDetail##", CustSellToAddress_);
                    myString = myString.Replace("##ContactName##", salesQuoteDetails.ContactPersonName);

                    // Show ShippingDetails for Finance mail as well.
                    // If Job-to address isn't provided, fall back to the sell-to/consignee address.
                    myString = myString.Replace("##ShippingHeader##", "ShippingDetails");
                    if (!string.IsNullOrWhiteSpace(salesQuoteDetails.JobtoAddress) && salesQuoteDetails.JobtoCode != "-1")
                    {
                        string[] CustJobtoAddress = salesQuoteDetails.JobtoAddress.Split(',');
                        string CustJobtoAddress_ = string.Join(",<br />", CustJobtoAddress);
                        myString = myString.Replace("##ShippingDetail##", CustJobtoAddress_);
                    }
                    else
                    {
                        myString = myString.Replace("##ShippingDetail##", CustSellToAddress_);
                    }

                    string str_lineTable = "<table cellpadding=\"0\" cellspacing=\"1\" border=\"1\" width=\"100%\" align=\"left\" style=\"border:1px solid #BEBEBE\">";
                    str_lineTable += "<tr style=\"background:#ccc;font-weight:bold !important;text-transform:capitalize;font-size:12\">";
                    str_lineTable += "<td width =\"5%\" align=\"left\">SR.NO.</td>";
                    str_lineTable += "<td width =\"10%\" align=\"left\">Product</td>";
                    str_lineTable += "<td width =\"10%\" align=\"left\">Packaging Style</td>";
                    str_lineTable += "<td width =\"5%\" align=\"right\">Qty(MT)</td>";
                    str_lineTable += "<td width =\"5%\" align=\"right\">Sales Price(Rs.)</td>";
                    str_lineTable += "<td width =\"5%\" align=\"right\">Margin</td>";
                    str_lineTable += "<td width =\"5%\" align=\"right\">IncoTerms</td>";
                    str_lineTable += "<td width =\"5%\" align=\"right\">GST</td>";
                    str_lineTable += "</tr>";
                    int counter = 1;

                    var gstPlaceOfSupply = (salesQuoteDetails.ShiptoCode == "-1" && salesQuoteDetails.JobtoCode == "-1")
                        ? "Bill-to Address"
                        : "Ship-to Address";

                    for (int a = 0; a < salesQuoteDetails.Products.Count; a++)
                    {
                        str_lineTable += "<tr style=\"font-size:10\">";
                        str_lineTable += $"<td align=\"left\" valign=\"top\" bgcolor=\"#FFFFFF\" width=\"5%\">{counter}</td>";
                        str_lineTable += $"<td align=\"left\" valign=\"top\" bgcolor=\"#FFFFFF\" width=\"5%\">{salesQuoteDetails.Products[a].ProductName}</td>";
                        str_lineTable += $"<td align=\"left\" valign=\"top\" bgcolor=\"#FFFFFF\" width=\"5%\">{salesQuoteDetails.Products[a].PCPL_Packing_Style_Code}</td>";
                        str_lineTable += $"<td align=\"right\" valign=\"top\" bgcolor=\"#FFFFFF\" width=\"5%\">{salesQuoteDetails.Products[a].Quantity}</td>";
                        str_lineTable += $"<td align=\"right\" valign=\"top\" bgcolor=\"#FFFFFF\" width=\"5%\">{salesQuoteDetails.Products[a].Unit_Price}</td>";
                        str_lineTable += $"<td align=\"right\" valign=\"top\" bgcolor=\"#FFFFFF\" width=\"5%\">{salesQuoteDetails.Products[a].PCPL_Margin}</td>";
                        str_lineTable += $"<td align=\"right\" valign=\"top\" bgcolor=\"#FFFFFF\" width=\"5%\">{salesQuoteDetails.ShipmentMethodCode}</td>";
                        str_lineTable += $"<td align=\"right\" valign=\"top\" bgcolor=\"#FFFFFF\" width=\"5%\">{salesQuoteDetails.Products[a].PCPL_GST_Amt}</td>";
                        str_lineTable += "</tr>";
                        counter++;
                    }

                    str_lineTable += "</table>";
                    myString = myString.Replace("##totalcreditlimit##", salesQuoteDetails.TotalCreditLimit);
                    myString = myString.Replace("##usedcreditlimit##", salesQuoteDetails.UsedCreditLimit);
                    myString = myString.Replace("##availablecreditlimit##", salesQuoteDetails.AvailableCreditLimit);
                    myString = myString.Replace("##custno##", salesQuoteDetails.CustomerNo);
                    myString = myString.Replace("##custname##", salesQuoteDetails.ContactCompanyName);
                    myString = myString.Replace("##QuoteValidityDays##", QuoteValidityDays <= 1 ? $"{QuoteValidityDays} Day" : $"{QuoteValidityDays} Days");
                    myString = myString.Replace("##TaxGroupDetails##", "GST: As Applicable");
                    var paymentTerms = (responseSQHeader?.Payment_Terms_Code ?? salesQuoteDetails?.PaymentTermsCode ?? "").Trim();
                    myString = myString.Replace("##PaymentTermsHeader##", string.IsNullOrWhiteSpace(paymentTerms) ? "" : "Payment Terms : ");
                    myString = myString.Replace("##PaymentTerms##", paymentTerms);

                    var scheduleStatus = (responseSQHeader?.TPTPL_Schedule_status ?? "").Trim();
                    myString = myString.Replace("##ScheduleHeader##", string.IsNullOrWhiteSpace(scheduleStatus) ? "" : "Schedule : ");
                    myString = myString.Replace("##Schedule##", scheduleStatus);

                    myString = myString.Replace("##Note##", "Price ruling at the time of delivery for GACL & GNFC products.");
                    myString = myString.Replace("##LineDetails##", str_lineTable);
                    myString = myString.Replace("##SalesQuoteApprovalFormURL##", salesQuoteDetails.SQApprovalFormURL);

                    // Footer block (contact + warehouse).
                    var (spContactName, spContactEmail, spContactPhone) = TryGetSalespersonContact(ac, salesQuoteDetails?.SalespersonCode ?? "");
                    if (string.IsNullOrWhiteSpace(spContactName)) spContactName = (SPName ?? "").Trim();
                    if (string.IsNullOrWhiteSpace(spContactEmail)) spContactEmail = FirstEmailOrEmpty(LoggedInSPUserEmail ?? "");

                    var locationCodeForFooter = (responseSQHeader?.Location_Code ?? salesQuoteDetails?.LocationCode ?? "").Trim();
                    var loc = TryGetLocation(ac, locationCodeForFooter);
                    var warehouseManagerPhone = (loc?.Phone_No ?? "").Trim();

                    var warehouseAddressOverride = (ConfigurationManager.AppSettings["QuoteEmailWarehouseAddress_" + locationCodeForFooter] ?? ConfigurationManager.AppSettings["QuoteEmailWarehouseAddress"] ?? "").Trim();
                    var warehouseAddressHtml = !string.IsNullOrWhiteSpace(warehouseAddressOverride)
                        ? HtmlEncodeInline(warehouseAddressOverride)
                        : BuildWarehouseAddressHtml(ac, locationCodeForFooter);

                    var ci = TryGetCompanyInformation(ac);
                    var companyName = (ci?.Name ?? "").Trim();
                    var bankName = (ci?.Bank_Name ?? "").Trim();
                    var bankAccountNo = (ci?.Bank_Account_No ?? "").Trim();
                    var ifsc = (ci?.IFSC ?? "").Trim();

                    var dispatchTiming = (ConfigurationManager.AppSettings["QuoteEmailDispatchTiming_" + locationCodeForFooter] ?? ConfigurationManager.AppSettings["QuoteEmailDispatchTiming"] ?? "").Trim();
                    myString = myString.Replace("##FooterDetails##", BuildFooterDetailsHtml(spContactName, spContactPhone, spContactEmail, warehouseAddressHtml, warehouseManagerPhone, dispatchTiming, companyName, bankName, bankAccountNo, ifsc));

                    string emailSubject = $"Sales Quote Approval - {responseSQHeader.No} - {CustomerName} - {DateTime.Now:dd/MM/yyyy} - {ApprovalForText}";

                    EmailService emailService = new EmailService();
                    StringBuilder sbMailBody = new StringBuilder();
                    sbMailBody.Append(myString);

                    try
                    {
                        emailService.SendEmailWithHTMLBody(toEmail, ccEmail, "", emailSubject, sbMailBody.ToString());
                    }
                    catch (Exception ex)
                    {
                        responseSQHeader.errorDetails = new errorDetails { isSuccess = false, message = $"Failed to send email: {ex.Message}" };
                        return responseSQHeader; // Abort if email sending fails
                    }
                }

                return responseSQHeader;
            }
            else
            {
                // Update existing Sales Quote
                requestSQHeaderUpdate.Quote_Valid_Until_Date = salesQuoteDetails.ValidUntillDate;
                requestSQHeaderUpdate.PCPL_Contact_Person = salesQuoteDetails.ContactPersonNo;
                requestSQHeaderUpdate.Payment_Terms_Code = salesQuoteDetails.PaymentTermsCode;
                requestSQHeaderUpdate.Shipment_Method_Code = salesQuoteDetails.ShipmentMethodCode;
                requestSQHeaderUpdate.Ship_to_Code = salesQuoteDetails.ShiptoCode == "-1" ? "" : salesQuoteDetails.ShiptoCode;
                requestSQHeaderUpdate.PCPL_Job_to_Code = salesQuoteDetails.JobtoCode == "-1" ? "" : salesQuoteDetails.JobtoCode;

                // Preserve the same approval/status scenario as New Sales Quote.
                // NOTE: Json.NET default serialization includes nulls; we set these explicitly to avoid BC resetting to option '0'.
                var approvalFor = (salesQuoteDetails?.ApprovalFor ?? "").Trim();
                if (string.Equals(approvalFor, "0", StringComparison.OrdinalIgnoreCase))
                    approvalFor = "";

                var justification = (salesQuoteDetails?.JustificationDetails ?? "").Trim();
                if (!string.IsNullOrWhiteSpace(justification))
                    requestSQHeaderUpdate.WorkDescription = justification;

                if (string.Equals(approvalFor, "Negative Credit Limit", StringComparison.OrdinalIgnoreCase))
                {
                    requestSQHeaderUpdate.PCPL_Approver = financeUserDetails.No;
                    requestSQHeaderUpdate.PCPL_Status = "Approval pending from finance";
                    requestSQHeaderUpdate.PCPL_ApprovalFor = "Credit Limit";
                    requestSQHeaderUpdate.PCPL_Submitted_On = DateTime.Now.ToString("yyyy-MM-dd");
                    requestSQHeaderUpdate.PCPL_ApproverHOD = "";
                }
                else if (string.Equals(approvalFor, "Negative Margin", StringComparison.OrdinalIgnoreCase))
                {
                    requestSQHeaderUpdate.PCPL_ApproverHOD = reportingPersonDetails.Reporting_Person_No;
                    requestSQHeaderUpdate.PCPL_Status = "Approval pending from HOD";
                    requestSQHeaderUpdate.PCPL_ApprovalFor = "Margin";
                    requestSQHeaderUpdate.PCPL_Submitted_On = DateTime.Now.ToString("yyyy-MM-dd");
                    requestSQHeaderUpdate.PCPL_Approver = "";
                }
                else if (string.Equals(approvalFor, "Both", StringComparison.OrdinalIgnoreCase))
                {
                    requestSQHeaderUpdate.PCPL_Approver = financeUserDetails.No;
                    requestSQHeaderUpdate.PCPL_Status = "Approval pending from finance";
                    requestSQHeaderUpdate.PCPL_ApprovalFor = "Both";
                    requestSQHeaderUpdate.PCPL_Submitted_On = DateTime.Now.ToString("yyyy-MM-dd");
                    requestSQHeaderUpdate.PCPL_ApproverHOD = reportingPersonDetails.Reporting_Person_No;
                }
                else
                {
                    requestSQHeaderUpdate.PCPL_Approver = "";
                    requestSQHeaderUpdate.PCPL_Status = "Approved";
                    requestSQHeaderUpdate.PCPL_ApprovalFor = "";
                    requestSQHeaderUpdate.PCPL_Submitted_On = "1900-01-01";
                    requestSQHeaderUpdate.PCPL_ApproverHOD = "";
                }

                result = PatchItemSQ("SalesQuoteDotNetAPI", requestSQHeaderUpdate, responseSQHeader, $"Document_Type='Quote',No='{salesQuoteDetails.QuoteNo}'");

                if (result?.Result.Item1 == null || !result.Result.Item2.isSuccess)
                {
                    responseSQHeader.errorDetails = result?.Result.Item2 ?? new errorDetails { isSuccess = false, message = "Failed to update sales quote header." };
                    return responseSQHeader;
                }

                responseSQHeader = result.Result.Item1;
                ed = result.Result.Item2;
                responseSQHeader.errorDetails = ed;
                CustomerName = responseSQHeader.Sell_to_Customer_Name ?? "";
                LocationCode = salesQuoteDetails.LocationCode;

                // Process line items for update
                SPSQLinesUpdate reqSQLineUpdate = new SPSQLinesUpdate();
                SPSQLiquidLinesUpdate reqSQLiquidLineUpdate = new SPSQLiquidLinesUpdate();
                SPSQLines resSQLine = new SPSQLines();
                errorDetails ed1 = new errorDetails();

                for (int a = 0; a < salesQuoteDetails.Products.Count; a++)
                {
                    var product = salesQuoteDetails.Products[a];
                    var result1 = (dynamic)null;

                    if (!Convert.ToBoolean(product.IsLiquidProd))
                    {
                        reqSQLineUpdate.PCPL_MRP = product.PCPL_MRP;
                        reqSQLineUpdate.Location_Code = LocationCode;
                        reqSQLineUpdate.Quantity = product.Quantity;
                        reqSQLineUpdate.Unit_Price = product.Unit_Price;
                        reqSQLineUpdate.PCPL_Packing_Style_Code = product.PCPL_Packing_Style_Code;
                        reqSQLineUpdate.PCPL_Transport_Method = product.PCPL_Transport_Method;
                        reqSQLineUpdate.PCPL_Transport_Cost = product.PCPL_Transport_Cost;
                        reqSQLineUpdate.PCPL_Commission_Payable = product.PCPL_Commission_Payable ?? "";
                        reqSQLineUpdate.PCPL_Commission_Type = product.PCPL_Commission_Type ?? "";
                        reqSQLineUpdate.PCPL_Commission = product.PCPL_Commission;
                        reqSQLineUpdate.PCPL_Commission_Amount = product.PCPL_Commission_Amount;
                        reqSQLineUpdate.PCPL_Sales_Discount = product.PCPL_Sales_Discount;
                        reqSQLineUpdate.PCPL_Credit_Days = product.PCPL_Credit_Days;
                        reqSQLineUpdate.PCPL_Margin = product.PCPL_Margin;
                        reqSQLineUpdate.PCPL_Margin_Percent = product.PCPL_Margin_Percent;
                        reqSQLineUpdate.PCPL_Interest = product.PCPL_Interest;
                        reqSQLineUpdate.PCPL_Interest_Rate = product.PCPL_Interest_Rate;
                        reqSQLineUpdate.PCPL_Total_Cost = product.PCPL_Total_Cost;
                        reqSQLineUpdate.Delivery_Date = product.Delivery_Date;
                        reqSQLineUpdate.Drop_Shipment = product.Drop_Shipment;
                        reqSQLineUpdate.PCPL_Vendor_No = product.PCPL_Vendor_No ?? "";
                        reqSQLineUpdate.GST_Place_Of_Supply = salesQuoteDetails.ShiptoCode == "-1" && salesQuoteDetails.JobtoCode == "-1" ? "Bill-to Address" : "Ship-to Address";
                        reqSQLineUpdate.New_Margin = product.New_Margin;
                        reqSQLineUpdate.New_Price = product.New_Price;
                        reqSQLineUpdate.Price_Updated = product.Price_Updated;

                        int SQLineNo = Convert.ToInt32(product.Line_No);
                        result1 = PatchItemSQLines("SalesQuoteSubFormDotNetAPI", "SQLine", reqSQLineUpdate, reqSQLiquidLineUpdate, resSQLine, $"Document_Type='Quote',Document_No='{responseSQHeader.No}',Line_No={SQLineNo}");
                    }
                    else
                    {
                        reqSQLiquidLineUpdate.PCPL_MRP = product.PCPL_MRP;
                        reqSQLiquidLineUpdate.Location_Code = LocationCode;
                        reqSQLiquidLineUpdate.PCPL_Concentration_Rate_Percent = product.PCPL_Concentration_Rate_Percent;
                        reqSQLiquidLineUpdate.Net_Weight = product.Net_Weight;
                        reqSQLiquidLineUpdate.PCPL_Liquid_Rate = product.PCPL_Liquid_Rate;
                        reqSQLiquidLineUpdate.PCPL_Liquid = product.IsLiquidProd;
                        reqSQLiquidLineUpdate.PCPL_Packing_Style_Code = product.PCPL_Packing_Style_Code;
                        reqSQLiquidLineUpdate.PCPL_Transport_Method = product.PCPL_Transport_Method;
                        reqSQLiquidLineUpdate.PCPL_Transport_Cost = product.PCPL_Transport_Cost;
                        reqSQLiquidLineUpdate.PCPL_Commission_Payable = product.PCPL_Commission_Payable ?? "";
                        reqSQLiquidLineUpdate.PCPL_Commission_Type = product.PCPL_Commission_Type ?? "";
                        reqSQLiquidLineUpdate.PCPL_Commission = product.PCPL_Commission;
                        reqSQLiquidLineUpdate.PCPL_Commission_Amount = product.PCPL_Commission_Amount;
                        reqSQLiquidLineUpdate.PCPL_Sales_Discount = product.PCPL_Sales_Discount;
                        reqSQLiquidLineUpdate.PCPL_Credit_Days = product.PCPL_Credit_Days;
                        reqSQLiquidLineUpdate.PCPL_Margin = product.PCPL_Margin;
                        reqSQLiquidLineUpdate.PCPL_Margin_Percent = product.PCPL_Margin_Percent;
                        reqSQLiquidLineUpdate.PCPL_Interest = product.PCPL_Interest;
                        reqSQLiquidLineUpdate.PCPL_Interest_Rate = product.PCPL_Interest_Rate;
                        reqSQLiquidLineUpdate.PCPL_Total_Cost = product.PCPL_Total_Cost;
                        reqSQLiquidLineUpdate.Delivery_Date = product.Delivery_Date;
                        reqSQLiquidLineUpdate.Drop_Shipment = product.Drop_Shipment;
                        reqSQLiquidLineUpdate.PCPL_Vendor_No = product.PCPL_Vendor_No ?? "";
                        reqSQLiquidLineUpdate.GST_Place_Of_Supply = salesQuoteDetails.ShiptoCode == "-1" && salesQuoteDetails.JobtoCode == "-1" ? "Bill-to Address" : "Ship-to Address";
                        reqSQLiquidLineUpdate.New_Margin = product.New_Margin;
                        reqSQLiquidLineUpdate.New_Price = product.New_Price;
                        reqSQLiquidLineUpdate.Price_Updated = product.Price_Updated;


                        int SQLineNo = Convert.ToInt32(product.Line_No);
                        result1 = PatchItemSQLines("SalesQuoteSubFormDotNetAPI", "SQLiquidLine", reqSQLineUpdate, reqSQLiquidLineUpdate, resSQLine, $"Document_Type='Quote',Document_No='{responseSQHeader.No}',Line_No={SQLineNo}");
                    }

                    if (result1?.Result.Item1 == null || !result1.Result.Item2.isSuccess)
                    {
                        responseSQHeader.errorDetails = result1?.Result.Item2 ?? new errorDetails { isSuccess = false, message = "Failed to update sales quote line item." };
                        return responseSQHeader;
                    }

                    resSQLine = result1.Result.Item1;
                    ed1 = result1.Result.Item2;
                    responseSQHeader.errorDetails = ed1;
                    responseSQHeader.ItemLineNo += $"{resSQLine.No}_{resSQLine.Line_No},";
                }

                // Send approval email on UPDATE as well (same scenario as New Sales Quote).
                // This is required for flows like "Update Price" where approval may become required after price changes.
                // Only send approval emails when called from the Sales Quote Save flow
                // (UI includes the zipped template payload). Item price update flows must not send approval emails.
                var hasApprovalTemplate = salesQuoteDetails?.zipApprovalFormatFile != null
                    && salesQuoteDetails.zipApprovalFormatFile.Length > 0
                    && !string.IsNullOrWhiteSpace(ApprovalFormatFile);

                if (!string.IsNullOrWhiteSpace(approvalFor) && hasApprovalTemplate)
                {
                    string toEmail = "";
                    string ccEmail = LoggedInSPUserEmail;

                    if (approvalFor == "Negative Credit Limit")
                        toEmail = FirstEmailOrEmpty(financeUserDetails.Company_E_Mail);
                    else if (approvalFor == "Negative Margin")
                        toEmail = FirstEmailOrEmpty(reportingPersonDetails.PCPL_Reporting_Person_Email);
                    else if (approvalFor == "Both")
                        // Sequential approvals: first finance, then HOD after finance approval.
                        toEmail = FirstEmailOrEmpty(financeUserDetails.Company_E_Mail);

                    string ApprovalForText = approvalFor == "Both" ? "Negative Credit Limit And Margin" : approvalFor;

                    DateTime quoteDate = Convert.ToDateTime(salesQuoteDetails.OrderDate);
                    DateTime quoteValidUntilDate = Convert.ToDateTime(salesQuoteDetails.ValidUntillDate);
                    QuoteValidityDays = (quoteValidUntilDate - quoteDate).Days;

                    string myString = ApprovalFormatFile;

                    // AprDetailsJustificationReason (shown in UI) maps to Sales Quote WorkDescription.
                    // For the approval mail, take it from the request payload.
                    var aprDetailsJustificationReason = (salesQuoteDetails?.JustificationDetails ?? "").Trim();
                    var justificationSuffix = "";
                    if (!string.IsNullOrWhiteSpace(aprDetailsJustificationReason))
                    {
                        var safeJustification = System.Net.WebUtility.HtmlEncode(aprDetailsJustificationReason)
                            .Replace("\r\n", "<br />")
                            .Replace("\n", "<br />")
                            .Replace("\r", "<br />");
                        justificationSuffix = "<br /><b>Justification : </b>" + safeJustification;
                    }

                    if (approvalFor == "Negative Credit Limit")
                    {
                        myString = myString.Replace("##pageheading##", " CREDIT LIMIT EXCEEDED ");
                        myString = myString.Replace("##heading##", $"The user '{SPName}' was trying to update the quote, but the Credit limit is exceeded.{justificationSuffix}");
                    }
                    else if (approvalFor == "Negative Margin")
                    {
                        myString = myString.Replace("##pageheading##", " MARGIN IS LESS THAN ZERO ");
                        myString = myString.Replace("##heading##", $"The user '{SPName}' was trying to update the quote, but the Margin is less than zero.{justificationSuffix}");
                    }
                    else if (approvalFor == "Both")
                    {
                        myString = myString.Replace("##pageheading##", " CREDIT LIMIT EXCEEDED ");
                        myString = myString.Replace("##heading##", $"The user '{SPName}' was trying to update the quote, but the Credit limit is exceeded and Margin is less than Zero.{justificationSuffix}");
                    }

                    var approvalLinkRole = "";
                    if (approvalFor == "Negative Credit Limit" || approvalFor == "Both")
                        approvalLinkRole = "Finance";
                    else if (approvalFor == "Negative Margin")
                        approvalLinkRole = "HOD";

                    salesQuoteDetails.SQApprovalFormURL += $"?SQNo={responseSQHeader.No}&ScheduleStatus=''&SQStatus={responseSQHeader.PCPL_Status}&SQFor=ApproveReject&LoggedInUserRole={approvalLinkRole}";
                    myString = myString.Replace("##SalesQuoteNo##", responseSQHeader.No);
                    myString = myString.Replace("##SalesQuoteDate##", responseSQHeader.Order_Date ?? "");
                    myString = myString.Replace("##ApprovalForText##", ApprovalForText);

                    string[] CustSellToAddress = (salesQuoteDetails.ConsigneeAddress ?? "").Split('_');

                    string CustName = CustSellToAddress.ElementAtOrDefault(0) ?? "";
                    string line1 = CustSellToAddress.ElementAtOrDefault(1) ?? "";
                    string line2 = CustSellToAddress.ElementAtOrDefault(2) ?? "";
                    string line3 = CustSellToAddress.ElementAtOrDefault(3) ?? "";
                    string pin = CustSellToAddress.ElementAtOrDefault(4) ?? "";

                    string CustSellToAddress_ = $"{CustName}<br />{line1},<br />{line2},<br />{line3}-{pin}";

                    myString = myString.Replace("##CustomerDetail##", CustSellToAddress_);
                    myString = myString.Replace("##ContactName##", salesQuoteDetails.ContactPersonName);

                    // Show ShippingDetails for Finance mail as well.
                    // If Job-to address isn't provided, fall back to the sell-to/consignee address.
                    myString = myString.Replace("##ShippingHeader##", "ShippingDetails");
                    if (!string.IsNullOrWhiteSpace(salesQuoteDetails.JobtoAddress) && salesQuoteDetails.JobtoCode != "-1")
                    {
                        string[] CustJobtoAddress = salesQuoteDetails.JobtoAddress.Split(',');
                        string CustJobtoAddress_ = string.Join(",<br />", CustJobtoAddress);
                        myString = myString.Replace("##ShippingDetail##", CustJobtoAddress_);
                    }
                    else
                    {
                        myString = myString.Replace("##ShippingDetail##", CustSellToAddress_);
                    }

                    string str_lineTable = "<table cellpadding=\"0\" cellspacing=\"1\" border=\"1\" width=\"100%\" align=\"left\" style=\"border:1px solid #BEBEBE\">";
                    str_lineTable += "<tr style=\"background:#ccc;font-weight:bold !important;text-transform:capitalize;font-size:12\">";
                    str_lineTable += "<td width =\"5%\" align=\"left\">SR.NO.</td>";
                    str_lineTable += "<td width =\"10%\" align=\"left\">Product</td>";
                    str_lineTable += "<td width =\"10%\" align=\"left\">Packaging Style</td>";
                    str_lineTable += "<td width =\"5%\" align=\"right\">Qty(MT)</td>";
                    str_lineTable += "<td width =\"5%\" align=\"right\">Sales Price(Rs.)</td>";
                    str_lineTable += "<td width =\"5%\" align=\"right\">Margin</td>";
                    str_lineTable += "<td width =\"5%\" align=\"right\">IncoTerms</td>";
                    str_lineTable += "<td width =\"5%\" align=\"right\">GST</td>";
                    str_lineTable += "</tr>";
                    int counter = 1;

                    var gstPlaceOfSupply = (salesQuoteDetails.ShiptoCode == "-1" && salesQuoteDetails.JobtoCode == "-1")
                        ? "Bill-to Address"
                        : "Ship-to Address";

                    for (int ai = 0; ai < salesQuoteDetails.Products.Count; ai++)
                    {
                        str_lineTable += "<tr style=\"font-size:10\">";
                        str_lineTable += $"<td align=\"left\" valign=\"top\" bgcolor=\"#FFFFFF\" width=\"5%\">{counter}</td>";
                        str_lineTable += $"<td align=\"left\" valign=\"top\" bgcolor=\"#FFFFFF\" width=\"5%\">{salesQuoteDetails.Products[ai].ProductName}</td>";
                        str_lineTable += $"<td align=\"left\" valign=\"top\" bgcolor=\"#FFFFFF\" width=\"5%\">{salesQuoteDetails.Products[ai].PCPL_Packing_Style_Code}</td>";
                        str_lineTable += $"<td align=\"right\" valign=\"top\" bgcolor=\"#FFFFFF\" width=\"5%\">{salesQuoteDetails.Products[ai].Quantity}</td>";
                        str_lineTable += $"<td align=\"right\" valign=\"top\" bgcolor=\"#FFFFFF\" width=\"5%\">{salesQuoteDetails.Products[ai].Unit_Price}</td>";
                        str_lineTable += $"<td align=\"right\" valign=\"top\" bgcolor=\"#FFFFFF\" width=\"5%\">{salesQuoteDetails.Products[ai].PCPL_Margin}</td>";
                        str_lineTable += $"<td align=\"right\" valign=\"top\" bgcolor=\"#FFFFFF\" width=\"5%\">{salesQuoteDetails.ShipmentMethodCode}</td>";
                        str_lineTable += $"<td align=\"right\" valign=\"top\" bgcolor=\"#FFFFFF\" width=\"5%\">{salesQuoteDetails.Products[ai].PCPL_GST_Amt}</td>";
                        str_lineTable += "</tr>";
                        counter++;
                    }

                    str_lineTable += "</table>";
                    myString = myString.Replace("##totalcreditlimit##", salesQuoteDetails.TotalCreditLimit);
                    myString = myString.Replace("##usedcreditlimit##", salesQuoteDetails.UsedCreditLimit);
                    myString = myString.Replace("##availablecreditlimit##", salesQuoteDetails.AvailableCreditLimit);
                    myString = myString.Replace("##custno##", salesQuoteDetails.CustomerNo);
                    myString = myString.Replace("##custname##", salesQuoteDetails.ContactCompanyName);
                    myString = myString.Replace("##QuoteValidityDays##", QuoteValidityDays <= 1 ? $"{QuoteValidityDays} Day" : $"{QuoteValidityDays} Days");
                    myString = myString.Replace("##TaxGroupDetails##", "GST: As Applicable");
                    var paymentTerms = (responseSQHeader?.Payment_Terms_Code ?? salesQuoteDetails?.PaymentTermsCode ?? "").Trim();
                    myString = myString.Replace("##PaymentTermsHeader##", string.IsNullOrWhiteSpace(paymentTerms) ? "" : "Payment Terms : ");
                    myString = myString.Replace("##PaymentTerms##", paymentTerms);

                    var scheduleStatus = (responseSQHeader?.TPTPL_Schedule_status ?? "").Trim();
                    myString = myString.Replace("##ScheduleHeader##", string.IsNullOrWhiteSpace(scheduleStatus) ? "" : "Schedule : ");
                    myString = myString.Replace("##Schedule##", scheduleStatus);

                    myString = myString.Replace("##Note##", "Price ruling at the time of delivery for GACL & GNFC products.");
                    myString = myString.Replace("##LineDetails##", str_lineTable);
                    myString = myString.Replace("##SalesQuoteApprovalFormURL##", salesQuoteDetails.SQApprovalFormURL);

                    // Footer block (contact + warehouse).
                    var (spContactName, spContactEmail, spContactPhone) = TryGetSalespersonContact(ac, salesQuoteDetails?.SalespersonCode ?? "");
                    if (string.IsNullOrWhiteSpace(spContactName)) spContactName = (SPName ?? "").Trim();
                    if (string.IsNullOrWhiteSpace(spContactEmail)) spContactEmail = FirstEmailOrEmpty(LoggedInSPUserEmail ?? "");

                    var locationCodeForFooter = (responseSQHeader?.Location_Code ?? salesQuoteDetails?.LocationCode ?? "").Trim();
                    var locFooter = TryGetLocation(ac, locationCodeForFooter);
                    var warehouseManagerPhone = (locFooter?.Phone_No ?? "").Trim();

                    var warehouseAddressOverride = (ConfigurationManager.AppSettings["QuoteEmailWarehouseAddress_" + locationCodeForFooter] ?? ConfigurationManager.AppSettings["QuoteEmailWarehouseAddress"] ?? "").Trim();
                    var warehouseAddressHtml = !string.IsNullOrWhiteSpace(warehouseAddressOverride)
                        ? HtmlEncodeInline(warehouseAddressOverride)
                        : BuildWarehouseAddressHtml(ac, locationCodeForFooter);

                    var ci = TryGetCompanyInformation(ac);
                    var companyName = (ci?.Name ?? "").Trim();
                    var bankName = (ci?.Bank_Name ?? "").Trim();
                    var bankAccountNo = (ci?.Bank_Account_No ?? "").Trim();
                    var ifsc = (ci?.IFSC ?? "").Trim();

                    var dispatchTiming = (ConfigurationManager.AppSettings["QuoteEmailDispatchTiming_" + locationCodeForFooter] ?? ConfigurationManager.AppSettings["QuoteEmailDispatchTiming"] ?? "").Trim();
                    myString = myString.Replace("##FooterDetails##", BuildFooterDetailsHtml(spContactName, spContactPhone, spContactEmail, warehouseAddressHtml, warehouseManagerPhone, dispatchTiming, companyName, bankName, bankAccountNo, ifsc));

                    string emailSubject = $"Sales Quote Approval - {responseSQHeader.No} - {CustomerName} - {DateTime.Now:dd/MM/yyyy} - {ApprovalForText}";

                    EmailService emailService = new EmailService();
                    StringBuilder sbMailBody = new StringBuilder();
                    sbMailBody.Append(myString);

                    try
                    {
                        emailService.SendEmailWithHTMLBody(toEmail, ccEmail, "", emailSubject, sbMailBody.ToString());
                    }
                    catch (Exception ex)
                    {
                        responseSQHeader.errorDetails = new errorDetails { isSuccess = false, message = $"Failed to send email: {ex.Message}" };
                        return responseSQHeader;
                    }
                }

                return responseSQHeader;
            }
        }

        [Route("GetSQListDataForApproveReject")]
        public List<SPSQListForApproveReject> GetSQListDataForApproveReject(string LoggedInUserNo, string UserRoleORReportingPerson, int skip, int top, string orderby, string filter)
        {
            API ac = new API();
            List<SPSQListForApproveReject> sqList = new List<SPSQListForApproveReject>();

            if (filter == "" || filter == null)
            {
                if (UserRoleORReportingPerson == "Finance")
                    filter = "PCPL_Approver eq '" + LoggedInUserNo + "' and PCPL_IsInquiry eq false and PCPL_Status eq 'Approval pending from finance'";
                else if (UserRoleORReportingPerson == "ReportingPerson")
                    filter = "PCPL_IsInquiry eq false and PCPL_ApproverHOD eq '" + LoggedInUserNo + "' and PCPL_Status eq 'Approval pending from HOD'";
            }
            else
            {
                if (UserRoleORReportingPerson == "Finance")
                    filter = filter + " and PCPL_Approver eq '" + LoggedInUserNo + "' and PCPL_IsInquiry eq false and PCPL_Status eq 'Approval pending from finance'";
                else if (UserRoleORReportingPerson == "ReportingPerson")
                    filter = filter + " and PCPL_IsInquiry eq false and PCPL_ApproverHOD eq '" + LoggedInUserNo + "' and PCPL_Status eq 'Approval pending from HOD'";
            }

            //if (filter == "" || filter == null)
            //    filter = "PCPL_Approver eq '" + LoggedInUserNo + "' and PCPL_IsInquiry eq false"; // and PCPL_Status ne '0'
            //else
            //    filter = filter + " and PCPL_Approver eq '" + LoggedInUserNo + "' and PCPL_IsInquiry eq false"; // and PCPL_Status ne '0'

            var result = ac.GetData1<SPSQListForApproveReject>("SalesQuoteDotNetAPI", filter, skip, top, orderby); // and Contact_Business_Relation eq 'Customer'

            if (result.Result.Item1.value.Count > 0)
                sqList = result.Result.Item1.value;

            for (int i = 0; i < sqList.Count; i++)
            {
                string[] strDate = sqList[i].Order_Date.Split('-');
                sqList[i].Order_Date = strDate[2] + '-' + strDate[1] + '-' + strDate[0];

                //string[] strDate1 = salesquotes[i].Requested_Delivery_Date.Split('-');
                //salesquotes[i].Requested_Delivery_Date = strDate1[2] + '-' + strDate1[1] + '-' + strDate1[0];
            }

            return sqList;
        }

        [Route("SQApproveReject")]
        public string SQApproveReject(string SQNosAndApprovalFor, string LoggedInUserNo, string Action, string UserRoleORReportingPerson, string RejectRemarks, string LoggedInUserEmail)
        {
            string resMsg = "";
            API ac = new API();

            //SPBusinessPlanCustWiseForApprove businessPlanCustWiseForApprove = new SPBusinessPlanCustWiseForApprove();
            //SPBusinessPlanCustWiseForReject businessPlanCustWiseForReject = new SPBusinessPlanCustWiseForReject();
            //SPBusinessPlanDetails businessPlanDetails = new SPBusinessPlanDetails();
            errorDetails ed = new errorDetails();
            var result = (dynamic)null;
            SQNosAndApprovalFor = SQNosAndApprovalFor.Substring(0, SQNosAndApprovalFor.Length - 1);
            string[] SQNosAndApprovalForDetails_ = SQNosAndApprovalFor.Split(',');
            string SQNos = "";

            for (int a = 0; a < SQNosAndApprovalForDetails_.Length; a++)
            {
                SPSQForApprove sqForApprove = new SPSQForApprove();
                SPSQForReject sqForReject = new SPSQForReject();
                SPSQForApproveHOD sqForApproveHOD = new SPSQForApproveHOD();
                SPSQForRejectHOD sqForRejectHOD = new SPSQForRejectHOD();
                SPSQHeader sqHeader = new SPSQHeader();
                string[] SQNosAndApprovalFor_ = SQNosAndApprovalForDetails_[a].Split(':');
                string SQNo = SQNosAndApprovalFor_[0];
                string ApprovalFor = SQNosAndApprovalFor_[1];
                string SPEmail = SQNosAndApprovalFor_[2];

                if (UserRoleORReportingPerson == "Finance")
                {
                    if (Action == "Approve" && ApprovalFor == "Credit Limit")
                    {
                        //sqForApprove.PCPL_Approver = LoggedInUserNo;
                        sqForApprove.PCPL_Approved_By_Rejected_By = LoggedInUserNo;
                        sqForApprove.PCPL_Status = "Approved";
                        sqForApprove.PCPL_Approved_Rejected_On = DateTime.Now.ToString("yyyy-MM-dd");

                        result = PatchItemSQApproveReject("SalesQuoteDotNetAPI", sqForApprove, sqForReject, sqHeader, "Approve", "Document_Type='Quote',No='" + SQNo + "'");
                    }
                    else if (Action == "Approve" && ApprovalFor == "Both")
                    {
                        //sqForApprove.PCPL_Approver = LoggedInUserNo;
                        sqForApprove.PCPL_Approved_By_Rejected_By = LoggedInUserNo;
                        sqForApprove.PCPL_Status = "Approval pending from HOD";
                        sqForApprove.PCPL_Approved_Rejected_On = DateTime.Now.ToString("yyyy-MM-dd");

                        result = PatchItemSQApproveReject("SalesQuoteDotNetAPI", sqForApprove, sqForReject, sqHeader, "Approve", "Document_Type='Quote',No='" + SQNo + "'");
                    }
                    else if (Action == "Reject")
                    {
                        //sqForReject.PCPL_Approver = LoggedInUserNo;
                        sqForReject.PCPL_Approved_By_Rejected_By = LoggedInUserNo;
                        sqForReject.PCPL_Status = "Rejected by finance";
                        sqForReject.PCPL_Rejected_Reason = RejectRemarks;
                        sqForReject.PCPL_Approved_Rejected_On = DateTime.Now.ToString("yyyy-MM-dd");

                        result = PatchItemSQApproveReject("SalesQuoteDotNetAPI", sqForApprove, sqForReject, sqHeader, "Reject", "Document_Type='Quote',No='" + SQNo + "'");
                    }
                }
                else if (UserRoleORReportingPerson == "ReportingPerson")
                {
                    if (Action == "Approve")
                    {
                        //sqForApprove.PCPL_Approver = LoggedInUserNo;
                        sqForApproveHOD.PCPL_ApprovedBy_RejectedBy_HOD = LoggedInUserNo;
                        sqForApproveHOD.PCPL_Status = "Approved";
                        sqForApproveHOD.PCPL_Approved_Rejected_On_HOD = DateTime.Now.ToString("yyyy-MM-dd");

                        result = PatchItemSQApproveRejectHOD("SalesQuoteDotNetAPI", sqForApproveHOD, sqForRejectHOD, sqHeader, "Approve", "Document_Type='Quote',No='" + SQNo + "'");
                    }
                    else if (Action == "Reject")
                    {
                        //sqForReject.PCPL_Approver = LoggedInUserNo;
                        sqForRejectHOD.PCPL_ApprovedBy_RejectedBy_HOD = LoggedInUserNo;
                        sqForRejectHOD.PCPL_Status = "Rejected by HOD";
                        sqForRejectHOD.PCPL_Rejected_Reason_HOD = RejectRemarks;
                        sqForRejectHOD.PCPL_Approved_Rejected_On_HOD = DateTime.Now.ToString("yyyy-MM-dd");

                        result = PatchItemSQApproveRejectHOD("SalesQuoteDotNetAPI", sqForApproveHOD, sqForRejectHOD, sqHeader, "Reject", "Document_Type='Quote',No='" + SQNo + "'");
                    }
                }

                if (result.Result.Item1.No != null)
                {
                    resMsg = "True";
                    sqHeader = result.Result.Item1;
                    ed = result.Result.Item2;
                    sqHeader.errorDetails = ed;
                    SQNos = SQNo + ",";
                }

                if (!sqHeader.errorDetails.isSuccess)
                    resMsg = "Error:" + sqHeader.errorDetails.message;

                if (!string.IsNullOrWhiteSpace(SPEmail))
                {
                    var template = LoadApprovalEmailTemplate();
                    var portalBase = (System.Configuration.ConfigurationManager.AppSettings["SPPortalUrl"]?.ToString() ?? "").Trim();
                    if (!string.IsNullOrWhiteSpace(portalBase))
                        portalBase = portalBase.TrimEnd('/') + "/";

                    var logoUrl = !string.IsNullOrWhiteSpace(portalBase) ? (portalBase + "Files/logo-3.jfif") : "";
                    var statusLink = !string.IsNullOrWhiteSpace(portalBase) ? (portalBase + "SPSalesQuotes/SalesQuote") : "";

                    var approvalForText = ApprovalFor == "Both" ? "Negative Credit Limit And Margin" : (ApprovalFor ?? "");
                    var statusJustification = (sqHeader?.WorkDescription ?? "").Trim();
                    var justificationSuffix = "";
                    if (!string.IsNullOrWhiteSpace(statusJustification))
                    {
                        var safeJustification = System.Net.WebUtility.HtmlEncode(statusJustification)
                            .Replace("\r\n", "<br />")
                            .Replace("\n", "<br />")
                            .Replace("\r", "<br />");
                        justificationSuffix = "<br /><b>Justification : </b>" + safeJustification;
                    }

                    var rejectionSuffix = "";
                    if (Action == "Reject" && !string.IsNullOrWhiteSpace(RejectRemarks))
                    {
                        var safeReject = System.Net.WebUtility.HtmlEncode(RejectRemarks)
                            .Replace("\r\n", "<br />")
                            .Replace("\n", "<br />")
                            .Replace("\r", "<br />");
                        rejectionSuffix = "<br /><b>Reject Reason : </b>" + safeReject;
                    }

                    var customerName = (sqHeader?.Sell_to_Customer_Name ?? "").Trim();
                    var customerAddress = (sqHeader?.Sell_to_Address ?? "").Trim();
                    var customerCity = (sqHeader?.Sell_to_City ?? "").Trim();
                    var customerPin = (sqHeader?.Sell_to_Post_Code ?? "").Trim();
                    var customerDetailHtml = customerName;
                    if (!string.IsNullOrWhiteSpace(customerAddress))
                        customerDetailHtml += "<br />" + customerAddress;
                    if (!string.IsNullOrWhiteSpace(customerCity) || !string.IsNullOrWhiteSpace(customerPin))
                        customerDetailHtml += "<br />" + customerCity + (!string.IsNullOrWhiteSpace(customerPin) ? ("-" + customerPin) : "");

                    var contactName = (sqHeader?.PCPL_Contact_Person_Name ?? "").Trim();
                    if (string.IsNullOrWhiteSpace(contactName))
                        contactName = (sqHeader?.Sell_to_Contact ?? "").Trim();

                    // ShippingDetails flow: prefer Job-to; else Ship-to; else Sell-to.
                    var shippingHeader = "ShippingDetails";
                    var shippingDetailHtml = customerDetailHtml;
                    try
                    {
                        var sellToCustomerNo = (sqHeader?.Sell_to_Customer_No ?? "").Trim();
                        var jobToCode = (sqHeader?.PCPL_Job_to_Code ?? "").Trim();
                        var shipToCode = (sqHeader?.Ship_to_Code ?? "").Trim();

                        if (!string.IsNullOrWhiteSpace(sellToCustomerNo))
                        {
                            if (!string.IsNullOrWhiteSpace(jobToCode) && jobToCode != "-1")
                            {
                                var requestJobtoAddress = new SPSQJobtoAddress { customerno = sellToCustomerNo };
                                var responseJobtoAddress = new List<SPSQJobtoAddressRes>();
                                var resultGetJobToAddress = PostItemForSQGetJobtoAddress<SPSQJobtoAddressRes>("", requestJobtoAddress, responseJobtoAddress);
                                var jobToAddresses = resultGetJobToAddress.Result.Item1;
                                var jobToMatch = jobToAddresses?.FirstOrDefault(addrItem => string.Equals((addrItem?.Code ?? "").Trim(), jobToCode, StringComparison.OrdinalIgnoreCase));
                                var addr = (jobToMatch?.Address ?? "").Trim();
                                if (!string.IsNullOrWhiteSpace(addr))
                                    shippingDetailHtml = customerName + "<br />" + string.Join(",<br />", addr.Split(','));
                            }
                            else if (!string.IsNullOrWhiteSpace(shipToCode) && shipToCode != "-1")
                            {
                                var requestShiptoAddress = new SPSQShiptoAddress { customerno = sellToCustomerNo };
                                var responseShiptoAddress = new List<SPSQShiptoAddressRes>();
                                var resultGetShipToAddress = PostItemForSQGetShiptoAddress<SPSQShiptoAddressRes>("", requestShiptoAddress, responseShiptoAddress);
                                var shipToAddresses = resultGetShipToAddress.Result.Item1;
                                var shipToMatch = shipToAddresses?.FirstOrDefault(addrItem => string.Equals((addrItem?.Code ?? "").Trim(), shipToCode, StringComparison.OrdinalIgnoreCase));
                                var addr = (shipToMatch?.Address ?? "").Trim();
                                if (!string.IsNullOrWhiteSpace(addr))
                                    shippingDetailHtml = customerName + "<br />" + string.Join(",<br />", addr.Split(','));
                            }
                        }
                    }
                    catch
                    {
                    }

                    // Build line items table (with GST column).
                    var lineItems = GetSalesLineItems(SQNo);
                    var lineDetailsHtml = "";
                    if (lineItems != null && lineItems.Count > 0)
                    {
                        var shipmentMethodCode = sqHeader?.Shipment_Method_Code ?? "";
                        var table = new StringBuilder();
                        table.Append("<table cellpadding=\"0\" cellspacing=\"1\" border=\"1\" width=\"100%\" align=\"left\" style=\"border:1px solid #BEBEBE\">");
                        table.Append("<tr style=\"background:#ccc;font-weight:bold !important;text-transform:capitalize;font-size:12\">");
                        table.Append("<td width=\"5%\" align=\"left\">SR.NO.</td>");
                        table.Append("<td width=\"10%\" align=\"left\">Product</td>");
                        table.Append("<td width=\"10%\" align=\"left\">Packaging Style</td>");
                        table.Append("<td width=\"5%\" align=\"right\">Qty(MT)</td>");
                        table.Append("<td width=\"5%\" align=\"right\">Sales Price(Rs.)</td>");
                        table.Append("<td width=\"5%\" align=\"right\">Margin</td>");
                        table.Append("<td width=\"5%\" align=\"right\">IncoTerms</td>");
                        table.Append("<td width=\"5%\" align=\"right\">GST</td>");
                        table.Append("</tr>");

                        var rowCounter = 1;
                        foreach (var li in lineItems)
                        {
                            table.Append("<tr style=\"font-size:10\">");
                            table.Append($"<td align=\"left\" valign=\"top\" bgcolor=\"#FFFFFF\" width=\"5%\">{rowCounter}</td>");
                            var prodName = !string.IsNullOrWhiteSpace(li?.Description) ? li.Description : (li?.No ?? "");
                            table.Append($"<td align=\"left\" valign=\"top\" bgcolor=\"#FFFFFF\" width=\"5%\">{prodName}</td>");
                            table.Append($"<td align=\"left\" valign=\"top\" bgcolor=\"#FFFFFF\" width=\"5%\">{(li?.PCPL_Packing_Style_Code ?? "")}</td>");
                            table.Append($"<td align=\"right\" valign=\"top\" bgcolor=\"#FFFFFF\" width=\"5%\">{(li != null ? li.Quantity.ToString() : "")}</td>");
                            table.Append($"<td align=\"right\" valign=\"top\" bgcolor=\"#FFFFFF\" width=\"5%\">{(li != null ? li.Unit_Price.ToString() : "")}</td>");
                            table.Append($"<td align=\"right\" valign=\"top\" bgcolor=\"#FFFFFF\" width=\"5%\">{(li != null ? li.PCPL_Margin.ToString() : "")}</td>");
                            table.Append($"<td align=\"right\" valign=\"top\" bgcolor=\"#FFFFFF\" width=\"5%\">{shipmentMethodCode}</td>");

                            var gstPlaceOfSupplyForLine = (li?.GST_Place_Of_Supply ?? "").Trim();
                            if (string.IsNullOrWhiteSpace(gstPlaceOfSupplyForLine))
                            {
                                var shipToCode = (sqHeader?.Ship_to_Code ?? "").Trim();
                                var jobToCode = (sqHeader?.PCPL_Job_to_Code ?? "").Trim();
                                var hasShipOrJob = (!string.IsNullOrWhiteSpace(shipToCode) && shipToCode != "-1")
                                    || (!string.IsNullOrWhiteSpace(jobToCode) && jobToCode != "-1");
                                gstPlaceOfSupplyForLine = hasShipOrJob ? "Ship-to Address" : "Bill-to Address";
                            }
                            table.Append($"<td align=\"right\" valign=\"top\" bgcolor=\"#FFFFFF\" width=\"5%\">{gstPlaceOfSupplyForLine}</td>");
                            table.Append("</tr>");
                            rowCounter++;
                        }

                        table.Append("</table>");
                        lineDetailsHtml = table.ToString();
                    }

                    var totalCreditLimit = (sqHeader?.Customer_Credit_Limit_LCY ?? "").Trim();
                    var usedCreditLimit = (sqHeader?.Customer_Balance_Due_LCY ?? "").Trim();
                    if (string.IsNullOrWhiteSpace(usedCreditLimit))
                        usedCreditLimit = (sqHeader?.Customer_Balance_LCY ?? "").Trim();

                    var quoteValidityDaysText = "";
                    if (DateTime.TryParse(sqHeader?.Order_Date, out var od) && DateTime.TryParse(sqHeader?.Quote_Valid_Until_Date, out var vd))
                    {
                        var days = (vd - od).Days;
                        quoteValidityDaysText = days <= 1 ? $"{days} Day" : $"{days} Days";
                    }

                    var statusTitle = Action == "Approve" ? "APPROVED" : "REJECTED";
                    var statusBy = UserRoleORReportingPerson == "Finance" ? "Finance" : "HOD";
                    var headingText = $"Sales Quote {SQNo} {statusTitle} By {statusBy}.{justificationSuffix}{rejectionSuffix}";

                    string mailBody;
                    if (!string.IsNullOrWhiteSpace(template))
                    {
                        mailBody = template;

                        // For reject status mails, remove the "Approval For Quote - ..." header row from the template.
                        if (string.Equals((Action ?? "").Trim(), "Reject", StringComparison.OrdinalIgnoreCase))
                        {
                            mailBody = System.Text.RegularExpressions.Regex.Replace(
                                mailBody,
                                "(?is)<tr[^>]*>\\s*<td[^>]*colspan=\"4\"[^>]*>\\s*<h3[^>]*>\\s*Approval\\s+For\\s+Quote\\s*-\\s*##ApprovalForText##\\s*</h3>\\s*</td>\\s*</tr>",
                                "");
                        }

                        mailBody = mailBody.Replace("##pageheading##", $" SALES QUOTE {statusTitle} ");
                        mailBody = mailBody.Replace("##heading##", headingText);
                        mailBody = mailBody.Replace("##ApprovalForText##", approvalForText);
                        mailBody = mailBody.Replace("##SalesQuoteNo##", SQNo ?? "");
                        mailBody = mailBody.Replace("##SalesQuoteDate##", sqHeader?.Order_Date ?? "");
                        mailBody = mailBody.Replace("##SalesQuoteApprovalFormURL##", statusLink);
                        mailBody = mailBody.Replace("{ApprovalFileFormatURL}", logoUrl);

                        mailBody = mailBody.Replace("##CustomerDetail##", customerDetailHtml);
                        mailBody = mailBody.Replace("##ContactName##", contactName);
                        mailBody = mailBody.Replace("##ShippingHeader##", shippingHeader);
                        mailBody = mailBody.Replace("##ShippingDetail##", shippingDetailHtml);
                        mailBody = mailBody.Replace("##LineDetails##", lineDetailsHtml);
                        mailBody = mailBody.Replace("##totalcreditlimit##", totalCreditLimit);
                        mailBody = mailBody.Replace("##usedcreditlimit##", usedCreditLimit);
                        mailBody = mailBody.Replace("##availablecreditlimit##", "");
                        mailBody = mailBody.Replace("##PaymentTermsHeader##", string.IsNullOrWhiteSpace(sqHeader?.Payment_Terms_Code) ? "" : "Payment Terms : ");
                        mailBody = mailBody.Replace("##PaymentTerms##", (sqHeader?.Payment_Terms_Code ?? ""));
                        mailBody = mailBody.Replace("##QuoteValidityDays##", quoteValidityDaysText);
                        var scheduleStatus = (sqHeader?.TPTPL_Schedule_status ?? "").Trim();
                        mailBody = mailBody.Replace("##ScheduleHeader##", string.IsNullOrWhiteSpace(scheduleStatus) ? "" : "Schedule : ");
                        mailBody = mailBody.Replace("##Schedule##", scheduleStatus);

                        mailBody = mailBody.Replace("##TaxGroupDetails##", "GST: As Applicable");
                        mailBody = mailBody.Replace("##Note##", "Price ruling at the time of delivery for GACL & GNFC products.");

                        // Footer block (contact + warehouse).
                        var (spContactName, spContactEmail, spContactPhone) = TryGetSalespersonContact(ac, sqHeader?.Salesperson_Code ?? "");
                        if (string.IsNullOrWhiteSpace(spContactEmail)) spContactEmail = FirstEmailOrEmpty(SPEmail ?? "");
                        if (string.IsNullOrWhiteSpace(spContactEmail)) spContactEmail = FirstEmailOrEmpty(LoggedInUserEmail ?? "");
                        var locationCodeForFooter = (sqHeader?.Location_Code ?? "").Trim();
                        var loc = TryGetLocation(ac, locationCodeForFooter);
                        var warehouseManagerPhone = (loc?.Phone_No ?? "").Trim();

                        var warehouseAddressOverride = (ConfigurationManager.AppSettings["QuoteEmailWarehouseAddress_" + locationCodeForFooter] ?? ConfigurationManager.AppSettings["QuoteEmailWarehouseAddress"] ?? "").Trim();
                        var warehouseAddressHtml = !string.IsNullOrWhiteSpace(warehouseAddressOverride)
                            ? HtmlEncodeInline(warehouseAddressOverride)
                            : BuildWarehouseAddressHtml(ac, locationCodeForFooter);

                        var ci = TryGetCompanyInformation(ac);
                        var companyName = (ci?.Name ?? "").Trim();
                        var bankName = (ci?.Bank_Name ?? "").Trim();
                        var bankAccountNo = (ci?.Bank_Account_No ?? "").Trim();
                        var ifsc = (ci?.IFSC ?? "").Trim();

                        var dispatchTiming = (ConfigurationManager.AppSettings["QuoteEmailDispatchTiming_" + locationCodeForFooter] ?? ConfigurationManager.AppSettings["QuoteEmailDispatchTiming"] ?? "").Trim();
                        mailBody = mailBody.Replace("##FooterDetails##", BuildFooterDetailsHtml(spContactName, spContactPhone, spContactEmail, warehouseAddressHtml, warehouseManagerPhone, dispatchTiming, companyName, bankName, bankAccountNo, ifsc));

                        mailBody = System.Text.RegularExpressions.Regex.Replace(mailBody, "##[^#]+##", "");
                    }
                    else
                    {
                        var fallbackBody = new StringBuilder();
                        fallbackBody.Append("<p>Hi,</p>");
                        fallbackBody.Append($"<p>{headingText}</p>");
                        if (!string.IsNullOrWhiteSpace(statusLink))
                            fallbackBody.Append("<p><a href='" + statusLink + "'>Open Sales Quote</a></p>");
                        mailBody = fallbackBody.ToString();
                    }

                    var subject = $"Sales Quote {SQNo} {statusTitle} - {approvalForText}";
                    var emailService = new EmailService();
                    emailService.SendEmailWithHTMLBody(SPEmail, LoggedInUserEmail, "", subject, mailBody);
                }

                // If finance approved a "Both" case, trigger the next approval email to HOD.
                // IMPORTANT: Do not send anything to HOD when Finance rejects.
                if (string.Equals((UserRoleORReportingPerson ?? "").Trim(), "Finance", StringComparison.OrdinalIgnoreCase)
                    && string.Equals((Action ?? "").Trim(), "Approve", StringComparison.OrdinalIgnoreCase)
                    && string.Equals((ApprovalFor ?? "").Trim(), "Both", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        // Fetch reporting person email based on salesperson code.
                        var salesPersonCode = sqHeader?.Salesperson_Code ?? "";
                        if (!string.IsNullOrWhiteSpace(salesPersonCode))
                        {
                            SPUserReportingPersonDetails reportingPersonDetails = new SPUserReportingPersonDetails();
                            var rpRes = ac.GetData<SPUserReportingPersonDetails>("EmployeesDotNetAPI", "Salespers_Purch_Code eq '" + salesPersonCode + "'");
                            if (rpRes?.Result.Item1.value.Count > 0)
                                reportingPersonDetails = rpRes.Result.Item1.value[0];

                            var hodEmail = FirstEmailOrEmpty(reportingPersonDetails?.PCPL_Reporting_Person_Email ?? "");
                            if (string.IsNullOrWhiteSpace(hodEmail))
                            {
                                // Fallback: resolve reporting person email by employee no.
                                var rpNo = (reportingPersonDetails?.Reporting_Person_No ?? "").Trim();
                                if (!string.IsNullOrWhiteSpace(rpNo))
                                {
                                    var rpEmpRes = ac.GetData<UserInfo>("EmployeesDotNetAPI", "No eq '" + rpNo.Replace("'", "''") + "'");
                                    var rpEmp = rpEmpRes?.Result.Item1?.value?.FirstOrDefault();
                                    hodEmail = FirstEmailOrEmpty(rpEmp?.Company_E_Mail ?? "");
                                }
                            }
                            if (!string.IsNullOrWhiteSpace(hodEmail))
                            {
                                var portalBase = (System.Configuration.ConfigurationManager.AppSettings["SPPortalUrl"]?.ToString() ?? "").Trim();
                                if (!string.IsNullOrWhiteSpace(portalBase) && !portalBase.EndsWith("/"))
                                    portalBase += "/";

                                var approvalUrl = !string.IsNullOrWhiteSpace(portalBase)
                                    ? (portalBase + "SPSalesQuotes/SalesQuote" + "?SQNo=" + SQNo + "&ScheduleStatus=''" + "&SQStatus=" + Uri.EscapeDataString(sqHeader?.PCPL_Status ?? "Approval pending from HOD") + "&SQFor=ApproveReject" + "&LoggedInUserRole=HOD")
                                    : "";

                                var subject = $"Sales Quote Approval - {SQNo} - {(sqHeader?.Sell_to_Customer_Name ?? "")} - {DateTime.Now:dd/MM/yyyy} - Negative Margin";
                                var template = LoadApprovalEmailTemplate();

                                string htmlBody;
                                if (!string.IsNullOrWhiteSpace(template))
                                {
                                    htmlBody = template;
                                    var portalFileBase = (System.Configuration.ConfigurationManager.AppSettings["SPPortalUrl"]?.ToString() ?? "").Trim();
                                    if (!string.IsNullOrWhiteSpace(portalFileBase))
                                    {
                                        portalFileBase = portalFileBase.TrimEnd('/') + "/";
                                    }

                                    var logoUrl = !string.IsNullOrWhiteSpace(portalFileBase) ? (portalFileBase + "Files/logo-3.jfif") : "";

                                    var customerName = (sqHeader?.Sell_to_Customer_Name ?? "").Trim();
                                    var customerAddress = (sqHeader?.Sell_to_Address ?? "").Trim();
                                    var customerCity = (sqHeader?.Sell_to_City ?? "").Trim();
                                    var customerPin = (sqHeader?.Sell_to_Post_Code ?? "").Trim();
                                    var customerDetailHtml = customerName;
                                    if (!string.IsNullOrWhiteSpace(customerAddress))
                                        customerDetailHtml += "<br />" + customerAddress;
                                    if (!string.IsNullOrWhiteSpace(customerCity) || !string.IsNullOrWhiteSpace(customerPin))
                                        customerDetailHtml += "<br />" + customerCity + (!string.IsNullOrWhiteSpace(customerPin) ? ("-" + customerPin) : "");

                                    var contactName = (sqHeader?.PCPL_Contact_Person_Name ?? "").Trim();
                                    if (string.IsNullOrWhiteSpace(contactName))
                                        contactName = (sqHeader?.Sell_to_Contact ?? "").Trim();

                                    // Keep ShippingDetails flow consistent with Finance mail:
                                    // prefer Job-to address; if not available, fall back to Sell-to.
                                    var shippingHeader = "ShippingDetails";
                                    var shippingDetailHtml = customerDetailHtml;

                                    try
                                    {
                                        var sellToCustomerNo = (sqHeader?.Sell_to_Customer_No ?? "").Trim();
                                        var jobToCode = (sqHeader?.PCPL_Job_to_Code ?? "").Trim();
                                        var shipToCode = (sqHeader?.Ship_to_Code ?? "").Trim();

                                        if (!string.IsNullOrWhiteSpace(sellToCustomerNo))
                                        {
                                            // Job-to (preferred)
                                            if (!string.IsNullOrWhiteSpace(jobToCode) && jobToCode != "-1")
                                            {
                                                var requestJobtoAddress = new SPSQJobtoAddress { customerno = sellToCustomerNo };
                                                var responseJobtoAddress = new List<SPSQJobtoAddressRes>();
                                                var resultGetJobToAddress = PostItemForSQGetJobtoAddress<SPSQJobtoAddressRes>("", requestJobtoAddress, responseJobtoAddress);

                                                var jobToAddresses = resultGetJobToAddress.Result.Item1;
                                                var jobToMatch = jobToAddresses?.FirstOrDefault(addrItem => string.Equals((addrItem?.Code ?? "").Trim(), jobToCode, StringComparison.OrdinalIgnoreCase));
                                                var addr = (jobToMatch?.Address ?? "").Trim();
                                                if (!string.IsNullOrWhiteSpace(addr))
                                                {
                                                    shippingDetailHtml = customerName;
                                                    shippingDetailHtml += "<br />" + string.Join(",<br />", addr.Split(','));
                                                }
                                            }
                                            // Ship-to (fallback when Job-to is not present)
                                            else if (!string.IsNullOrWhiteSpace(shipToCode) && shipToCode != "-1")
                                            {
                                                var requestShiptoAddress = new SPSQShiptoAddress { customerno = sellToCustomerNo };
                                                var responseShiptoAddress = new List<SPSQShiptoAddressRes>();
                                                var resultGetShipToAddress = PostItemForSQGetShiptoAddress<SPSQShiptoAddressRes>("", requestShiptoAddress, responseShiptoAddress);

                                                var shipToAddresses = resultGetShipToAddress.Result.Item1;
                                                var shipToMatch = shipToAddresses?.FirstOrDefault(addrItem => string.Equals((addrItem?.Code ?? "").Trim(), shipToCode, StringComparison.OrdinalIgnoreCase));
                                                var addr = (shipToMatch?.Address ?? "").Trim();
                                                if (!string.IsNullOrWhiteSpace(addr))
                                                {
                                                    shippingDetailHtml = customerName;
                                                    shippingDetailHtml += "<br />" + string.Join(",<br />", addr.Split(','));
                                                }
                                            }
                                        }
                                    }
                                    catch
                                    {
                                        // Keep best-effort fallback to Sell-to address.
                                    }

                                    var lineItems = GetSalesLineItems(SQNo);
                                    string lineDetailsHtml = "";
                                    if (lineItems != null && lineItems.Count > 0)
                                    {
                                        var shipmentMethodCode = sqHeader?.Shipment_Method_Code ?? "";
                                        var table = new StringBuilder();
                                        table.Append("<table cellpadding=\"0\" cellspacing=\"1\" border=\"1\" width=\"100%\" align=\"left\" style=\"border:1px solid #BEBEBE\">");
                                        table.Append("<tr style=\"background:#ccc;font-weight:bold !important;text-transform:capitalize;font-size:12\">");
                                        table.Append("<td width=\"5%\" align=\"left\">SR.NO.</td>");
                                        table.Append("<td width=\"10%\" align=\"left\">Product</td>");
                                        table.Append("<td width=\"10%\" align=\"left\">Packaging Style</td>");
                                        table.Append("<td width=\"5%\" align=\"right\">Qty(MT)</td>");
                                        table.Append("<td width=\"5%\" align=\"right\">Sales Price(Rs.)</td>");
                                        table.Append("<td width=\"5%\" align=\"right\">Margin</td>");
                                        table.Append("<td width=\"5%\" align=\"right\">IncoTerms</td>");
                                        table.Append("<td width=\"5%\" align=\"right\">GST</td>");
                                        table.Append("</tr>");

                                        var rowCounter = 1;
                                        foreach (var li in lineItems)
                                        {
                                            table.Append("<tr style=\"font-size:10\">");
                                            table.Append($"<td align=\"left\" valign=\"top\" bgcolor=\"#FFFFFF\" width=\"5%\">{rowCounter}</td>");
                                            var prodName = !string.IsNullOrWhiteSpace(li?.Description) ? li.Description : (li?.No ?? "");
                                            table.Append($"<td align=\"left\" valign=\"top\" bgcolor=\"#FFFFFF\" width=\"5%\">{prodName}</td>");
                                            table.Append($"<td align=\"left\" valign=\"top\" bgcolor=\"#FFFFFF\" width=\"5%\">{(li?.PCPL_Packing_Style_Code ?? "")}</td>");
                                            table.Append($"<td align=\"right\" valign=\"top\" bgcolor=\"#FFFFFF\" width=\"5%\">{(li != null ? li.Quantity.ToString() : "")}</td>");
                                            table.Append($"<td align=\"right\" valign=\"top\" bgcolor=\"#FFFFFF\" width=\"5%\">{(li != null ? li.Unit_Price.ToString() : "")}</td>");
                                            table.Append($"<td align=\"right\" valign=\"top\" bgcolor=\"#FFFFFF\" width=\"5%\">{(li != null ? li.PCPL_Margin.ToString() : "")}</td>");
                                            table.Append($"<td align=\"right\" valign=\"top\" bgcolor=\"#FFFFFF\" width=\"5%\">{shipmentMethodCode}</td>");

                                            var gstPlaceOfSupplyForLine = (li?.GST_Place_Of_Supply ?? "").Trim();
                                            if (string.IsNullOrWhiteSpace(gstPlaceOfSupplyForLine))
                                            {
                                                var shipToCode = (sqHeader?.Ship_to_Code ?? "").Trim();
                                                var jobToCode = (sqHeader?.PCPL_Job_to_Code ?? "").Trim();

                                                var hasShipOrJob = (!string.IsNullOrWhiteSpace(shipToCode) && shipToCode != "-1")
                                                    || (!string.IsNullOrWhiteSpace(jobToCode) && jobToCode != "-1");

                                                gstPlaceOfSupplyForLine = hasShipOrJob ? "Ship-to Address" : "Bill-to Address";
                                            }

                                            table.Append($"<td align=\"right\" valign=\"top\" bgcolor=\"#FFFFFF\" width=\"5%\">{gstPlaceOfSupplyForLine}</td>");
                                            table.Append("</tr>");
                                            rowCounter++;
                                        }
                                        table.Append("</table>");
                                        lineDetailsHtml = table.ToString();
                                    }

                                    var totalCreditLimit = (sqHeader?.Customer_Credit_Limit_LCY ?? "").Trim();
                                    var usedCreditLimit = (sqHeader?.Customer_Balance_Due_LCY ?? "").Trim();
                                    if (string.IsNullOrWhiteSpace(usedCreditLimit))
                                        usedCreditLimit = (sqHeader?.Customer_Balance_LCY ?? "").Trim();

                                    var quoteValidityDaysText = "";
                                    if (DateTime.TryParse(sqHeader?.Order_Date, out var od) && DateTime.TryParse(sqHeader?.Quote_Valid_Until_Date, out var vd))
                                    {
                                        var days = (vd - od).Days;
                                        quoteValidityDaysText = days <= 1 ? $"{days} Day" : $"{days} Days";
                                    }

                                    htmlBody = htmlBody.Replace("##pageheading##", " MARGIN IS LESS THAN ZERO ");
                                    var hodJustificationText = (sqHeader?.WorkDescription ?? "").Trim();
                                    var hodJustificationSuffix = "";
                                    if (!string.IsNullOrWhiteSpace(hodJustificationText))
                                    {
                                        var safeJustification = System.Net.WebUtility.HtmlEncode(hodJustificationText)
                                            .Replace("\r\n", "<br />")
                                            .Replace("\n", "<br />")
                                            .Replace("\r", "<br />");
                                        hodJustificationSuffix = "<br /><b>Justification : </b>" + safeJustification;
                                    }

                                    htmlBody = htmlBody.Replace("##heading##", "Finance has approved the credit limit. Sales Quote now requires HOD approval (Negative Margin)." + hodJustificationSuffix);
                                    htmlBody = htmlBody.Replace("##SalesQuoteNo##", SQNo ?? "");

                                    htmlBody = htmlBody.Replace("##SalesQuoteDate##", sqHeader?.Order_Date ?? "");
                                    htmlBody = htmlBody.Replace("##ApprovalForText##", "Negative Margin");
                                    htmlBody = htmlBody.Replace("##SalesQuoteApprovalFormURL##", approvalUrl);
                                    htmlBody = htmlBody.Replace("{ApprovalFileFormatURL}", logoUrl);

                                    // Populate the same template fields as the initial approval email.
                                    htmlBody = htmlBody.Replace("##CustomerDetail##", customerDetailHtml);
                                    htmlBody = htmlBody.Replace("##ContactName##", contactName);
                                    htmlBody = htmlBody.Replace("##ShippingHeader##", shippingHeader);
                                    htmlBody = htmlBody.Replace("##ShippingDetail##", shippingDetailHtml);
                                    htmlBody = htmlBody.Replace("##LineDetails##", lineDetailsHtml);
                                    htmlBody = htmlBody.Replace("##totalcreditlimit##", totalCreditLimit);
                                    htmlBody = htmlBody.Replace("##usedcreditlimit##", usedCreditLimit);
                                    htmlBody = htmlBody.Replace("##availablecreditlimit##", "");
                                    htmlBody = htmlBody.Replace("##PaymentTermsHeader##", string.IsNullOrWhiteSpace(sqHeader?.Payment_Terms_Code) ? "" : "Payment Terms : ");
                                    htmlBody = htmlBody.Replace("##PaymentTerms##", (sqHeader?.Payment_Terms_Code ?? ""));
                                    htmlBody = htmlBody.Replace("##QuoteValidityDays##", quoteValidityDaysText);
                                    var scheduleStatus = (sqHeader?.TPTPL_Schedule_status ?? "").Trim();
                                    htmlBody = htmlBody.Replace("##ScheduleHeader##", string.IsNullOrWhiteSpace(scheduleStatus) ? "" : "Schedule : ");
                                    htmlBody = htmlBody.Replace("##Schedule##", scheduleStatus);

                                    htmlBody = htmlBody.Replace("##TaxGroupDetails##", "GST: As Applicable");
                                    htmlBody = htmlBody.Replace("##Note##", "Price ruling at the time of delivery for GACL & GNFC products.");

                                    var (spContactName, spContactEmail, spContactPhone) = TryGetSalespersonContact(ac, sqHeader?.Salesperson_Code ?? "");
                                    if (string.IsNullOrWhiteSpace(spContactEmail)) spContactEmail = FirstEmailOrEmpty(SPEmail ?? "");
                                    if (string.IsNullOrWhiteSpace(spContactEmail)) spContactEmail = FirstEmailOrEmpty(LoggedInUserEmail ?? "");
                                    var locationCodeForFooter = (sqHeader?.Location_Code ?? "").Trim();
                                    var loc = TryGetLocation(ac, locationCodeForFooter);
                                    var warehouseManagerPhone = (loc?.Phone_No ?? "").Trim();
                                    var warehouseAddressOverride = (ConfigurationManager.AppSettings["QuoteEmailWarehouseAddress_" + locationCodeForFooter] ?? ConfigurationManager.AppSettings["QuoteEmailWarehouseAddress"] ?? "").Trim();
                                    var warehouseAddressHtml = !string.IsNullOrWhiteSpace(warehouseAddressOverride)
                                        ? HtmlEncodeInline(warehouseAddressOverride)
                                        : BuildWarehouseAddressHtml(ac, locationCodeForFooter);

                                    var ci = TryGetCompanyInformation(ac);
                                    var companyName = (ci?.Name ?? "").Trim();
                                    var bankName = (ci?.Bank_Name ?? "").Trim();
                                    var bankAccountNo = (ci?.Bank_Account_No ?? "").Trim();
                                    var ifsc = (ci?.IFSC ?? "").Trim();

                                    var dispatchTiming = (ConfigurationManager.AppSettings["QuoteEmailDispatchTiming_" + locationCodeForFooter] ?? ConfigurationManager.AppSettings["QuoteEmailDispatchTiming"] ?? "").Trim();
                                    htmlBody = htmlBody.Replace("##FooterDetails##", BuildFooterDetailsHtml(spContactName, spContactPhone, spContactEmail, warehouseAddressHtml, warehouseManagerPhone, dispatchTiming, companyName, bankName, bankAccountNo, ifsc));

                                    // Strip any remaining ##TOKEN## placeholders so the mail looks clean.
                                    htmlBody = System.Text.RegularExpressions.Regex.Replace(htmlBody, "##[^#]+##", "");
                                }
                                else
                                {
                                    var body = new StringBuilder();
                                    body.Append("<p>Hi,</p>");
                                    body.Append($"<p>Sales Quote <strong>{SQNo}</strong> requires HOD approval (Negative Margin).</p>");
                                    body.Append("<p><a href='" + approvalUrl + "'>Open Sales Quote for Approval</a></p>");
                                    body.Append("<p>&nbsp;</p><p>Warm Regards,</p><p>Support Team</p>");
                                    htmlBody = body.ToString();
                                }

                                // CC salesperson if available, else CC the finance user who approved.
                                var cc = !string.IsNullOrWhiteSpace(SPEmail) ? SPEmail : (LoggedInUserEmail ?? "");
                                if (string.IsNullOrWhiteSpace(cc))
                                    cc = hodEmail; // avoid empty CC which breaks SendEmailWithHTMLBody

                                new EmailService().SendEmailWithHTMLBody(hodEmail, cc, "", subject, htmlBody);
                            }
                        }
                    }
                    catch
                    {
                        // Don't fail the approval operation if the follow-up email fails.
                    }
                }

            }

            //SQNos = SQNos.Substring(0, SQNos.Length - 1);

            return resMsg;

        }

        [HttpPost]
        [Route("AddUpdateOnSaveProd")]
        public bool AddUpdateOnSaveProd(SPSQLinesPost salesQuoteLine)
        {
            SPSQLines responseSQLines = new SPSQLines();
            errorDetails ed = new errorDetails();

            //salesQuoteLine.Document_Type = "Quote";
            salesQuoteLine.Type = "Item";
            //salesQuoteLine.Location_Code = "AHM DOM";
            //salesQuoteLine.Location_Code = "";
            //salesQuoteLine.Line_Amount = salesQuoteLine.Unit_Price * salesQuoteLine.Quantity;
            //salesQuoteLine.GST_Place_Of_Supply = "Bill-to Address";

            //var result = PostItemSQLines("SalesQuoteSubFormDotNetAPI", salesQuoteLine, responseSQLines);

            //if (result.Result.Item1 != null)
            //    responseSQLines = result.Result.Item1;

            //if (result.Result.Item2.message != null)
            //    ed = result.Result.Item2;

            return true;
        }

        [HttpPost]
        [Route("AddNewIncoTerm")]
        public bool AddNewIncoTerm(string IncoTermCode, string IncoTerm)
        {
            API ac = new API();
            SPSQIncoTerm incoTerm = new SPSQIncoTerm();
            SPSQIncoTerm incoTermRes = new SPSQIncoTerm();
            errorDetails ed = new errorDetails();

            incoTerm.Code = IncoTermCode;
            incoTerm.Description = IncoTerm;

            var result = ac.PostItem("ShipmentMethodsDotNetAPI", incoTerm, incoTermRes);

            if (result.Result.Item1 != null)
                incoTermRes = result.Result.Item1;

            if (result.Result.Item2.message != null)
                ed = result.Result.Item2;

            return true;
        }

        [Route("GetGeneratedSQNo")]
        public string GetGeneratedSQNo(string NoSeriesCode)
        {
            SPSQGetNextNo requestSQGetNextNo = new SPSQGetNextNo();
            SPSQGetNextNoRes responseSQGetNextNoRes = new SPSQGetNextNoRes();
            errorDetails edForGetNextNo = new errorDetails();
            requestSQGetNextNo.noseriescode = NoSeriesCode;

            string todayDate = DateTime.Now.ToShortDateString();
            string[] todayDate_ = todayDate.Split('-');
            requestSQGetNextNo.usagedate = todayDate_[2] + "-" + todayDate_[1] + "-" + todayDate_[0];

            var resultGetNextNo = PostItemForSQGetNextNo("", requestSQGetNextNo, responseSQGetNextNoRes);
            string generatedSQNo = "";

            if (resultGetNextNo.Result.Item1 != null)
                generatedSQNo = resultGetNextNo.Result.Item1.value;

            return generatedSQNo;
        }

        [Route("GetAllCompanyForDDL")]
        public List<SPCompanyList> GetAllCompanyForDDL(string SPCode)
        {
            API ac = new API();
            List<SPCompanyList> companies = new List<SPCompanyList>();

            var result = ac.GetData<SPCompanyList>("ContactDotNetAPI", "Type eq 'Company' and Salesperson_Code eq '" + SPCode + "'");

            if (result != null && result.Result.Item1?.value != null && result.Result.Item1.value.Count > 0)
                companies = result.Result.Item1.value;

            List<SPCompanyList> company2 = new List<SPCompanyList>();

            var result2 = ac.GetData<SPCompanyList>("ContactDotNetAPI", "PCPL_Secondary_SP_Code eq '" + SPCode + "' and Salesperson_Code ne '" + SPCode + "' and Type eq 'Company'");

            if (result2 != null && result2.Result.Item1?.value != null && result2.Result.Item1.value.Count > 0)
            {
                company2 = result2.Result.Item1.value;

                companies.AddRange(company2);
            }

            return companies;
        }

        [Route("GetInquiriesForDDL")]
        public List<SPSQInquiryNos> GetInquiriesForDDL(string SPCode)
        {
            API ac = new API();
            List<SPSQInquiryNos> inquirynos = new List<SPSQInquiryNos>();

            var result = ac.GetData<SPSQInquiryNos>("InquiryDotNetAPI", "Salesperson_Code eq '" + SPCode + "' and Document_Type eq 'Quote' and PCPL_IsInquiry eq true");

            if (result.Result.Item1.value.Count > 0)
                inquirynos = result.Result.Item1.value;

            return inquirynos;
        }

        [Route("GetInquiryDetails")]
        public SPInquiryList GetInquiryDetails(string InqNo)
        {
            API ac = new API();
            SPInquiryList inquirydetails = new SPInquiryList();

            var result = ac.GetData<SPInquiryList>("InquiryDotNetAPI", "Inquiry_No eq '" + InqNo + "'");

            if (result.Result.Item1.value.Count > 0)
                inquirydetails = result.Result.Item1.value[0];

            return inquirydetails;
        }

        [Route("GetInquiryProdDetails")]
        public List<SPInquiryProducts> GetInquiryProdDetails(string InqNo)
        {
            API ac = new API();
            List<SPInquiryProducts> inquiryproducts = new List<SPInquiryProducts>();

            var result = ac.GetData<SPInquiryProducts>("InquiryProductsDotNetAPI", "Document_No eq '" + InqNo + "' and PCPL_Convert_Quote eq false");

            if (result.Result.Item1.value.Count > 0)
                inquiryproducts = result.Result.Item1.value;

            return inquiryproducts;
        }

        [Route("GetAllContactsOfCompany")]
        public List<SPSQContacts> GetAllContactsOfCompany(string companyName)
        {
            API ac = new API();
            List<SPSQContacts> contacts = new List<SPSQContacts>();

            var result = ac.GetData<SPSQContacts>("ContactDotNetAPI", "Type eq 'Person' and Company_Name eq '" + companyName + "'");

            if (result.Result.Item1.value.Count > 0)
                contacts = result.Result.Item1.value;

            return contacts;
        }

        [Route("GetAllSQLinesOfSQ")]
        public List<SPSQLines> GetAllSQLinesOfSQ(string QuoteNo, string SQLinesFor)
        {
            API ac = new API();
            List<SPSQLines> SQLines = new List<SPSQLines>();
            var result = (dynamic)null;

            if (SQLinesFor == "SalesQuote")
                result = ac.GetData<SPSQLines>("SalesQuoteSubFormDotNetAPI", "Document_No eq '" + QuoteNo + "'");
            else if (SQLinesFor == "ScheduleOrder")
                result = ac.GetData<SPSQLines>("SalesQuoteSubFormDotNetAPI", "Document_No eq '" + QuoteNo + "' and TPTPL_Short_Closed eq false");

            if (result.Result.Item1.value.Count > 0)
                SQLines = result.Result.Item1.value;

            return SQLines;
        }

        [Route("GetAllUsers")]
        public List<SPSQUser> GetAllUsers()
        {
            API ac = new API();
            List<SPSQUser> users = new List<SPSQUser>();

            var result = ac.GetData<SPSQUser>("EmployeesDotNetAPI","");

            if (result.Result.Item1 != null && result.Result.Item1.value != null && result.Result.Item1.value.Count > 0)
            {
                users = result.Result.Item1.value;
                users = users.OrderBy(a => a.First_Name).ToList();
            }

            return users;
        }

        [HttpPost]
        [Route("UpdateScheduleQty")]
        public bool UpdateScheduleQty(string QuoteNo, string ProdLineNo, double ScheduleQty)
        {
            bool flag = false;
            SPSQScheduleQtyPost scheduleQtyReq = new SPSQScheduleQtyPost();
            SPSQScheduleQty scheduleQtyRes = new SPSQScheduleQty();
            errorDetails ed = new errorDetails();

            scheduleQtyReq.TPTPL_Qty_to_Order = ScheduleQty;

            var result = PatchItemScheduleQty("SalesQuoteSubFormDotNetAPI", scheduleQtyReq, scheduleQtyRes, "Document_Type='Quote',Document_No='" + QuoteNo + "',Line_No=" + ProdLineNo);

            if (result.Result.Item1 != null)
            {
                flag = true;
                scheduleQtyRes = result.Result.Item1;
            }

            if (result.Result.Item2.message != null)
                ed = result.Result.Item2;

            return flag;
        }

        //public string ScheduleOrder(string QuoteNo, string ScheduleDate, string ExternalDocNo)

        [HttpPost]
        [Route("ScheduleOrder")]
        public string ScheduleOrder(SPSQScheduleOrderDetails scheduleOrderDetails)
        {
            string response = "", Err = "";
            SPSQScheduleOrderPost scheduleOrderReq = new SPSQScheduleOrderPost();
            SPSQScheduleOrderOData scheduleOrderRes = new SPSQScheduleOrderOData();
            errorDetails ed = new errorDetails();

            if (scheduleOrderDetails.SchQtyProds != null && scheduleOrderDetails.SchQtyProds.Count > 0)
            {
                for (int a = 0; a < scheduleOrderDetails.SchQtyProds.Count; a++)
                {
                    bool flag = false;
                    SPSQScheduleQtyPost scheduleQtyReq = new SPSQScheduleQtyPost();
                    SPSQScheduleQty scheduleQtyRes = new SPSQScheduleQty();
                    errorDetails edSchQty = new errorDetails();

                    scheduleQtyReq.TPTPL_Qty_to_Order = scheduleOrderDetails.SchQtyProds[a].ScheduleQty;
                    scheduleQtyReq.PCPL_Remarks = scheduleOrderDetails.SchQtyProds[a].PCPL_Remarks;

                    var resultSchQty = PatchItemScheduleQty("SalesQuoteSubFormDotNetAPI", scheduleQtyReq, scheduleQtyRes, "Document_Type='Quote',Document_No='" +
                            scheduleOrderDetails.SchQtyProds[a].QuoteNo + "',Line_No=" + scheduleOrderDetails.SchQtyProds[a].ProdLineNo);

                    if (resultSchQty.Result.Item1 != null)
                    {
                        flag = true;
                        scheduleQtyRes = resultSchQty.Result.Item1;
                        edSchQty = resultSchQty.Result.Item2;
                        scheduleOrderRes.errorDetails = edSchQty;
                    }
                    else
                        Err = "Error";

                    //if (resultSchQty.Result.Item2.message != null)
                    //    edSchQty = resultSchQty.Result.Item2;
                }
            }

            if (!scheduleOrderRes.errorDetails.isSuccess)
            {
                response = "Error_:" + scheduleOrderRes.errorDetails.message;
                return response;
            }

            if (Err == "")
            {
                scheduleOrderReq.quoteNo = scheduleOrderDetails.QuoteNo;
                scheduleOrderReq.scheduledate = scheduleOrderDetails.ScheduleDate;
                scheduleOrderReq.externaldocumentno = scheduleOrderDetails.ExternalDocNo;
                scheduleOrderReq.documentdate = scheduleOrderDetails.DocumentDate;
                scheduleOrderReq.assignto = scheduleOrderDetails.AssignTo == null || scheduleOrderDetails.AssignTo == "" ? "" : scheduleOrderDetails.AssignTo;
                var result = PostItemForScheduleOrder<SPSQScheduleOrderOData>("", scheduleOrderReq, scheduleOrderRes);

                response = result.Result.Item1.value;
                ed = result.Result.Item2;
                scheduleOrderRes.errorDetails = ed;

                if (!scheduleOrderRes.errorDetails.isSuccess)
                {
                    response = "Error_:" + scheduleOrderRes.errorDetails.message;
                    return response;
                }

                //if (result.Result.Item2.message != null)
                //    ed = result.Result.Item2;

                if (response != "" && (!response.Contains("Error_:")))
                {
                    if (scheduleOrderDetails.InvQuantities != null && scheduleOrderDetails.InvQuantities.Count > 0)
                    {
                        SPSQInvQtyReserveOData invQtyReserveOData = new SPSQInvQtyReserveOData();
                        List<SPSQInvQtyReserve> invQtyReserve = new List<SPSQInvQtyReserve>();
                        errorDetails ed2 = new errorDetails();
                        string orderNo = response;

                        for (int b = 0; b < scheduleOrderDetails.InvQuantities.Count; b++)
                        {
                            invQtyReserve.Add(new SPSQInvQtyReserve()
                            {
                                OrderNo = orderNo,
                                LineNo = scheduleOrderDetails.InvQuantities[b].LineNo,
                                ItemNo = scheduleOrderDetails.InvQuantities[b].ItemNo,
                                LotNo = scheduleOrderDetails.InvQuantities[b].LotNo,
                                Qty = scheduleOrderDetails.InvQuantities[b].Qty,
                                LocationCode = scheduleOrderDetails.InvQuantities[b].LocationCode

                            });

                        }

                        var result2 = PostItemInvQtyReserve<SPSQInvQtyReserveOData>("", invQtyReserve, invQtyReserveOData);
                        invQtyReserveOData = result2.Result.Item1;
                        ed2 = result2.Result.Item2;
                        scheduleOrderRes.errorDetails = ed2;

                        //if (result2.Result.Item2.message != null)
                        //    ed2 = result2.Result.Item2;

                        if (!scheduleOrderRes.errorDetails.isSuccess)
                            response = "Error_:" + scheduleOrderRes.errorDetails.message;

                    }
                }
            }

            return response;
        }

        [Route("GetCreditLimitAndCustDetails")]
        public SPSQCreditLimitAndCustDetails GetCreditLimitAndCustDetails(string companyName)
        {
            API ac = new API();
            SPContCustForBusRel contCustForBusRel = new SPContCustForBusRel();
            SPConBusinessRelation conBusinessRelation = new SPConBusinessRelation();
            SPSQCreditLimitAndCustDetails creditlimitcustdetails = new SPSQCreditLimitAndCustDetails();

            var resultCompanyNo = ac.GetData<ContCustForBusRel>("ContactDotNetAPI", "Type eq 'Company' and Name eq '" + companyName + "'");

            if (resultCompanyNo.Result.Item1.value.Count > 0)
                contCustForBusRel.Company_No = resultCompanyNo.Result.Item1.value[0].Company_No;

            var resultCustomerNo = ac.GetData<ConBusinessRelation>("ContactBusinessRelationsDotNetAPI", "Contact_No eq '" + contCustForBusRel.Company_No + "'");
            var shipToPincode = ac.GetData<SPSQHeader>("SalesQuoteDotNetAPI", "Sell_to_Customer_No eq '" + conBusinessRelation.No + "'");

            if (resultCustomerNo.Result.Item1.value.Count > 0)
            {
                conBusinessRelation.No = resultCustomerNo.Result.Item1.value[0].No;

                var resultCustomer = ac.GetData<SPCustomer>("CustomerCardDotNetAPI", "No eq '" + conBusinessRelation.No + "'");

                if (resultCustomer.Result.Item1.value.Count > 0)
                {
                    creditlimitcustdetails.CreditLimit = resultCustomer.Result.Item1.value[0].Credit_Limit_LCY.ToString("#,##0.00");
                    double AccBal = Math.Abs(resultCustomer.Result.Item1.value[0].Balance_LCY);
                    double createdQuoteAmt = Math.Abs(resultCustomer.Result.Item1.value[0].PCPL_Credit_Limit_LCY);
                    creditlimitcustdetails.AccountBalance = AccBal.ToString("#,##0.00");
                    creditlimitcustdetails.UsedCreditLimit = (AccBal + createdQuoteAmt).ToString("#,##0.00");
                    creditlimitcustdetails.AvailableCredit = Convert.ToDouble(resultCustomer.Result.Item1.value[0].Credit_Limit_LCY - (AccBal + createdQuoteAmt)).ToString("#,##0.00");
                    creditlimitcustdetails.OutstandingDue = resultCustomer.Result.Item1.value[0].Balance_Due_LCY.ToString("#,##0.00");
                    creditlimitcustdetails.CustNo = resultCustomer.Result.Item1.value[0].No;
                    creditlimitcustdetails.CustName = resultCustomer.Result.Item1.value[0].Name;
                    creditlimitcustdetails.Address = resultCustomer.Result.Item1.value[0].Address;
                    creditlimitcustdetails.Address_2 = resultCustomer.Result.Item1.value[0].Address_2;
                    creditlimitcustdetails.City = resultCustomer.Result.Item1.value[0].City;
                    creditlimitcustdetails.Post_Code = resultCustomer.Result.Item1.value[0].Post_Code;
                    creditlimitcustdetails.PANNo = resultCustomer.Result.Item1.value[0].P_A_N_No;
                    creditlimitcustdetails.PcplClass = resultCustomer.Result.Item1.value[0].PCPL_Class;
                    creditlimitcustdetails.AverageDelayDays = resultCustomer.Result.Item1.value[0].PCPL_ADD_Average_Delay_Days;

                    SPSQShiptoAddress requestShiptoAddress = new SPSQShiptoAddress();
                    List<SPSQShiptoAddressRes> responseShiptoAddress = new List<SPSQShiptoAddressRes>();
                    errorDetails edForShiptoAddress = new errorDetails();
                    requestShiptoAddress.customerno = conBusinessRelation.No;

                    var resultGetShipToAddress = PostItemForSQGetShiptoAddress<SPSQShiptoAddressRes>("", requestShiptoAddress, responseShiptoAddress);
                    List<SPSQShiptoAddressRes> shiptoaddresses = new List<SPSQShiptoAddressRes>();

                    if (resultGetShipToAddress.Result.Item1.Count > 0)
                    {
                        responseShiptoAddress = resultGetShipToAddress.Result.Item1;
                        creditlimitcustdetails.ShiptoAddress = responseShiptoAddress.OrderBy(a => a.Address).ToList();
                    }

                    SPSQJobtoAddress requestJobtoAddress = new SPSQJobtoAddress();
                    List<SPSQJobtoAddressRes> responseJobtoAddress = new List<SPSQJobtoAddressRes>();
                    errorDetails edForJobtoAddress = new errorDetails();
                    requestJobtoAddress.customerno = conBusinessRelation.No;

                    var resultGetJobToAddress = PostItemForSQGetJobtoAddress<SPSQJobtoAddressRes>("", requestJobtoAddress, responseJobtoAddress);
                    List<SPSQJobtoAddressRes> jobtoaddresses = new List<SPSQJobtoAddressRes>();

                    if (resultGetJobToAddress.Result.Item1.Count > 0)
                    {
                        responseJobtoAddress = resultGetJobToAddress.Result.Item1;
                        creditlimitcustdetails.JobtoAddress = responseJobtoAddress.OrderBy(a => a.Address).ToList();
                    }

                }

            }
            else
            {
                creditlimitcustdetails.CreditLimit = "0.00";
                creditlimitcustdetails.AvailableCredit = "0.00";
                creditlimitcustdetails.OutstandingDue = "0.00";

                SPCompanyList companyList = new SPCompanyList();
                var resultCompanyDetails = ac.GetData<SPCompanyList>("ContactDotNetAPI", "Type eq 'Company' and No eq '" + contCustForBusRel.Company_No + "'");

                if (resultCompanyDetails.Result.Item1.value.Count > 0)
                {
                    companyList = resultCompanyDetails.Result.Item1.value[0];
                    creditlimitcustdetails.CompanyNo = companyList.No;
                    creditlimitcustdetails.CompanyName = companyList.Name;
                    creditlimitcustdetails.Address = companyList.Address;
                    creditlimitcustdetails.Address_2 = companyList.Address_2;
                    creditlimitcustdetails.City = companyList.City;
                    creditlimitcustdetails.Post_Code = companyList.Post_Code;
                    creditlimitcustdetails.Credit_Limit = companyList.Credit_Limit;
                    creditlimitcustdetails.Salesperson_Code = companyList.Salesperson_Code;
                }
            }


            return creditlimitcustdetails;
        }

        //[Route("GetAllProducts")]
        //public List<SPItemList> GetAllProducts()
        //{
        //    API ac = new API();
        //    List<SPItemList> items = new List<SPItemList>();
        //    //string filter = "PCPL_MRP_Price gt 0", orderby="No asc";
        //    //int skip = 0, top = 10;

        //    var result = ac.GetData<SPItemList>("ItemDotNetAPI", "No eq 'TRD0001' or No eq 'TRD0002' or No eq 'TRD0032' or No eq 'TRD0016' or No eq 'TRD0036' or No eq 'TRD0037' or No eq 'ITM001'");

        //    //var result = ac.GetData1<SPItemList>("ItemDotNetAPI", filter, skip, top, orderby);

        //    if (result != null && result.Result.Item1.value.Count > 0)
        //        items = result.Result.Item1.value;

        //    return items;
        //}

        [Route("GetAllProducts")]
        public List<SPContactProducts> GetAllProducts(string CCompanyNo)
        {
            API ac = new API();
            List<SPContactProducts> contactProducts = new List<SPContactProducts>();
            //string filter = "PCPL_MRP_Price gt 0", orderby="No asc";
            //int skip = 0, top = 10;

            var result = ac.GetData<SPContactProducts>("ContactProductsDotNetAPI", "Contact_No eq '" + CCompanyNo + "' and Blocked eq false");

            //var result = ac.GetData1<SPItemList>("ItemDotNetAPI", filter, skip, top, orderby);

            if (result != null && result.Result.Item1.value.Count > 0)
                contactProducts = result.Result.Item1.value;

            return contactProducts;
        }

        [Route("GetAllProductsForShowAllProd")]
        public List<SPItemList> GetAllProductsForShowAllProd()
        {
            API ac = new API();
            List<SPItemList> prods = new List<SPItemList>();

            var result = ac.GetData<SPItemList>("ItemDotNetAPI", "Blocked eq false"); // and Contact_Business_Relation eq 'Customer'

            if (result.Result.Item1.value.Count > 0)
                prods = result.Result.Item1.value;

            return prods;

        }

        //[Route("GetNoSeriesForDDL")]
        //public List<SPNoSeries> GetNoSeriesForDDL()
        //{
        //    API ac = new API();
        //    List<SPNoSeries> locations = new List<SPNoSeries>();

        //    var result = ac.GetData<SPNoSeries>("NoSeriesRelDotNetAPI", "Code eq 'SQ'"); // and Contact_Business_Relation eq 'Customer'

        //    if (result.Result.Item1.value.Count > 0)
        //        locations = result.Result.Item1.value;

        //    return locations;
        //}

        [Route("GetLocationsForDDL")]
        public List<SPLocations> GetLocationsForDDL()
        {
            API ac = new API();
            List<SPLocations> locations = new List<SPLocations>();

            var result = ac.GetData<SPLocations>("LocationsDotNetAPI", ""); // and Contact_Business_Relation eq 'Customer'

            if (result.Result.Item1.value.Count > 0)
                locations = result.Result.Item1.value;

            return locations;
        }

        [Route("GetPaymentTermsForDDL")]
        public List<SPSQPaymentTerms> GetPaymentTermsForDDL()
        {
            API ac = new API();
            List<SPSQPaymentTerms> paymentterms = new List<SPSQPaymentTerms>();

            var result = ac.GetData<SPSQPaymentTerms>("PaymentTermsDotNetAPI", ""); // and Contact_Business_Relation eq 'Customer'

            if (result.Result.Item1.value.Count > 0)
                paymentterms = result.Result.Item1.value;

            return paymentterms;
        }

        [Route("GetItemVendorsForDDL")]
        public List<SPSQItemVendors> GetItemVendorsForDDL(string ProdNo)
        {
            API ac = new API();
            List<SPSQItemVendors> itemVendors = new List<SPSQItemVendors>();

            var result = ac.GetData<SPSQItemVendors>("ItemVendorCatalogDotNetAPI", "Item_No eq '" + ProdNo + "'"); // and Contact_Business_Relation eq 'Customer'

            if (result.Result.Item1.value.Count > 0)
                itemVendors = result.Result.Item1.value;

            return itemVendors;
        }

        [Route("GetIncoTermsForDDL")]
        public List<SPSQShipmentMethods> GetIncoTermsForDDL()
        {
            API ac = new API();
            List<SPSQShipmentMethods> shipmentmethods = new List<SPSQShipmentMethods>();

            var result = ac.GetData<SPSQShipmentMethods>("ShipmentMethodsDotNetAPI", ""); // and Contact_Business_Relation eq 'Customer'

            if (result.Result.Item1.value.Count > 0)
                shipmentmethods = result.Result.Item1.value;

            return shipmentmethods;
        }

        [Route("GetVendorsForDDL")]
        public List<SPSQVendors> GetVendorsForDDL()
        {
            API ac = new API();
            List<SPSQVendors> vendors = new List<SPSQVendors>();

            var result = ac.GetData<SPSQVendors>("VendorDotNetAPI", "PCPL_Broker eq true");   //PCPL_Broker eq true and Contact_Business_Relation eq 'Customer'

            if (result.Result.Item1.value.Count > 0)
                vendors = result.Result.Item1.value;

            return vendors;
        }

        [Route("GetPaymentMethodsForDDL")]
        public List<SPSQPaymentMethods> GetPaymentMethodsForDDL()
        {
            API ac = new API();
            List<SPSQPaymentMethods> paymentmethods = new List<SPSQPaymentMethods>();

            var result = ac.GetData<SPSQPaymentMethods>("PaymentMethodsDotNetAPI", "");

            if (result.Result.Item1.value.Count > 0)
                paymentmethods = result.Result.Item1.value;

            return paymentmethods;
        }

        [Route("GetTransportMethodsForDDL")]
        public List<SPSQTransportMethods> GetTransportMethodsForDDL()
        {
            API ac = new API();
            List<SPSQTransportMethods> transportMethods = new List<SPSQTransportMethods>();

            var result = ac.GetData<SPSQTransportMethods>("TransportMethodsDotNetAPI", "");

            if (result.Result.Item1.value.Count > 0)
                transportMethods = result.Result.Item1.value;

            return transportMethods;
        }

        [Route("GetProductDetails")]
        public SPSQProductDetails GetProductDetails(string productName)
        {
            API ac = new API();
            SPSQProductDetails productdetails = new SPSQProductDetails();

            var result = ac.GetData<SPSQProductDetails>("ItemDotNetAPI", "Description eq '" + productName + "'"); // and Contact_Business_Relation eq 'Customer'

            if (result.Result.Item1.value.Count > 0)
                productdetails = result.Result.Item1.value[0];

            return productdetails;
        }

        [Route("GetProductPackingStyle")]
        public List<SPSQProductPackingStyle> GetProductPackingStyle(string prodNo)
        {
            API ac = new API();

            List<SPSQProductPackingStyle> packingStyle = new List<SPSQProductPackingStyle>();

            var result = ac.GetData<SPSQProductPackingStyle>("ItemPackingStyleDotNetAPI", "Item_No eq '" + prodNo + "'"); // and Contact_Business_Relation eq 'Customer'

            if (result.Result.Item1.value.Count > 0)
                packingStyle = result.Result.Item1.value;

            return packingStyle;
        }

        [Route("updateInquiryNotifcationStatus")]
        public bool updateInquiryNotifcationStatus(string InqNo)
        {
            //use table InquiryMessageMst
            //code for update field Status='Complete' of table InquiryMessageMst
            return true;
        }

        [Route("GetAllDepartmentForDDL")]
        public List<Departments> GetAllDepartmentForDDL()
        {
            API ac = new API();
            List<Departments> departments = new List<Departments>();

            var result = ac.GetData<Departments>("DepartmentsDotNetAPI", "");

            if (result != null && result.Result.Item1.value.Count > 0)
                departments = result.Result.Item1.value;

            return departments;
        }

        [Route("GetSalesQuoteFromNo")]
        public SPSQHeaderDetails GetSalesQuoteFromNo(string SQNo)
        {
            API ac = new API();
            SPSQHeaderDetails SQHeaderDetails = new SPSQHeaderDetails();
            SPSQHeader SQHeader = new SPSQHeader();
            List<SPSQLines> SQLines = new List<SPSQLines>();

            var result = ac.GetData<SPSQHeader>("SalesQuoteDotNetAPI", "No eq '" + SQNo + "'");

            if (result.Result.Item1.value.Count > 0)
            {
                SQHeader = result.Result.Item1.value[0];

                SQHeaderDetails.QuoteNo = SQHeader.No;
                SQHeaderDetails.InquiryNo = SQHeader.PCPL_Inquiry_No;
                SQHeaderDetails.ValidUntillDate = SQHeader.Quote_Valid_Until_Date;
                SQHeaderDetails.LocationCode = SQHeader.Location_Code;
                SQHeaderDetails.ContactCompanyNo = SQHeader.Sell_to_Contact_No;
                SQHeaderDetails.ContactCompanyName = SQHeader.Sell_to_Contact;
                SQHeaderDetails.ContactPersonNo = SQHeader.PCPL_Contact_Person;
                SQHeaderDetails.CustomerNo = SQHeader.Sell_to_Customer_No;
                SQHeaderDetails.OrderDate = SQHeader.Order_Date;
                SQHeaderDetails.PCPL_Location_Post_Code = SQHeader.PCPL_Location_Post_Code;
                //SQHeaderDetails.PaymentMethodCode = SQHeader.Payment_Method_Code;
                SQHeaderDetails.TransportMethodCode = SQHeader.Transport_Method;
                SQHeaderDetails.PaymentTermsCode = SQHeader.Payment_Terms_Code;
                SQHeaderDetails.ShipmentMethodCode = SQHeader.Shipment_Method_Code;
                SQHeaderDetails.ShiptoCode = SQHeader.Ship_to_Code;
                SQHeaderDetails.JobtoCode = SQHeader.PCPL_Job_to_Code;
                SQHeaderDetails.ShortcloseStatus = SQHeader.TPTPL_Short_Close;
                SQHeaderDetails.SCRemarksSetupValue = SQHeader.TPTPL_SC_Reason_Setup_Value;
                SQHeaderDetails.Status = SQHeader.PCPL_Status;
                SQHeaderDetails.ApprovalFor = SQHeader.PCPL_ApprovalFor;
                SQHeaderDetails.WorkDescription = SQHeader.WorkDescription;
                SQHeaderDetails.SalespersonEmail = SQHeader.PCPL_SalesPerson_Email;
                SQHeaderDetails.AvailableCreditLimit = SQHeader.Customer_Balance_LCY;
                SQHeaderDetails.TotalCreditLimit = SQHeader.Customer_Credit_Limit_LCY;
                SQHeaderDetails.OutstandingDue = SQHeader.Customer_Balance_Due_LCY;

                var resultSQLines = ac.GetData<SPSQLines>("SalesQuoteSubFormDotNetAPI", "Document_No eq '" + SQNo + "'");

                if (resultSQLines.Result.Item1.value.Count > 0)
                {
                    SQLines = resultSQLines.Result.Item1.value;
                    List<SPSQLines> SQLineList = new List<SPSQLines>();
                    for (int a = 0; a < SQLines.Count; a++)
                    {
                        SQLineList.Add(new SPSQLines()
                        {
                            Line_No = SQLines[a].Line_No,
                            No = SQLines[a].No,
                            Description = SQLines[a].Description,
                            Location_Code = SQLines[a].Location_Code,
                            Unit_Price = SQLines[a].Unit_Price,
                            Unit_Cost_LCY = SQLines[a].Unit_Cost_LCY,
                            Quantity = SQLines[a].Quantity,
                            PCPL_Concentration_Rate_Percent = SQLines[a].PCPL_Concentration_Rate_Percent,
                            Net_Weight = SQLines[a].Net_Weight,
                            PCPL_Liquid_Rate = SQLines[a].PCPL_Liquid_Rate,
                            PCPL_Liquid = SQLines[a].PCPL_Liquid,
                            Delivery_Date = SQLines[a].Delivery_Date,
                            Unit_of_Measure_Code = SQLines[a].Unit_of_Measure_Code,
                            PCPL_Packing_Style_Code = SQLines[a].PCPL_Packing_Style_Code,
                            PCPL_Transport_Method = SQLines[a].PCPL_Transport_Method,
                            PCPL_Transport_Cost = SQLines[a].PCPL_Transport_Cost,
                            PCPL_MRP = SQLines[a].PCPL_MRP,
                            Drop_Shipment = SQLines[a].Drop_Shipment,
                            PCPL_Vendor_No = SQLines[a].PCPL_Vendor_No,
                            PCPL_Vendor_Name = SQLines[a].PCPL_Vendor_Name,
                            PCPL_Total_Cost = SQLines[a].PCPL_Total_Cost,
                            PCPL_Margin = SQLines[a].PCPL_Margin,
                            PCPL_Margin_Percent = SQLines[a].PCPL_Margin_Percent,
                            PCPL_Sales_Discount = SQLines[a].PCPL_Sales_Discount,
                            PCPL_Commission_Type = SQLines[a].PCPL_Commission_Type,
                            PCPL_Commission = SQLines[a].PCPL_Commission,
                            PCPL_Commission_Amount = SQLines[a].PCPL_Commission_Amount,
                            PCPL_Credit_Days = SQLines[a].PCPL_Credit_Days,
                            PCPL_Interest = SQLines[a].PCPL_Interest,
                            PCPL_Commission_Payable = SQLines[a].PCPL_Commission_Payable,
                            PCPL_Commission_Payable_Name = SQLines[a].PCPL_Commission_Payable_Name,
                            TPTPL_Short_Closed = SQLines[a].TPTPL_Short_Closed,
                            PCPL_Packing_MRP_Price = SQLines[a].PCPL_Packing_MRP_Price,
                            New_Price = SQLines[a].New_Price,
                            New_Margin = SQLines[a].New_Margin,
                            Price_Updated = SQLines[a].Price_Updated

                        });
                    }

                    SQHeaderDetails.ProductsRes = SQLineList;

                }

            }


            return SQHeaderDetails;
        }

        [Route("GetCustomerTemplateCode")]
        public string GetCustomerTemplateCode()
        {
            string customerTemplateCode = "";
            API ac = new API();
            SPSalesReceivableSetup salesReceivableSetup = new SPSalesReceivableSetup();
            //string filter = "PCPL_MRP_Price gt 0", orderby="No asc";
            //int skip = 0, top = 10;

            var result = ac.GetData<SPSalesReceivableSetup>("SalesReceivableSetupDotNetAPI", "");

            //var result = ac.GetData1<SPItemList>("ItemDotNetAPI", filter, skip, top, orderby);

            if (result != null && result.Result.Item1.value.Count > 0)
            {
                salesReceivableSetup = result.Result.Item1.value[0];
                customerTemplateCode = salesReceivableSetup.PCPL_Inquiry_Customer_Template;
            }

            return customerTemplateCode;
        }

        [Route("GetSQDetailsBySQNo")]
        public List<SPSQLines> GetSQDetailsBySQNo(string DocumentNo)
        {
            API ac = new API();
            List<SPSQLines> lineitems = new List<SPSQLines>();

            var result = ac.GetData<SPSQLines>("SalesQuoteSubFormDotNetAPI", "Document_No eq '" + DocumentNo + "' and PCPL_Margin lt " + 0); // and Contact_Business_Relation eq 'Customer'

            if (result.Result.Item1.value.Count > 0)
                lineitems = result.Result.Item1.value;

            return lineitems;
        }

        [HttpGet]
        [Route("PrintQuote")]
        public string PrintQuote(string QuoteNo)
        {
            var printQuoteResponse = new PrintQuoteResponse();

            PrintQuoteRequest printQuoteRequest = new PrintQuoteRequest
            {
                docno = QuoteNo
            };

            var result = (dynamic)null;
            result = PostQuotePrint("APIMngt_SalesQuoteReportPrint", printQuoteRequest, printQuoteResponse);

            var base64PDF = "";
            if (result.Result.Item1 != null)
            {
                base64PDF = result.Result.Item1.value;

            }
            return base64PDF;
        }

        [HttpGet]
        [Route("GenerateCostSheet")]
        public string GenerateCostSheet(string SQNo, int ItemLineNo)
        {
            string response = "";
            API ac = new API();
            SPSQCostSheet costSheet = new SPSQCostSheet();
            SPSQCostSheetOData costSheetOData = new SPSQCostSheetOData();

            costSheet.salesdoctype = "0";
            costSheet.salesdocno = SQNo;
            costSheet.doclineno = ItemLineNo;
            costSheet.frombc = false;

            var result = PostItemForGenerateCostSheet<SPSQCostSheetOData>("", costSheet, costSheetOData);

            response = result.Result.Item1.value;

            return response;
        }

        [Route("GetCostSheetDetails")]
        public List<SPSQCostSheetDetails> GetCostSheetDetails(string SQNo, int ItemLineNo)
        {
            API ac = new API();
            List<SPSQCostSheetDetails> costSheetDetails = new List<SPSQCostSheetDetails>();

            var resultCostSheet = ac.GetData<SPSQCostSheetDetails>("SalesCostSheetLines", "TPTPL_Document_Type eq 'Quote' and TPTPL_Document_No eq '" + SQNo + "' and TPTPL_Source_Document_Line_No eq " + ItemLineNo);

            if (resultCostSheet.Result.Item1.value.Count > 0)
            {
                //responseShiptoAddress = resultGetShipToAddress.Result.Item1;
                //creditlimitcustdetails.ShiptoAddress = responseShiptoAddress;
                costSheetDetails = resultCostSheet.Result.Item1.value;

            }

            return costSheetDetails;
        }


        [Route("UpdateCostSheet")]
        public bool UpdateCostSheet(string SQNo, int CostSheetLineNo, double RatePerUnit)
        {
            bool flag = false;
            SPSQUpdateCostSheet reqCostSheetUpdate = new SPSQUpdateCostSheet();
            SPSQCostSheetDetails resCostSheetUpdate = new SPSQCostSheetDetails();
            errorDetails ed = new errorDetails();

            reqCostSheetUpdate.TPTPL_Rate_per_Unit = RatePerUnit;

            var result = PatchItemCostSheet("SalesCostSheetLines", reqCostSheetUpdate, resCostSheetUpdate, "TPTPL_Document_Type='Quote',TPTPL_Document_No='" + SQNo + "',TPTPL_Line_No=" + CostSheetLineNo);

            if (result.Result.Item1 != null)
            {
                flag = true;
                resCostSheetUpdate = result.Result.Item1;
            }

            if (result.Result.Item2.message != null)
                ed = result.Result.Item2;

            return flag;
        }

        [Route("GetLocationCode")]
        public string GetLocationCode(string NoSeriesCode)
        {
            API ac = new API();
            List<SPSQNoSeriesDetails> noSeriesDetails = new List<SPSQNoSeriesDetails>();

            var result = ac.GetData<SPSQNoSeriesDetails>("NoSeriesRelDotNetAPI", "Series_Code eq '" + NoSeriesCode + "'");

            if (result.Result.Item1.value.Count > 0)
                noSeriesDetails = result.Result.Item1.value;

            string Location_Code = noSeriesDetails[0].PCPL_Location_Code;

            return Location_Code;

        }

        [Route("GetInterestRate")]
        public SPSalesReceivableSetup GetInterestRate()
        {
            API ac = new API();
            SPSalesReceivableSetup salesReceivableSetups = new SPSalesReceivableSetup();

            var result = ac.GetData<SPSalesReceivableSetup>("SalesReceivableSetupDotNetAPI", "");

            if (result.Result.Item1.value.Count > 0)
                salesReceivableSetups = result.Result.Item1.value[0];

            return salesReceivableSetups;

        }

        [Route("GetInventoryDetails")]
        public List<SPSQInvDetailsRes> GetInventoryDetails(string ProdNo, string LocCode)
        {
            API ac = new API();
            SPSQInvDetails reqInvDetails = new SPSQInvDetails();
            List<SPSQInvDetailsRes> resInvDetails = new List<SPSQInvDetailsRes>();

            reqInvDetails.itemno = ProdNo;
            reqInvDetails.locationcode = LocCode;

            var result = PostItemForGetInventoryDetails<SPSQInvDetailsRes>("", reqInvDetails, resInvDetails);

            if (result.Result.Item1.Count > 0)
                resInvDetails = result.Result.Item1;

            return resInvDetails;
        }

        [Route("GetPurDiscountDetails")]
        public List<SPSQPurDiscountDetails> GetPurDiscountDetails(string ProdNo)
        {
            API ac = new API();
            List<SPSQPurDiscountDetails> purDisDetails = new List<SPSQPurDiscountDetails>();

            int CurYear = DateTime.Now.Year;
            int CurMon = DateTime.Now.Month;
            int CurDay = DateTime.Now.Day;
            string CurDate = CurYear + "-" + CurMon + "-" + CurDay;

            var result = ac.GetData<SPSQPurDiscountDetails>("PurchaseLineCostingDiscountsDotNetAPI", "Item_No eq '" + ProdNo + "' and (Starting_Date le " + CurDate + " and Ending_Date ge " + CurDate + ")");

            if (result.Result.Item1.value.Count > 0)
                purDisDetails = result.Result.Item1.value;

            return purDisDetails;
        }

        [Route("GetTransSalesInvoiceLine")]
        public List<SPSQSalesInvoiceDetails> GetTransSalesInvoiceLine(string CustNo, /*string LocCode,*/ string TransType, string ProdNo)
        {
            API ac = new API();
            List<SPSQSalesInvoiceDetails> salesInvioceLines = new List<SPSQSalesInvoiceDetails>();

            var result = (dynamic)null;
            if (TransType == "CustTrans")
            {
                if (ProdNo == null)
                {
                    result = ac.GetData1<SPSQSalesInvoiceDetails>("PostedSalesInvoiceLinesDotNetAPI", "Sell_to_Customer_No eq '" + CustNo /*+ "' and Location_Code eq '" + LocCode*/ + "' and Type eq 'Item'", 0, 3, "Posting_Date desc");

                }
                else
                {
                    result = ac.GetData1<SPSQSalesInvoiceDetails>("PostedSalesInvoiceLinesDotNetAPI", "Sell_to_Customer_No eq '" + CustNo /*+ "' and Location_Code eq '" + LocCode*/ + "' and Type eq 'Item' and No eq '" + ProdNo + "'", 0, 3, "Posting_Date desc");

                }
            }

            if (TransType == "ProdTrans")
                result = ac.GetData1<SPSQSalesInvoiceDetails>("PostedSalesInvoiceLinesDotNetAPI", "Sell_to_Customer_No eq '" + CustNo /*+ "' and Location_Code eq '" + LocCode*/ + "' and No eq '" + ProdNo + "' and Type eq 'Item'", 0, 3, "Posting_Date desc");

            if (result.Result.Item1 != null)
                salesInvioceLines = result.Result.Item1.value;

            return salesInvioceLines;
        }

        [Route("GetSQNoFromInqNo")]
        public string GetSQNoFromInqNo(string InqNo)
        {
            API ac = new API();
            SPSQHeader SQHeader = new SPSQHeader();
            string SQNo = "", ScheduleStatus = "";

            var result = ac.GetData<SPSQHeader>("SalesQuoteDotNetAPI", "PCPL_Inquiry_No eq '" + InqNo + "'");

            if (result.Result.Item1.value.Count > 0)
            {
                SQHeader = result.Result.Item1.value[0];
                SQNo = SQHeader.No;
                ScheduleStatus = SQHeader.TPTPL_Schedule_status;
            }
            else
            {
                return "";
            }

            string SQNo_ScheduleStatus = SQNo + "_" + ScheduleStatus;

            return SQNo_ScheduleStatus;
        }


        [HttpPost]
        [Route("SalesQuoteSendEmail")]
        public bool SalesQuoteSendEmail(string custEmail, string SPEmail, string SQNo)
        {
            bool flag = false;
            string emailBody = "";

            emailBody += "<table width=\"100%\" border=\"1\"><thead><tr style=\"background-color:darkblue;color:white\"><th>Quote No</th><th>Quote Date</th><th>Customer</th><th>Payment Terms</th><th>Inco Terms</th><th>Transport Method</th></tr></thead><tbody>";

            API ac = new API();
            SPSQHeader spSQHeader = new SPSQHeader();
            List<SPSQLines> spSQLines = new List<SPSQLines>();

            var result = ac.GetData<SPSQHeader>("SalesQuoteDotNetAPI", "No eq '" + SQNo + "'");

            if (result.Result.Item1.value.Count > 0)
                spSQHeader = result.Result.Item1.value[0];

            if (spSQHeader.No != null)
            {
                emailBody += "<tr><td>" + spSQHeader.No + "</td><td>" + spSQHeader.Order_Date + "</td><td>" + spSQHeader.Sell_to_Contact
                        + "</td><td>" + spSQHeader.Payment_Terms_Code + "</td><td>" + spSQHeader.Shipment_Method_Code +
                        "</td><td>" + spSQHeader.Transport_Method + "</td></tr>";

                emailBody += "<tr><td></td><td colspan=\"6\"><table width=\"50%\" border=\"1\"><thead><tr style=\"background-color:gray;color:black\"><th>Product Name</th><th>Qty</th><th>Packing Style</th><th>UOM</th></tr></thead><tbody>";

                var resultSQLines = ac.GetData<SPSQLines>("SalesQuoteSubFormDotNetAPI", "Document_No eq '" + SQNo + "'");

                if (resultSQLines.Result.Item1.value.Count > 0)
                {
                    spSQLines = resultSQLines.Result.Item1.value;

                    for (int a = 0; a < spSQLines.Count; a++)
                    {
                        emailBody += "<tr><td>" + spSQLines[a].Description + "</td><td>" + spSQLines[a].Quantity + "</td><td>" + spSQLines[a].PCPL_Packing_Style_Code + "</td><td>" + spSQLines[a].Unit_of_Measure_Code + "</td></tr>";
                    }

                    emailBody += "</tbody></table></td></tr>";
                }

                emailBody += "</tbody></table>";

                EmailService emailService = new EmailService();
                StringBuilder sbMailBody = new StringBuilder();
                sbMailBody.Append("");
                sbMailBody.Append("<p>Hi,</p>");
                sbMailBody.Append("<p>Welcome to the <strong>Prakash CRM Portal</strong>.</p>");
                sbMailBody.Append("<p>Sales Quote Details</p>");

                var justification = (spSQHeader?.WorkDescription ?? "").Trim();
                if (!string.IsNullOrWhiteSpace(justification))
                {
                    var safeJustification = System.Net.WebUtility.HtmlEncode(justification)
                        .Replace("\r\n", "<br />")
                        .Replace("\n", "<br />")
                        .Replace("\r", "<br />");
                    sbMailBody.Append("<p><strong>Justification : </strong>" + safeJustification + "</p>");
                }

                sbMailBody.Append(emailBody);
                sbMailBody.Append("<p>&nbsp;</p>");
                sbMailBody.Append("<p>Warm Regards,</p>");
                sbMailBody.Append("<p>Support Team</p>");
                emailService.SendEmail(custEmail, SPEmail, "Sales Quote Details - PrakashCRM", sbMailBody.ToString());
                flag = true;
            }
            return flag;
        }

        [HttpPost]
        [Route("SalesQuoteShortclose")]
        public string SalesQuoteShortclose(string Type, string SQNo, string SQProdLineNo, string ShortcloseReason, string ShortcloseRemarks)
        {
            bool flag = false;
            string errMsg = "";
            SPSQShortclose sqShortclose = new SPSQShortclose();
            SPSQProdShortclose sqProdShortclose = new SPSQProdShortclose();
            SPSQShortcloseOData sqShortcloseOData = new SPSQShortcloseOData();
            errorDetails ed = new errorDetails();
            var result = (dynamic)null;

            if (Type == "SalesQuote")
            {
                sqShortclose.salesheader = SQNo;
                sqShortclose.shortclosereason = ShortcloseReason;
                sqShortclose.shortcloseremarks = ShortcloseRemarks == null ? "" : ShortcloseRemarks;

                result = PostItemSQShortclose("", sqShortclose, sqShortcloseOData);
            }
            else if (Type == "SalesQuoteProd")
            {
                sqProdShortclose.salesheader = SQNo;
                sqProdShortclose.lineno = SQProdLineNo;
                sqProdShortclose.shortclosereason = ShortcloseReason;
                sqProdShortclose.shortcloseremarks = ShortcloseRemarks == null ? "" : ShortcloseRemarks;

                result = PostItemSQProdShortclose("", sqProdShortclose, sqShortcloseOData);
            }

            sqShortcloseOData = result.Result.Item1;
            ed = result.Result.Item2;
            sqShortcloseOData.errorDetails = ed;

            if (!sqShortcloseOData.errorDetails.isSuccess)
                errMsg = sqShortcloseOData.errorDetails.message;

            //flag = Convert.ToBoolean(sqShortcloseOData.value);

            return errMsg;
        }

        [Route("GetPincodeForDDL")]
        public List<PostCodes> GetPincodeForDDL(string prefix)
        {
            API ac = new API();
            List<PostCodes> pincode = new List<PostCodes>();

            var result = ac.GetData<PostCodes>("PostCodesDotNetAPI", "startswith(Code,'" + prefix + "')");

            if (result != null && result.Result.Item1.value.Count > 0)
                pincode = result.Result.Item1.value;

            List<PostCodes> returnpc = pincode.DistinctBy(x => x.Code).ToList();

            return returnpc;
        }

        [HttpPost]
        [Route("AddNewContactPerson")]
        public SPContact AddNewContactPerson(SPContact reqCPerson)
        {
            API ac = new API();
            SPContact resCPerson = new SPContact();
            errorDetails ed = new errorDetails();

            var result = ac.PostItem("ContactDotNetAPI", reqCPerson, resCPerson);

            if (result.Result.Item1 != null)
                resCPerson = result.Result.Item1;

            if (result.Result.Item2.message != null)
                ed = result.Result.Item2;

            return resCPerson;
        }

        [HttpPost]
        [Route("AddNewBillToAddress")]
        public SPInqNewShiptoAddressRes AddNewBillToAddress(SPInqNewShiptoAddress reqNewShiptoAddress)
        {
            API ac = new API();
            SPInqNewShiptoAddressRes resNewShiptoAddress = new SPInqNewShiptoAddressRes();
            errorDetails ed = new errorDetails();

            reqNewShiptoAddress.Address_2 = reqNewShiptoAddress.Address_2 == null || reqNewShiptoAddress.Address_2 == "" ? "" : reqNewShiptoAddress.Address_2;
            reqNewShiptoAddress.Ship_to_GST_Customer_Type = "Registered";

            var result = PostItemAddNewShiptoAddress("ShiptoAddressDotNetAPI", reqNewShiptoAddress, resNewShiptoAddress);

            resNewShiptoAddress = result.Result.Item1;
            ed = result.Result.Item2;
            resNewShiptoAddress.errorDetails = ed;

            return resNewShiptoAddress;
        }

        [HttpPost]
        [Route("AddNewDeliveryToAddress")]
        public SPInqNewJobtoAddressRes AddNewDeliveryToAddress(SPInqNewJobtoAddress reqNewJobtoAddress)
        {
            API ac = new API();
            SPInqNewJobtoAddressRes resNewJobtoAddress = new SPInqNewJobtoAddressRes();
            errorDetails ed = new errorDetails();

            reqNewJobtoAddress.Address_2 = reqNewJobtoAddress.Address_2 == null || reqNewJobtoAddress.Address_2 == "" ? "" : reqNewJobtoAddress.Address_2;
            reqNewJobtoAddress.Job_to_GST_Customer_Type = "Registered";

            var result = PostItemAddNewJobtoAddress("JobtoAddressDotNetAPI", reqNewJobtoAddress, resNewJobtoAddress);

            resNewJobtoAddress = result.Result.Item1;
            ed = result.Result.Item2;
            resNewJobtoAddress.errorDetails = ed;

            return resNewJobtoAddress;
        }

        [Route("GetDetailsByCode")]
        public List<PostCodes> GetDetailsByCode(string Code)
        {
            API ac = new API();
            List<PostCodes> postcodes = new List<PostCodes>();

            var result = ac.GetData<PostCodes>("PostCodesDotNetAPI", "Code eq '" + Code + "'");

            if (result != null && result.Result.Item1.value.Count > 0)
                postcodes = result.Result.Item1.value;

            return postcodes;
        }

        [Route("GetAreasByPincodeForDDL")]
        public List<Area> GetAreasByPincodeForDDL(string Pincode)
        {
            API ac = new API();
            List<Area> areas = new List<Area>();

            var result = ac.GetData<Area>("AreasListDotNetAPI", "Pincode eq '" + Pincode + "'");

            if (result != null && result.Result.Item1.value.Count > 0)
                areas = result.Result.Item1.value;

            return areas;
        }

        [Route("GetShortcloseReasons")]
        public List<SPSQShortcloseReasons> GetShortcloseReasons()
        {
            API ac = new API();
            List<SPSQShortcloseReasons> shortCloseReasons = new List<SPSQShortcloseReasons>();

            var result = ac.GetData<SPSQShortcloseReasons>("ShortCloseReasonDotNetAPI", "");

            if (result != null && result.Result.Item1.value.Count > 0)
                shortCloseReasons = result.Result.Item1.value;

            return shortCloseReasons;
        }

        [Route("GetSalesQuoteJustificationDetails")]
        public List<SPSQJustificationDetails> GetSalesQuoteJustificationDetails(int skip, int top, string orderby, string filter)
        {
            API ac = new API();
            List<SPSQJustificationDetails> salesquotes = new List<SPSQJustificationDetails>();

            var result = ac.GetData1<SPSQJustificationDetails>("SalesQuoteDotNetAPI", filter, skip, top, orderby);

            if (result.Result.Item1.value.Count > 0)
                salesquotes = result.Result.Item1.value;

            for (int a = 0; a < salesquotes.Count; a++)
            {
                DateTime date_ = Convert.ToDateTime(salesquotes[a].PCPL_Target_Date);
                salesquotes[a].PCPL_Target_Date = date_.ToString("dd/MM/yyyy");
            }

            return salesquotes;
        }

        [Route("GetCompanyIndustry")]
        public SPSQCompanyIndustry GetCompanyIndustry(string CCompanyNo)
        {
            API ac = new API();
            SPSQCompanyIndustry companyIndustry = new SPSQCompanyIndustry();

            var result = ac.GetData<SPSQCompanyIndustry>("ContactDotNetAPI", "No eq '" + CCompanyNo + "'"); // and Contact_Business_Relation eq 'Customer'

            if (result.Result.Item1.value.Count > 0)
                companyIndustry = result.Result.Item1.value[0];

            return companyIndustry;

        }

        public async Task<(SPSQHeader, errorDetails)> PostItemSQ<SPSQHeader>(string apiendpoint, SPSQHeaderPost requestModel, SPSQHeader responseModel)
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
            catch (Exception)
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
                    responseModel = res.ToObject<SPSQHeader>();

                    errordetail.code = response.StatusCode.ToString();
                    errordetail.message = response.ReasonPhrase;
                }
                catch (Exception)
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
                catch (Exception)
                {
                }
            }
            return (responseModel, errordetail);
        }

        public async Task<(SPSQHeader, errorDetails)> PostItemSQWithCustTemplateCode<SPSQHeader>(string apiendpoint, SPSQHeaderPostWithCustTemplateCode requestModel, SPSQHeader responseModel)
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
            catch (Exception)
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
                    responseModel = res.ToObject<SPSQHeader>();

                    errordetail.code = response.StatusCode.ToString();
                    errordetail.message = response.ReasonPhrase;
                }
                catch (Exception)
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
                catch (Exception)
                {
                }
            }
            return (responseModel, errordetail);
        }

        public async Task<(SPSQLines, errorDetails)> PostItemSQLines<SPSQLines>(string apiendpoint, string SQLineType, SPSQLinesPost requestModel, SPSQLiquidLinesPost requestModel1, SPSQLines responseModel)
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

            string ItemCardObjString = "";

            if (SQLineType == "SQLine")
                ItemCardObjString = JsonConvert.SerializeObject(requestModel);
            else if (SQLineType == "SQLiquidLine")
                ItemCardObjString = JsonConvert.SerializeObject(requestModel1);

            var content = new StringContent(ItemCardObjString, Encoding.UTF8, "application/json");
            HttpResponseMessage response = null;
            try
            {
                response = _httpClient.PostAsync(baseuri, content).Result;
            }
            catch (Exception)
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
                    responseModel = res.ToObject<SPSQLines>();

                    errordetail.code = response.StatusCode.ToString();
                    errordetail.message = response.ReasonPhrase;
                }
                catch (Exception)
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
                catch (Exception)
                {
                }
            }
            return (responseModel, errordetail);
        }

        public async Task<(SPSQGetNextNoRes, errorDetails)> PostItemForSQGetNextNo<SPSQGetNextNoRes>(string apiendpoint, SPSQGetNextNo requestModel, SPSQGetNextNoRes responseModel)
        {
            string _baseURL = System.Configuration.ConfigurationManager.AppSettings["BaseURL"];
            string _tenantId = System.Configuration.ConfigurationManager.AppSettings["TenantID"];
            string _environment = System.Configuration.ConfigurationManager.AppSettings["Environment"];
            string _companyName = System.Configuration.ConfigurationManager.AppSettings["CompanyName"];

            API ac = new API();
            var accessToken = await ac.GetAccessToken();

            HttpClient _httpClient = new HttpClient();
            string encodeurl = Uri.EscapeUriString("https://api.businesscentral.dynamics.com/v2.0/e55ad508-ef1a-489f-afe3-ae21f856e440/Sandbox/ODataV4/DeleteDotNetAPIs_GetNextNo?Company=\'Prakash Company\'");
            Uri baseuri = new Uri(encodeurl);
            _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken.Token);


            string ItemCardObjString = JsonConvert.SerializeObject(requestModel);
            var content = new StringContent(ItemCardObjString, Encoding.UTF8, "application/json");
            HttpResponseMessage response = null;
            try
            {
                response = _httpClient.PostAsync(baseuri, content).Result;
            }
            catch (Exception)
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
                    responseModel = res.ToObject<SPSQGetNextNoRes>();

                    errordetail.code = response.StatusCode.ToString();
                    errordetail.message = response.ReasonPhrase;
                }
                catch (Exception)
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
                catch (Exception)
                {
                }
            }
            return (responseModel, errordetail);
        }

        public async Task<(List<SPSQShiptoAddressRes>, errorDetails)> PostItemForSQGetShiptoAddress<SPSQShiptoAddressRes>(string apiendpoint, SPSQShiptoAddress requestModel, List<SPSQShiptoAddressRes> responseModel)
        {
            string _baseURL = System.Configuration.ConfigurationManager.AppSettings["BaseURL"];
            string _tenantId = System.Configuration.ConfigurationManager.AppSettings["TenantID"];
            string _environment = System.Configuration.ConfigurationManager.AppSettings["Environment"];
            string _companyName = System.Configuration.ConfigurationManager.AppSettings["CompanyName"];

            API ac = new API();
            var accessToken = await ac.GetAccessToken();

            HttpClient _httpClient = new HttpClient();
            string encodeurl = Uri.EscapeUriString("https://api.businesscentral.dynamics.com/v2.0/e55ad508-ef1a-489f-afe3-ae21f856e440/Sandbox/ODataV4/CodeunitAPIMgmt_GetShipToAddress?company=\'Prakash Company\'");
            Uri baseuri = new Uri(encodeurl);
            _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken.Token);


            string ItemCardObjString = JsonConvert.SerializeObject(requestModel);
            var content = new StringContent(ItemCardObjString, Encoding.UTF8, "application/json");
            HttpResponseMessage response = null;
            try
            {
                response = _httpClient.PostAsync(baseuri, content).Result;
            }
            catch (Exception)
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
                    SPSQAddressOData shiptoAddressOData = res.ToObject<SPSQAddressOData>();
                    string shipToAddressData = shiptoAddressOData.value.ToString();
                    responseModel = JsonConvert.DeserializeObject<List<SPSQShiptoAddressRes>>(shipToAddressData);

                    errordetail.code = response.StatusCode.ToString();
                    errordetail.message = response.ReasonPhrase;
                }
                catch (Exception)
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
                catch (Exception)
                {
                }
            }
            return (responseModel, errordetail);
        }

        public async Task<(List<SPSQJobtoAddressRes>, errorDetails)> PostItemForSQGetJobtoAddress<SPSQJobtoAddressRes>(string apiendpoint, SPSQJobtoAddress requestModel, List<SPSQJobtoAddressRes> responseModel)
        {
            string _baseURL = System.Configuration.ConfigurationManager.AppSettings["BaseURL"];
            string _tenantId = System.Configuration.ConfigurationManager.AppSettings["TenantID"];
            string _environment = System.Configuration.ConfigurationManager.AppSettings["Environment"];
            string _companyName = System.Configuration.ConfigurationManager.AppSettings["CompanyName"];

            API ac = new API();
            var accessToken = await ac.GetAccessToken();

            HttpClient _httpClient = new HttpClient();
            string encodeurl = Uri.EscapeUriString("https://api.businesscentral.dynamics.com/v2.0/e55ad508-ef1a-489f-afe3-ae21f856e440/Sandbox/ODataV4/APIMngt_GetJobToAddress?company=\'Prakash Company\'");
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
                    SPSQAddressOData jobtoAddressOData = res.ToObject<SPSQAddressOData>();
                    string jobToAddressData = jobtoAddressOData.value.ToString();
                    responseModel = JsonConvert.DeserializeObject<List<SPSQJobtoAddressRes>>(jobToAddressData);

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

        public async Task<(List<SPSQInvDetailsRes>, errorDetails)> PostItemForGetInventoryDetails<SPSQInvDetailsRes>(string apiendpoint, SPSQInvDetails requestModel, List<SPSQInvDetailsRes> responseModel)
        {
            string _baseURL = System.Configuration.ConfigurationManager.AppSettings["BaseURL"];
            string _tenantId = System.Configuration.ConfigurationManager.AppSettings["TenantID"];
            string _environment = System.Configuration.ConfigurationManager.AppSettings["Environment"];
            string _companyName = System.Configuration.ConfigurationManager.AppSettings["CompanyName"];

            API ac = new API();
            var accessToken = await ac.GetAccessToken();

            HttpClient _httpClient = new HttpClient();
            string encodeurl = Uri.EscapeUriString("https://api.businesscentral.dynamics.com/v2.0/e55ad508-ef1a-489f-afe3-ae21f856e440/Sandbox/ODataV4/CreateAvailableQty_GetAvailableQty?company=\'Prakash Company\'");
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
                    SPSQInvDetailsOData invDetailsOData = res.ToObject<SPSQInvDetailsOData>();
                    string invDetailsData = invDetailsOData.value.ToString();
                    responseModel = JsonConvert.DeserializeObject<List<SPSQInvDetailsRes>>(invDetailsData);

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

        public async Task<(SPSQHeader, errorDetails)> PatchItemSQ<SPSQHeader>(string apiendpoint, SPSQHeaderUpdate requestModel, SPSQHeader responseModel, string fieldWithValue)
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

            // Avoid patching nulls (BC may reset option/string fields to default like '0').
            // Only send explicitly set properties.
            var jsonSettings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
            string ItemCardObjString = JsonConvert.SerializeObject(requestModel, jsonSettings);
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
                    responseModel = res.ToObject<SPSQHeader>();


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


        public async Task<(SPSQLines, errorDetails)> PatchItemSQLines<SPSQLines>(string apiendpoint, string SQLineType, SPSQLinesUpdate requestModel, SPSQLiquidLinesUpdate requestModel1, SPSQLines responseModel, string fieldWithValue)
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

            string ItemCardObjString = "";

            if (SQLineType == "SQLine")
                ItemCardObjString = JsonConvert.SerializeObject(requestModel);
            else if (SQLineType == "SQLiquidLine")
                ItemCardObjString = JsonConvert.SerializeObject(requestModel1);

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
                    responseModel = res.ToObject<SPSQLines>();


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

        public async Task<(SPInqLines, errorDetails)> PatchItemInqToQuote<SPInqLines>(string apiendpoint, SPSQUpdateInqToQuote requestModel, SPInqLines responseModel, string fieldWithValue)
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
                    responseModel = res.ToObject<SPInqLines>();


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

        public async Task<(SPInquiry, errorDetails)> PatchItemInquiryStatus<SPInquiry>(string apiendpoint, SPInquiryUpdate requestModel, SPInquiry responseModel, string fieldWithValue)
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
                    responseModel = res.ToObject<SPInquiry>();

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


        public async Task<(SPSQScheduleQty, errorDetails)> PatchItemScheduleQty<SPSQScheduleQty>(string apiendpoint, SPSQScheduleQtyPost requestModel, SPSQScheduleQty responseModel, string fieldWithValue)
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
                    responseModel = res.ToObject<SPSQScheduleQty>();


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

        public async Task<(SPSQScheduleOrderOData, errorDetails)> PostItemForScheduleOrder<SPSQScheduleOrderOData>(string apiendpoint, SPSQScheduleOrderPost requestModel, SPSQScheduleOrderOData responseModel)
        {
            string _baseURL = System.Configuration.ConfigurationManager.AppSettings["BaseURL"];
            string _tenantId = System.Configuration.ConfigurationManager.AppSettings["TenantID"];
            string _environment = System.Configuration.ConfigurationManager.AppSettings["Environment"];
            string _companyName = System.Configuration.ConfigurationManager.AppSettings["CompanyName"];

            API ac = new API();
            var accessToken = await ac.GetAccessToken();

            HttpClient _httpClient = new HttpClient();
            string encodeurl = Uri.EscapeUriString("https://api.businesscentral.dynamics.com/v2.0/e55ad508-ef1a-489f-afe3-ae21f856e440/Sandbox/ODataV4/CodeunitEventMngt_MakeOrder?company=\'Prakash Company\'");
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
                    responseModel = res.ToObject<SPSQScheduleOrderOData>();

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

        //public async Task<(SPSQDetailsForPrintOData, errorDetails)> PostItemForGetSQDetailsForPrint<SPSQDetailsForPrintOData>(string apiendpoint, SPSQDetailsForPrintPost requestModel, SPSQDetailsForPrintOData responseModel)
        //{
        //    string _baseURL = System.Configuration.ConfigurationManager.AppSettings["BaseURL"];
        //    string _tenantId = System.Configuration.ConfigurationManager.AppSettings["TenantID"];
        //    string _environment = System.Configuration.ConfigurationManager.AppSettings["Environment"];
        //    string _companyName = System.Configuration.ConfigurationManager.AppSettings["CompanyName"];

        //    API ac = new API();
        //    var accessToken = await ac.GetAccessToken();

        //    HttpClient _httpClient = new HttpClient();
        //    string encodeurl = Uri.EscapeUriString("https://api.businesscentral.dynamics.com/v2.0/e55ad508-ef1a-489f-afe3-ae21f856e440/Sandbox/ODataV4/APIMngt_SalesQuoteReportPrint?company=\'Prakash Company\'");
        //    Uri baseuri = new Uri(encodeurl);
        //    _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken.Token);


        //    string ItemCardObjString = JsonConvert.SerializeObject(requestModel);
        //    var content = new StringContent(ItemCardObjString, Encoding.UTF8, "application/json");
        //    HttpResponseMessage response = null;
        //    try
        //    {
        //        response = _httpClient.PostAsync(baseuri, content).Result;
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //    errorDetails errordetail = new errorDetails();
        //    errordetail.isSuccess = response.IsSuccessStatusCode;
        //    if (response.IsSuccessStatusCode)
        //    {
        //        var JsonData = response.Content.ReadAsStringAsync().Result;
        //        try
        //        {
        //            JObject res = JObject.Parse(JsonData);
        //            //SPSQScheduleOrderOData scheduleOrderOData = res.ToObject<SPSQScheduleOrderOData>();
        //            responseModel = res.ToObject<SPSQDetailsForPrintOData>();

        //            //string scheduleOrderData = "{\"value\":" + scheduleOrderOData.value + "}";
        //            //responseModel = JsonConvert.DeserializeObject<SPSQScheduleOrder>(scheduleOrderData);

        //            errordetail.code = response.StatusCode.ToString();
        //            errordetail.message = response.ReasonPhrase;
        //        }
        //        catch (Exception ex1)
        //        {
        //        }
        //    }
        //    else
        //    {
        //        var JsonData = response.Content.ReadAsStringAsync().Result;

        //        try
        //        {
        //            JObject res = JObject.Parse(JsonData);
        //            errorMaster<errorDetails> emd = res.ToObject<errorMaster<errorDetails>>();
        //            errordetail = emd.error;
        //        }
        //        catch (Exception ex1)
        //        {
        //        }
        //    }
        //    return (responseModel, errordetail);
        //}

        public async Task<(PrintQuoteResponse, errorDetails)> PostQuotePrint<PrintQuoteRequest>(string apiendpoint, PrintQuoteRequest requestModel, PrintQuoteResponse responseModel)
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
                    responseModel = res.ToObject<PrintQuoteResponse>();

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

        public async Task<(SPSQUpdateInqStatusOData, errorDetails)> PostItemForUpdateInqStatus<SPSQUpdateInqStatusOData>(string apiendpoint, SPSQUpdateInqStatus requestModel, SPSQUpdateInqStatusOData responseModel)
        {
            string _baseURL = System.Configuration.ConfigurationManager.AppSettings["BaseURL"];
            string _tenantId = System.Configuration.ConfigurationManager.AppSettings["TenantID"];
            string _environment = System.Configuration.ConfigurationManager.AppSettings["Environment"];
            string _companyName = System.Configuration.ConfigurationManager.AppSettings["CompanyName"];

            API ac = new API();
            var accessToken = await ac.GetAccessToken();

            HttpClient _httpClient = new HttpClient();
            string encodeurl = Uri.EscapeUriString("https://api.businesscentral.dynamics.com/v2.0/e55ad508-ef1a-489f-afe3-ae21f856e440/Sandbox/ODataV4/CodeunitAPIMgmt_UpdateInquirytoQuoteandStatus?company=\'Prakash Company\'");
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
                    responseModel = res.ToObject<SPSQUpdateInqStatusOData>();

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

        public async Task<(SPSQInvQtyReserveOData, errorDetails)> PostItemInvQtyReserve<SPSQInvQtyReserveOData>(string apiendpoint, List<SPSQInvQtyReserve> requestModel, SPSQInvQtyReserveOData responseModel)
        {
            string _baseURL = System.Configuration.ConfigurationManager.AppSettings["BaseURL"];
            string _tenantId = System.Configuration.ConfigurationManager.AppSettings["TenantID"];
            string _environment = System.Configuration.ConfigurationManager.AppSettings["Environment"];
            string _companyName = System.Configuration.ConfigurationManager.AppSettings["CompanyName"];

            API ac = new API();
            var accessToken = await ac.GetAccessToken();

            HttpClient _httpClient = new HttpClient();
            string encodeurl = Uri.EscapeUriString("https://api.businesscentral.dynamics.com/v2.0/e55ad508-ef1a-489f-afe3-ae21f856e440/Sandbox/ODataV4/APIMngt_InsertReservationEntry?company=\'Prakash Company\'");
            Uri baseuri = new Uri(encodeurl);
            _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken.Token);

            SPSQInvQtyReservePost invQtyReservePost = new SPSQInvQtyReservePost();
            //invQtyReservePost.text = requestModel;
            string ObjString_ = JsonConvert.SerializeObject(requestModel);
            string txtString = ObjString_.Replace("\"", "'");
            invQtyReservePost.text = txtString;
            string txtString_ = JsonConvert.SerializeObject(invQtyReservePost);
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
                    SPSQInvQtyReserveOData invQtyReserveOData = res.ToObject<SPSQInvQtyReserveOData>();
                    responseModel = invQtyReserveOData;

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

        public async Task<(SPSQCostSheetOData, errorDetails)> PostItemForGenerateCostSheet<SPSQCostSheetOData>(string apiendpoint, SPSQCostSheet requestModel, SPSQCostSheetOData responseModel)
        {
            string _baseURL = System.Configuration.ConfigurationManager.AppSettings["BaseURL"];
            string _tenantId = System.Configuration.ConfigurationManager.AppSettings["TenantID"];
            string _environment = System.Configuration.ConfigurationManager.AppSettings["Environment"];
            string _companyName = System.Configuration.ConfigurationManager.AppSettings["CompanyName"];

            API ac = new API();
            var accessToken = await ac.GetAccessToken();

            HttpClient _httpClient = new HttpClient();
            string encodeurl = Uri.EscapeUriString("https://api.businesscentral.dynamics.com/v2.0/e55ad508-ef1a-489f-afe3-ae21f856e440/Sandbox/ODataV4/Sales_Cost_Sheet_Mngt_CheckandCreateCostSheet?company=\'Prakash Company\'");
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
                    responseModel = res.ToObject<SPSQCostSheetOData>();
                    //SPSQCostSheetOData costSheetOData = res.ToObject<SPSQCostSheetOData>();

                    //string costSheetData = "{\"value\":" + costSheetOData.value + "}";
                    //responseModel = JsonConvert.DeserializeObject<SPSQScheduleOrder>(costSheetData);

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


        public async Task<(SPSQCostSheetDetails, errorDetails)> PatchItemCostSheet<SPSQCostSheetDetails>(string apiendpoint, SPSQUpdateCostSheet requestModel, SPSQCostSheetDetails responseModel, string fieldWithValue)
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
                    responseModel = res.ToObject<SPSQCostSheetDetails>();


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

        public async Task<(SPSQShortcloseOData, errorDetails)> PostItemSQShortclose<SPSQShortcloseOData>(string apiendpoint, SPSQShortclose requestModel, SPSQShortcloseOData responseModel)
        {
            string _baseURL = System.Configuration.ConfigurationManager.AppSettings["BaseURL"];
            string _tenantId = System.Configuration.ConfigurationManager.AppSettings["TenantID"];
            string _environment = System.Configuration.ConfigurationManager.AppSettings["Environment"];
            string _companyName = System.Configuration.ConfigurationManager.AppSettings["CompanyName"];

            API ac = new API();
            var accessToken = await ac.GetAccessToken();

            HttpClient _httpClient = new HttpClient();
            string encodeurl = Uri.EscapeUriString("https://api.businesscentral.dynamics.com/v2.0/e55ad508-ef1a-489f-afe3-ae21f856e440/Sandbox/ODataV4/ShortCloseMngt_ShortCloseQuote?company=\'Prakash Company\'");
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
                    responseModel = res.ToObject<SPSQShortcloseOData>();

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

        public async Task<(SPSQShortcloseOData, errorDetails)> PostItemSQProdShortclose<SPSQShortcloseOData>(string apiendpoint, SPSQProdShortclose requestModel, SPSQShortcloseOData responseModel)
        {
            string _baseURL = System.Configuration.ConfigurationManager.AppSettings["BaseURL"];
            string _tenantId = System.Configuration.ConfigurationManager.AppSettings["TenantID"];
            string _environment = System.Configuration.ConfigurationManager.AppSettings["Environment"];
            string _companyName = System.Configuration.ConfigurationManager.AppSettings["CompanyName"];

            API ac = new API();
            var accessToken = await ac.GetAccessToken();

            HttpClient _httpClient = new HttpClient();
            string encodeurl = Uri.EscapeUriString("https://api.businesscentral.dynamics.com/v2.0/e55ad508-ef1a-489f-afe3-ae21f856e440/Sandbox/ODataV4/ShortCloseMngt_ShortCloseQuoteLine?company=\'Prakash Company\'");
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
                    responseModel = res.ToObject<SPSQShortcloseOData>();

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

        public async Task<(SPSQHeader, errorDetails)> PatchItemSQApproveReject<SPSQHeader>(string apiendpoint, SPSQForApprove requestModelApprove, SPSQForReject requestModelReject, SPSQHeader responseModel, string Action, string fieldWithValue)
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

            string ItemCardObjString = "";

            if (Action == "Approve")
                ItemCardObjString = JsonConvert.SerializeObject(requestModelApprove);
            else if (Action == "Reject")
                ItemCardObjString = JsonConvert.SerializeObject(requestModelReject);

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
                    responseModel = res.ToObject<SPSQHeader>();


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

        public async Task<(SPSQHeader, errorDetails)> PatchItemSQApproveRejectHOD<SPSQHeader>(string apiendpoint, SPSQForApproveHOD requestModelApprove, SPSQForRejectHOD requestModelReject, SPSQHeader responseModel, string Action, string fieldWithValue)
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

            string ItemCardObjString = "";

            if (Action == "Approve")
                ItemCardObjString = JsonConvert.SerializeObject(requestModelApprove);
            else if (Action == "Reject")
                ItemCardObjString = JsonConvert.SerializeObject(requestModelReject);

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
                    responseModel = res.ToObject<SPSQHeader>();


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

        public async Task<(SPInqNewShiptoAddressRes, errorDetails)> PostItemAddNewShiptoAddress<SPInqNewShiptoAddressRes>(string apiendpoint, SPInqNewShiptoAddress requestModel, SPInqNewShiptoAddressRes responseModel)
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
                    responseModel = res.ToObject<SPInqNewShiptoAddressRes>();

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

        public async Task<(SPInqNewJobtoAddressRes, errorDetails)> PostItemAddNewJobtoAddress<SPInqNewJobtoAddressRes>(string apiendpoint, SPInqNewJobtoAddress requestModel, SPInqNewJobtoAddressRes responseModel)
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
                    responseModel = res.ToObject<SPInqNewJobtoAddressRes>();

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

        public static string Unzip(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
                return "";

            try
            {
                using (var msi = new MemoryStream(bytes))
                using (var mso = new MemoryStream())
                {
                    using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                    {
                        gs.CopyTo(mso);
                    }
                    return Encoding.UTF8.GetString(mso.ToArray());
                }
            }
            catch (InvalidDataException)
            {
                // Not gzipped (or corrupted). Best-effort: treat as plain UTF8.
                return Encoding.UTF8.GetString(bytes);
            }
            catch
            {
                return "";
            }
        }

        public static void CopyTo(Stream src, Stream dest)
        {
            byte[] bytes = new byte[4096];

            int cnt;

            while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
            {
                dest.Write(bytes, 0, cnt);
            }
        }
        // Dispatch Details list 

        [Route("GetDispatchDetails")]
        public List<SPDispacthDetails> GetDispatchDetails(string FromDate, string ToDate, string Search)
        {
            API ac = new API();
            List<SPDispacthDetails> dispatchDetails = new List<SPDispacthDetails>();

            string filter = string.Empty;
            if (!string.IsNullOrEmpty(FromDate) && !string.IsNullOrEmpty(ToDate))
            {
                filter = $"Posting_Date ge {FromDate} and Posting_Date le {ToDate}";
            }
            else if (!string.IsNullOrEmpty(Search))
            {
                filter = $"No eq '{Search}'";
            }
            var result = ac.GetData<SPDispacthDetails>("DispatchDetailsDotNetAPI", filter);

            if (result != null && result.Result.Item1.value != null && result.Result.Item1.value.Count > 0)
            {
                dispatchDetails = result.Result.Item1.value;
            }

            return dispatchDetails;
        }

        [HttpGet]
        [Route("GetCSOutstandingDuelist")]
        public List<CSOutstandingDuelist> GetCSOutstandingDuelist(string CustomerName /*,string ProductName*/)

        {
            API ac = new API();
            List<CSOutstandingDuelist> csutstandingDuelist = new List<CSOutstandingDuelist>();
            // "PCPL_Secondary_SP_Code eq '" + SPCode + "' and Salesperson_Code ne '" + SPCode + "' and Type eq 'Company'");

            var result = ac.GetData<CSOutstandingDuelist>("CustomerOverDueDotNetAPI", "Customer_Name eq '" + CustomerName +/* "and Product_Name eq '" + ProductName +*/ "'");//
            if (result != null && result.Result.Item1.value.Count > 0)
            {
                csutstandingDuelist = result.Result.Item1.value;
            }
            return csutstandingDuelist;
        }

        [HttpGet]
        [Route("GetTransporterMethod")]
        public List<TransporterRateMethods> GetTransporterMethod(string PackingUOMs, string FromToPincode, string JobToPincode)
        {
            API ac = new API();
            List<TransporterRateMethods> transporterRateCards = new List<TransporterRateMethods>();

            string filter = "";

            if (!string.IsNullOrEmpty(PackingUOMs) && !string.IsNullOrEmpty(FromToPincode) && !string.IsNullOrEmpty(JobToPincode))
            {
                filter = "UOM eq '" + PackingUOMs + "' and From_Post_Code eq '" + FromToPincode + "' and To_Post_Code eq '" + JobToPincode + "' and Latest_Rate eq true";
            }


            var result = ac.GetData<TransporterRateMethods>("Transporter_Rate_Details", filter);

            if (result != null && result.Result.Item1.value.Count > 0)
            {
                transporterRateCards = result.Result.Item1.value;
            }

            return transporterRateCards;
        }

        [HttpGet]
        [Route("GetAvailableQuantity")]
        public List<SPAvailableQuantity> GetAvailableQuantity(string ProdNo, string LocationCode)
        {
            API ac = new API();
            List<SPAvailableQuantity> availablequantity = new List<SPAvailableQuantity>();

            string filter = "";

            if (!string.IsNullOrEmpty(ProdNo) && !string.IsNullOrEmpty(LocationCode))
            {
                filter = "Item_No eq '" + ProdNo + "' and Location_Code eq '" + LocationCode + "' and Remaining_Quantity gt 0";
            }

            var result = ac.GetData<SPAvailableQuantity>("ItemLedgerEntriesDotNetAPI", filter);

            if (result != null && result.Result.Item1.value.Count > 0)
                availablequantity = result.Result.Item1.value;

            return availablequantity;
        }

    }

}