using Api.ManagerGift.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.ManagerGift.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        TestServices _testServices = new TestServices();

        [HttpGet("GetAllUnits"), Authorize]
        public IActionResult GetAllUnits()
        {
            return Ok(_testServices.GetAllUnits());
        }
        
        [HttpGet("GetAllUsers"), AllowAnonymous]
        public IActionResult GetAllUsers()
        {
            return Ok(_testServices.GetAllUsers());
        }
    }
}