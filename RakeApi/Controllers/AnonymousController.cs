using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace RakeApi.Controllers
{
    [Route("v1")]
    [ApiController]
    public class AnonymousController : ControllerBase
    {
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }
    }
}