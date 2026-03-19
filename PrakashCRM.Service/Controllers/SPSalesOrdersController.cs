using PrakashCRM.Data.Models;
using PrakashCRM.Service.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace PrakashCRM.Service.Controllers
{
    [RoutePrefix("api/SPSalesOrders")]
    public class SPSalesOrdersController : ApiController
    {
        [Route("GetSalesOrders")]
        public List<SPSalesOrdersList> GetSalesOrders(string SPCode, int skip, int top, string orderby, string filter, string allOpenFilter)
        {
            API ac = new API();
            List<SPSalesOrdersList> salesorders = new List<SPSalesOrdersList>();
            var result = (dynamic)null;

            if(allOpenFilter == "All")
            {
                if (filter == "" || filter == null)
                    filter = "Salesperson_Code eq '" + SPCode + "'";
                else
                    filter = filter + " and Salesperson_Code eq '" + SPCode + "'";
            }

            if(allOpenFilter == "Open")
            {
                if (filter == "" || filter == null)
                    filter = "Salesperson_Code eq '" + SPCode + "' and Status eq 'Open'";
                else
                    filter = filter + " and Salesperson_Code eq '" + SPCode + "' and Status eq 'Open'";
            }

            result = ac.GetData1<SPSalesOrdersList>("SalesOrdersListDotNetAPI", filter, skip, top, orderby); // and Contact_Business_Relation eq 'Customer'

            if (result.Result.Item1.value.Count > 0)
                salesorders = result.Result.Item1.value;

            for(int i = 0; i < salesorders.Count; i++)
            {
                string[] strDate = salesorders[i].Document_Date.Split('-');
                salesorders[i].Document_Date = strDate[2] + '-' + strDate[1] + '-' + strDate[0];
            }
            
            return salesorders;
        }

        [Route("GetApiRecordsCount")]
        public int GetApiRecordsCount(string SPCode, string apiEndPointName, string filter, string allOpenFilter)
        {
            API ac = new API();

            if (allOpenFilter == "All")
            {
                if (filter == "" || filter == null)
                    filter = "Salesperson_Code eq '" + SPCode + "'";
                else
                    filter = filter + " and Salesperson_Code eq '" + SPCode + "'";
            }

            if(allOpenFilter == "Open")
            {
                if (filter == "" || filter == null)
                    filter = "Salesperson_Code eq '" + SPCode + "' and Status eq 'Open'";
                else
                    filter = filter + " and Salesperson_Code eq '" + SPCode + "' and Status eq 'Open'";
            }

            var count = ac.CalculateCount(apiEndPointName, filter);

            return Convert.ToInt32(count.Result);
        }
    }
}
