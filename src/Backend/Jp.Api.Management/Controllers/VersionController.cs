using Microsoft.AspNetCore.Mvc;

namespace Jp.Api.Management.Controllers
{
    [Route("version"), ApiController]
    public class VersionController : ControllerBase
    {
        [HttpGet]
        public string Get() => "full";

    }
}
