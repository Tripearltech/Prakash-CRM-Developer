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
using System.Net.Http.Headers;
using System.Web.Configuration;
using System.Data;
using System.Drawing;
using System.Web.Http.Results;
using Microsoft.SqlServer.Server;

using System.Web.Http.Cors;


namespace PrakashCRM.Service.Controllers
{
    [RoutePrefix("api/SPVisitEntry")]

    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class SPVisitEntryController : ApiController
    {
        [Route("GetVisitTypesForDDL")]
        public List<SPVisitTypes> GetVisitTypesForDDL()
        {
            API ac = new API();
            List<SPVisitTypes> visittypes = new List<SPVisitTypes>();

            var result = ac.GetData<SPVisitTypes>("VisitTypesDotNetAPI", ""); // and Contact_Business_Relation eq 'Customer'

            if (result.Result.Item1.value.Count > 0)
                visittypes = result.Result.Item1.value;

            return visittypes;
        }

        [Route("GetVisitSubTypesForDDL")]
        public List<SPVisitSubTypes> GetVisitSubTypesForDDL(string TypeNo)
        {
            API ac = new API();
            List<SPVisitSubTypes> visitsubtypes = new List<SPVisitSubTypes>();

            var result = ac.GetData<SPVisitSubTypes>("VisitSubTypesDotNetAPI", "Type eq '" + TypeNo + "'"); // and Contact_Business_Relation eq 'Customer'

            if (result.Result.Item1.value.Count > 0)
                visitsubtypes = result.Result.Item1.value;

            return visitsubtypes;
        }

        [Route("GetProductsForDDL")]
        public List<SPVEContactProducts> GetProductsForDDL(string SPCode, string CCompanyNo)
        {
            API ac = new API();
            List<SPVEContactProducts> prods = new List<SPVEContactProducts>();

            var result = ac.GetData<SPVEContactProducts>("ContactProductsDotNetAPI", "SalesPerson_Code eq '" + SPCode + "' and Contact_No eq '" + CCompanyNo + "' and Blocked eq false"); // and Contact_Business_Relation eq 'Customer'

            if (result.Result.Item1.value.Count > 0)
                prods = result.Result.Item1.value;

            return prods;
        }

        [Route("GetAllProductsForDDL")]
        public List<SPItemList> GetAllProductsForDDL()
        {
            API ac = new API();
            List<SPItemList> prods = new List<SPItemList>();

            var result = ac.GetData<SPItemList>("ItemDotNetAPI", "Blocked eq false"); // and Contact_Business_Relation eq 'Customer'

            if (result.Result.Item1.value.Count > 0)
                prods = result.Result.Item1.value;

            return prods;
        }

        [Route("GetInvoiceProductsForDDL")]
        public List<SPVEInvoiceProducts> GetInvoiceProductsForDDL(string InvNo)
        {
            API ac = new API();
            List<SPVEInvoiceProducts> invoiceprods = new List<SPVEInvoiceProducts>();

            var result = ac.GetData<SPVEInvoiceProducts>("postedSalesInvoiceSubformDotNetAPI", "Document_No eq '" + InvNo + "'"); // and Contact_Business_Relation eq 'Customer'

            if (result.Result.Item1.value.Count > 0)
                invoiceprods = result.Result.Item1.value;

            return invoiceprods;
        }

        [Route("GetCompetitorsForDDL")]
        public List<SPVECompetitors> GetCompetitorsForDDL()
        {
            API ac = new API();
            List<SPVECompetitors> competitors = new List<SPVECompetitors>();

            var result = ac.GetData<SPVECompetitors>("CompetitorsDotNetAPI", ""); // and Contact_Business_Relation eq 'Customer'

            if (result.Result.Item1.value.Count > 0)
                competitors = result.Result.Item1.value;

            return competitors;
        }

        [Route("GetProductDetails")]
        public SPVEProductDetails GetProductDetails(string CCompanyNo, string ProductNo)
        {
            API ac = new API();
            SPItemList itemDetails = new SPItemList();
            SPVEContactProductDetails contactProdDetails = new SPVEContactProductDetails();
            SPVEProductDetails prodDetails = new SPVEProductDetails();

            var result = (dynamic)null;

            if (CCompanyNo == null || CCompanyNo == "")
            {
                result = ac.GetData<SPItemList>("ItemDotNetAPI", "No eq '" + ProductNo + "'");

                if (result.Result.Item1.value.Count > 0)
                {
                    itemDetails = result.Result.Item1.value[0];
                    prodDetails.UOM = itemDetails.Base_Unit_of_Measure;
                    prodDetails.Competitor = "";
                }
            }
            else
            {
                result = ac.GetData<SPVEContactProductDetails>("ContactProductsDotNetAPI", "Contact_No eq '" + CCompanyNo + "' and Item_No eq '" + ProductNo + "'");

                if (result.Result.Item1.value.Count > 0)
                {
                    contactProdDetails = result.Result.Item1.value[0];
                    prodDetails.UOM = contactProdDetails.PCPL_Unit_Of_Measure;
                    prodDetails.Competitor = contactProdDetails.Competitor;
                }

            }

            //string ProdUOM = itemdetails.PCPL_Unit_Of_Measure;

            //return ProdUOM;
            //return itemdetails;
            return prodDetails;
        }

        [Route("GetSalespersonForDDL")]
        public List<VisitEntrySP> GetSalespersonForDDL()
        {
            API ac = new API();
            List<VisitEntrySP> salespersons = new List<VisitEntrySP>();

            var result = ac.GetData<VisitEntrySP>("SalespersonPurchaserDotNetAPI", ""); // and Contact_Business_Relation eq 'Customer'

            if (result.Result.Item1.value.Count > 0)
                salespersons = result.Result.Item1.value;

            return salespersons;
        }

        [Route("GetReportingPersonEmail")]
        public string GetReportingPersonEmail(string SPCode)
        {
            if (string.IsNullOrWhiteSpace(SPCode))
                return "";

            API ac = new API();
            var result = ac.GetData<SPUserReportingPersonDetails>("EmployeesDotNetAPI", "Salespers_Purch_Code eq '" + SPCode + "'");

            if (result.Result.Item1 != null && result.Result.Item1.value != null && result.Result.Item1.value.Count > 0)
                return result.Result.Item1.value[0].PCPL_Reporting_Person_Email ?? "";

            return "";
        }

        [Route("GetContactCompanyForDDL")]
        public List<SPVEContactCompany> GetContactCompanyForDDL(string SPCode)
        {
            API ac = new API();
            List<SPVEContactCompany> companies = new List<SPVEContactCompany>();

            var result = ac.GetData<SPVEContactCompany>("ContactDotNetAPI", "Type eq 'Company' and Salesperson_Code eq '" + SPCode + "'"); // and Contact_Business_Relation eq 'Customer'

            if (result.Result.Item1.value.Count > 0)
                companies = result.Result.Item1.value;

            List<SPVEContactCompany> company2 = new List<SPVEContactCompany>();

            var result2 = ac.GetData<SPVEContactCompany>("ContactDotNetAPI", "PCPL_Secondary_SP_Code eq '" + SPCode + "' and Salesperson_Code ne '" + SPCode + "' and Type eq 'Company'");

            if (result2 != null && result2.Result.Item1.value.Count > 0)
            {
                company2 = result2.Result.Item1.value;

                companies.AddRange(company2);
            }

            return companies;
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

        [Route("GetContactPersonForDDL")]
        public List<SPVEContactPerson> GetContactPersonForDDL(string CompanyNo)
        {
            API ac = new API();
            List<SPVEContactPerson> contacts = new List<SPVEContactPerson>();

            var result = ac.GetData<SPVEContactPerson>("ContactDotNetAPI", "Type eq 'Person' and Company_No eq '" + CompanyNo + "'"); // and Contact_Business_Relation eq 'Customer'

            if (result.Result.Item1.value.Count > 0)
                contacts = result.Result.Item1.value;

            return contacts;
        }

        [Route("GetCustomerInvoiceForDDL")]
        public List<SPDailyVisitInvoiceDetails> GetCustomerInvoiceForDDL(string CompanyNo)
        {
            API ac = new API();
            ConBusinessRelation conBusinessRelation = new ConBusinessRelation();
            string CustNo = "";

            var resultCustomerNo = ac.GetData<ConBusinessRelation>("ContactBusinessRelationsDotNetAPI", "Contact_No eq '" + CompanyNo + "'");

            if (resultCustomerNo.Result.Item1.value.Count > 0)
                CustNo = conBusinessRelation.No = resultCustomerNo.Result.Item1.value[0].No;

            List<SPDailyVisitInvoiceDetails> invoiceDetails = new List<SPDailyVisitInvoiceDetails>();

            var result = ac.GetData<SPDailyVisitInvoiceDetails>("PostedSalesInvoicesDotNetAPI", "Sell_to_Customer_No eq '" + CustNo + "'");

            if (result.Result.Item1.value.Count > 0)
                invoiceDetails = result.Result.Item1.value;

            return invoiceDetails;
        }

        [Route("GetDailyWeeklyPlanEventsForDDL")]
        public List<SPWeeklyDailyPlanEvents> GetDailyWeeklyPlanEventsForDDL(string TypeNo, string SubTypeNo)
        {
            API ac = new API();
            List<SPWeeklyDailyPlanEvents> events = new List<SPWeeklyDailyPlanEvents>();

            var result = ac.GetData<SPWeeklyDailyPlanEvents>("EventMastersDotNetAPI", "Type_No eq '" + TypeNo + "' and SubType_No eq '" + SubTypeNo + "'"); // and Contact_Business_Relation eq 'Customer'

            if (result.Result.Item1.value.Count > 0)
                events = result.Result.Item1.value;

            return events;
        }

        [Route("GetWeekPlanForDailyPlan")]
        public List<SPVEWeekSalesPlan> GetWeekPlanForDailyPlan(string date, string SPCode)
        {
            API ac = new API();
            List<SPVEWeekSalesPlan> weekplans = new List<SPVEWeekSalesPlan>();

            var result = ac.GetData<SPVEWeekSalesPlan>("WeeklySalesPlanCardDotNetAPI", "Week_Plan_Date eq " + date + " and SalesPerson_Code eq '" + SPCode + "'" + " and Entry_Type eq 'Plan'");

            if (result.Result.Item1.value.Count > 0)
                weekplans = result.Result.Item1.value;

            return weekplans;
        }

        [Route("GetWeekPlanNoDetailsForDDL")]
        public List<SPWeekPlanNoDetails> GetWeekPlanNoDetailsForDDL(string SPCode)
        {
            API ac = new API();
            List<SPWeekPlanNoDetails> weekPlanNoDetails = new List<SPWeekPlanNoDetails>();

            var result = ac.GetData<SPWeekPlanNoDetails>("WeeklyPlanSalesPersonWise", "Sales_Person_Code eq '" + SPCode + "'");

            if (result.Result.Item1.value.Count > 0)
                weekPlanNoDetails = result.Result.Item1.value;

            return weekPlanNoDetails;
        }

        [Route("YearMonthVisitPlan")]
        public SPVEYearMonthPlan YearMonthVisitPlan(SPVEYearMonthPlanPost reqYearMonthPlan, bool isEdit, string YearMonthPlanNo)
        {
            var ac = new API();
            errorDetails ed = new errorDetails();
            SPVEYearMonthPlan resYearMonthPlan = new SPVEYearMonthPlan();

            var result = (dynamic)null;

            //if (isEdit)
            //    result = PatchItemYearMonthPlan("YearlyMonthVisitDotNetAPI", reqYearMonthPlan, resYearMonthPlan, "No='" + YearMonthPlanNo + "'");
            //else
            //    result = PostItemYearMonthPlan("YearlyMonthVisitDotNetAPI", reqYearMonthPlan, resYearMonthPlan);

            if (isEdit)
            {
                reqYearMonthPlan.IsActive = true;
                result = PatchItemYearMonthPlan("YearlyMonthVisitDotNetAPI", reqYearMonthPlan, resYearMonthPlan, "No='" + YearMonthPlanNo + "'");
            }
            else
            {
                SPVEYearMonthPlan yearMonthPlan = chkYearMonthVisitPlan(reqYearMonthPlan.Year, reqYearMonthPlan.Visit_Type, reqYearMonthPlan.Visit_Sub_Type, reqYearMonthPlan.SalesPerson_Code);
                SPVEYearMonthPlanPost yearMonthPlanForMultEntry = new SPVEYearMonthPlanPost();
                SPVEYearMonthPlan yearMonthPlanForMultEntryRes = new SPVEYearMonthPlan();
                errorDetails ed1 = new errorDetails();

                if (yearMonthPlan.No != null)
                {
                    yearMonthPlanForMultEntry.Edate = yearMonthPlan.Edate;
                    yearMonthPlanForMultEntry.Year = yearMonthPlan.Year;
                    yearMonthPlanForMultEntry.Visit_Type = yearMonthPlan.Visit_Type;
                    yearMonthPlanForMultEntry.Visit_Sub_Type = yearMonthPlan.Visit_Sub_Type;
                    yearMonthPlanForMultEntry.SalesPerson_Code = yearMonthPlan.SalesPerson_Code;
                    yearMonthPlanForMultEntry.No_of_Visit = yearMonthPlan.No_of_Visit + reqYearMonthPlan.No_of_Visit;
                    yearMonthPlanForMultEntry.IsActive = true;

                    result = PatchItemYearMonthPlan("YearlyMonthVisitDotNetAPI", yearMonthPlanForMultEntry, yearMonthPlanForMultEntryRes, "No='" + yearMonthPlan.No + "'");

                    if (result.Result.Item1.No != null)
                    {
                        yearMonthPlanForMultEntryRes = result.Result.Item1;

                        reqYearMonthPlan.No_of_Visit = yearMonthPlan.No_of_Visit;
                        reqYearMonthPlan.IsActive = false;
                        var result1 = PostItemYearMonthPlan("YearlyMonthVisitDotNetAPI", reqYearMonthPlan, resYearMonthPlan);

                        if (result1.Result.Item1.No != null)
                            resYearMonthPlan = result1.Result.Item1;

                        if (result1.Result.Item2.message != null)
                        {
                            ed1 = result1.Result.Item2;
                            resYearMonthPlan.errorDetails = ed1;
                        }

                    }

                    if (result.Result.Item2.message != null)
                    {
                        ed = result.Result.Item2;
                        resYearMonthPlan.errorDetails = ed;
                    }

                }
                else
                {
                    reqYearMonthPlan.IsActive = true;
                    var result2 = PostItemYearMonthPlan("YearlyMonthVisitDotNetAPI", reqYearMonthPlan, resYearMonthPlan);

                    resYearMonthPlan = result2.Result.Item1;
                    ed1 = result2.Result.Item2;
                    resYearMonthPlan.errorDetails = ed1;

                    //if (result2.Result.Item1.No != null)
                    //    resYearMonthPlan = result2.Result.Item1;

                    //if (result2.Result.Item2.message != null)
                    //    ed1 = result2.Result.Item2;
                }


            }


            resYearMonthPlan = result.Result.Item1;
            ed = result.Result.Item2;
            resYearMonthPlan.errorDetails = ed;

            //if (result.Result.Item2.message != null)
            //    ed = result.Result.Item2;

            return resYearMonthPlan;
        }

        [Route("GetApiRecordsCount")]
        public int GetApiRecordsCount(string SPCode, string Page, string apiEndPointName, string filter)
        {
            API ac = new API();
            if (Page == "DailyVisit")
            {
                if (filter == null)
                    filter = "Salesperson_Code eq '" + SPCode + "' and Entry_Type eq 'ENTRY'";
                else
                    filter += " and Salesperson_Code eq '" + SPCode + "' and Entry_Type eq 'ENTRY'";
            }
            else if (Page == "YearMonthPlan")
            {
                if (filter == null)
                    filter = "SalesPerson_Code eq '" + SPCode + "' and IsActive eq true";
                else
                    filter += " and SalesPerson_Code eq '" + SPCode + "' and IsActive eq true";
            }
            else if (Page == "DailyVisitExpenseReport")
            {
                if (filter == null)
                    filter = "";
            }

            var count = ac.CalculateCount(apiEndPointName, filter);

            return Convert.ToInt32(count.Result);
        }

        public SPVEYearMonthPlan chkYearMonthVisitPlan(string Year, string TypeNo, string SubTypeNo, string SPCode)
        {
            API ac = new API();
            SPVEYearMonthPlan yearMonthPlanData = new SPVEYearMonthPlan();

            var result = ac.GetData<SPVEYearMonthPlan>("YearlyMonthVisitDotNetAPI", "SalesPerson_Code eq '" + SPCode + "' and Year eq '" + Year + "' and Visit_Type eq '" + TypeNo + "' and Visit_Sub_Type eq '" + SubTypeNo + "' and IsActive eq true");
            if (result.Result.Item1.value.Count > 0)
                yearMonthPlanData = result.Result.Item1.value[0];

            return yearMonthPlanData;

        }

        [Route("GetYearMonthPlanListData")]
        public List<SPVEYearMonthPlan> GetYearMonthPlanListData(string SPCode, int skip, int top, string orderby, string filter, bool isExport = false)
        {
            API ac = new API();
            List<SPVEYearMonthPlan> yearMonthPlanData = new List<SPVEYearMonthPlan>();

            if (filter == null)
                filter = "SalesPerson_Code eq '" + SPCode + "' and IsActive eq true";
            else
                filter += " and SalesPerson_Code eq '" + SPCode + "' and IsActive eq true";

            var result = (dynamic)null;

            if (isExport)
                result = ac.GetData<SPVEYearMonthPlan>("YearlyMonthVisitDotNetAPI", filter);
            else
                result = ac.GetData1<SPVEYearMonthPlan>("YearlyMonthVisitDotNetAPI", filter, skip, top, orderby);

            if (result.Result.Item1.value.Count > 0)
                yearMonthPlanData = result.Result.Item1.value;

            return yearMonthPlanData;
        }

        [Route("GetYearMonthVisitPlanNo")]
        public SPVEYearMonthPlanPost GetYearMonthVisitPlanNo(string No)
        {
            API ac = new API();
            SPVEYearMonthPlanPost yearMonthPlan = new SPVEYearMonthPlanPost();

            var result = ac.GetData<SPVEYearMonthPlanPost>("YearlyMonthVisitDotNetAPI", "No eq '" + No + "'");

            if (result.Result.Item1.value.Count > 0)
            {
                yearMonthPlan = result.Result.Item1.value[0];

            }

            return yearMonthPlan;
        }

        [Route("GetYearMonthPlanDataForYear")]
        public List<SPVEYearMonthPlan> GetYearMonthPlanDataForYear(string SPCode, string Year)
        {
            API ac = new API();
            List<SPVEYearMonthPlan> yearmonthplan = new List<SPVEYearMonthPlan>();

            var result = ac.GetData<SPVEYearMonthPlan>("YearlyMonthVisitDotNetAPI", "SalesPerson_Code eq '" + SPCode + "' and Year eq '" + Year + "' and IsActive eq true");


            if (result.Result.Item1.value.Count > 0)
                yearmonthplan = result.Result.Item1.value;

            return yearmonthplan;
        }

        [HttpPost]
        [Route("AddMonthlyVisit")]
        public bool AddMonthlyVisit(string YearPlanNo, string Month, string Type, string SubType, int YearNoOfVisit, string Edate, int NoOfVisit, string Year, string SPCode)
        {
            var ac = new API();
            bool flag = false;
            errorDetails ed = new errorDetails();
            errorDetails ed1 = new errorDetails();
            SPVEMonthlyPlanPost reqMonthPlanUpdate = new SPVEMonthlyPlanPost();
            SPVEMonthlyPlan resMonthPlanUpdate = new SPVEMonthlyPlan();

            SPVEMonthlyPlanPost reqMonthPlan = new SPVEMonthlyPlanPost();
            SPVEMonthlyPlan resMonthPlan = new SPVEMonthlyPlan();

            SPVEMonthlyPlan monthPlan = chkMonthPlan(Year, Month, SPCode, Type, SubType);

            if (monthPlan.No != null)
            {
                reqMonthPlanUpdate.Yearly_Plan_No = monthPlan.Yearly_Plan_No;
                reqMonthPlanUpdate.Visit_Month = monthPlan.Visit_Month;
                reqMonthPlanUpdate.Visit_Type_No = monthPlan.Visit_Type_No;
                reqMonthPlanUpdate.Visit_Sub_Type_No = monthPlan.Visit_Sub_Type_No;
                reqMonthPlanUpdate.No_of_Visit = monthPlan.No_of_Visit + NoOfVisit;
                reqMonthPlanUpdate.Visit_Year = monthPlan.Visit_Year;
                reqMonthPlanUpdate.Salesperson_Code = monthPlan.Salesperson_Code;
                reqMonthPlanUpdate.IsActive = monthPlan.IsActive;

                var result = PatchItemForMonthVisit("MonthlyVisitsDotNetAPI", reqMonthPlanUpdate, resMonthPlanUpdate, "No='" + monthPlan.No + "'");
                if (result.Result.Item1.No != null)
                {
                    //flag = true;
                    resMonthPlanUpdate = result.Result.Item1;

                    reqMonthPlan.Yearly_Plan_No = YearPlanNo;
                    reqMonthPlan.Visit_Month = Month;
                    reqMonthPlan.Visit_Type_No = Type;
                    reqMonthPlan.Visit_Sub_Type_No = SubType;
                    reqMonthPlan.No_of_Visit = monthPlan.No_of_Visit;
                    reqMonthPlan.Visit_Year = Year;
                    reqMonthPlan.Salesperson_Code = SPCode;
                    reqMonthPlan.IsActive = false;

                    var result1 = PostItemForMonthVisit("MonthlyVisitsDotNetAPI", reqMonthPlan, resMonthPlan);
                    if (result1.Result.Item1.No != null)
                    {
                        flag = true;
                        resMonthPlan = result1.Result.Item1;
                    }

                    if (result1.Result.Item2.message != null)
                        ed1 = result1.Result.Item2;
                }
            }
            else
            {
                reqMonthPlan.Yearly_Plan_No = YearPlanNo;
                reqMonthPlan.Visit_Month = Month;
                reqMonthPlan.Visit_Type_No = Type;
                reqMonthPlan.Visit_Sub_Type_No = SubType;
                reqMonthPlan.No_of_Visit = NoOfVisit;
                reqMonthPlan.Visit_Year = Year;
                reqMonthPlan.Salesperson_Code = SPCode;
                reqMonthPlan.IsActive = true;

                var result1 = PostItemForMonthVisit("MonthlyVisitsDotNetAPI", reqMonthPlan, resMonthPlan);
                if (result1.Result.Item1.No != null)
                {
                    flag = true;
                    resMonthPlan = result1.Result.Item1;
                }

                if (result1.Result.Item2.message != null)
                    ed1 = result1.Result.Item2;
            }

            List<SPVEMonthlyPlan> monthPlans = new List<SPVEMonthlyPlan>();

            var result2 = ac.GetData<SPVEMonthlyPlan>("MonthlyVisitsDotNetAPI", "Salesperson_Code eq '" + SPCode + "' and Visit_Year eq '" + Year + "' and Visit_Type_No eq '" + Type + "' and Visit_Sub_Type_No eq '" + SubType + "' and IsActive eq true");

            if (result2.Result.Item1.value.Count > 0)
                monthPlans = result2.Result.Item1.value;

            int totalMonthlyNoOfVisit = 0;
            if (monthPlans.Count > 0)
            {
                for (int a = 0; a < monthPlans.Count; a++)
                {
                    totalMonthlyNoOfVisit += monthPlans[a].No_of_Visit;
                }

                if (totalMonthlyNoOfVisit > YearNoOfVisit)
                {
                    SPVEYearMonthPlanPost reqYearMonthPlan = new SPVEYearMonthPlanPost();
                    SPVEYearMonthPlan resYearMonthPlan = new SPVEYearMonthPlan();
                    errorDetails ed2 = new errorDetails();

                    reqYearMonthPlan.Year = Year;
                    reqYearMonthPlan.Edate = Edate;
                    reqYearMonthPlan.Visit_Type = Type;
                    reqYearMonthPlan.Visit_Sub_Type = SubType;
                    reqYearMonthPlan.No_of_Visit = totalMonthlyNoOfVisit;
                    reqYearMonthPlan.SalesPerson_Code = SPCode;
                    reqYearMonthPlan.IsActive = true;

                    var result3 = PatchItemYearMonthPlan("YearlyMonthVisitDotNetAPI", reqYearMonthPlan, resYearMonthPlan, "No='" + YearPlanNo + "'");

                    if (result3.Result.Item1.No != null)
                        resYearMonthPlan = result3.Result.Item1;

                    if (result3.Result.Item2.message != null)
                        ed2 = result3.Result.Item2;

                }

            }

            //bool flag = false;

            //reqMonthPlan.Yearly_Plan_No = YearPlanNo;
            //reqMonthPlan.Visit_Month = Month;
            //reqMonthPlan.Visit_Type_No = Type;
            //reqMonthPlan.Visit_Sub_Type_No = SubType;
            //reqMonthPlan.No_of_Visit = NoOfVisit;
            //reqMonthPlan.Visit_Year = Year;
            //reqMonthPlan.Salesperson_Code = SPCode;
            //reqMonthPlan.IsActive = true;

            //var result = PostItemForMonthVisit("MonthlyVisitsDotNetAPI", reqMonthPlan, resMonthPlan);

            ////var result = PostItemYearMonthPlan("YearlyMonthVisitDotNetAPI", reqYearMonthPlan, resYearMonthPlan);

            //if (result.Result.Item1.No != null)
            //{
            //    flag = true;
            //    resMonthPlan = result.Result.Item1;
            //}

            //if (result.Result.Item2.message != null)
            //    ed = result.Result.Item2;

            return flag;
        }

        [HttpPost]
        [Route("api/SPVisitEntry/AddNewCompetitors")]
        public string AddNewCompetitors(string NewCompetitors)
        {
            string resMsg = "";
            string[] NewCompetitors_ = NewCompetitors.Split(',');

            for (int a = 0; a < NewCompetitors_.Length; a++)
            {
                SPVECompetitorPost reqCompetitor = new SPVECompetitorPost();
                SPVECompetitors resCompetitor = new SPVECompetitors();
                errorDetails ed = new errorDetails();

                reqCompetitor.competitor_Name = NewCompetitors_[a];
                reqCompetitor.IsActive = true;

                var result = PostItemCompetitor("CompetitorsDotNetAPI", reqCompetitor, resCompetitor);

                if (result.Result.Item1 != null)
                {
                    resCompetitor = result.Result.Item1;
                    ed = result.Result.Item2;

                    if (!ed.isSuccess)
                    {
                        resMsg = "Error:" + ed.message;
                        return resMsg;
                    }
                }
            }

            return resMsg;
        }
        [Route("GetMonthlyVisitDataForYear")]
        public List<SPVEMonthlyPlanList> GetMonthlyVisitDataForYear(string SPCode, string Year, string TypeNo, string SubTypeNo)
        {
            API ac = new API();
            List<SPVEMonthlyPlanList> monthplanlist = new List<SPVEMonthlyPlanList>();

            var result = ac.GetData<SPVEMonthlyPlanList>("MonthlyVisitsDotNetAPI", "Salesperson_Code eq '" + SPCode + "' and Visit_Year eq '" + Year + "' and Visit_Type_No eq '" + TypeNo + "' and Visit_Sub_Type_No eq '" + SubTypeNo + "' and IsActive eq true");


            if (result.Result.Item1.value.Count > 0)
                monthplanlist = result.Result.Item1.value;
            return monthplanlist;
        }

        public SPVEMonthlyPlan chkMonthPlan(string Year, string Month, string SPCode, string Type, string SubType)
        {
            API ac = new API();
            SPVEMonthlyPlan monthPlanData = new SPVEMonthlyPlan();

            var result = ac.GetData<SPVEMonthlyPlan>("MonthlyVisitsDotNetAPI", "Visit_Year eq '" + Year + "' and Visit_Month eq '" + Month + "' and Visit_Type_No eq '" + Type + "' and Visit_Sub_Type_No eq '" + SubType + "' and Salesperson_Code eq '" + SPCode + "' and IsActive eq true");
            if (result.Result.Item1.value.Count > 0)
                monthPlanData = result.Result.Item1.value[0];

            return monthPlanData;
        }

        [HttpPost]
        [Route("DeleteYearMonthPlan")]
        public bool DeleteYearMonthPlan(string No, string Type, string SubType, int NoOfVisit, string Year, string Edate, string SPCode)
        {
            bool flag = false;
            API ac = new API();
            SPVEYearMonthPlanPost yearMonthPlanReq = new SPVEYearMonthPlanPost();
            SPVEYearMonthPlan yearMonthPlanRes = new SPVEYearMonthPlan();
            errorDetails ed = new errorDetails();

            yearMonthPlanReq.Visit_Type = Type;
            yearMonthPlanReq.Visit_Sub_Type = SubType;
            yearMonthPlanReq.No_of_Visit = NoOfVisit;
            yearMonthPlanReq.Year = Year;
            yearMonthPlanReq.Edate = Edate;
            yearMonthPlanReq.SalesPerson_Code = SPCode;
            yearMonthPlanReq.IsActive = false;

            var result = PatchItemForDelYearMonthPlan("YearlyMonthVisitDotNetAPI", yearMonthPlanReq, yearMonthPlanRes, "No='" + No + "'");

            if (result.Result.Item1.No != null)
            {
                yearMonthPlanRes = result.Result.Item1;
                flag = true;
            }

            if (result.Result.Item2.message != null)
                ed = result.Result.Item2;

            return flag;
        }

        [Route("WeekPlan")]
        public SPVEWeekSalesPlan WeekPlan(SPVEWeekSalesPlanDetails weekSalesPlanDetails, bool isEdit, string No)
        {
            var ac = new API();
            errorDetails ed = new errorDetails();
            SPVEWeekSalesPlanPost reqWeekSalesPlan = new SPVEWeekSalesPlanPost();
            SPVEWeekSalesPlanUpdate reqWeekSalesPlanUpdate = new SPVEWeekSalesPlanUpdate();
            SPVEWeekSalesPlan resWeekSalesPlan = new SPVEWeekSalesPlan();
            var result = (dynamic)null;
            string WeekNo = weekSalesPlanDetails.Week_No;
            string FinancialYear = weekSalesPlanDetails.Financial_Year;

            if (!isEdit)
            {
                reqWeekSalesPlan.Financial_Year = weekSalesPlanDetails.Financial_Year;
                reqWeekSalesPlan.Week_No = weekSalesPlanDetails.Week_No;
                reqWeekSalesPlan.Week_Start_Date = weekSalesPlanDetails.Week_Start_Date;
                reqWeekSalesPlan.Week_End_Date = weekSalesPlanDetails.Week_End_Date;
                reqWeekSalesPlan.Week_Plan_Date = weekSalesPlanDetails.Week_Plan_Date;
                reqWeekSalesPlan.Visit_Type = weekSalesPlanDetails.Visit_Type;
                reqWeekSalesPlan.Visit_Sub_Type = weekSalesPlanDetails.Visit_Sub_Type;
                reqWeekSalesPlan.Contact_Company_No = weekSalesPlanDetails.Contact_Company_No == null || weekSalesPlanDetails.Contact_Company_No == "" ? "" : weekSalesPlanDetails.Contact_Company_No;
                reqWeekSalesPlan.Contact_Person_No = weekSalesPlanDetails.Contact_Person_No == null || weekSalesPlanDetails.Contact_Person_No == "" ? "" : weekSalesPlanDetails.Contact_Person_No;
                reqWeekSalesPlan.Event_No = weekSalesPlanDetails.Event_No == null || weekSalesPlanDetails.Event_No == "" ? "" : weekSalesPlanDetails.Event_No;
                reqWeekSalesPlan.Topic_Name = weekSalesPlanDetails.Topic_Name == null || weekSalesPlanDetails.Topic_Name == "" ? "" : weekSalesPlanDetails.Topic_Name;
                reqWeekSalesPlan.Mode_of_Visit = weekSalesPlanDetails.Mode_of_Visit;
                reqWeekSalesPlan.Pur_Visit = weekSalesPlanDetails.Pur_Visit;
                reqWeekSalesPlan.Remarks = weekSalesPlanDetails.Remarks == null || weekSalesPlanDetails.Remarks == "" ? "" : weekSalesPlanDetails.Remarks;
                reqWeekSalesPlan.SalesPerson_Code = weekSalesPlanDetails.SalesPerson_Code;
                reqWeekSalesPlan.IsActive = true;

                result = PostItemWeekPlan("WeeklySalesPlanCardDotNetAPI", reqWeekSalesPlan, resWeekSalesPlan);
            }
            else
            {
                reqWeekSalesPlanUpdate.Week_Plan_Date = weekSalesPlanDetails.Week_Plan_Date;
                reqWeekSalesPlanUpdate.Visit_Type = weekSalesPlanDetails.Visit_Type;
                reqWeekSalesPlanUpdate.Visit_Sub_Type = weekSalesPlanDetails.Visit_Sub_Type;
                reqWeekSalesPlanUpdate.Contact_Company_No = weekSalesPlanDetails.Contact_Company_No == null || weekSalesPlanDetails.Contact_Company_No == "" ? "" : weekSalesPlanDetails.Contact_Company_No;
                reqWeekSalesPlanUpdate.Contact_Person_No = weekSalesPlanDetails.Contact_Person_No == null || weekSalesPlanDetails.Contact_Person_No == "" ? "" : weekSalesPlanDetails.Contact_Person_No;
                reqWeekSalesPlanUpdate.Event_No = weekSalesPlanDetails.Event_No == null || weekSalesPlanDetails.Event_No == "" ? "" : weekSalesPlanDetails.Event_No;
                reqWeekSalesPlanUpdate.Topic_Name = weekSalesPlanDetails.Topic_Name == null || weekSalesPlanDetails.Topic_Name == "" ? "" : weekSalesPlanDetails.Topic_Name;
                reqWeekSalesPlanUpdate.Mode_of_Visit = weekSalesPlanDetails.Mode_of_Visit;
                reqWeekSalesPlanUpdate.Pur_Visit = weekSalesPlanDetails.Pur_Visit;
                reqWeekSalesPlanUpdate.Remarks = weekSalesPlanDetails.Remarks == null || weekSalesPlanDetails.Remarks == "" ? "" : weekSalesPlanDetails.Remarks;

                result = PatchItemWeekPlan("WeeklySalesPlanCardDotNetAPI", reqWeekSalesPlanUpdate, resWeekSalesPlan, "No='" + weekSalesPlanDetails.No + "',Entry_Type='Plan',Line_No=0");
            }

            if (result.Result.Item1 != null)
            {
                resWeekSalesPlan = result.Result.Item1;
                ed = result.Result.Item2;
                resWeekSalesPlan.errorDetails = ed;

                if (result.Result.Item1.No != null)
                {

                    if (weekSalesPlanDetails.Products != null && weekSalesPlanDetails.Products.Count > 0)
                    {
                        SPVEWeeklyDailyPlanProdsPost reqWeekPlanProd = new SPVEWeeklyDailyPlanProdsPost();
                        SPVEWeeklyDailyPlanProdsUpdate reqWeekPlanProdUpdate = new SPVEWeeklyDailyPlanProdsUpdate();
                        SPVEWeeklyDailyPlanProds resWeekPlanProd = new SPVEWeeklyDailyPlanProds();
                        errorDetails ed1 = new errorDetails();

                        for (int a = 0; a < weekSalesPlanDetails.Products.Count; a++)
                        {
                            var result1 = (dynamic)null;
                            if (!isEdit)
                            {
                                reqWeekPlanProd.Daily_Visit_Plan_No = resWeekSalesPlan.No;
                                reqWeekPlanProd.Product_No = weekSalesPlanDetails.Products[a].Product_No;
                                reqWeekPlanProd.Quantity = weekSalesPlanDetails.Products[a].Quantity;
                                reqWeekPlanProd.Weekly_No = WeekNo;
                                reqWeekPlanProd.Financial_Year = FinancialYear;
                                reqWeekPlanProd.Competitor = weekSalesPlanDetails.Products[a].Competitor;
                                reqWeekPlanProd.IsActive = true;

                                result1 = PostItemWeeklyPlanProds("DailyVisitProductSubformDotNetAPI", reqWeekPlanProd, resWeekPlanProd);

                            }
                            else
                            {
                                reqWeekPlanProdUpdate.Quantity = weekSalesPlanDetails.Products[a].Quantity;

                                result1 = PatchItemWeekPlanProds("DailyVisitProductSubformDotNetAPI", reqWeekPlanProdUpdate, resWeekPlanProd, "No='" + weekSalesPlanDetails.Products[a].No + "'");
                            }

                            if (result1.Result.Item1 != null)
                            {
                                resWeekPlanProd = result1.Result.Item1;
                                ed1 = result1.Result.Item2;
                                resWeekSalesPlan.errorDetails = ed1;

                            }
                        }
                    }


                }
            }

            //if (isEdit)
            //    result = PatchItemWeekPlan("WeeklySalesPlanCardDotNetAPI", reqWeekSalesPlan, resWeekSalesPlan, "No='" + No + "'");
            //else


            //if (result.Result.Item1.No != null)
            //    resWeekSalesPlan = result.Result.Item1;

            //if (result.Result.Item2.message != null)
            //    ed = result.Result.Item2;

            return resWeekSalesPlan;
        }

        [Route("GetWeekPlanNoDetailsForList")]
        public List<SPWeekPlanNoDetails> GetWeekPlanNoDetailsForList(string SPCode)
        {
            API ac = new API();
            List<SPWeekPlanNoDetails> weekPlanDetails = new List<SPWeekPlanNoDetails>();

            var result = ac.GetData<SPWeekPlanNoDetails>("WeeklyPlanSalesPersonWise", "Sales_Person_Code eq '" + SPCode + "'");

            if (result.Result.Item1.value.Count > 0)
                weekPlanDetails = result.Result.Item1.value;

            return weekPlanDetails;
        }

        [Route("GetWeekPlanDetailsTypeSubTypeWise")]
        public List<SPWeekPlanDetailsTypeWise> GetWeekPlanDetailsTypeSubTypeWise(string SPCode, int WeekNo, string FromDate, string ToDate)
        {
            API ac = new API();
            SPWeekPlanDetailsTypeWise WeekPlanDetailsTypeWise;
            List<SPWeekPlanDetailsTypeWise> WeekPlanDetailsTypeWiseList = new List<SPWeekPlanDetailsTypeWise>();
            List<WeekPlanTypeWiseTotal> WeekPlanTypeWiseTotal = new List<WeekPlanTypeWiseTotal>();
            List<WeekPlanTypeWiseTotalActual> weekPlanTypeWiseTotalActual = new List<WeekPlanTypeWiseTotalActual>();
            string[] FromDateDetails = FromDate.Split('-');
            string FromDate_ = FromDateDetails[2] + '-' + FromDateDetails[1] + '-' + FromDateDetails[0];

            string[] ToDateDetails = ToDate.Split('-');
            string ToDate_ = ToDateDetails[2] + '-' + ToDateDetails[1] + '-' + ToDateDetails[0];


            var result = ac.GetData<WeekPlanTypeWiseTotal>("WeeklyVisitTotalTypeWise", "Salesperson_Code eq '" + SPCode + "' and Week_No_ eq " + WeekNo + " and Week_Start_Date eq " + FromDate_ + " and Week_End_Date eq " + ToDate_);

            if (result.Result.Item1.value.Count > 0)
                WeekPlanTypeWiseTotal = result.Result.Item1.value;

            var result1 = ac.GetData<WeekPlanTypeWiseTotalActual>("WeeklyvisittotalsTypeWiseActual", "Salesperson_Code eq '" + SPCode + "' and Week_No_ eq " + WeekNo + " and Week_Start_Date eq " + FromDate_ + " and Week_End_Date eq " + ToDate_);

            if (result1.Result.Item1.value.Count > 0)
                weekPlanTypeWiseTotalActual = result1.Result.Item1.value;

            if (WeekPlanTypeWiseTotal.Count > 0)
            {
                for (int a = 0; a < WeekPlanTypeWiseTotal.Count; a++)
                {

                    WeekPlanDetailsTypeWise = new SPWeekPlanDetailsTypeWise();

                    WeekPlanDetailsTypeWise.Visit_Type = WeekPlanTypeWiseTotal[a].Visit_Type;
                    WeekPlanDetailsTypeWise.Visit_Sub_Type = WeekPlanTypeWiseTotal[a].Visit_Sub_Type;
                    WeekPlanDetailsTypeWise.Visit_Name = WeekPlanTypeWiseTotal[a].Visit_Name;
                    WeekPlanDetailsTypeWise.Visit_SubType_Name = WeekPlanTypeWiseTotal[a].Visit_SubType_Name;
                    WeekPlanDetailsTypeWise.DailyVisitPlanCount = WeekPlanTypeWiseTotal[a].DailyVisitPlanCount;

                    WeekPlanDetailsTypeWiseList.Add(WeekPlanDetailsTypeWise);
                    //WeekPlanDetailsTypeWise[a].Visit_Name = WeekPlanTypeWiseTotal[a].Visit_Name;
                    //WeekPlanDetailsTypeWise[a].Visit_SubType_Name = WeekPlanTypeWiseTotal[a].Visit_SubType_Name;
                    //WeekPlanDetailsTypeWise[a].DailyVisitPlanCount = WeekPlanTypeWiseTotal[a].DailyVisitPlanCount;

                }

                if (weekPlanTypeWiseTotalActual.Count > 0)
                {
                    for (int b = 0; b < weekPlanTypeWiseTotalActual.Count; b++)
                    {
                        if (WeekPlanDetailsTypeWiseList[b].Visit_Name == weekPlanTypeWiseTotalActual[b].Visit_Name &&
                            WeekPlanDetailsTypeWiseList[b].Visit_SubType_Name == weekPlanTypeWiseTotalActual[b].Visit_SubType_Name)
                        {
                            WeekPlanDetailsTypeWiseList[b].DailyVisitActualCount = weekPlanTypeWiseTotalActual[b].DailyVisitPlanCount;
                        }

                    }
                }
                else
                {
                    for (int b = 0; b < WeekPlanTypeWiseTotal.Count; b++)
                    {
                        WeekPlanDetailsTypeWiseList[b].DailyVisitActualCount = 0;
                    }
                }

            }

            return WeekPlanDetailsTypeWiseList;

        }

        [Route("GetWeekPlanTypeWiseCountDetails")]
        public List<SPVEWeekSalesPlan> GetWeekPlanTypeWiseCountDetails(string SPCode, string VisitType, string VisitSubType, string FromDate, string ToDate)
        {
            API ac = new API();
            List<SPVEWeekSalesPlan> weekPlanDatailsTypeWise = new List<SPVEWeekSalesPlan>();
            string[] FromDate_ = FromDate.Split('-');
            string FromDateDetails = FromDate_[2] + "-" + FromDate_[1] + "-" + FromDate_[0];
            string[] ToDate_ = ToDate.Split('-');
            string ToDateDetails = ToDate_[2] + "-" + ToDate_[1] + "-" + ToDate_[0];

            var result = ac.GetData<SPVEWeekSalesPlan>("WeeklySalesPlanCardDotNetAPI", "SalesPerson_Code eq '" + SPCode + "' and Visit_Type eq '" + VisitType + "' and Visit_Sub_Type eq '" + VisitSubType + "' and Week_Start_Date eq " +
                FromDateDetails + " and Week_End_Date eq " + ToDateDetails + " and Entry_Type eq 'Plan'");

            if (result.Result.Item1.value.Count > 0)
                weekPlanDatailsTypeWise = result.Result.Item1.value;

            return weekPlanDatailsTypeWise;

        }

        [Route("GetWeekPlanListData")]
        public List<SPVEWeekSalesPlan> GetWeekPlanListData(string SPCode, int skip, int top, string orderby, string filter, bool isExport = false)
        {
            API ac = new API();
            List<SPVEWeekSalesPlan> weekPlanData = new List<SPVEWeekSalesPlan>();
            if (filter == null)
                filter = "SalesPerson_Code eq '" + SPCode + "'";
            else
                filter += " and SalesPerson_Code eq '" + SPCode + "'";

            var result = (dynamic)null;

            if (isExport)
                result = ac.GetData<SPVEWeekSalesPlan>("WeeklySalesPlanCardDotNetAPI", filter);
            else
                result = ac.GetData1<SPVEWeekSalesPlan>("WeeklySalesPlanCardDotNetAPI", filter, skip, top, orderby);

            if (result.Result.Item1.value.Count > 0)
                weekPlanData = result.Result.Item1.value;

            return weekPlanData;
        }

        [Route("GetWeekPlan")]
        public SPVEWeekSalesPlan GetWeekPlan(string No)
        {
            API ac = new API();
            SPVEWeekSalesPlan weekPlan = new SPVEWeekSalesPlan();

            var result = ac.GetData<SPVEWeekSalesPlan>("WeeklySalesPlanCardDotNetAPI", "No eq '" + No + "'");

            if (result.Result.Item1.value.Count > 0)
                weekPlan = result.Result.Item1.value[0];

            return weekPlan;
        }

        //[Route("GetInquiryFromInquiryNo")]
        //public SPInquiry GetInquiryFromInquiryNo(string Inquiry_No)
        //{
        //    API ac = new API();
        //    SPInquiry inquiry = new SPInquiry();

        //    var result = ac.GetData<SPInquiry>("InquiryDotNetAPI", "Inquiry_No eq '" + Inquiry_No + "'");

        //    if (result.Result.Item1.value.Count > 0)
        //        inquiry = result.Result.Item1.value[0];

        //    return inquiry;
        //}

        [HttpPost]
        [Route("DeleteWeekPlan")]
        public bool DeleteWeekPlan(string No)
        {
            bool flag = false;
            API ac = new API();
            SPVEWeekSalesPlanDetails weekPlan = new SPVEWeekSalesPlanDetails();
            SPVEWeekSalesPlanPost weekPlanDel = new SPVEWeekSalesPlanPost();
            SPVEWeekSalesPlan weekPlanDelRes = new SPVEWeekSalesPlan();

            var result = ac.GetData<SPVEWeekSalesPlanDetails>("WeeklySalesPlanListDotNetAPI", "No eq '" + No + "'");

            if (result.Result.Item1.value.Count > 0)
            {
                weekPlan = result.Result.Item1.value[0];
                //weekPlanDel.Week_Date = weekPlan.Week_Date;
                weekPlanDel.Contact_Company_No = weekPlan.Contact_Company_No;
                weekPlanDel.Visit_Type = weekPlan.Visit_Type;
                weekPlanDel.Visit_Sub_Type = weekPlan.Visit_Sub_Type;
                weekPlanDel.Pur_Visit = weekPlan.Pur_Visit;
                weekPlanDel.Remarks = weekPlan.Remarks;
                weekPlanDel.Week_Plan_Date = weekPlan.Week_Plan_Date;
                //weekPlanDel.Week_Day = weekPlan.Week_Day;
                //weekPlanDel.Month_Detail_No = weekPlan.Month_Detail_No;
                weekPlanDel.SalesPerson_Code = weekPlan.SalesPerson_Code;
                //weekPlanDel.Status = weekPlan.Status;
                //weekPlanDel.IsActive = false;

                var result1 = PatchItemWeekPlan("WeeklySalesPlanListDotNetAPI", weekPlanDel, weekPlanDelRes, "No='" + No + "'");

                if (result1.Result.Item1.No != null)
                    flag = true;

            }

            return flag;
        }

        [Route("DailyVisit")]
        public SPDailyVisit DailyVisit(SPDailyVisitDetails dailyVisitDetails, bool isEdit, string No, string CustomerName, string CustEmail, string Entry_Type, string UserRoleORReportingPerson = "", string LoggedInSPUserEmail = "")
        {
            bool flag = false;
            var ac = new API();
            SPDailyVisitPost reqDailyVisit = new SPDailyVisitPost();
            SPDailyVisit resDailyVisit = new SPDailyVisit();
            errorDetails ed = new errorDetails();

            var resolvedEntryType = Entry_Type;
            if (string.IsNullOrWhiteSpace(resolvedEntryType))
                resolvedEntryType = dailyVisitDetails?.Entry_Type;

            if (string.IsNullOrWhiteSpace(resolvedEntryType) && isEdit && !string.IsNullOrWhiteSpace(No))
            {
                var currentDailyVisitResult = ac.GetData<SPDailyVisit>("DailyVisitsDotNetAPI", "No eq '" + No + "'");
                if (currentDailyVisitResult != null && currentDailyVisitResult.Result.Item1.value.Count > 0)
                    resolvedEntryType = currentDailyVisitResult.Result.Item1.value[0].Entry_Type;
            }

            if (string.IsNullOrWhiteSpace(resolvedEntryType))
                resolvedEntryType = "ENTRY";

            reqDailyVisit.Financial_Year = dailyVisitDetails.Financial_Year;
            reqDailyVisit.Date = dailyVisitDetails.Date;
            reqDailyVisit.Contact_Company_No = dailyVisitDetails.Contact_Company_No == null || dailyVisitDetails.Contact_Company_No == "" || dailyVisitDetails.Contact_Company_No == "-1" ? "" : dailyVisitDetails.Contact_Company_No;
            reqDailyVisit.Contact_Person_No = dailyVisitDetails.Contact_Person_No == null || dailyVisitDetails.Contact_Person_No == "" || dailyVisitDetails.Contact_Person_No == "-1" ? "" : dailyVisitDetails.Contact_Person_No;
            reqDailyVisit.Visit_Type = dailyVisitDetails.Visit_Type;
            reqDailyVisit.Visit_SubType_No = dailyVisitDetails.Visit_SubType_No;
            reqDailyVisit.Feedback = dailyVisitDetails.Feedback == null || dailyVisitDetails.Feedback == "" ? "" : dailyVisitDetails.Feedback;
            //reqDailyVisit.Competitor_Name = dailyVisitDetails.Competitor_Name;
            reqDailyVisit.Mode_of_Visit = dailyVisitDetails.Mode_of_Visit;
            reqDailyVisit.Complain_Subject = dailyVisitDetails.Complain_Subject == null || dailyVisitDetails.Complain_Subject == "" ? "" : dailyVisitDetails.Complain_Subject;
            reqDailyVisit.Complain_Products = dailyVisitDetails.Complain_Products == null || dailyVisitDetails.Complain_Products == "" ? "" : dailyVisitDetails.Complain_Products;
            reqDailyVisit.Complain_Invoice = dailyVisitDetails.Complain_Invoice == "-1" ? "" : dailyVisitDetails.Complain_Invoice;
            reqDailyVisit.Com_Date = dailyVisitDetails.Com_Date == null || dailyVisitDetails.Com_Date == "" ? null : dailyVisitDetails.Com_Date;
            reqDailyVisit.Complain_Assign_To = dailyVisitDetails.Complain_Assign_To == "-1" ? "" : dailyVisitDetails.Complain_Assign_To;
            reqDailyVisit.Root_Analysis = dailyVisitDetails.Root_Analysis == null || dailyVisitDetails.Root_Analysis == "" ? null : dailyVisitDetails.Root_Analysis;
            reqDailyVisit.Root_Analysis_date = dailyVisitDetails.Root_Analysis_date == null || dailyVisitDetails.Root_Analysis_date == "" ? null : dailyVisitDetails.Root_Analysis_date;
            reqDailyVisit.Corrective_Action = dailyVisitDetails.Corrective_Action == null || dailyVisitDetails.Corrective_Action == "" ? "" : dailyVisitDetails.Corrective_Action;
            reqDailyVisit.Corrective_Action_Date = dailyVisitDetails.Corrective_Action_Date == null || dailyVisitDetails.Corrective_Action_Date == "" ? null : dailyVisitDetails.Corrective_Action_Date;
            reqDailyVisit.Preventive_Action = dailyVisitDetails.Preventive_Action == null || dailyVisitDetails.Preventive_Action == "" ? "" : dailyVisitDetails.Preventive_Action;
            reqDailyVisit.Preventive_Date = dailyVisitDetails.Preventive_Date == null || dailyVisitDetails.Preventive_Date == "" ? null : dailyVisitDetails.Preventive_Date;
            reqDailyVisit.Market_Update = dailyVisitDetails.Market_Update == null || dailyVisitDetails.Market_Update == "" ? "" : dailyVisitDetails.Market_Update;
            reqDailyVisit.Market_Update_Date = dailyVisitDetails.Market_Update_Date == null || dailyVisitDetails.Market_Update_Date == "" ? null : dailyVisitDetails.Market_Update_Date;
            reqDailyVisit.Payment_Amt = dailyVisitDetails.Payment_Amt;
            reqDailyVisit.Payment_Date = dailyVisitDetails.Payment_Date == null || dailyVisitDetails.Payment_Date == "" ? null : dailyVisitDetails.Payment_Date;
            reqDailyVisit.Payment_Remarks = dailyVisitDetails.Payment_Remarks == null || dailyVisitDetails.Payment_Remarks == "" ? "" : dailyVisitDetails.Payment_Remarks;
            reqDailyVisit.Salesperson_Code = dailyVisitDetails.Salesperson_Code;
            reqDailyVisit.Remarks = dailyVisitDetails.Remarks == null || dailyVisitDetails.Remarks == "" ? "" : dailyVisitDetails.Remarks;
            reqDailyVisit.Event_No = dailyVisitDetails.Event_No == "-1" ? "" : dailyVisitDetails.Event_No;
            reqDailyVisit.Topic_Name = dailyVisitDetails.Topic_Name == null || dailyVisitDetails.Topic_Name == "" ? "" : dailyVisitDetails.Topic_Name;
            reqDailyVisit.Week_No = dailyVisitDetails.Week_No;
            reqDailyVisit.Week_Start_Date = dailyVisitDetails.Week_Start_Date;
            reqDailyVisit.Week_End_Date = dailyVisitDetails.Week_End_Date;
            reqDailyVisit.Entry_Type = resolvedEntryType;
            reqDailyVisit.Is_PDC = dailyVisitDetails.Is_PDC;
            reqDailyVisit.Pur_Visit = dailyVisitDetails.Pur_Visit;
            reqDailyVisit.IsActive = true;

            var result = (dynamic)null;

            if (isEdit && !string.IsNullOrWhiteSpace(No))
                result = PatchItemDailyVisit("DailyVisitsDotNetAPI", reqDailyVisit, resDailyVisit, "No='" + No + "',Entry_Type='" + resolvedEntryType + "'");
            else
                result = PostItemDailyVisit("DailyVisitsDotNetAPI", reqDailyVisit, resDailyVisit);

            if (result.Result.Item1 != null)
            {
                resDailyVisit = result.Result.Item1;
                ed = result.Result.Item2;
                resDailyVisit.errorDetails = ed;

                if (result.Result.Item1.No != null)
                {
                    SPDailyVisitExpansePost reqDailyVisitExp = new SPDailyVisitExpansePost();
                    SPDailyVisitExpanse resDailyVisitExp = new SPDailyVisitExpanse();
                    errorDetails ed1 = new errorDetails();

                    reqDailyVisitExp.Daily_Visit_Plan_No = resDailyVisit.No;
                    reqDailyVisitExp.Visit_Date = reqDailyVisit.Date;
                    reqDailyVisitExp.Start_Time = dailyVisitDetails.dailyVisitExpense.Start_Time == null || dailyVisitDetails.dailyVisitExpense.Start_Time == "" ? "" : dailyVisitDetails.dailyVisitExpense.Start_Time;
                    reqDailyVisitExp.End_Time = dailyVisitDetails.dailyVisitExpense.End_Time == null || dailyVisitDetails.dailyVisitExpense.End_Time == "" ? "" : dailyVisitDetails.dailyVisitExpense.End_Time;
                    reqDailyVisitExp.Total_Time = dailyVisitDetails.dailyVisitExpense.Total_Time == null || dailyVisitDetails.dailyVisitExpense.Total_Time == "" ? "" : dailyVisitDetails.dailyVisitExpense.Total_Time;
                    reqDailyVisitExp.Start_km = dailyVisitDetails.dailyVisitExpense.Start_km;
                    reqDailyVisitExp.End_km = dailyVisitDetails.dailyVisitExpense.End_km;
                    reqDailyVisitExp.Total_km = dailyVisitDetails.dailyVisitExpense.Total_km ;
                    reqDailyVisitExp.User_Code = resDailyVisit.Salesperson_Code;
                    reqDailyVisitExp.IsActive = true;

                    var result1 = (dynamic)null;

                    if (isEdit)
                    {
                        var resultExpanse = ac.GetData<SPDailyVisitExpanse>("DailyVisitExpensesDotNetAPI", "Daily_Visit_Plan_No eq '" + resDailyVisit.No + "'");
                        if (resultExpanse != null && resultExpanse.Result.Item1.value.Count > 0)
                            result1 = PatchItemDailyVisitExpanse("DailyVisitExpensesDotNetAPI", reqDailyVisitExp, resDailyVisitExp, "No='" + resultExpanse.Result.Item1.value[0].No + "'");
                        else
                            result1 = PostItemDailyVisitExpanse("DailyVisitExpensesDotNetAPI", reqDailyVisitExp, resDailyVisitExp);
                    }
                    else
                        result1 = PostItemDailyVisitExpanse("DailyVisitExpensesDotNetAPI", reqDailyVisitExp, resDailyVisitExp);

                    if (result1.Result.Item1 != null)
                    {
                        resDailyVisitExp = result1.Result.Item1;
                        ed1 = result1.Result.Item2;
                        resDailyVisit.errorDetails = ed1;
                    }

                    if (dailyVisitDetails.dailyVisitProds != null && dailyVisitDetails.dailyVisitProds.Count > 0)
                    {
                        for (int a = 0; a < dailyVisitDetails.dailyVisitProds.Count; a++)
                        {
                            SPVEWeeklyDailyPlanProdsPost reqDailyPlanProd = new SPVEWeeklyDailyPlanProdsPost();
                            SPVEWeeklyDailyPlanProds resDailyPlanProd = new SPVEWeeklyDailyPlanProds();
                            errorDetails ed2 = new errorDetails();

                            reqDailyPlanProd.Daily_Visit_Plan_No = resDailyVisit.No;
                            reqDailyPlanProd.Product_No = dailyVisitDetails.dailyVisitProds[a].Product_No;
                            reqDailyPlanProd.Quantity = dailyVisitDetails.dailyVisitProds[a].Quantity;
                            reqDailyPlanProd.Weekly_No = dailyVisitDetails.Week_No == 0 ? "" : dailyVisitDetails.Week_No.ToString();
                            reqDailyPlanProd.Financial_Year = dailyVisitDetails.Financial_Year;
                            reqDailyPlanProd.Competitor = dailyVisitDetails.dailyVisitProds[a].Competitor == null || dailyVisitDetails.dailyVisitProds[a].Competitor == "" ? "" : dailyVisitDetails.dailyVisitProds[a].Competitor;
                            reqDailyPlanProd.IsActive = true;

                            var result2 = (dynamic)null;
                            if (isEdit && !string.IsNullOrWhiteSpace(dailyVisitDetails.dailyVisitProds[a].No))
                            {
                                SPVEWeeklyDailyPlanProdsUpdate reqDailyPlanProdUpdate = new SPVEWeeklyDailyPlanProdsUpdate();
                                reqDailyPlanProdUpdate.Quantity = dailyVisitDetails.dailyVisitProds[a].Quantity;

                                result2 = PatchItemWeekPlanProds("DailyVisitProductSubformDotNetAPI", reqDailyPlanProdUpdate, resDailyPlanProd, "No='" + dailyVisitDetails.dailyVisitProds[a].No + "'");
                            }
                            else
                                result2 = PostItemDailyVisitProds("DailyVisitProductSubformDotNetAPI", reqDailyPlanProd, resDailyPlanProd);

                            if (result2.Result.Item1 != null)
                            {
                                resDailyPlanProd = result2.Result.Item1;
                                ed2 = result2.Result.Item2;
                                resDailyVisit.errorDetails = ed2;
                            }
                        }
                    }

                }

            }

            string toEmail = "";
            string ccEmail = string.IsNullOrWhiteSpace(LoggedInSPUserEmail) ? "" : LoggedInSPUserEmail;

            if (!string.IsNullOrWhiteSpace(UserRoleORReportingPerson) && UserRoleORReportingPerson.Contains("@"))
            {
                toEmail = UserRoleORReportingPerson;
            }
            else
            {
                var resultUserDetails = ac.GetData<SPUserReportingPersonDetails>("EmployeesDotNetAPI", "Salespers_Purch_Code eq '" + dailyVisitDetails.Salesperson_Code + "'");
                if (resultUserDetails.Result.Item1 != null
                    && resultUserDetails.Result.Item1.value != null
                    && resultUserDetails.Result.Item1.value.Count > 0)
                {
                    toEmail = resultUserDetails.Result.Item1.value[0].PCPL_Reporting_Person_Email;
                }
            }

            string productEmailBody = "";
            string productNoColumn = "";
            string quantityColumn = "";
            string uomColumn = "";
            string competitorColumn = "";
            if (dailyVisitDetails.dailyVisitProds != null && dailyVisitDetails.dailyVisitProds.Count > 0)
            {
                productEmailBody += "<br /><p><b>Product Details:</b></p>";
                productEmailBody += "<table border=\"1\"><thead><tr style=\"background-color:darkblue;color:white\"><th>Product No</th><th>Quantity</th><th>UOM</th><th>Competitor</th></tr></thead><tbody>";

                for (int i = 0; i < dailyVisitDetails.dailyVisitProds.Count; i++)
                {
                    var prod = dailyVisitDetails.dailyVisitProds[i];
                    productEmailBody += "<tr><td>" + (prod.Product_No ?? "") + "</td><td>" + prod.Quantity + "</td><td>" + (prod.Unit_of_Measure ?? "") + "</td><td>" + (prod.Competitor ?? "") + "</td></tr>";
                    productNoColumn += (prod.Product_No ?? "") + "<br/>";
                    quantityColumn += prod.Quantity + "<br/>";
                    uomColumn += (prod.Unit_of_Measure ?? "") + "<br/>";
                    competitorColumn += (prod.Competitor ?? "") + "<br/>";
                }

                productEmailBody += "</tbody></table>";
            }

            if (!string.IsNullOrWhiteSpace(toEmail) && !string.IsNullOrWhiteSpace(resDailyVisit.No))
            {
                var actionText = isEdit ? "Updated" : "Created";
                EmailService emailService = new EmailService();
                StringBuilder sbMailBody = new StringBuilder();
                sbMailBody.Append("");
                sbMailBody.Append("<p>Hi,</p>");
                sbMailBody.Append("<p>Daily Visit has been <strong>" + actionText + "</strong> in <strong>Prakash CRM Portal</strong>.</p>");
                sbMailBody.Append("<table border=\"1\"><thead><tr style=\"background-color:darkblue;color:white\"><th>Daily Visit No</th><th>Date</th><th>Customer</th><th>Visit Type</th><th>Visit Sub Type</th><th>Visit Mode</th><th>Feedback</th><th>Product No</th><th>Quantity</th><th>UOM</th><th>Competitor</th></tr></thead><tbody>");
                sbMailBody.Append("<tr><td>" + resDailyVisit.No + "</td><td>" + resDailyVisit.Date + "</td><td>" + CustomerName + "</td><td>" + (resDailyVisit.Visit_Name ?? "") + "</td><td>" + (resDailyVisit.Visit_SubType_Name ?? "") + "</td><td>" + (resDailyVisit.Mode_of_Visit ?? "") + "</td><td>" + (resDailyVisit.Feedback ?? "") + "</td><td>" + productNoColumn + "</td><td>" + quantityColumn + "</td><td>" + uomColumn + "</td><td>" + competitorColumn + "</td></tr></tbody></table>");
                sbMailBody.Append("<p>&nbsp;</p>");
                sbMailBody.Append("<p>Warm Regards,</p>");
                sbMailBody.Append("<p>Support Team</p>");

                emailService.SendEmail(toEmail, ccEmail, "Daily Visit " + actionText + " - PrakashCRM", sbMailBody.ToString());
            }

            if (resDailyVisit.Payment_Amt > 0)
            {
                string emailBody = "";
                string paymentDate = dailyVisitDetails.Payment_Date == null || dailyVisitDetails.Payment_Date == "" ? "" : dailyVisitDetails.Payment_Date;
                string paymentRemarks = dailyVisitDetails.Payment_Remarks == null || dailyVisitDetails.Payment_Remarks == "" ? "" : dailyVisitDetails.Payment_Remarks;

                emailBody += "<table border=\"1\"><thead><tr style=\"background-color:darkblue;color:white\"><th>Payment Date</th><th>Payment Amount</th><th>PDC</th><th>Remarks</th></tr></thead><tbody>";
                emailBody += "<tr><td>" + dailyVisitDetails.Payment_Date + "</td><td>" + dailyVisitDetails.Payment_Amt + "</td>";

                if (dailyVisitDetails.Is_PDC)
                    emailBody += "<td>Yes</td>";
                else
                    emailBody += "<td>No</td>";

                emailBody += "<td>" + dailyVisitDetails.Payment_Remarks + "</td></tr></tbody></table>";

                EmailService emailService = new EmailService();
                StringBuilder sbMailBody = new StringBuilder();
                sbMailBody.Append("");
                sbMailBody.Append("<p>Hi,</p>");
                sbMailBody.Append("<p>Welcome to the <strong>Prakash CRM Portal</strong>.</p>");
                sbMailBody.Append("<p>Daily Visit Plan No - " + resDailyVisit.No + "</p>");
                sbMailBody.Append("<p>Daily Visit Plan Date - " + resDailyVisit.Date + "</p>");
                sbMailBody.Append("<p>Customer - " + CustomerName + "</p>");
                sbMailBody.Append(emailBody);
                sbMailBody.Append(productEmailBody);
                sbMailBody.Append("<p>&nbsp;</p>");
                sbMailBody.Append("<p>Warm Regards,</p>");
                sbMailBody.Append("<p>Support Team</p>");

                if (!string.IsNullOrWhiteSpace(toEmail))
                    emailService.SendEmail(toEmail, ccEmail, "Payment Details - Daily Visit Plan - PrakashCRM", sbMailBody.ToString());

            }

            if (resDailyVisit.Complain_Subject != "")
            {
                string emailBody = "";

                string CompSubject = dailyVisitDetails.Complain_Subject == null || dailyVisitDetails.Complain_Subject == "" ? "" : dailyVisitDetails.Complain_Subject;
                string CompProducts = dailyVisitDetails.Complain_Products == null || dailyVisitDetails.Complain_Products == "" ? "" : dailyVisitDetails.Complain_Products;
                string CompInvoice = dailyVisitDetails.Complain_Invoice == null || dailyVisitDetails.Complain_Invoice == "" ? "" : dailyVisitDetails.Complain_Invoice;
                string CompDate = dailyVisitDetails.Com_Date == null || dailyVisitDetails.Com_Date == "" ? "" : dailyVisitDetails.Com_Date;
                string CompAssignTo = dailyVisitDetails.Complain_Assign_To == null || dailyVisitDetails.Complain_Assign_To == "" ? "" : dailyVisitDetails.Complain_Assign_To;
                string RootAnalysis = dailyVisitDetails.Root_Analysis == null || dailyVisitDetails.Root_Analysis == "" ? "" : dailyVisitDetails.Root_Analysis;
                string RootAnalysisDate = dailyVisitDetails.Root_Analysis_date == null || dailyVisitDetails.Root_Analysis_date == "" ? "" : dailyVisitDetails.Root_Analysis_date;
                string CorrectiveAction = dailyVisitDetails.Corrective_Action == null || dailyVisitDetails.Corrective_Action == "" ? "" : dailyVisitDetails.Corrective_Action;
                string CorrectiveActionDate = dailyVisitDetails.Corrective_Action_Date == null || dailyVisitDetails.Corrective_Action_Date == "" ? "" : dailyVisitDetails.Corrective_Action_Date;
                string PreventiveAction = dailyVisitDetails.Preventive_Action == null || dailyVisitDetails.Preventive_Action == "" ? "" : dailyVisitDetails.Preventive_Action;
                string PreventiveActionDate = dailyVisitDetails.Preventive_Date == null || dailyVisitDetails.Preventive_Date == "" ? "" : dailyVisitDetails.Preventive_Date;

                emailBody += "<table border=\"1\"><thead><tr style=\"background-color:darkblue;color:white\"><th>Complain Date</th><th>Complain Subject</th><th>Complain Products</th><th>Complain Invoice No</th><th>Root Cause Analysis</th><th>Root Cause Analysis Date</th><th>Corrective Action</th><th>Corrective Action Date</th><th>Prevention Action</th><th>Prevention Action Date</th></tr></thead><tbody>";
                emailBody += "<tr><td>" + CompDate + "</td><td>" + CompSubject + "</td><td>" + CompProducts + "</td><td>" + CompInvoice + "</td><td>" + RootAnalysis + "</td><td>" + RootAnalysisDate + "</td><td>" + CorrectiveAction + "</td><td>" + CorrectiveActionDate + "</td><td>" + PreventiveAction + "</td><td>" + PreventiveActionDate + "</td></tr></tbody></table>";

                EmailService emailService = new EmailService();
                StringBuilder sbMailBody = new StringBuilder();
                sbMailBody.Append("");
                sbMailBody.Append("<p>Hi,</p>");
                sbMailBody.Append("<p>Welcome to the <strong>Prakash CRM Portal</strong>.</p>");
                sbMailBody.Append("<p>Daily Visit Plan No - " + resDailyVisit.No + "</p>");
                sbMailBody.Append("<p>Daily Visit Plan Date - " + resDailyVisit.Date + "</p>");
                sbMailBody.Append("<p>Customer - " + CustomerName + "</p>");
                sbMailBody.Append(emailBody);
                sbMailBody.Append(productEmailBody);
                sbMailBody.Append("<p>&nbsp;</p>");
                sbMailBody.Append("<p>Warm Regards,</p>");
                sbMailBody.Append("<p>Support Team</p>");

                if (!string.IsNullOrWhiteSpace(toEmail))
                    emailService.SendEmail(toEmail, ccEmail, "Approval For Complain - Daily Visit Plan - PrakashCRM", sbMailBody.ToString());

            }

            if (resDailyVisit.Preventive_Action != "")
            {
                EmailService emailService = new EmailService();
                StringBuilder sbMailBody = new StringBuilder();
                sbMailBody.Append("");
                sbMailBody.Append("<p><b>Dear, " + CustomerName + "</b></p>");
                sbMailBody.Append("<p>Welcome to the <strong>Prakash CRM Portal</strong>.</p>");
                sbMailBody.Append("<p><b>Complain Prevention Action Details:</b></p>");
                sbMailBody.Append("<table border=\"1\"><thead><tr style=\"background-color:darkblue;color:white\"><th>Daily Visit Plan No</th><th>Prevention Action</th><th>Prevention Action Date</th></tr></thead><tbody>");
                sbMailBody.Append("<tr><td>" + resDailyVisit.No + "</td><td>" + (resDailyVisit.Preventive_Action ?? "") + "</td><td>" + (resDailyVisit.Preventive_Date ?? "") + "</td></tr></tbody></table>");
                sbMailBody.Append(productEmailBody);
                sbMailBody.Append("<p>&nbsp;</p>");
                sbMailBody.Append("<p>Warm Regards,</p>");
                sbMailBody.Append("<p>Support Team</p>");

                if (!string.IsNullOrWhiteSpace(toEmail))
                    emailService.SendEmail(toEmail, ccEmail, "Complain Prevention Action Details - Daily Visit Plan - PrakashCRM", sbMailBody.ToString());
            }

            return resDailyVisit;
        }

        [Route("GetDailyVisitsListData")]
        public List<SPDailyVisit> GetDailyVisitsListData(string SPCode, int skip, int top, string orderby, string filter, bool isExport = false)
        {
            API ac = new API();
            List<SPDailyVisit> dailyVisitData = new List<SPDailyVisit>();
            if (filter == null)
                filter = "Salesperson_Code eq '" + SPCode + "' and Entry_Type eq 'ENTRY'";
            else
                filter += " and Salesperson_Code eq '" + SPCode + "' and Entry_Type eq 'ENTRY'";

            var result = (dynamic)null;

            if (isExport)
                result = ac.GetData<SPDailyVisit>("DailyVisitsDotNetAPI", filter);
            else
                result = ac.GetData1<SPDailyVisit>("DailyVisitsDotNetAPI", filter, skip, top, orderby);

            if (result.Result.Item1?.value?.Count > 0)
                dailyVisitData = result.Result.Item1.value;

            return dailyVisitData;
        }

        [Route("GetDailyVisitsDetails")]
        public List<SPDailyVisit> GetDailyVisitsDetails(string SPCode, string VisitType, string VisitSubType, string FromDate, string ToDate)
        {
            API ac = new API();
            List<SPDailyVisit> dailyVisitData = new List<SPDailyVisit>();
            string[] FromDate_ = FromDate.Split('-');
            string FromDateDetails = FromDate_[2] + "-" + FromDate_[1] + "-" + FromDate_[0];
            string[] ToDate_ = ToDate.Split('-');
            string ToDateDetails = ToDate_[2] + "-" + ToDate_[1] + "-" + ToDate_[0];

            var result = ac.GetData<SPDailyVisit>("DailyVisitsDotNetAPI", "Salesperson_Code eq '" + SPCode + "' and Visit_Type eq '" + VisitType + "' and Visit_SubType_No eq '" + VisitSubType +
                    "' and Date ge " + FromDateDetails + " and Date le " + ToDateDetails + " and Entry_Type eq 'ENTRY'");

            if (result.Result.Item1.value.Count > 0)
                dailyVisitData = result.Result.Item1.value;

            return dailyVisitData;
        }

        [Route("GetComplainDetails")]
        public List<SPDVComplainList> GetComplainDetails(string dvpNo)
        {
            API ac = new API();
            List<SPDVComplainList> complainlist = new List<SPDVComplainList>();

            var result = ac.GetData<SPDVComplainList>("DailyVisitsDotNetAPI", "No eq '" + dvpNo + "'");

            if (result != null && result.Result.Item1.value.Count > 0)
                complainlist = result.Result.Item1.value;

            return complainlist;
        }

        [Route("GetPaymentDetails")]
        public List<SPDVPaymentList> GetPaymentDetails(string dvpNo)
        {
            API ac = new API();
            List<SPDVPaymentList> paymentlist = new List<SPDVPaymentList>();

            var result = ac.GetData<SPDVPaymentList>("DailyVisitsDotNetAPI", "No eq '" + dvpNo + "'");

            if (result != null && result.Result.Item1.value.Count > 0)
                paymentlist = result.Result.Item1.value;

            return paymentlist;
        }

        [Route("GetExpanseDetails")]
        public List<SPDailyVisitExpanse> GetExpanseDetails(string dvpNo)
        {
            API ac = new API();
            List<SPDailyVisitExpanse> expanselist = new List<SPDailyVisitExpanse>();

            if (string.IsNullOrWhiteSpace(dvpNo))
                return expanselist;

            var safeNo = dvpNo.Replace("'", "''");
            var result = ac.GetData<SPDailyVisitExpanse>("DailyVisitExpensesDotNetAPI", "Daily_Visit_Plan_No eq '" + safeNo + "'");

            if (result != null && result.Result.Item1.value.Count > 0)
                expanselist = result.Result.Item1.value;

            return expanselist;
        }

        [Route("GetAllLinesOfWeekPlan")]
        public List<SPVEWeeklyDailyPlanProds> GetAllLinesOfWeekPlan(string No)
        {
            API ac = new API();
            List<SPVEWeeklyDailyPlanProds> WeekPlanLines = new List<SPVEWeeklyDailyPlanProds>();

            if (string.IsNullOrWhiteSpace(No))
                return WeekPlanLines;

            var safeNo = No.Replace("'", "''");
            var result = ac.GetData<SPVEWeeklyDailyPlanProds>("DailyVisitProductSubformDotNetAPI", "Daily_Visit_Plan_No eq '" + safeNo + "'");

            if (result.Result.Item1.value.Count > 0)
                WeekPlanLines = result.Result.Item1.value;

            return WeekPlanLines;
        }

        [Route("GetDailyVisitProductDetails")]
        public List<SPVEWeeklyDailyPlanProds> GetDailyVisitProductDetails(string dvpNo)
        {
            API ac = new API();
            List<SPVEWeeklyDailyPlanProds> dailyvisitprods = new List<SPVEWeeklyDailyPlanProds>();

            if (string.IsNullOrWhiteSpace(dvpNo))
                return dailyvisitprods;

            var safeNo = dvpNo.Replace("'", "''");
            var result = ac.GetData<SPVEWeeklyDailyPlanProds>("DailyVisitProductSubformDotNetAPI", "Daily_Visit_Plan_No eq '" + safeNo + "'");

            if (result.Result.Item1.value.Count > 0)
                dailyvisitprods = result.Result.Item1.value;

            return dailyvisitprods;
        }

        [Route("GetDailyVisitByNo")]
        public SPDailyVisit GetDailyVisitByNo(string dvpNo)
        {
            if (string.IsNullOrWhiteSpace(dvpNo))
                return new SPDailyVisit();

            var safeNo = dvpNo.Replace("'", "''");
            API ac = new API();

            var result = ac.GetData<SPDailyVisit>("DailyVisitsDotNetAPI", "No eq '" + safeNo + "'");

            if (result != null && result.Result.Item1.value.Count > 0)
                return result.Result.Item1.value[0];

            return new SPDailyVisit();
        }

        public async Task<(SPVEYearMonthPlan, errorDetails)> PostItemYearMonthPlan<SPVEYearMonthPlan>(string apiendpoint, SPVEYearMonthPlanPost requestModel, SPVEYearMonthPlan responseModel)
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
                    responseModel = res.ToObject<SPVEYearMonthPlan>();

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

        public async Task<(SPVEMonthlyPlan, errorDetails)> PostItemForMonthVisit<SPVEMonthlyPlan>(string apiendpoint, SPVEMonthlyPlanPost requestModel, SPVEMonthlyPlan responseModel)
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
                    responseModel = res.ToObject<SPVEMonthlyPlan>();

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

        public async Task<(SPVEYearMonthPlan, errorDetails)> PatchItemForDelYearMonthPlan<SPVEYearMonthPlan>(string apiendpoint, SPVEYearMonthPlanPost requestModel, SPVEYearMonthPlan responseModel, string fieldWithValue)
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
                //response = _httpClient.PutAsync(baseuri, content).Result;
                response = _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).Result;
            }
            catch (Exception ex)
            {

            }

            errorDetails errordetail = new errorDetails();
            if (response == null)
            {
                errordetail.isSuccess = false;
                errordetail.message = "Daily Visit product update request failed.";
                return (responseModel, errordetail);
            }
            errordetail.isSuccess = response.IsSuccessStatusCode;
            if (response.IsSuccessStatusCode)
            {
                var JsonData = response.Content.ReadAsStringAsync().Result;
                try
                {
                    JObject res = JObject.Parse(JsonData);
                    responseModel = res.ToObject<SPVEYearMonthPlan>();


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
        public async Task<(SPVEYearMonthPlan, errorDetails)> PatchItemYearMonthPlan<SPVEYearMonthPlan>(string apiendpoint, SPVEYearMonthPlanPost requestModel, SPVEYearMonthPlan responseModel, string fieldWithValue)
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
                //response = _httpClient.PutAsync(baseuri, content).Result;
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
                    responseModel = res.ToObject<SPVEYearMonthPlan>();


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

        public async Task<(SPVEMonthlyPlan, errorDetails)> PatchItemForMonthVisit<SPVEMonthlyPlan>(string apiendpoint, SPVEMonthlyPlanPost requestModel, SPVEMonthlyPlan responseModel, string fieldWithValue)
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
                //response = _httpClient.PutAsync(baseuri, content).Result;
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
                    responseModel = res.ToObject<SPVEMonthlyPlan>();


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
        public async Task<(SPVEWeekSalesPlan, errorDetails)> PostItemWeekPlan<SPVEWeekSalesPlan>(string apiendpoint, SPVEWeekSalesPlanPost requestModel, SPVEWeekSalesPlan responseModel)
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
            if (response == null)
            {
                errordetail.isSuccess = false;
                errordetail.message = "Daily Visit save request failed.";
                return (responseModel, errordetail);
            }
            errordetail.isSuccess = response.IsSuccessStatusCode;
            if (response.IsSuccessStatusCode)
            {
                var JsonData = response.Content.ReadAsStringAsync().Result;
                try
                {
                    JObject res = JObject.Parse(JsonData);
                    responseModel = res.ToObject<SPVEWeekSalesPlan>();

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

        public async Task<(SPVEWeekSalesPlan, errorDetails)> PatchItemWeekPlan<SPVEWeekSalesPlan>(string apiendpoint, SPVEWeekSalesPlanUpdate requestModel, SPVEWeekSalesPlan responseModel, string fieldWithValue)
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
                //response = _httpClient.PutAsync(baseuri, content).Result;
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
                    responseModel = res.ToObject<SPVEWeekSalesPlan>();


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

        public async Task<(SPVEWeeklyDailyPlanProds, errorDetails)> PostItemWeeklyPlanProds<SPVEWeeklyDailyPlanProds>(string apiendpoint, SPVEWeeklyDailyPlanProdsPost requestModel, SPVEWeeklyDailyPlanProds responseModel)
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
                    responseModel = res.ToObject<SPVEWeeklyDailyPlanProds>();

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

        public async Task<(SPVEWeekSalesPlan, errorDetails)> PatchItemWeekPlan<SPVEWeekSalesPlan>(string apiendpoint, SPVEWeekSalesPlanPost requestModel, SPVEWeekSalesPlan responseModel, string fieldWithValue)
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
                //response = _httpClient.PutAsync(baseuri, content).Result;
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
                    responseModel = res.ToObject<SPVEWeekSalesPlan>();


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
        public async Task<(SPVEWeeklyDailyPlanProds, errorDetails)> PatchItemWeekPlanProds<SPVEWeeklyDailyPlanProds>(string apiendpoint, SPVEWeeklyDailyPlanProdsUpdate requestModel, SPVEWeeklyDailyPlanProds responseModel, string fieldWithValue)
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
                //response = _httpClient.PutAsync(baseuri, content).Result;
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
                    responseModel = res.ToObject<SPVEWeeklyDailyPlanProds>();


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
        public async Task<(SPDailyVisit, errorDetails)> PostItemDailyVisit<SPDailyVisit>(string apiendpoint, SPDailyVisitPost requestModel, SPDailyVisit responseModel)
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
                    responseModel = res.ToObject<SPDailyVisit>();

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

        public async Task<(SPDailyVisit, errorDetails)> PatchItemDailyVisit<SPDailyVisit>(string apiendpoint, SPDailyVisitPost requestModel, SPDailyVisit responseModel, string fieldWithValue)
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
            if (response == null)
            {
                errordetail.isSuccess = false;
                errordetail.message = "Daily Visit update request failed.";
                return (responseModel, errordetail);
            }
            errordetail.isSuccess = response.IsSuccessStatusCode;
            if (response.IsSuccessStatusCode)
            {
                var JsonData = response.Content.ReadAsStringAsync().Result;
                try
                {
                    JObject res = JObject.Parse(JsonData);
                    responseModel = res.ToObject<SPDailyVisit>();

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

        public async Task<(SPDailyVisitExpanse, errorDetails)> PostItemDailyVisitExpanse<SPDailyVisitExpanse>(string apiendpoint, SPDailyVisitExpansePost requestModel, SPDailyVisitExpanse responseModel)
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
            if (response == null)
            {
                errordetail.isSuccess = false;
                errordetail.message = "Daily Visit expense save request failed.";
                return (responseModel, errordetail);
            }
            errordetail.isSuccess = response.IsSuccessStatusCode;
            if (response.IsSuccessStatusCode)
            {
                var JsonData = response.Content.ReadAsStringAsync().Result;
                try
                {
                    JObject res = JObject.Parse(JsonData);
                    responseModel = res.ToObject<SPDailyVisitExpanse>();

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

        public async Task<(SPDailyVisitExpanse, errorDetails)> PatchItemDailyVisitExpanse<SPDailyVisitExpanse>(string apiendpoint, SPDailyVisitExpansePost requestModel, SPDailyVisitExpanse responseModel, string fieldWithValue)
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
            if (response == null)
            {
                errordetail.isSuccess = false;
                errordetail.message = "Daily Visit expense update request failed.";
                return (responseModel, errordetail);
            }
            errordetail.isSuccess = response.IsSuccessStatusCode;
            if (response.IsSuccessStatusCode)
            {
                var JsonData = response.Content.ReadAsStringAsync().Result;
                try
                {
                    JObject res = JObject.Parse(JsonData);
                    responseModel = res.ToObject<SPDailyVisitExpanse>();

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

        public async Task<(SPVEWeeklyDailyPlanProds, errorDetails)> PostItemDailyVisitProds<SPVEWeeklyDailyPlanProds>(string apiendpoint, SPVEWeeklyDailyPlanProdsPost requestModel, SPVEWeeklyDailyPlanProds responseModel)
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
            if (response == null)
            {
                errordetail.isSuccess = false;
                errordetail.message = "Daily Visit product save request failed.";
                return (responseModel, errordetail);
            }
            errordetail.isSuccess = response.IsSuccessStatusCode;
            if (response.IsSuccessStatusCode)
            {
                var JsonData = response.Content.ReadAsStringAsync().Result;
                try
                {
                    JObject res = JObject.Parse(JsonData);
                    responseModel = res.ToObject<SPVEWeeklyDailyPlanProds>();

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

        public async Task<(SPVECompetitors, errorDetails)> PostItemCompetitor<SPVECompetitors>(string apiendpoint, SPVECompetitorPost requestModel, SPVECompetitors responseModel)
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
                    responseModel = res.ToObject<SPVECompetitors>();

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

        [Route("GetDailyVisitExpenseReport")]
        public List<SPDailyVisitExpenseForReport> GetDailyVisitExpenseReport(int skip, int top, string orderby, string filter, bool isExport = false)
        {
            API ac = new API();
            List<SPDailyVisitExpenseForReport> dailyVisitExpense = new List<SPDailyVisitExpenseForReport>();
            filter = string.IsNullOrEmpty(filter) ? "No_of_Personal_Visit eq 1" : "No_of_Personal_Visit eq 1 and " + filter;

            var result = (dynamic)null;

            if (isExport)
                result = ac.GetData<SPDailyVisitExpenseForReport>("DailyVisitExpensesDotNetAPI", filter);
            else
                result = ac.GetData1<SPDailyVisitExpenseForReport>("DailyVisitExpensesDotNetAPI", filter, skip, top, orderby);
            if (result.Result.Item1.value.Count > 0)
                dailyVisitExpense = result.Result.Item1.value;

            return dailyVisitExpense;
        }

        //[Route("GetDailyvisitDDL")]
        //public List<SPVisitALLDataDDL> GetDailyvisitDDL(string dvpNo)
        //{
        //    API ac = new API();
        //    List<SPVisitALLDataDDL> invoiceprods = new List<SPVisitALLDataDDL>();

        //    var result = ac.GetData<SPVisitALLDataDDL>("DailyVisitsDotNetAPI", "No eq '" + dvpNo + "'"); // and Contact_Business_Relation eq 'Customer'

        //    if (result.Result.Item1.value.Count > 0)
        //        invoiceprods = result.Result.Item1.value;

        //    return invoiceprods;
        //}

    }
}
