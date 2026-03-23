using PrakashCRM.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Xml.Linq;

namespace PrakashCRM.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        public ActionResult ContactList()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult ErrorStatus(int code = 500)
        {
            Response.TrySkipIisCustomErrors = true;
            Response.StatusCode = (int)HttpStatusCode.OK;

            ViewBag.ErrorCode = code;
            ViewBag.ErrorTitle = GetErrorTitle(code);
            ViewBag.ErrorMessage = GetErrorMessage(code);

            return View();
        }

        private static string GetErrorTitle(int code)
        {
            switch (code)
            {
                case 400:
                    return "Bad Request";
                case 401:
                    return "Unauthorized";
                case 403:
                    return "Forbidden";
                case 404:
                    return "Page Not Found";
                case 409:
                    return "Conflict";
                case 503:
                    return "Service Unavailable";
                default:
                    return "Server Error";
            }
        }

        private static string GetErrorMessage(int code)
        {
            switch (code)
            {
                case 400:
                    return "The request could not be processed. Verify the submitted data and try again.";
                case 401:
                    return "Your session may have expired. Sign in again and retry the action.";
                case 403:
                    return "You do not have permission to access this resource.";
                case 404:
                    return "The requested page could not be found.";
                case 409:
                    return "The request could not be completed because of a data conflict.";
                case 503:
                    return "The service is temporarily unavailable. Please try again after a short time.";
                default:
                    return "An unexpected error occurred while processing your request.";
            }
        }
    }
}