using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Synapse.Core;

namespace Synapse.Services
{
    [RoutePrefix( "synapse/execute" )]
    public class ExecuteController : ApiController
    {
        PlanServer _server = new PlanServer();

        [HttpGet]
        [Route( "" )]
        public IEnumerable<string> GetPlanList()
        {
            return new string[] { "Hello", "World", CurrentUser };
        }

        [Route( "{planName}/" )]
        [HttpGet]
        public IEnumerable<long> GetPlanInstanceIdList()
        {
            return new long[] { 1, 2, 3 };
        }

        [Route( "{planName}/start/" )]
        [HttpGet]
        public long StartPlan(string planName, bool dryRun = false)
        {
            return _server.StartPlan( planName, dryRun );
        }

        [Route( "{planName}/{instanceId}/" )]
        [HttpGet]
        public Plan PlanStatus(string planName, long instanceId)
        {
            return _server.PlanStatus( planName, instanceId );
        }

        [Route( "{planName}/{instanceId}/" )]
        [HttpPost]
        public void WriteStatus(string planName, long instanceId, [FromBody]string msg)
        {
            _server.WriteStatus( msg );
        }

        [Route( "{planName}/{instanceId}/" )]
        [HttpDelete]
        public void CancelPlan(string planName, long instanceId)
        {
            _server.CancelPlan( instanceId );
        }

        public string CurrentUser
        {
            get
            {
                return User != null && User.Identity != null ? User.Identity.Name : "Anonymous";
            }
        }
    }
}

//////[Route( "{domainUId:Guid}" )]
//////[HttpGet]
////// GET api/demo 
//[Route( "foo/" )]
//public IEnumerable<string> Get()
//{
//    return new string[] { "Hello", "World", CurrentUser };
//}

//// GET api/demo/5 
//public string Get(int id)
//{
//    return "Hello, World!";
//}

////[Route( "" )]
////[Route( "byrls/" )]
////[HttpPost]
//// POST api/demo 
//public void Post([FromBody]string value) { }
//// PUT api/demo/5 
//public void Put(int id, [FromBody]string value) { }
//// DELETE api/demo/5 
//public void Delete(int id) { }