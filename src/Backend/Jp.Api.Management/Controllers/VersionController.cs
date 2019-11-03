using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Jp.Api.Management.Controllers
{
    [Route("version"), Authorize(Policy = "ReadOnly"), ApiController]
    public class VersionController : ControllerBase
    {
        [HttpGet]
        public string Get() => "full";

        [HttpPost]
        public OkObjectResult Teste()
        {
            return Ok(DateTime.Now.ToString("HH:mm:ss tt zz"));
        }
    }
}
