using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jp.Api.Management.Controllers
{
    [Route("version"), Authorize(Policy = "ReadOnly"), ApiController]
    public class VersionController : ControllerBase
    {
        [HttpGet]
        public string Get() => "full";

    }
}
