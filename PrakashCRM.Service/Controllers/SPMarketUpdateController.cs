using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using PrakashCRM.Data.Models;
using PrakashCRM.Service.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Threading.Tasks;

namespace PrakashCRM.Service.Controllers
{
    [RoutePrefix("api/SPMarketUpdate")]
    public class SPMarketUpdateController : ApiController
    {

        [Route("GetAllMarketUpdateDetails")]
        public List<SPMarketUpdateList> GetAllMarketUpdateDetails(string SPNo, int skip, int top, string orderby, string filter, bool isExport = false)
        {
            API ac = new API();
            List<SPMarketUpdateList> marketUpdateList = new List<SPMarketUpdateList>();

            if (filter == null)
                filter = "";

            //if (filter == "" || filter == null)
            //    filter = "Salesperson_Code eq '" + SPNo + "'";
            //else
            //    filter = filter + " and Salesperson_Code eq '" + SPNo + "'";

            var result = (dynamic)null;

            if (isExport)
                result = ac.GetData<SPMarketUpdateList>("MarketUpdateDotNetAPI", filter);
            else
                result = ac.GetData1<SPMarketUpdateList>("MarketUpdateDotNetAPI", filter, skip, top, orderby);

            if (result.Result.Item1.value.Count > 0)
                marketUpdateList = result.Result.Item1.value;

            for (int i = 0; i < marketUpdateList.Count; i++)
            {
                string[] strDate = marketUpdateList[i].Update_Date.Split('-');
                marketUpdateList[i].Update_Date = strDate[2] + '-' + strDate[1] + '-' + strDate[0];
            }

            return marketUpdateList;
        }

        [Route("GetApiRecordsCount")]
        public int GetApiRecordsCount(string SPNo, string apiEndPointName, string filter)
        {
            API ac = new API();

            if (filter == null)
                filter = "";

            //if (filter == "" || filter == null)
            //    filter = "Salesperson_Code eq '" + SPNo + "'";
            //else
            //    filter = filter + " and Salesperson_Code eq '" + SPNo + "'";

            var count = ac.CalculateCount(apiEndPointName, filter);

            return Convert.ToInt32(count.Result);
        }

    }
}
