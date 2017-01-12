using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace Synapse.Services
{
    [RoutePrefix( "synapse/control" )]
    public class RuntimeController : ApiController
    {
        //[Route( "{domainUId:Guid}" )]
        //[HttpGet]
        // GET api/demo 
        public IEnumerable<string> Get()
        {
            WindowsPrincipal user = RequestContext.Principal as WindowsPrincipal;
            return new string[] { "Hello", "World", user.Identity.Name, User.Identity.Name };
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
        public void Post([FromBody]string value)
        {
        }

        // PUT api/demo/5 
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/demo/5 
        public void Delete(int id)
        {
        }
    }
}