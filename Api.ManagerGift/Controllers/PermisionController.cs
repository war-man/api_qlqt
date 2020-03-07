using Api.ManagerGift.Entities;
using Api.ManagerGift.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.ManagerGift.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PermisionController : ControllerBase
    {
        PermisionService _PermisionService = new PermisionService();

        /// <summary>
        /// every component get permision for check permision_button
        /// </summary>
        [HttpGet("{url}")]
        public IActionResult GetPermision(string url) => Ok(_PermisionService.Get(url, HttpContext.User));

        [HttpGet]
        public IActionResult Get() => Ok(_PermisionService.GetAll());

        [HttpGet("GetPermision/{url}")]
        public IActionResult GetPer(string url) => Ok(_PermisionService.GetPer(url));

        [HttpPut]
        public IActionResult EditPermision([FromBody] PermisionDetail per) => Ok(_PermisionService.EditPermision(per, HttpContext.User));
    }
}