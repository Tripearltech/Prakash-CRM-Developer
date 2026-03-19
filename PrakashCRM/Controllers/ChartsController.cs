using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PrakashCRM.Controllers
{
    public class DataPoint
    {
        public DataPoint(string label, double y)
        {
            this.Label = label;
            this.Y = y;
        }

        //Explicitly setting the name to be used while serializing to JSON.
        
        public string Label = "";

        //Explicitly setting the name to be used while serializing to JSON.
        
        public Nullable<double> Y = null;
    }


    [RedirectingAction]
    public class ChartsController : Controller
    {
        // GET: Charts
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult CustomerSatisfaction()
        {
            List<DataPoint> dataPoints = new List<DataPoint>();

            dataPoints.Add(new DataPoint("Samsung", 25));
            dataPoints.Add(new DataPoint("Micromax", 13));
            dataPoints.Add(new DataPoint("Lenovo", 8));
            dataPoints.Add(new DataPoint("Intex", 7));
            dataPoints.Add(new DataPoint("Reliance", 6.8));
            dataPoints.Add(new DataPoint("Others", 40.2));

            ViewBag.DataPoints = JsonConvert.SerializeObject(dataPoints);

            return View();
        }
    }
}