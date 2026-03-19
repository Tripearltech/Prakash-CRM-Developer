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
using System.Web.Http;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Routing;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Xml.Linq;
using System.Web.Helpers;
using Microsoft.Ajax.Utilities;

namespace PrakashCRM.Service.Controllers
{
    [RoutePrefix("api/SPInquiry")]
    public class SPInquiryController : ApiController
    {
        [Route("GetAllInquiry")]
        public List<SPInquiryList> GetAllInquiry(string SPCode, int skip, int top, string orderby, string filter)
        {
            API ac = new API();
            List<SPInquiryList> inquiries = new List<SPInquiryList>();

            string SPCodes = "";
            if (SPCode.Contains(",") == true)
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

            if (filter == "" || filter == null)
            {
                if (SPCode.Contains(",") == true)
                    filter = SPCodes + " and Document_Type eq 'Quote' and PCPL_IsInquiry eq true";
                else
                    filter = "Salesperson_Code eq '" + SPCode + "' and Document_Type eq 'Quote' and PCPL_IsInquiry eq true";
            }
            else
            {
                if (SPCode.Contains(",") == true)
                    filter = filter + " and " + SPCodes + " and Document_Type eq 'Quote' and PCPL_IsInquiry eq true";
                else
                    filter = filter + " and Salesperson_Code eq '" + SPCode + "' and Document_Type eq 'Quote' and PCPL_IsInquiry eq true";

            }

            //if (filter == "" || filter == null)
            //    filter = "Salesperson_Code eq '" + SPCode + "' and Document_Type eq 'Quote' and PCPL_IsInquiry eq true";
            //else
            //    filter = filter + " and Salesperson_Code eq '" + SPCode + "' and Document_Type eq 'Quote' and PCPL_IsInquiry eq true";

            var result = ac.GetData1<SPInquiryList>("InquiryDotNetAPI", filter, skip, top, orderby);

            if (result.Result.Item1.value.Count > 0)
                inquiries = result.Result.Item1.value;

            for (int i = 0; i < inquiries.Count; i++)
            {
                string[] strDate = inquiries[i].Inquiry_Date.Split('-');
                inquiries[i].Inquiry_Date = strDate[2] + '-' + strDate[1] + '-' + strDate[0];

                //string[] strDate1 = inquiries[i].Delivery_Date.Split('-');
                //inquiries[i].Delivery_Date = strDate1[2] + '-' + strDate1[1] + '-' + strDate1[0];
            }

            return inquiries;
        }

        [Route("GetApiRecordsCount")]
        public int GetApiRecordsCount(string SPCode, string apiEndPointName, string filter)
        {
            API ac = new API();

            string SPCodes = "";
            if (SPCode.Contains(",") == true)
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

            if (filter == "" || filter == null)
            {
                if (SPCode.Contains(",") == true)
                    filter = SPCodes + " and Document_Type eq 'Quote' and PCPL_IsInquiry eq true";
                else
                    filter = "Salesperson_Code eq '" + SPCode + "' and Document_Type eq 'Quote' and PCPL_IsInquiry eq true";
            }
            else
            {
                if (SPCode.Contains(",") == true)
                    filter = filter + " and " + SPCodes + " and Document_Type eq 'Quote' and PCPL_IsInquiry eq true";
                else
                    filter = filter + " and Salesperson_Code eq '" + SPCode + "' and Document_Type eq 'Quote' and PCPL_IsInquiry eq true";

            }

            //if (filter == "" || filter == null)
            //    filter = "Salesperson_Code eq '" + SPCode + "' and Document_Type eq 'Quote' and PCPL_IsInquiry eq true";
            //else
            //    filter = filter + " and Salesperson_Code eq '" + SPCode + "' and Document_Type eq 'Quote' and PCPL_IsInquiry eq true";

            var count = ac.CalculateCount(apiEndPointName, filter);

            return Convert.ToInt32(count.Result);
        }

        [Route("GetAllInquiryProduct")]
        public List<SPInquiryProducts> GetAllInquiryProduct(string Document_No)
        {
            API ac = new API();
            List<SPInquiryProducts> inquiryproducts = new List<SPInquiryProducts>();

            var result = ac.GetData<SPInquiryProducts>("InquiryProductsDotNetAPI", "Document_No eq '" + Document_No + "'");

            if (result.Result.Item1.value.Count > 0)
                inquiryproducts = result.Result.Item1.value;

            return inquiryproducts;
        }

        [Route("GetAllCompanyForDDL")]
        public List<Company> GetAllCompanyForDDL(string SPNo, string CompanyNo)
        {
            API ac = new API();
            List<Company> company = new List<Company>();
            var filterSP1 = "";
            var filterSSP1 = "";
            if (CompanyNo != null)
            {
              filterSP1 = "Salesperson_Code eq '" + SPNo + "' and Type eq 'Company' and Company_No eq '" + CompanyNo + "'";
              filterSSP1 = "PCPL_Secondary_SP_Code eq '" + SPNo + "' and Salesperson_Code ne '" + SPNo + "' and Type eq 'Company' and Company_No eq '" + CompanyNo + "'";
            }
            else
            {
                filterSP1 = "Salesperson_Code eq '" + SPNo + "' and Type eq 'Company'";
              filterSSP1 = "PCPL_Secondary_SP_Code eq '" + SPNo + "' and Salesperson_Code ne '" + SPNo + "' and Type eq 'Company'";

            }

            var result = ac.GetData<Company>("ContactDotNetAPI", filterSP1);

            if (result != null && result.Result.Item1.value.Count > 0)
                company = result.Result.Item1.value;

            List<Company> company2 = new List<Company>();

            var result2 = ac.GetData<Company>("ContactDotNetAPI", filterSSP1);

            if (result2 != null && result2?.Result.Item1?.value.Count > 0)
            {
                company2 = result2.Result.Item1?.value;

                company.AddRange(company2);
            }

            return company;
        }

        [Route("GetAllContactForDDL")]
        public List<Contact> GetAllContactForDDL(string Company_No)
        {
            API ac = new API();
            List<Contact> contact = new List<Contact>();

            var result = ac.GetData<Contact>("ContactDotNetAPI", "Type eq 'Person' and Company_No eq '" + Company_No + "'");

            if (result != null && result.Result.Item1.value.Count > 0)
                contact = result.Result.Item1.value;

            return contact;
        }

        [Route("GetAllPaymentTermsForDDL")]
        public List<PaymentTerms> GetAllPaymentTermsForDDL()
        {
            API ac = new API();
            List<PaymentTerms> paymentTerms = new List<PaymentTerms>();

            var result = ac.GetData<PaymentTerms>("PaymentTermsDotNetAPI", "");

            if (result != null && result.Result.Item1.value.Count > 0)
                paymentTerms = result.Result.Item1.value;

            return paymentTerms;
        }

        [Route("GetAllSalespersonForDDL")]
        public List<UserInfo> GetAllSalespersonForDDL()
        {
            API ac = new API();
            List<UserInfo> salesperson = new List<UserInfo>();

            var result = ac.GetData<UserInfo>("EmployeesDotNetAPI", "Salespers_Purch_Code ne ''");

            if (result != null && result.Result.Item1.value.Count > 0)
            {
                salesperson = result.Result.Item1.value;
                salesperson = salesperson.OrderBy(a => a.First_Name).ToList();
            }
                

            return salesperson;
        }

        [Route("GetCustDetails")]
        public SPInqCustDetails GetCustDetails(string companyDetails)
        {
            API ac = new API();
            SPContCustForBusRel contCustForBusRel = new SPContCustForBusRel();
            SPConBusinessRelation conBusinessRelation = new SPConBusinessRelation();
            SPInqCustDetails custdetails = new SPInqCustDetails();

            if (companyDetails != "" || companyDetails != null)
            {

                string[] companyDetails_ = companyDetails.Split('_');
                string companyNo = companyDetails_[0];

                var resultCompanyNo = ac.GetData<ContCustForBusRel>("ContactDotNetAPI", "Type eq 'Company' and No eq '" + companyNo + "'");

                if (resultCompanyNo.Result.Item1.value.Count > 0)
                    contCustForBusRel.Company_No = resultCompanyNo.Result.Item1.value[0].Company_No;

                var resultCustomerNo = ac.GetData<ConBusinessRelation>("ContactBusinessRelationsDotNetAPI", "Contact_No eq '" + contCustForBusRel.Company_No + "'");

                if (resultCustomerNo.Result.Item1.value.Count > 0)
                {
                    conBusinessRelation.No = resultCustomerNo.Result.Item1.value[0].No;

                    var resultCustomer = ac.GetData<SPCustomer>("CustomerCardDotNetAPI", "No eq '" + conBusinessRelation.No + "'");

                    if (resultCustomer.Result.Item1.value.Count > 0)
                    {
                        custdetails.CustNo = resultCustomer.Result.Item1.value[0].No;
                        custdetails.CustName = resultCustomer.Result.Item1.value[0].Name;
                        custdetails.Address = resultCustomer.Result.Item1.value[0].Address;
                        custdetails.Address_2 = resultCustomer.Result.Item1.value[0].Address_2;
                        custdetails.City = resultCustomer.Result.Item1.value[0].City;
                        custdetails.Post_Code = resultCustomer.Result.Item1.value[0].Post_Code;
                        custdetails.Country_Region_Code = resultCustomer.Result.Item1.value[0].Country_Region_Code;
                        custdetails.PANNo = resultCustomer.Result.Item1.value[0].P_A_N_No;

                        SPInqShiptoAddress requestShiptoAddress = new SPInqShiptoAddress();
                        List<SPInqShiptoAddressRes> responseShiptoAddress = new List<SPInqShiptoAddressRes>();
                        errorDetails edForShiptoAddress = new errorDetails();
                        requestShiptoAddress.customerno = conBusinessRelation.No;

                        var resultGetShipToAddress = PostItemForInqGetShiptoAddress<SPInqShiptoAddressRes>("", requestShiptoAddress, responseShiptoAddress);
                        List<SPInqShiptoAddressRes> shiptoaddresses = new List<SPInqShiptoAddressRes>();

                        if (resultGetShipToAddress.Result.Item1.Count > 0)
                        {
                            responseShiptoAddress = resultGetShipToAddress.Result.Item1;
                            custdetails.ShiptoAddress = responseShiptoAddress.OrderBy(a => a.Address).ToList();
                        }

                        SPInqJobtoAddress requestJobtoAddress = new SPInqJobtoAddress();
                        List<SPInqJobtoAddressRes> responseJobtoAddress = new List<SPInqJobtoAddressRes>();
                        errorDetails edForJobtoAddress = new errorDetails();
                        requestJobtoAddress.customerno = conBusinessRelation.No;

                        var resultGetJobToAddress = PostItemForInqGetJobtoAddress<SPInqJobtoAddressRes>("", requestJobtoAddress, responseJobtoAddress);
                        List<SPInqJobtoAddressRes> jobtoaddresses = new List<SPInqJobtoAddressRes>();

                        if (resultGetJobToAddress.Result.Item1.Count > 0)
                        {
                            responseJobtoAddress = resultGetJobToAddress.Result.Item1;
                            custdetails.JobtoAddress = responseJobtoAddress.OrderBy(a => a.Address).ToList();
                        }

                    }

                }
                else
                {
                    SPCompanyList companyList = new SPCompanyList();
                    var resultCompanyDetails = ac.GetData<SPCompanyList>("ContactDotNetAPI", "Type eq 'Company' and No eq '" + contCustForBusRel.Company_No + "'");

                    if (resultCompanyDetails.Result.Item1.value.Count > 0)
                    {
                        companyList = resultCompanyDetails.Result.Item1.value[0];
                        custdetails.CompanyNo = companyList.No;
                        custdetails.CompanyName = companyList.Name;
                        custdetails.Address = companyList.Address;
                        custdetails.Address_2 = companyList.Address_2;
                        custdetails.City = companyList.City;
                        custdetails.Post_Code = companyList.Post_Code;
                        custdetails.Country_Region_Code = companyList.Country_Region_Code;
                    }
                }
            }
            return custdetails;
        }

        [Route("GetAllProductForDDL")]
        public List<Product> GetAllProductForDDL()
        {
            API ac = new API();
            List<Product> product = new List<Product>();

            var result = ac.GetData<Product>("ItemDotNetAPI", "");

            if (result != null && result.Result.Item1.value.Count > 0)
                product = result.Result.Item1.value;

            return product;
        }

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

        [Route("GetProductDetails")]
        public SPInqProductDetails GetProductDetails(string productName)
        {
            API ac = new API();
            SPInqProductDetails productdetails = new SPInqProductDetails();

            var result = ac.GetData<SPInqProductDetails>("ItemDotNetAPI", "Description eq '" + productName + "'"); // and Contact_Business_Relation eq 'Customer'

            if (result.Result.Item1.value.Count > 0)
                productdetails = result.Result.Item1.value[0];

            return productdetails;
        }

        [Route("GetProductPackingStyle")]
        public List<SPInqProductPackingStyle> GetProductPackingStyle(string prodNo)
        {
            API ac = new API();

            List<SPInqProductPackingStyle> packingStyle = new List<SPInqProductPackingStyle>();

            var result = ac.GetData<SPInqProductPackingStyle>("ItemPackingStyleDotNetAPI", "Item_No eq '" + prodNo + "'"); // and Contact_Business_Relation eq 'Customer'

            if (result.Result.Item1.value.Count > 0)
                packingStyle = result.Result.Item1.value;

            return packingStyle;
        }

        [HttpPost]
        [Route("AddUpdateOnSaveProd")]
        public bool AddUpdateOnSaveProd(SPInqLinesPost inquiryLine, bool isEdit, string LineNo)
        {
            SPInqLines resInqLines = new SPInqLines();
            SPInqLinesUpdate inqLineUpdate = new SPInqLinesUpdate();
            errorDetails ed = new errorDetails();

            var result = (dynamic)null;

            if (isEdit)
            {
                inqLineUpdate.Quantity = inquiryLine.Quantity;
                result = PatchItemInqLines("InquiryProductsDotNetAPI", inqLineUpdate, resInqLines, "Document_No='" + inquiryLine.Document_No + "',Line_No=" + LineNo);
            }
            else
                result = PostItemInqLines("InquiryProductsDotNetAPI", inquiryLine, resInqLines);

            if (result.Result.Item1 != null)
                resInqLines = result.Result.Item1;

            if (result.Result.Item2.message != null)
                ed = result.Result.Item2;

            return true;
        }

        //[Route("GetNewProductDetails")]
        //public List<SPNewInquiryProducts> GetNewProductDetails()
        //{
        //    API ac = new API();
        //    List<SPNewInquiryProducts> inquiryproducts = new List<SPNewInquiryProducts>();

        //    var result = ac.GetData<SPNewInquiryProducts>("InquiryProductsDotNetAPI", "");

        //    if (result.Result.Item1.value.Count > 0)
        //        inquiryproducts = result.Result.Item1.value;

        //    return inquiryproducts;
        //}

        [Route("GetConsigneeAddress")]
        public List<ConsigneeAddress> GetConsigneeAddress(string Company_No)
        {
            API ac = new API();
            ContactBusinessRel contactBusinessRel = new ContactBusinessRel();

            List<ConsigneeAddress> consigneeAddress = new List<ConsigneeAddress>();

            var resultCustomerNo = ac.GetData<ContactBusinessRel>("ContactBusinessRelationsDotNetAPI", "Contact_No eq '" + Company_No + "'");
            if (resultCustomerNo != null && resultCustomerNo.Result.Item1.value.Count > 0)
            {
                contactBusinessRel.No = resultCustomerNo.Result.Item1.value[0].No;
            }

            var payload = new
            {
                customerno = contactBusinessRel.No
            };

            var resultShiptoAddress = ac.GetDataCodeUnit<ConsigneeAddress>("CodeunitAPIMgmt_GetShipToAddress", payload);

            if (resultShiptoAddress != null && resultShiptoAddress.Result.Item1.value.Count > 0)
            {
                consigneeAddress = resultShiptoAddress.Result.Item1.value;
                //consigneeAddress[0].CustomerNo = contactBusinessRel.No;

            }
            else
            {

            }
            var dummyConsignee = new ConsigneeAddress
            {
                CustomerNo = contactBusinessRel.No,
                IsDummy = true,
            };
            consigneeAddress.Add(dummyConsignee);

            return consigneeAddress;
        }

        [Route("GetGeneratedInqNo")]
        public string GetGeneratedInqNo()
        {
            SPInqNextNoOData responseInqGetNextNoRes = new SPInqNextNoOData();
            errorDetails edForGetNextNo = new errorDetails();

            var resultGetNextNo = PostItemForInqGetNextNo("", "", responseInqGetNextNoRes);
            string generatedInqNo = "";

            if (resultGetNextNo.Result.Item1 != null)
                generatedInqNo = resultGetNextNo.Result.Item1.value;

            return generatedInqNo;
        }

        [Route("Inquiry")]
        public SPInquiry Inquiry(SPInqHeaderDetails inquiryheader, bool isEdit, string IQNo, string portalUrl)
        {
            errorDetails ed = new errorDetails();
            SPInquiryPost requestInquiry = new SPInquiryPost();
            SPInquiryPostWithCustTemplateCode reqInqWithCustTemplateCode =
                new SPInquiryPostWithCustTemplateCode();
            SPInquiryUpdate reqInquiryUpdate = new SPInquiryUpdate();
            SPInquiry responseInquiry = new SPInquiry();
            var result = (dynamic)null;

            if (!isEdit)
            {
                if(inquiryheader.CustomerNo != "")
                {
                    requestInquiry.Inquiry_No = inquiryheader.InquiryNo;
                    requestInquiry.Inquiry_Date = inquiryheader.InquiryDate;
                    requestInquiry.Inquiry_Customer_Contact = inquiryheader.ContactCompanyNo;
                    requestInquiry.Contact_Company_Name = inquiryheader.CustomerName;
                    requestInquiry.PCPL_Contact_Person = inquiryheader.ContactPersonNo;
                    requestInquiry.Inquiry_Customer_No = inquiryheader.CustomerNo;
                    requestInquiry.Ship_to_Code = inquiryheader.ShiptoCode != "-1" ? inquiryheader.ShiptoCode : "";
                    requestInquiry.PCPL_Job_to_Code = inquiryheader.JobtoCode != "-1" ? inquiryheader.JobtoCode : "";
                    requestInquiry.Payment_Terms = inquiryheader.PaymentTerms;

                    if (inquiryheader.Inquiry_Status_Remarks == null)
                        requestInquiry.Inquiry_Status_Remarks = "";
                    else
                        requestInquiry.Inquiry_Status_Remarks = inquiryheader.Inquiry_Status_Remarks;

                    requestInquiry.Inquiry_Status = "Pending";
                    requestInquiry.Salesperson_Code = inquiryheader.SPCode;
                    requestInquiry.PCPL_IsInquiry = true;
                    requestInquiry.IsActive = true;


                    if (inquiryheader.InquiryDate != null)
                    {
                        DateTime InquiryDate = Convert.ToDateTime(inquiryheader.InquiryDate);
                        requestInquiry.Inquiry_Date = InquiryDate.ToString("yyyy-MM-dd");
                    }

                    result = PostItemInquiry("InquiryDotNetAPI", requestInquiry, responseInquiry);
                }
                else
                {
                    reqInqWithCustTemplateCode.Inquiry_No = inquiryheader.InquiryNo;
                    reqInqWithCustTemplateCode.Inquiry_Date = inquiryheader.InquiryDate;
                    reqInqWithCustTemplateCode.Inquiry_Customer_Contact = inquiryheader.ContactCompanyNo;
                    reqInqWithCustTemplateCode.Contact_Company_Name = inquiryheader.CustomerName;
                    reqInqWithCustTemplateCode.PCPL_Contact_Person = inquiryheader.ContactPersonNo;
                    reqInqWithCustTemplateCode.Inquiry_Customer_No = inquiryheader.CustomerNo;
                    reqInqWithCustTemplateCode.Ship_to_Code = inquiryheader.ShiptoCode != "-1" ? inquiryheader.ShiptoCode : "";
                    reqInqWithCustTemplateCode.PCPL_Job_to_Code = inquiryheader.JobtoCode != "-1" ? inquiryheader.JobtoCode : "";
                    reqInqWithCustTemplateCode.Payment_Terms = inquiryheader.PaymentTerms;

                    if (inquiryheader.Inquiry_Status_Remarks == null)
                        reqInqWithCustTemplateCode.Inquiry_Status_Remarks = "";
                    else
                        reqInqWithCustTemplateCode.Inquiry_Status_Remarks = inquiryheader.Inquiry_Status_Remarks;

                    reqInqWithCustTemplateCode.Inquiry_Status = "Pending";
                    reqInqWithCustTemplateCode.Salesperson_Code = inquiryheader.SPCode;
                    reqInqWithCustTemplateCode.PCPL_IsInquiry = true;
                    reqInqWithCustTemplateCode.IsActive = true;
                    reqInqWithCustTemplateCode.Sell_to_Customer_Templ_Code = reqInqWithCustTemplateCode.Bill_to_Customer_Templ_Code = inquiryheader.CustomerTemplateCode;


                    if (inquiryheader.InquiryDate != null)
                    {
                        DateTime InquiryDate = Convert.ToDateTime(inquiryheader.InquiryDate);
                        reqInqWithCustTemplateCode.Inquiry_Date = InquiryDate.ToString("yyyy-MM-dd");
                    }

                    result = PostItemInquiryWithCustTemplateCode("InquiryDotNetAPI", reqInqWithCustTemplateCode, responseInquiry);
                }
            }
            else
            {
                reqInquiryUpdate.PCPL_Contact_Person = inquiryheader.ContactPersonNo;
                reqInquiryUpdate.PCPL_Job_to_Code = inquiryheader.JobtoCode != "-1" ? inquiryheader.JobtoCode : "";
                reqInquiryUpdate.Ship_to_Code = inquiryheader.ShiptoCode != "-1" ? inquiryheader.ShiptoCode : "";
                reqInquiryUpdate.Payment_Terms = inquiryheader.PaymentTerms;

                if (inquiryheader.Inquiry_Status_Remarks == null)
                    reqInquiryUpdate.Inquiry_Status_Remarks = "";
                else
                    reqInquiryUpdate.Inquiry_Status_Remarks = inquiryheader.Inquiry_Status_Remarks;


                result = PatchItemInquiry("InquiryDotNetAPI", reqInquiryUpdate, responseInquiry, "Document_Type='Quote',Inquiry_No='" + IQNo + "'");
            }

            if (result.Result.Item1 != null)
            {
                responseInquiry = result.Result.Item1;
                ed = result.Result.Item2;
                responseInquiry.errorDetails = ed;

                if (result.Result.Item1.Inquiry_No != null)
                {
                    //responseInquiry = result.Result.Item1;

                    SPInqLinesPost reqInqLine = new SPInqLinesPost();
                    SPInqLinesUpdate reqInqLineUpdate = new SPInqLinesUpdate();
                    SPInqLines resInqLine = new SPInqLines();
                    errorDetails ed1 = new errorDetails();

                    for (int a = 0; a < inquiryheader.Products.Count; a++)
                    {
                        var result1 = (dynamic)null;
                        if (!isEdit)
                        {
                            reqInqLine.Document_No = responseInquiry.Inquiry_No;
                            reqInqLine.Product_No = inquiryheader.Products[a].Product_No;
                            reqInqLine.Type = "Item";
                            reqInqLine.Quantity = inquiryheader.Products[a].Quantity;
                            reqInqLine.PCPL_Packing_Style_Code = inquiryheader.Products[a].PCPL_Packing_Style_Code;
                            if (inquiryheader.Products[a].Delivery_Date != null)
                            {
                                DateTime deliverDate = Convert.ToDateTime(inquiryheader.Products[a].Delivery_Date);
                                reqInqLine.Delivery_Date = deliverDate.ToString("yyyy-MM-dd");
                            }
                            //reqInqLine.Delivery_Date = inquiryheader.Products[a].Delivery_Date;
                            reqInqLine.PCPL_Convert_Quote = false;

                            result1 = PostItemInqLines("InquiryProductsDotNetAPI", reqInqLine, resInqLine);
                        }
                        else
                        {
                            reqInqLineUpdate.Quantity = inquiryheader.Products[a].Quantity;
                            reqInqLineUpdate.PCPL_Packing_Style_Code = inquiryheader.Products[a].PCPL_Packing_Style_Code;
                            if (inquiryheader.Products[a].Delivery_Date != null)
                            {
                                DateTime deliverDate = Convert.ToDateTime(inquiryheader.Products[a].Delivery_Date);
                                reqInqLineUpdate.Delivery_Date = deliverDate.ToString("yyyy-MM-dd");
                            }

                            result1 = PatchItemInqLines("InquiryProductsDotNetAPI", reqInqLineUpdate, resInqLine, "Document_Type='Quote',Document_No='" + responseInquiry.Inquiry_No +
                                "',Line_No=" + Convert.ToInt32(inquiryheader.Products[a].Line_No));
                        }


                        if (result1.Result.Item1 != null)
                        {
                            resInqLine = result1.Result.Item1;
                            ed1 = result1.Result.Item2;
                            responseInquiry.errorDetails = ed1;

                        }

                        //if (result1.Result.Item1.Line_No != null)
                        //    resInqLine = result1.Result.Item1;

                        //if (result1.Result.Item2.message != null)
                        //    ed1 = result1.Result.Item2;

                    }

                }

            }

            //if (result.Result.Item2.message != null)
            //    ed = result.Result.Item2;

            return responseInquiry;
        }

        [Route("GetInquiryFromInquiryNo")]
        public SPInquiry GetInquiryFromInquiryNo(string Inquiry_No)
        {
            API ac = new API();
            SPInquiry inquiry = new SPInquiry();

            var result = ac.GetData<SPInquiry>("InquiryDotNetAPI", "Inquiry_No eq '" + Inquiry_No + "'");

            if (result.Result.Item1.value.Count > 0)
                inquiry = result.Result.Item1.value[0];

            return inquiry;
        }

        [Route("GetAllInqLinesOfInq")]
        public List<SPInqLines> GetAllInqLinesOfInq(string InquiryNo)
        {
            API ac = new API();
            List<SPInqLines> InqLines = new List<SPInqLines>();

            var result = ac.GetData<SPInqLines>("InquiryProductsDotNetAPI", "Document_No eq '" + InquiryNo + "'");

            if (result.Result.Item1.value.Count > 0)
                InqLines = result.Result.Item1.value;

            return InqLines;
        }

        [HttpPost]
        [Route("DeleteInqLine")]
        public bool DeleteInqLine(string DocumentNo, string LineNo)
        {
            API ac = new API();
            SPInqLinesDelete inqLineDelete = new SPInqLinesDelete();
            inqLineDelete.Document_No = DocumentNo;
            inqLineDelete.Line_No = LineNo;

            var result = DeleteItemInqLine("InquiryProductsDotNetAPI", inqLineDelete, "Document_No='" + DocumentNo + "',Line_No=" + LineNo);

            return true;
        }

        [Route("GetAllUserForSendNotif")]
        public List<UserInfo> GetAllUserForSendNotif()
        {
            API ac = new API();
            List<UserInfo> users = new List<UserInfo>();

            var result = ac.GetData<UserInfo>("EmployeesDotNetAPI", "");

            if (result.Result.Item1.value.Count > 0)
                users = result.Result.Item1.value;

            return users;
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

        [HttpPost]
        [Route("SendNotification")]
        public bool SendNotification(string userEmail, string SPEmail, string InqNos, string NotifMsg)
        {
            bool flag = false;
            string emailBody = "";
            InqNos = InqNos.Substring(0, InqNos.Length - 1);
            string[] InqNos_ = InqNos.Split(',');

            emailBody += "<table width=\"100%\" border=\"1\"><thead><tr style=\"background-color:darkblue;color:white\"><th>Inq No</th><th>Inq Date</th><th>Customer</th><th>Payment Terms</th><th>Status</th><th>Consignee Address</th></tr></thead><tbody>";
            for (int a = 0; a < InqNos_.Length; a++)
            {

                API ac = new API();
                SPInquiry inquiry = new SPInquiry();

                var result = ac.GetData<SPInquiry>("InquiryDotNetAPI", "Inquiry_No eq '" + InqNos_[a] + "'");

                if (result.Result.Item1.value.Count > 0)
                    inquiry = result.Result.Item1.value[0];

                if (inquiry.Inquiry_No != null)
                {
                    emailBody += "<tr><td>" + inquiry.Inquiry_No + "</td><td>" + inquiry.Inquiry_Date.ToString("dd-MM-yyyy") + "</td><td>" + inquiry.Contact_Company_Name
                            + "</td><td>" + inquiry.Payment_Terms + "</td><td>" + inquiry.Inquiry_Status +
                            "</td><td>" + inquiry.Sell_to_Address + " " + inquiry.Sell_to_Address_2 + "," + inquiry.Sell_to_City + "</td></tr>";

                    emailBody += "<tr><td></td><td colspan=\"6\"><table width=\"50%\" border=\"1\"><thead><tr style=\"background-color:gray;color:black\"><th>Product Name</th><th>Qty</th><th>UOM</th></tr></thead><tbody>";

                    List<SPInqLines> inqProds = GetAllInqLinesOfInq(InqNos_[a]);
                    for (int b = 0; b < inqProds.Count; b++)
                    {
                        emailBody += "<tr><td>" + inqProds[b].Product_Name + "</td><td>" + inqProds[b].Quantity + "</td><td>" + inqProds[b].Unit_of_Measure + "</td></tr>";
                    }

                    emailBody += "</tbody></table></td></tr>";
                }

            }

            emailBody += "</tbody></table>";

            EmailService emailService = new EmailService();
            StringBuilder sbMailBody = new StringBuilder();
            sbMailBody.Append("");
            sbMailBody.Append("<p>Hi,</p>");
            sbMailBody.Append("<p>Welcome to the <strong>Prakash CRM Portal</strong>.</p>");
            sbMailBody.Append("<p>Inquiry Details</p>");
            sbMailBody.Append("<p><b>Notification Message :</b><br />" + NotifMsg + "</p>");
            sbMailBody.Append(emailBody);
            sbMailBody.Append("<p>&nbsp;</p>");
            sbMailBody.Append("<p>Warm Regards,</p>");
            sbMailBody.Append("<p>Support Team</p>");

            emailService.SendEmail(userEmail, SPEmail, "Inquiries - PrakashCRM", sbMailBody.ToString());
            flag = true;

            return flag;
        }

        [HttpPost]
        [Route("AssignToSalesperson")]
        public bool AssignToSalesperson(string SPDetails, string InqNos, string LoggedInSPName, string AssignToMsg)
        {
            bool flag = false;
            string errMsg = "";
            InqNos = InqNos.Replace("'", "");
            string[] inquiries = InqNos.Split(',');
            SPInqAssignTo reqInqAssignTo = new SPInqAssignTo();
            SPInquiry resInqAssignTo = new SPInquiry();
            errorDetails ed = new errorDetails();

            string[] SPDetails_ = SPDetails.Split('_');

            reqInqAssignTo.Salesperson_Code = SPDetails_[0];

            for(int a = 0; a < inquiries.Length - 1; a++)
            {
                var result = PatchItemInqAssignTo("InquiryDotNetAPI", reqInqAssignTo, resInqAssignTo, "Document_Type='Quote',Inquiry_No='" + inquiries[a] + "'");

                if (result.Result.Item1.Inquiry_No != null)
                    resInqAssignTo = result.Result.Item1;
                else
                    errMsg = "Error";

                if (result.Result.Item2.message != null)
                    ed = result.Result.Item2;
            }

            if (errMsg == "")
            {
                flag = true;
                string emailBody = "<ul>";
                string SPEmail = SPDetails_[1];
                string CCEmail = "nishant.m@tripearltech.com";
                for(int a = 0; a < inquiries.Length - 1; a++)
                {
                    emailBody += "<li>" + inquiries[a] + "</li>";
                }

                emailBody += "</ul>";

                EmailService emailService = new EmailService();
                StringBuilder sbMailBody = new StringBuilder();
                sbMailBody.Append("");
                sbMailBody.Append("<p>Hi,</p>");
                sbMailBody.Append("<p>Welcome to the <strong>Prakash CRM Portal</strong>.</p>");
                sbMailBody.Append("<p>Salesperson <b>" + LoggedInSPName + "</b> has assigned following inquiries to you</p>");
                sbMailBody.Append("<p><b>Inquiries:</b></p>");
                sbMailBody.Append(emailBody);
                sbMailBody.Append("<p><b>Message:</b>" + AssignToMsg + "</p>");
                sbMailBody.Append("<p>&nbsp;</p>");
                sbMailBody.Append("<p>Warm Regards,</p>");
                sbMailBody.Append("<p>Support Team</p>");

                emailService.SendEmail(SPEmail, CCEmail, "Inquiries assigned to you - PrakashCRM", sbMailBody.ToString());

            }
            
            return flag;

        }

        [Route("GetCustomerTemplateCode")]
        public string GetCustomerTemplateCode()
        {
            string customerTemplateCode = "";
            API ac = new API();
            SPSalesReceivableSetup salesReceivableSetup = new SPSalesReceivableSetup();
            
            var result = ac.GetData<SPSalesReceivableSetup>("SalesReceivableSetupDotNetAPI", "");

            if (result != null && result.Result.Item1.value.Count > 0)
            {
                salesReceivableSetup = result.Result.Item1.value[0];
                customerTemplateCode = salesReceivableSetup.PCPL_Inquiry_Customer_Template;
            }
            
            return customerTemplateCode;
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

        //[HttpPost]
        //[Route("CreateNotifTask")]
        //public bool CreateNotifTask(string TaskTitle)
        //{
        //    errorDetails ed = new errorDetails();
        //    SPUserTaskPost userTask = new SPUserTaskPost();
        //    SPUserTask userTaskRes = new SPUserTask();

        //    userTask.Title = TaskTitle;
        //    userTask.MultiLineTextControl = TaskTitle;
        //    userTask.Created_DateTime = DateTime.Now.ToString("yyyy-MM-dd");

        //    var result = PostItemCreateTask("InquiryDotNetAPI", userTask, userTaskRes); //PostItemSP("InquiryDotNetAPI", requestUser, responseUser);

        //        if (result.Result.Item1.ID != null)
        //            userTaskRes = result.Result.Item1;

        //        if (result.Result.Item2.message != null)
        //            ed = result.Result.Item2;

        //    return true;
        //}

        [Route("GetAllAreasForDDL")]
        public List<Area> GetAllAreasForDDL()
        {
            API ac = new API();
            List<Area> areas = new List<Area>();

            var result = ac.GetData<Area>("AreasListDotNetAPI", "IsActive eq true");

            if (result != null && result.Result.Item1.value.Count > 0)
                areas = result.Result.Item1.value;

            return areas;
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

        public async Task<(SPInquiry, errorDetails)> PostItemInquiry<SPInquiry>(string apiendpoint, SPInquiryPost requestModel, SPInquiry responseModel)
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
        
        public async Task<(SPInquiry, errorDetails)> PostItemInquiryWithCustTemplateCode<SPInquiry>(string apiendpoint, SPInquiryPostWithCustTemplateCode requestModel, SPInquiry responseModel)
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

        public async Task<(SPInquiry, errorDetails)> PatchItemInquiry<SPInquiry>(string apiendpoint, SPInquiryUpdate requestModel, SPInquiry responseModel, string fieldWithValue)
        {
            API ac = new API();
            errorDetails errordetail = new errorDetails();

            try
            {
                string _baseURL = System.Configuration.ConfigurationManager.AppSettings["BaseURL"];
                string _tenantId = System.Configuration.ConfigurationManager.AppSettings["TenantID"];
                string _environment = System.Configuration.ConfigurationManager.AppSettings["Environment"];
                string _companyName = System.Configuration.ConfigurationManager.AppSettings["CompanyName"];

                //API ac = new API();
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
                response = _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).Result;

                //errorDetails errordetail = new errorDetails();
                errordetail.isSuccess = response.IsSuccessStatusCode;
                if (response.IsSuccessStatusCode)
                {
                    var JsonData = response.Content.ReadAsStringAsync().Result;
                    JObject res = JObject.Parse(JsonData);
                    responseModel = res.ToObject<SPInquiry>();

                    errordetail.code = response.StatusCode.ToString();
                    errordetail.message = response.ReasonPhrase;

                    //ac.PostSiteActivity(responseModel);
                }
                else
                {
                    var JsonData = response.Content.ReadAsStringAsync().Result;
                    JObject res = JObject.Parse(JsonData);
                    errorMaster<errorDetails> emd = res.ToObject<errorMaster<errorDetails>>();
                    errordetail = emd.error;

                    string code = emd.error.code.Substring(0, 20).ToString();
                    string message = emd.error.message.Substring(0, 100).ToString();
                    string url = response.RequestMessage.RequestUri.ToString();
                    string method = response.RequestMessage.Method.ToString();

                    ac.PostSiteErrorWithResponse(code, message, url, method);
                }
            }
            catch (Exception ex)
            {
                ac.PostSiteError(ex);
            }
            return (responseModel, errordetail);
        }

        public async Task<(SPInqLines, errorDetails)> PostItemInqLines<SPInqLines>(string apiendpoint, SPInqLinesPost requestModel, SPInqLines responseModel)
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

        public async Task<(SPInqLines, errorDetails)> PatchItemInqLines<SPInqLines>(string apiendpoint, SPInqLinesUpdate requestModel, SPInqLines responseModel, string fieldWithValue)
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

        public async Task<(List<SPInqShiptoAddressRes>, errorDetails)> PostItemForInqGetShiptoAddress<SPInqShiptoAddressRes>(string apiendpoint, SPInqShiptoAddress requestModel, List<SPInqShiptoAddressRes> responseModel)
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
                    SPInqAddressOData shiptoAddressOData = res.ToObject<SPInqAddressOData>();
                    string shipToAddressData = shiptoAddressOData.value.ToString();
                    responseModel = JsonConvert.DeserializeObject<List<SPInqShiptoAddressRes>>(shipToAddressData);

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

        public async Task<(List<SPInqJobtoAddressRes>, errorDetails)> PostItemForInqGetJobtoAddress<SPInqJobtoAddressRes>(string apiendpoint, SPInqJobtoAddress requestModel, List<SPInqJobtoAddressRes> responseModel)
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
                    SPInqAddressOData jobtoAddressOData = res.ToObject<SPInqAddressOData>();
                    string jobToAddressData = jobtoAddressOData.value.ToString();
                    responseModel = JsonConvert.DeserializeObject<List<SPInqJobtoAddressRes>>(jobToAddressData);

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

        public async Task<(int, errorDetails)> DeleteItemInqLine(string apiendpoint, SPInqLinesDelete requestModel, string fieldWithValue)
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
            var request = new HttpRequestMessage(HttpMethod.Delete, baseuri + "(" + fieldWithValue + ")");
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

            var resData = "";
            errorDetails errordetail = new errorDetails();
            errordetail.isSuccess = response.IsSuccessStatusCode;
            if (response.IsSuccessStatusCode)
            {
                //var JsonData = response.Content.ReadAsStringAsync().Result;
                try
                {
                    //JObject res = JObject.Parse(JsonData);
                    //responseModel = res.ToObject<U>();
                    resData = response.Content.ReadAsStringAsync().Result;
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

            return (Convert.ToInt32(resData), errordetail);
        }

        public async Task<(SPInquiry, errorDetails)> PatchItemInqAssignTo<SPInquiry>(string apiendpoint, SPInqAssignTo requestModel, SPInquiry responseModel, string fieldWithValue)
        {
            API ac = new API();
            errorDetails errordetail = new errorDetails();

            try
            {
                string _baseURL = System.Configuration.ConfigurationManager.AppSettings["BaseURL"];
                string _tenantId = System.Configuration.ConfigurationManager.AppSettings["TenantID"];
                string _environment = System.Configuration.ConfigurationManager.AppSettings["Environment"];
                string _companyName = System.Configuration.ConfigurationManager.AppSettings["CompanyName"];

                //API ac = new API();
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
                response = _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).Result;

                //errorDetails errordetail = new errorDetails();
                errordetail.isSuccess = response.IsSuccessStatusCode;
                if (response.IsSuccessStatusCode)
                {
                    var JsonData = response.Content.ReadAsStringAsync().Result;
                    JObject res = JObject.Parse(JsonData);
                    responseModel = res.ToObject<SPInquiry>();

                    errordetail.code = response.StatusCode.ToString();
                    errordetail.message = response.ReasonPhrase;

                    //ac.PostSiteActivity(responseModel);
                }
                else
                {
                    var JsonData = response.Content.ReadAsStringAsync().Result;
                    JObject res = JObject.Parse(JsonData);
                    errorMaster<errorDetails> emd = res.ToObject<errorMaster<errorDetails>>();
                    errordetail = emd.error;

                    string code = emd.error.code.Substring(0, 20).ToString();
                    string message = emd.error.message.Substring(0, 100).ToString();
                    string url = response.RequestMessage.RequestUri.ToString();
                    string method = response.RequestMessage.Method.ToString();

                    ac.PostSiteErrorWithResponse(code, message, url, method);
                }
            }
            catch (Exception ex)
            {
                ac.PostSiteError(ex);
            }
            return (responseModel, errordetail);
        }

        public async Task<(SPUserTask, errorDetails)> PostItemCreateTask<SPUserTask>(string apiendpoint, SPUserTaskPost requestModel, SPUserTask responseModel)
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
                    responseModel = res.ToObject<SPUserTask>();

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

        public async Task<(SPInqNextNoOData, errorDetails)> PostItemForInqGetNextNo<SPInqNextNoOData>(string apiendpoint, string requestModel, SPInqNextNoOData responseModel)
        {
            string _baseURL = System.Configuration.ConfigurationManager.AppSettings["BaseURL"];
            string _tenantId = System.Configuration.ConfigurationManager.AppSettings["TenantID"];
            string _environment = System.Configuration.ConfigurationManager.AppSettings["Environment"];
            string _companyName = System.Configuration.ConfigurationManager.AppSettings["CompanyName"];

            API ac = new API();
            var accessToken = await ac.GetAccessToken();

            HttpClient _httpClient = new HttpClient();
            string encodeurl = Uri.EscapeUriString("https://api.businesscentral.dynamics.com/v2.0/e55ad508-ef1a-489f-afe3-ae21f856e440/Sandbox/ODataV4/CodeunitAPIMgmt_GetNextInqNo?company=\'Prakash Company\'");
            Uri baseuri = new Uri(encodeurl);
            _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken.Token);


            string ItemCardObjString = JsonConvert.SerializeObject(requestModel);
            var content = new StringContent(ItemCardObjString, Encoding.UTF8, "application/json");
            HttpResponseMessage response = null;
            try
            {
                response = _httpClient.PostAsync(baseuri, null).Result;
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
                    responseModel = res.ToObject<SPInqNextNoOData>();

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

    }
}