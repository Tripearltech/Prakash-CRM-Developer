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

namespace PrakashCRM.Service.Controllers
{
    [RoutePrefix("api/SPPostedSalesInvoice")]
    public class SPPostedSalesInvoiceController : ApiController
    {
        [Route("GetAllPostedSalesInvoice")]
        public List<SPPostedSalesInvoiceList> GetAllPostedSalesInvoice(string SPCode, int skip, int top, string orderby, string filter)
        {
            API ac = new API();
            List<SPPostedSalesInvoiceList> postedsalesInvoice = new List<SPPostedSalesInvoiceList>();

            if (filter == "" || filter == null)
                filter = "Salesperson_Code eq '" + SPCode + "'";
            else
                filter = filter + " and Salesperson_Code eq '" + SPCode + "'";

            var result = ac.GetData1<SPPostedSalesInvoiceList>("PostedSalesInvoicesDotNetAPI", filter, skip, top, orderby);

            if (result.Result.Item1.value.Count > 0)
                postedsalesInvoice = result.Result.Item1.value;

            for(int i = 0; i < postedsalesInvoice.Count; i++)
            {
                string[] strDate = postedsalesInvoice[i].Posting_Date.Split('-');
                postedsalesInvoice[i].Posting_Date = strDate[2] + '-' + strDate[1] + '-' + strDate[0];

                string[] strDate1 = postedsalesInvoice[i].Due_Date.Split('-');
                postedsalesInvoice[i].Due_Date = strDate1[2] + '-' + strDate1[1] + '-' + strDate1[0];
            }

            return postedsalesInvoice;
        }

        [Route("GetApiRecordsCount")]
        public int GetApiRecordsCount(string SPCode, string apiEndPointName, string filter)
        {
            API ac = new API();

            if (filter == "" || filter == null)
                filter = "Salesperson_Code eq '" + SPCode + "'";
            else
                filter = filter + " and Salesperson_Code eq '" + SPCode + "'";

            var count = ac.CalculateCount(apiEndPointName, filter);

            return Convert.ToInt32(count.Result);
        }
    }
}