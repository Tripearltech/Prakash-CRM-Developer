using PrakashCRM.Data.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace PrakashCRM.Controllers
{
    [RedirectingAction]
    public class SPActivitiesController : Controller
    {
        // GET: Activities
        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> Activities()
        {
            //string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "SPActivities/";

            //apiUrl = apiUrl + "GetAllActivities";

            //HttpClient client = new HttpClient();
            //List<SPActivities> activities = new List<SPActivities>();

            //client.BaseAddress = new Uri(apiUrl);
            //client.DefaultRequestHeaders.Accept.Clear();
            //client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            //HttpResponseMessage response = await client.GetAsync(apiUrl);
            //if (response.IsSuccessStatusCode)
            //{
            //    var data = await response.Content.ReadAsStringAsync();
            //    activities = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SPActivities>>(data);
            //}

            //return View(activities);

            return View();
        }
        public ActionResult Task(string No = "")
        {
            SPTask task = new SPTask();
            
            if (task != null)
                return View(task);
            else
                return View(new SPTask());
        }

        public ActionResult PhoneCall(string No = "")
        {
            SPPhoneCall phoneCall = new SPPhoneCall();

            if (phoneCall != null)
                return View(phoneCall);
            else
                return View(new SPPhoneCall());
        }

        public ActionResult Meeting(string No = "")
        {
            SPMeeting meeting = new SPMeeting();

            if (meeting != null)
                return View(meeting);
            else
                return View(new SPMeeting());
        }
    }
}