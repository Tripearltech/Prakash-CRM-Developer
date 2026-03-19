using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PrakashCRM.Controllers
{
    [RedirectingAction]
    public class InquiryController : Controller
    {
        // GET: Inquiry
        public ActionResult Index()
        {
            return View();
        }
    }
}