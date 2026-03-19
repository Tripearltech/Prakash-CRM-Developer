using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using PrakashCRM.Data.Models;
using PrakashCRM.Service.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using System.Web.UI;
using System.Web.Security;
using Microsoft.Ajax.Utilities;

namespace PrakashCRM.Service.Controllers
{
    [RoutePrefix("api/SPBusinessPlan")]
    public class SPBusinessPlanController : ApiController
    {
        [Route("GetAllCustomers")]
        public List<SPCompanyList> GetAllCustomers(string SPCode)
        {
            return GetAllCompanyForDDL(SPCode);
        }

        [Route("GetAllCompanyForDDL")]
        public List<SPCompanyList> GetAllCompanyForDDL(string SPCode)
        {
            API ac = new API();
            List<SPCompanyList> companies = new List<SPCompanyList>();

            var result = ac.GetData<SPCompanyList>("ContactDotNetAPI", "Type eq 'Company' and Salesperson_Code eq '" + SPCode + "'");

            if (result != null && result.Result.Item1.value.Count > 0)
                companies = result.Result.Item1.value;

            List<SPCompanyList> company2 = new List<SPCompanyList>();

            var result2 = ac.GetData<SPCompanyList>("ContactDotNetAPI", "PCPL_Secondary_SP_Code eq '" + SPCode + "' and Salesperson_Code ne '" + SPCode + "' and Type eq 'Company'");

            if (result2 != null && result2.Result.Item1.value.Count > 0)
            {
                company2 = result2.Result.Item1.value;

                companies.AddRange(company2);
            }

            return companies;
        }

        [Route("GetBusinessPlanCustWise")]
        public List<SPBusinessPlanDetails> GetBusinessPlanCustWise(string Page, string SPCode, string LoggedInUserNo, int skip, int top, string orderby, string filter, bool isExport = false)
        {
            API ac = new API();
            List<SPBusinessPlanDetails> businessPlanDetails = new List<SPBusinessPlanDetails>();

            if (filter == "" || filter == null)
                filter = "Salesperson_Purchaser eq '" + SPCode + "'";
            else
                filter = filter + " and Salesperson_Purchaser eq '" + SPCode + "'";

            if (Page == "CustWiseForPendingApproval")
                filter += " and Approver eq '" + LoggedInUserNo + "'";
            //filter = filter + " and (Status eq 'Submitted' or Status eq 'Approved' or Status eq 'Rejected')";

            var result = (dynamic)null;

            if (isExport)
                result = ac.GetData<SPBusinessPlanDetails>("Business_Plan_Customer_Wise", filter); // and Contact_Business_Relation eq 'Customer'
            else
                result = ac.GetData1<SPBusinessPlanDetails>("Business_Plan_Customer_Wise", filter, skip, top, orderby); // and Contact_Business_Relation eq 'Customer'

            if (result.Result.Item1.value.Count > 0)
                businessPlanDetails = result.Result.Item1.value;
            return businessPlanDetails;
        }

        [Route("GetBusinessPlanListDataAllForAssign")]
        public List<SPBusinessPlanAssignCustList> GetBusinessPlanListDataForAssign(string orderby, string filter, bool isExport = false)
        {
            API ac = new API();
            List<SPBusinessPlanRes> businessPlanDetails = new List<SPBusinessPlanRes>();
            List<SPBusinessPlanAssignCustList> businessPlanAssignCustList = new List<SPBusinessPlanAssignCustList>();

            filter = filter + " and Transfered_To eq '' and Approved eq true";

            if (filter.Contains("Sales_Person"))
            {
                orderby = "";

                var result = ac.GetData1<SPBusinessPlanRes>("BusinessPlanListDotNetAPI", filter, 0, 0, orderby); // and Contact_Business_Relation eq 'Customer'

                if (result.Result.Item1.value.Count > 0)
                {
                    businessPlanDetails = result.Result.Item1.value;
                    string[] customerName = businessPlanDetails.Select(x => x.PCPL_Contact_Company_Name).Distinct().ToArray();

                    for (int a = 0; a < customerName.Length; a++)
                    {
                        businessPlanAssignCustList.Add(new SPBusinessPlanAssignCustList()
                        {
                            PCPL_Contact_Company_Name = customerName[a]
                        });

                    }

                    for (int a = 0; a < businessPlanAssignCustList.Count; a++)
                    {
                        List<SPBusinessPlanAssignCustProdList> businessPlanAssignCustProdList = new List<SPBusinessPlanAssignCustProdList>();
                        for (int b = 0; b < businessPlanDetails.Count; b++)
                        {
                            if (businessPlanAssignCustList[a].PCPL_Contact_Company_Name == businessPlanDetails[b].PCPL_Contact_Company_Name)
                            {
                                businessPlanAssignCustList[a].Customer_No = businessPlanDetails[b].Customer_No;

                                businessPlanAssignCustProdList.Add(new SPBusinessPlanAssignCustProdList()
                                {
                                    Product_No = businessPlanDetails[b].Product_No,
                                    Product_Name = businessPlanDetails[b].Product_Name,
                                    PCPL_Plan_Year = businessPlanDetails[b].PCPL_Plan_Year,
                                    Demand = businessPlanDetails[b].Demand,
                                    Target = businessPlanDetails[b].Target,
                                    PCPL_Target_Revenue = businessPlanDetails[b].PCPL_Target_Revenue,
                                    Last_year_Sale_Qty = businessPlanDetails[b].Last_year_Sale_Qty

                                });
                            }
                        }
                        businessPlanAssignCustList[a].businessPlanAssignCustProds = businessPlanAssignCustProdList;

                    }
                }

            }

            return businessPlanAssignCustList;
        }

        [Route("GetAllContactProducts")]
        public List<SPContactProdsBusinessPlan> GetAllContactProducts(string CCompanyNo)
        {
            API ac = new API();
            List<SPContactProdsBusinessPlan> items = new List<SPContactProdsBusinessPlan>();

            var result = ac.GetData<SPContactProdsBusinessPlan>("ContactProductsDotNetAPI", "Contact_No eq '" + CCompanyNo + "'");

            if (result != null && result.Result.Item1.value.Count > 0)
                items = result.Result.Item1.value;

            if (items.Count > 0)
            {
                string lastYear = (DateTime.Now.Year - 1).ToString();
                string lastYearStartDate = lastYear + "-04-01";
                string lastYearEndDate = (DateTime.Now.Year).ToString() + "-03-31";
                //string CustomerNo = "";
                string CustomerNo = "";

                //ContCustForBusRel contCustForBusRel = new ContCustForBusRel();
                //ConBusinessRelation conBusinessRelation = new ConBusinessRelation();

                //var resultCompanyNo = ac.GetData<ContCustForBusRel>("ContactDotNetAPI", "No eq '" + CCompanyNo + "'");

                //if (resultCompanyNo.Result.Item1.value.Count > 0)
                //    contCustForBusRel.Company_No = resultCompanyNo.Result.Item1.value[0].Company_No;

                //var resultCustomerNo = ac.GetData<ConBusinessRelation>("ContactBusinessRelationsDotNetAPI", "Contact_No eq '" + contCustForBusRel.Company_No + "'");

                //if (resultCustomerNo.Result.Item1.value.Count > 0)
                //{
                //    conBusinessRelation.No = resultCustomerNo.Result.Item1.value[0].No;
                //    CustomerNo = conBusinessRelation.No;
                //}

                if (CustomerNo != "")
                {

                    for (int a = 0; a < items.Count; a++)
                    {
                        decimal lastOneYearSaleQty = 0;
                        decimal lastOneYearSaleAmt = 0;
                        List<SPBusinessPlanLastYearSale> lastYearSale = new List<SPBusinessPlanLastYearSale>();

                        //var result1 = ac.GetData<SPBusinessPlanLastYearSale>("ItemLedgerEntriesDotNetAPI", "Entry_Type eq 'Sale' and Item_No eq '" + items[a].Item_No +
                        //    "' and  and Source_No eq '" + CustomerNo + "' and (Posting_Date ge " + lastYearStartDate + " and Posting_Date le " + lastYearEndDate + ")");

                        var result1 = ac.GetData<SPBusinessPlanLastYearSale>("ItemLedgerEntriesDotNetAPI", "Entry_Type eq 'Sale' and Item_No eq 'TRD0008'" +
                            " and Source_No eq 'CUST00039' and (Posting_Date ge 2024-11-26 and Posting_Date le 2024-12-23)");

                        if (result1 != null && result1.Result.Item1.value.Count > 0)
                        {
                            lastYearSale = result1.Result.Item1.value;

                            for (int b = 0; b < lastYearSale.Count; b++)
                            {
                                lastOneYearSaleQty += lastYearSale[b].Quantity;
                                lastOneYearSaleAmt += lastYearSale[b].Sales_Amount_Actual;
                            }

                            items[a].LastOneYearSaleQty = (lastOneYearSaleQty * -1);
                            items[a].LastOneYearSaleAmt = lastOneYearSaleAmt;
                        }
                        else
                        {
                            items[a].LastOneYearSaleQty = 0;
                            items[a].LastOneYearSaleAmt = 0;
                        }
                    }
                }
                else
                {
                    for (int a = 0; a < items.Count; a++)
                    {
                        items[a].LastOneYearSaleQty = 0;
                        items[a].LastOneYearSaleAmt = 0;
                    }
                }

            }

            return items;
        }

        [Route("GetBusinessPlanSPList")]
        public List<SPBusinessPlanSPList> GetBusinessPlanSPList(string LoggedInUserNo, string filter)
        {
            API ac = new API();
            List<SPBusinessPlanSPList> salesperson = new List<SPBusinessPlanSPList>();
            List<SPBusinessPlanSPList> distinctSalesperson = new List<SPBusinessPlanSPList>();

            filter += " and Approver eq '" + LoggedInUserNo + "' and Status eq 'Submitted'";

            var result = ac.GetData<SPBusinessPlanSPList>("Business_Plan_Customer_Wise", filter);

            if (result != null && result.Result.Item1.value.Count > 0)
            {
                salesperson = result.Result.Item1.value;
                distinctSalesperson = salesperson.DistinctBy(x => x.Salesperson_Purchaser).ToList();
            }

            return distinctSalesperson;
        }

        [Route("GetCustomerBusinessPlan")]
        public List<SPCustBusinessPlan> GetCustomerBusinessPlan(string SPCode, string CustomerNo, string PlanYear)
        {
            API ac = new API();
            List<SPCustBusinessPlan> custBusinessPlan = new List<SPCustBusinessPlan>();

            var result = ac.GetData<SPCustBusinessPlan>("BusinessPlanListDotNetAPI", "Customer_No eq '" + CustomerNo + "' and PCPL_Plan_Year eq '" + PlanYear +
                    "' and Sales_Person eq '" + SPCode + "'");

            if (result != null && result.Result.Item1.value.Count > 0)
                custBusinessPlan = result.Result.Item1.value;

            return custBusinessPlan;
        }
        [Route("GetCustomerBusinessApproved")]
        public List<SPCustBusinessPlan> GetCustomerBusinessApproved(string SPCode, string CustomerNo, string PlanYear)
        {
            API ac = new API();
            List<SPCustBusinessPlan> custBusinessPlan = new List<SPCustBusinessPlan>();

            string filter = $"Customer_No eq '{CustomerNo}' and PCPL_Plan_Year eq '{PlanYear}' and Sales_Person eq '{SPCode}' and IsActive eq true and Approved eq false";

            var result = ac.GetData<SPCustBusinessPlan>("BusinessPlanListDotNetAPI", filter);

            if (result != null && result.Result.Item1.value.Count > 0)
                custBusinessPlan = result.Result.Item1.value;

            return custBusinessPlan;
        }

        [Route("GetAllProductsForDDL")]
        public List<SPItemList> GetAllProductsForDDL()
        {
            API ac = new API();
            List<SPItemList> prods = new List<SPItemList>();

            var result = ac.GetData1<SPItemList>("ItemDotNetAPI", "Blocked eq false", 0, 0, "Description asc");

            if (result.Result.Item1.value.Count > 0)
                prods = result.Result.Item1.value;

            return prods;
        }

        [Route("AddContactProducts")]
        public bool AddContactProducts(string CCompanyNo, string ProdNo)
        {
            bool flag = false;

            API ac = new API();
            SPContactProductsPost reqContactProduct = new SPContactProductsPost();
            reqContactProduct.Contact_No = CCompanyNo;
            reqContactProduct.Item_No = ProdNo;
            reqContactProduct.IsActive = true;

            errorDetails ed = new errorDetails();
            SPContactProductsPost resContactProduct = new SPContactProductsPost();

            var result = ac.PostItem("ContactProductsDotNetAPI", reqContactProduct, resContactProduct);

            if (result.Result.Item1 != null)
            {
                resContactProduct = result.Result.Item1;
                flag = true;
            }

            if (result.Result.Item2.message != null)
                ed = result.Result.Item2;

            return flag;
        }

        [Route("BusinessPlan")]
        public SPBusinessPlanDetails BusinessPlan(SPBusinessPlan businessPlan, string SPNo, string SPName)
        {
            bool flag = false;
            var ac = new API();
            errorDetails ed = new errorDetails();
            SPBusinessPlanPost businessPlanPost = new SPBusinessPlanPost();
            SPBusinessPlanRes businessPlanRes = new SPBusinessPlanRes();
            SPBusinessPlanDetails businessPlanDetails = new SPBusinessPlanDetails();

            for (int a = 0; a < businessPlan.Products.Count; a++)
            {
                //businessPlanPost.Customer_No = businessPlan.Customer_No;
                //businessPlanPost.PCPL_Plan_Year = businessPlan.PCPL_Plan_Year;
                //businessPlanPost.Product_No = businessPlan.Products[a].Product_No;
                businessPlanPost.Demand = businessPlan.Products[a].Demand;
                businessPlanPost.Target = businessPlan.Products[a].Target;
                businessPlanPost.PCPL_Target_Revenue = businessPlan.Products[a].PCPL_Target_Revenue;
                businessPlanPost.IsActive = true;

                var result = PatchItemBusinessPlan("BusinessPlanListDotNetAPI", businessPlanPost, businessPlanRes, "Customer_No='" + businessPlan.Customer_No +
                   "',Product_No='" + businessPlan.Products[a].Product_No + "',PCPL_Plan_Year='" + businessPlan.PCPL_Plan_Year + "',Sales_Person='" + SPNo + "'");
                if (result.Result.Item1 != null)
                {
                    businessPlanRes = result.Result.Item1;
                    ed = result.Result.Item2;
                    businessPlanDetails.errorDetails = ed;
                    flag = true;
                }
                else
                    flag = false;

                //if (result.Result.Item2.message != null)
                //    ed = result.Result.Item2;

            }

            if (flag)
            {
                SPBusinessPlanCustWisePost businessPlanCustWisePost = new SPBusinessPlanCustWisePost();
                errorDetails ed1 = new errorDetails();

                businessPlanCustWisePost.Status = "Filled";

                var result1 = PatchItemBusinessPlanStatus("Business_Plan_Customer_Wise", businessPlanCustWisePost, businessPlanDetails, "Customer_No='" + businessPlan.Customer_No +
                   "',Plan_Year='" + businessPlan.PCPL_Plan_Year + "',Salesperson_Purchaser='" + SPNo + "'");

                if (result1.Result.Item1 != null)
                {
                    businessPlanDetails = result1.Result.Item1;
                    ed1 = result1.Result.Item2;
                    businessPlanDetails.errorDetails = ed1;
                    flag = true;
                }
                else
                    flag = false;

                //if (result1.Result.Item2.message != null)
                //    ed1 = result1.Result.Item2;

            }


            /*if (flag)
            {
                string HODEmail = "arvind.k@tripearltech.com;mihir.s@tripearltech.com";
                string AdminEmail = "nishant.m@tripearltech.com";

                string path = "D:/BusinessPlanForCompany-AadinathAgency-2025-26.xlsx";
                Attachment attachment = new Attachment(path);

                EmailService emailService = new EmailService();
                StringBuilder sbMailBody = new StringBuilder();
                sbMailBody.Append("");
                sbMailBody.Append("<p>Hi,</p>");
                sbMailBody.Append("<p>Welcome to the <strong>Prakash CRM Portal</strong>.</p>");
                //sbMailBody.Append("<p>Approval Request - Business Plan For Company - <b>" + businessPlan.ContactCompanyName + "</b></p>");
                sbMailBody.Append("<p>Business Plan Year - <b>" + businessPlan.PCPL_Plan_Year + "</b></p>");
                sbMailBody.Append("<p>Business Plan Created By - <b>" + SPName + "</b></p>");
                sbMailBody.Append("<p>&nbsp;</p>");
                sbMailBody.Append("<p>Warm Regards,</p>");
                sbMailBody.Append("<p>Support Team</p>");

                //emailService.SendEmailTo(email, sbMailBody.ToString(), "Change Password - PrakashCRM");
                //emailService.SendEmail(HODEmail, AdminEmail, "Approval Request - Business Plan For Company - " + businessPlan.ContactCompanyName, sbMailBody.ToString());
                emailService.SendAttachmentEmail(HODEmail, AdminEmail,"", "Approval Request - Business Plan For Company - " + businessPlan.ContactCompanyName, attachment, sbMailBody.ToString());
            }*/

            return businessPlanDetails;
        }

        //[Route("GetApiRecordsCount")]
        //public int GetApiRecordsCount(string SPCode, string apiEndPointName, string filter)
        //{
        //    API ac = new API();

        //    if (filter == "" || filter == null)
        //        filter = "Salesperson_Code eq '" + SPCode + "'";
        //    else
        //        filter = filter + " and Salesperson_Code eq '" + SPCode + "'";

        //    var count = ac.CalculateCount(apiEndPointName, filter);

        //    return Convert.ToInt32(count.Result);
        //}

        [Route("GetAllSalespersonForDDL")]
        public List<SPSalespeoplePurchaser> GetAllSalespersonForDDL()
        {
            API ac = new API();
            List<SPSalespeoplePurchaser> salesperson = new List<SPSalespeoplePurchaser>();

            var result = ac.GetData<SPSalespeoplePurchaser>("SalespersonPurchaserDotNetAPI", "");

            if (result != null && result.Result.Item1.value.Count > 0)
                salesperson = result.Result.Item1.value;

            return salesperson;
        }

        [Route("GetApiRecordsCount")]
        public int GetApiRecordsCount(string page, string SPNo, string LoggedInUserNo, string apiEndPointName, string filter)
        {
            API ac = new API();


            if (page != "AssignBusinessPlan")
            {
                if (filter == "" || filter == null)
                    filter = "Salesperson_Purchaser eq '" + SPNo + "'";
                else
                    filter = filter + " and Salesperson_Purchaser eq '" + SPNo + "'";
            }

            if (page == "CustWiseForPendingApproval")
                filter += " and Approver eq '" + LoggedInUserNo + "'";

            if ((filter.Contains("Sales_Person") && page == "AssignBusinessPlan") || page != "AssignBusinessPlan")
            {
                var count = ac.CalculateCount(apiEndPointName, filter);

                return Convert.ToInt32(count.Result);
            }
            else
                return 0;

        }

        [Route("SendBusinessPlanForApproval")]
        public string SendBusinessPlanForApproval(string SPCode, string PlanYear)
        {
            string resMsg = "";
            API ac = new API();

            SPUserReportingPersonDetails reportingPersonDetails = new SPUserReportingPersonDetails();
            reportingPersonDetails.Reporting_Person_No = "";

            var resultUserDetails = ac.GetData<SPUserReportingPersonDetails>("EmployeesDotNetAPI", "Salespers_Purch_Code eq '" + SPCode + "'");

            if (resultUserDetails != null && resultUserDetails.Result.Item1.value.Count > 0)
                reportingPersonDetails = resultUserDetails.Result.Item1.value[0];

            List<SPBusinessPlanDetails> businessPlanDetails = new List<SPBusinessPlanDetails>();
            errorDetails ed = new errorDetails();

            var result = ac.GetData<SPBusinessPlanDetails>("Business_Plan_Customer_Wise", "Salesperson_Purchaser eq '" + SPCode + "' and Plan_Year eq '" +
                    PlanYear + "'");

            if (result.Result.Item1.value.Count > 0)
                businessPlanDetails = result.Result.Item1.value;

            for (int a = 0; a < businessPlanDetails.Count; a++)
            {

                if (businessPlanDetails[a].Status == "Filled")
                {

                    SPBusinessPlanCustWiseSendApproval businessPlanSendApproval = new SPBusinessPlanCustWiseSendApproval();
                    SPBusinessPlanDetails businessPlanSendApprovalRes = new SPBusinessPlanDetails();
                    errorDetails ed1 = new errorDetails();

                    businessPlanSendApproval.Status = "Submitted";
                    businessPlanSendApproval.Submitted_On = DateTime.Now;
                    businessPlanSendApproval.Approver = reportingPersonDetails.Reporting_Person_No;

                    var result1 = PatchItemBusinessPlanSendApproval("Business_Plan_Customer_Wise", businessPlanSendApproval, businessPlanSendApprovalRes, "Customer_No='" + businessPlanDetails[a].Customer_No +
                       "',Plan_Year='" + PlanYear + "',Salesperson_Purchaser='" + SPCode + "'");

                    //if (result1.Result.Item1 != null)
                    //flag = true;
                    //else
                    //flag = false;

                    if (result1.Result.Item1 != null)
                    {
                        resMsg = "True";
                        businessPlanSendApprovalRes = result1.Result.Item1;
                        ed1 = result1.Result.Item2;
                        businessPlanSendApprovalRes.errorDetails = ed1;
                    }

                    if (!businessPlanSendApprovalRes.errorDetails.isSuccess)
                        resMsg = "Error:" + businessPlanSendApprovalRes.errorDetails.message;

                    //if (result1.Result.Item2.message != null)
                    //    ed = result.Result.Item2;

                }

            }

            return resMsg;

        }

        [Route("BusinessPlanSPWiseApproveReject")]
        public string BusinessPlanSPWiseApproveReject(string SPCode, string LoggedInUserNo, string PlanYear, string Action, string RejectRemarks)
        {
            string resMsg = "";
            API ac = new API();

            SPBusinessPlanSPWiseForApproveReject businessPlanSPWiseApproveReject = new SPBusinessPlanSPWiseForApproveReject();
            SPBusinessPlanSPWiseOData businessPlanSPWiseOData = new SPBusinessPlanSPWiseOData();

            //SPBusinessPlanCustWiseForApprove businessPlanCustWiseForApprove = new SPBusinessPlanCustWiseForApprove();
            //SPBusinessPlanCustWiseForReject businessPlanCustWiseForReject = new SPBusinessPlanCustWiseForReject();
            //SPBusinessPlanDetails businessPlanDetails = new SPBusinessPlanDetails();
            errorDetails ed = new errorDetails();

            var result = (dynamic)null;

            if (Action == "Approve")
            {
                businessPlanSPWiseApproveReject.planyear = PlanYear;
                businessPlanSPWiseApproveReject.spcode = SPCode;
                businessPlanSPWiseApproveReject.approved_by_rejected_by = LoggedInUserNo;
                businessPlanSPWiseApproveReject.approvedorrejected = "Approved";
                businessPlanSPWiseApproveReject.rejectedreason = "";
                businessPlanSPWiseApproveReject.approved_rejected_on = DateTime.Now;

                //businessPlanCustWiseForApprove.Approver = LoggedInUserNo;
                //businessPlanCustWiseForApprove.Approved_By_Rejected_By = LoggedInUserNo;
                //businessPlanCustWiseForApprove.Status = "Approved";

                //result = PatchItemBusinessPlanApproveReject("Business_Plan_Customer_Wise", businessPlanCustWiseForApprove, businessPlanCustWiseForReject,
                //        businessPlanDetails, "Approve", "Plan_Year='" + PlanYear + "',Salesperson_Purchaser='" + SPCode + "',Status='Submitted'");
            }
            else if (Action == "Reject")
            {
                businessPlanSPWiseApproveReject.planyear = PlanYear;
                businessPlanSPWiseApproveReject.spcode = SPCode;
                businessPlanSPWiseApproveReject.approved_by_rejected_by = LoggedInUserNo;
                businessPlanSPWiseApproveReject.approvedorrejected = "Rejected";
                businessPlanSPWiseApproveReject.rejectedreason = RejectRemarks;
                businessPlanSPWiseApproveReject.approved_rejected_on = DateTime.Now;

                //businessPlanCustWiseForReject.Approver = LoggedInUserNo;
                //businessPlanCustWiseForReject.Approved_By_Rejected_By = LoggedInUserNo;
                //businessPlanCustWiseForReject.Status = "Rejected";
                //businessPlanCustWiseForReject.Rejected_Reason = "";

                //result = PatchItemBusinessPlanApproveReject("Business_Plan_Customer_Wise", businessPlanCustWiseForApprove, businessPlanCustWiseForReject,
                //        businessPlanDetails, "Reject", "Plan_Year='" + PlanYear + "',Salesperson_Purchaser='" + SPCode + "',Status='Submitted'");
            }

            result = PostItemForBusinessPlanSPWiseApproveReject<SPBusinessPlanSPWiseOData>("", businessPlanSPWiseApproveReject, businessPlanSPWiseOData);

            if (result.Result.Item1 != null)
            {
                resMsg = "True";
                //businessPlanDetails = result.Result.Item1;
                //ed = result.Result.Item2;
                //businessPlanDetails.errorDetails = ed;

                businessPlanSPWiseOData = result.Result.Item1;
                ed = result.Result.Item2;
                businessPlanSPWiseOData.errorDetails = ed;

            }

            if (!businessPlanSPWiseOData.errorDetails.isSuccess)
                resMsg = "Error:" + businessPlanSPWiseOData.errorDetails.message;

            return resMsg;

        }

        [Route("BusinessPlanApproveReject")]
        public string BusinessPlanApproveReject(string SPCode, string LoggedInUserNo, string PlanYear, string CustomerNo, string Action, string RejectReason)
        {
            string resMsg = "";
            API ac = new API();

            SPBusinessPlanCustWiseForApprove businessPlanCustWiseForApprove = new SPBusinessPlanCustWiseForApprove();
            SPBusinessPlanCustWiseForReject businessPlanCustWiseForReject = new SPBusinessPlanCustWiseForReject();
            SPBusinessPlanDetails businessPlanDetails = new SPBusinessPlanDetails();
            errorDetails ed = new errorDetails();
            var result = (dynamic)null;

            if (Action == "Approve")
            {
                businessPlanCustWiseForApprove.Approved_By_Rejected_By = LoggedInUserNo;
                businessPlanCustWiseForApprove.Status = "Approved";
                businessPlanCustWiseForApprove.Approved_Rejected_On = DateTime.Now;

                result = PatchItemBusinessPlanApproveReject("Business_Plan_Customer_Wise", businessPlanCustWiseForApprove, businessPlanCustWiseForReject,
                        businessPlanDetails, "Approve", "Customer_No='" + CustomerNo + "',Plan_Year='" + PlanYear + "',Salesperson_Purchaser='" + SPCode + "'");
            }
            else if (Action == "Reject")
            {
                businessPlanCustWiseForReject.Approved_By_Rejected_By = LoggedInUserNo;
                businessPlanCustWiseForReject.Status = "Rejected";
                businessPlanCustWiseForReject.Rejected_Reason = RejectReason;
                businessPlanCustWiseForReject.Approved_Rejected_On = DateTime.Now;

                result = PatchItemBusinessPlanApproveReject("Business_Plan_Customer_Wise", businessPlanCustWiseForApprove, businessPlanCustWiseForReject,
                        businessPlanDetails, "Reject", "Customer_No='" + CustomerNo + "',Plan_Year='" + PlanYear + "',Salesperson_Purchaser='" + SPCode + "'");
            }

            if (result.Result.Item1 != null)
            {
                resMsg = "True";
                businessPlanDetails = result.Result.Item1;
                ed = result.Result.Item2;
                businessPlanDetails.errorDetails = ed;
            }

            if (!businessPlanDetails.errorDetails.isSuccess)
                resMsg = "Error:" + businessPlanDetails.errorDetails.message;

            return resMsg;

        }

        [Route("AssignBusinessPlan")]
        public string AssignBusinessPlan(string FinancialYear, string FromSPCode, string ToSPCode, string CustomerNos)
        {
            string response = "", Err = "";
            API ac = new API();
            string[] CustomerNos_ = CustomerNos.Split(',');

            for (int a = 0; a < CustomerNos_.Length; a++)
            {
                SPBusinessPlanAssignPost reqBusinessPlanAssign = new SPBusinessPlanAssignPost();
                SPBusinessPlanAssignOData resBusinessPlanAssign = new SPBusinessPlanAssignOData();
                errorDetails ed = new errorDetails();

                reqBusinessPlanAssign.planyear = FinancialYear;
                reqBusinessPlanAssign.currentsalespersoncode = FromSPCode;
                reqBusinessPlanAssign.newsalespersoncode = ToSPCode;
                reqBusinessPlanAssign.customerno = CustomerNos_[a];

                var result = PostItemAssignBusinessPlan<SPBusinessPlanAssignOData>("", reqBusinessPlanAssign, resBusinessPlanAssign);

                response = result.Result.Item1.value;
                ed = result.Result.Item2;
                resBusinessPlanAssign.errorDetails = ed;

                if (!resBusinessPlanAssign.errorDetails.isSuccess)
                {
                    response = "Error_:" + resBusinessPlanAssign.errorDetails.message;
                    return response;
                }
                else
                    response = "True";
            }

            return response;
        }

        [Route("GetTotalDemandAndTargetQtyOfAllCust")]
        public SPBusinessPlanTotalQtyDetails GetTotalDemandAndTargetQtyOfAllCust(string SPCode, string filter)
        {
            API ac = new API();
            List<SPBusinessPlanDetails> businessPlanDetails = new List<SPBusinessPlanDetails>();
            SPBusinessPlanTotalQtyDetails qtyDetails = new SPBusinessPlanTotalQtyDetails();

            if (filter == "" || filter == null)
                filter = "Salesperson_Purchaser eq '" + SPCode + "'";
            else
                filter = filter + " and Salesperson_Purchaser eq '" + SPCode + "'";

            qtyDetails.totalDemandQty = qtyDetails.totalTargetQty = 0;

            var result = ac.GetData<SPBusinessPlanDetails>("Business_Plan_Customer_Wise", filter);

            if (result.Result.Item1.value.Count > 0)
            {
                businessPlanDetails = result.Result.Item1.value;

                for (int a = 0; a < businessPlanDetails.Count; a++)
                {
                    qtyDetails.totalDemandQty += Convert.ToDouble(businessPlanDetails[a].Total_Demand_Qty);
                    qtyDetails.totalTargetQty += Convert.ToDouble(businessPlanDetails[a].Targeted_Qty);
                }
            }

            return qtyDetails;
        }

        public async Task<(SPBusinessPlanRes, errorDetails)> PatchItemBusinessPlan<SPBusinessPlanRes>(string apiendpoint, SPBusinessPlanPost requestModel, SPBusinessPlanRes responseModel, string fieldWithValue)
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
                    responseModel = res.ToObject<SPBusinessPlanRes>();


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

        public async Task<(SPBusinessPlanDetails, errorDetails)> PatchItemBusinessPlanStatus<SPBusinessPlanDetails>(string apiendpoint, SPBusinessPlanCustWisePost requestModel, SPBusinessPlanDetails responseModel, string fieldWithValue)
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
                    responseModel = res.ToObject<SPBusinessPlanDetails>();


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

        public async Task<(SPBusinessPlanDetails, errorDetails)> PatchItemBusinessPlanSendApproval<SPBusinessPlanDetails>(string apiendpoint, SPBusinessPlanCustWiseSendApproval requestModel, SPBusinessPlanDetails responseModel, string fieldWithValue)
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
                    responseModel = res.ToObject<SPBusinessPlanDetails>();


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

        public async Task<(SPBusinessPlanDetails, errorDetails)> PatchItemBusinessPlanApproveReject<SPBusinessPlanDetails>(string apiendpoint, SPBusinessPlanCustWiseForApprove requestModelApprove, SPBusinessPlanCustWiseForReject requestModelReject, SPBusinessPlanDetails responseModel, string Action, string fieldWithValue)
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
                    responseModel = res.ToObject<SPBusinessPlanDetails>();


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

        public async Task<(SPBusinessPlanAssignOData, errorDetails)> PostItemAssignBusinessPlan<SPBusinessPlanAssignOData>(string apiendpoint, SPBusinessPlanAssignPost requestModel, SPBusinessPlanAssignOData responseModel)
        {
            string _baseURL = System.Configuration.ConfigurationManager.AppSettings["BaseURL"];
            string _tenantId = System.Configuration.ConfigurationManager.AppSettings["TenantID"];
            string _environment = System.Configuration.ConfigurationManager.AppSettings["Environment"];
            string _companyName = System.Configuration.ConfigurationManager.AppSettings["CompanyName"];

            API ac = new API();
            var accessToken = await ac.GetAccessToken();

            HttpClient _httpClient = new HttpClient();
            string encodeurl = Uri.EscapeUriString("https://api.businesscentral.dynamics.com/v2.0/e55ad508-ef1a-489f-afe3-ae21f856e440/Sandbox/ODataV4/CodeunitAPIMgmt_UpdateSalesPersonCode?company=\'Prakash Company\'");
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
                    responseModel = res.ToObject<SPBusinessPlanAssignOData>();

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

        public async Task<(SPBusinessPlanSPWiseOData, errorDetails)> PostItemForBusinessPlanSPWiseApproveReject<SPBusinessPlanSPWiseOData>(string apiendpoint, SPBusinessPlanSPWiseForApproveReject requestModel, SPBusinessPlanSPWiseOData responseModel)
        {
            string _baseURL = System.Configuration.ConfigurationManager.AppSettings["BaseURL"];
            string _tenantId = System.Configuration.ConfigurationManager.AppSettings["TenantID"];
            string _environment = System.Configuration.ConfigurationManager.AppSettings["Environment"];
            string _companyName = System.Configuration.ConfigurationManager.AppSettings["CompanyName"];

            API ac = new API();
            var accessToken = await ac.GetAccessToken();

            HttpClient _httpClient = new HttpClient();
            string encodeurl = Uri.EscapeUriString("https://api.businesscentral.dynamics.com/v2.0/e55ad508-ef1a-489f-afe3-ae21f856e440/Sandbox/ODataV4/CodeunitAPIMgmt_UpdateMultipleBusinessPlanCustWiseBySPCode?company=\'Prakash Company\'");
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
                    responseModel = res.ToObject<SPBusinessPlanSPWiseOData>();

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

        [HttpGet]
        [Route("GetBusinessReport")]
        public CombineSPBusinessPlanReport GetBusinessReport(string Sales_Person, string Year, string No, string SerachSP)
        {
            API ac = new API();
            CombineSPBusinessPlanReport combineSPBusinessPlan = new CombineSPBusinessPlanReport();
            List<SPBusinessPlanReport> businessreport = new List<SPBusinessPlanReport>();

            string normalizedYear = null;
            string filter = "";
            if (!string.IsNullOrWhiteSpace(Year))
            {
                normalizedYear = Year.Replace("Year eq", "").Replace("'", "").Trim();
                filter = $"Year eq '{normalizedYear}'";
            }
            if (!string.IsNullOrWhiteSpace(Sales_Person))
            {
                string sp = Sales_Person.Replace("Sales_Person eq", "").Replace("'", "").Trim();
                filter += (filter == "" ? "" : " and ") + $"Sales_Person eq '{sp}'";
            }
            if (!string.IsNullOrEmpty(SerachSP))
            {
                filter += " and SalesPerson_Name eq '" + SerachSP + "'";
            }
            var BusinessReport = ac.GetData<SPBusinessPlanReport>("BusinessPlanReport", filter);
            //login person Single BusinessPlan 

            if (BusinessReport?.Result.Item1?.value != null && BusinessReport.Result.Item1.value.Count > 0)
                businessreport = BusinessReport.Result.Item1.value;

            //Primary SalesPrson under 
            string businessplanFilter = "PCPL_Reporting_Person_No eq '" + No + "'";

            List<SPPCPLEmployeeList> allbusinessreport = new List<SPPCPLEmployeeList>();
            var AllBusinessplanreport = ac.GetData<SPPCPLEmployeeList>("PcplEmployeeList", businessplanFilter);
            if (AllBusinessplanreport.Result.Item1.value.Count > 0)
            {
                allbusinessreport = AllBusinessplanreport.Result.Item1.value;
            }
            else
            {
                allbusinessreport = null;
            }
            var EmployeePlanReport = new List<SPBusinessPlanReport>();

            foreach (var employeelist in allbusinessreport)
            {
                var PCPL_SP = employeelist.PCPL_Salespers_Purch_Name;

                // If a specific salesperson is requested, only fetch for that person
                if (!string.IsNullOrEmpty(SerachSP) && !string.Equals(PCPL_SP, SerachSP, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string filterEmployeeList;
                string spName = !string.IsNullOrEmpty(SerachSP) ? SerachSP : PCPL_SP;

                if (!string.IsNullOrEmpty(normalizedYear))
                {
                    filterEmployeeList = $"SalesPerson_Name eq '{spName}' and Year eq '{normalizedYear}'";
                }
                else
                {
                    filterEmployeeList = $"SalesPerson_Name eq '{spName}'";
                }

                var report = ac.GetData<SPBusinessPlanReport>("BusinessPlanReport", filterEmployeeList);

                if (report?.Result.Item1?.value != null && report.Result.Item1.value.Count > 0)
                {

                    EmployeePlanReport.AddRange(report.Result.Item1.value);
                    //EmployeePlanReport.AddRange(report.Result.Item1.value.DistinctBy(c => c.SalesPerson_Name).ToList());
                }
            }
            combineSPBusinessPlan.BusinessPlanReports = businessreport;
            combineSPBusinessPlan.EmployeePlanReports = EmployeePlanReport.Where(x => !businessreport.Any(y => y.SalesPerson_Name == x.SalesPerson_Name)).ToList();

            return combineSPBusinessPlan;
        }

        [HttpGet]
        [Route("GetBusinessReportSP")]
        public List<SPBusinessReportSP> GetBusinessReportSP(string SalesPerson, string Year = null)
        {
            API ac = new API();
            List<SPBusinessReportSP> businessreportsp = new List<SPBusinessReportSP>();

            string filter = "";
            filter += "SalesPerson_Name eq '" + SalesPerson + "'";

            if (!string.IsNullOrWhiteSpace(Year) && Year != "-1")
            {
                filter += " and Year eq '" + Year + "'";
            }

            var result = ac.GetData<SPBusinessReportSP>("BusinessPlanSPItemWiseDotNetAPI", filter);

            if (result != null && result.Result.Item1.value.Count > 0)
            {
                businessreportsp = result.Result.Item1.value;
            }
            return businessreportsp;
        }


        [HttpGet]
        [Route("GetSalespersonDropDwon")]
        public List<SPSalespersonDropDwon> GetSalespersonDropDwon()
        {
            API ac = new API();
            List<SPSalespersonDropDwon> salespersondropdwon = new List<SPSalespersonDropDwon>();
            var result = ac.GetData<SPSalespersonDropDwon>("BusinessPlanReport", "");
            if (result != null && result.Result.Item1.value.Count > 0)
            {
                salespersondropdwon = result.Result.Item1.value;
                salespersondropdwon = salespersondropdwon.DistinctBy(a => a.Sales_Person).ToList();
            }
            return salespersondropdwon;
        }

        [HttpGet]
        [Route("GetBusinessReportYears")]
        public List<string> GetBusinessReportYears()
        {
            API ac = new API();
            List<string> years = new List<string>();

            var result = ac.GetData<SPBusinessPlanReport>("BusinessPlanReport", "");
            if (result?.Result.Item1?.value != null && result.Result.Item1.value.Count > 0)
            {
                years = result.Result.Item1.value
                    .Where(x => !string.IsNullOrWhiteSpace(x.Year))
                    .Select(x => x.Year.Trim())
                    .Distinct()
                    .OrderByDescending(x => x)
                    .ToList();
            }

            return years;
        }

        [Route("AddBisunesPlanProducts")]
        public bool AddBisunesPlanProducts(string CCompanyNo, string ProdNo, string SPCode, string ContactNo)
        {
            bool flag = false;

            API ac = new API();
            SPBusinessProductsPost reqContactProduct = new SPBusinessProductsPost();
            reqContactProduct.Customer_No = CCompanyNo;
            reqContactProduct.Item_No = ProdNo;
            reqContactProduct.SalesPerson_Code = SPCode;
            reqContactProduct.Contact_No = ContactNo;


            errorDetails ed = new errorDetails();
            SPBusinessProductsPost resContactProduct = new SPBusinessProductsPost();

            var result = ac.PostItem("ContactProductsDotNetAPI", reqContactProduct, resContactProduct);

            if (result.Result.Item1 != null)
            {
                resContactProduct = result.Result.Item1;
                flag = true;
            }

            if (result.Result.Item2.message != null)
                ed = result.Result.Item2;

            return flag;
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