using Microsoft.AspNetCore.Mvc;

namespace TcmAiDiagnosis.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public ActionResult<string> Get()
        {
            return Ok("API Test - Working!");
        }

        [HttpGet("herb-test")]
        public ActionResult<string> HerbTest()
        {
            return Ok("Herb API Test - Working!");
        }
    }
}