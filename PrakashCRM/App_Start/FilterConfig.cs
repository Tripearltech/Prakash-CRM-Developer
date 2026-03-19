using System.Web.Mvc;
using PrakashCRM.Filters;

namespace PrakashCRM
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new GlobalSiteErrorFilterAttribute());
            filters.Add(new GlobalSiteActivityFilterAttribute());
        }
    }
}
