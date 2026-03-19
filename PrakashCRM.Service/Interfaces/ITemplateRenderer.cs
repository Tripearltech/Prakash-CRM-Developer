using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrakashCRM.Service.Interfaces
{
    public interface ITemplateRenderer
    {
        string Parse<T>(string template, T model, bool isHtml = true);
        string ParseFile<T>(string url, T model, bool isHtml = true);

        void RegisterTemplate(string p1, string p2);
    }
}
