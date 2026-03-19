using PrakashCRM.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Xipton.Razor;

namespace PrakashCRM.Service.Classes
{
    public class RazorRenderer : ITemplateRenderer
    {
        public RazorRenderer()
        {
        }

        public string Parse<T>(string template, T model, bool isHtml = true)
        {
            var rm = new RazorMachine(htmlEncode: false);

            var razorTemplate = rm.ExecuteContent(template, model, null, true);

            return razorTemplate.Result;
        }
        public string ParseFile<T>(string url, T model, bool isHtml = true)
        {
            var razorTemplate = new RazorMachine(includeGeneratedSourceCode: true).ExecuteUrl(url, model);

            return razorTemplate.Result;
        }

        public void RegisterTemplate(string p1, string p2)
        {
            var rm = new RazorMachine(htmlEncode: true);
            rm.RegisterTemplate(p1, p2);
        }
    }
}