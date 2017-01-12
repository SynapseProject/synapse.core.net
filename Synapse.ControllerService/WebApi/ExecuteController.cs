using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace Synapse.Services
{
    [RoutePrefix( "synapse/ex" )]
    public class ExecuteController : ApiController
    {
        PlanServer _server = new PlanServer();

        [Route( "{planName}" )]
        [HttpGet]
        public long StartPlan(string planName, bool dryRun = false)
        {
            //return DateTime.Now.Ticks;
            return _server.StartPlan( planName, dryRun );
        }


        ////[Route( "{domainUId:Guid}" )]
        ////[HttpGet]
        //// GET api/demo 
        [Route( "foo/" )]
        public IEnumerable<string> Get()
        {
            return new string[] { "Hello", "World", CurrentUser };
        }

        // GET api/demo/5 
        public string Get(int id)
        {
            return "Hello, World!";
        }

        //[Route( "" )]
        //[Route( "byrls/" )]
        //[HttpPost]
        // POST api/demo 
        public void Post([FromBody]string value) { }
        // PUT api/demo/5 
        public void Put(int id, [FromBody]string value) { }
        // DELETE api/demo/5 
        public void Delete(int id) { }


        public string CurrentUser
        {
            get
            {
                return User != null && User.Identity != null ? User.Identity.Name : "Anonymous";
            }
        }
    }
}