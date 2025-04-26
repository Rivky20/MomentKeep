using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MomentKeep.Controllers
{
    [ApiController]
    [Route("api")]
    public class HomeController : Controller
    {
        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new { status = "healthy" });
        }
    }
}