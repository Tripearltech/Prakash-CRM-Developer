
using PrakashCRM.Data.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Mvc;

namespace PrakashCRM.Controllers
{
    public class ItemsAPIController : ApiController
    {
        // GET api/<controller>
        //[System.Web.Http.ActionName("get"), System.Web.Http.HttpGet]
        //public IEnumerable<Item> Get()
        //{
        //    //var ac = new API();

        //    ////string encodeurl = Uri.EscapeUriString("/ODataV4/Company('" + companyname + "')/ITEMAPI");

        //    //var result = ac.GetData<Item>("ItemCardDotNetAPI", "");

        //    //return result.Result.Item1.value;
            
        //}

        // GET api/<controller>/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<controller>
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<controller>/5
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<controller>/5
        public void Delete(int id)
        {
        }
    }
}