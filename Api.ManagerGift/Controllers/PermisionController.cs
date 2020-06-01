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
        [HttpGet("GetAll")]
        public IActionResult GetAll() => Ok(_PermisionService.GetAlls(HttpContext.User));

        [HttpGet("GetPermision/{id}")]
        public IActionResult GetPer(string id) => Ok(_PermisionService.GetPer(id));

        [HttpPut]
        public IActionResult EditPermision([FromBody] PermisionDetail per) => Ok(_PermisionService.EditPermision(per, HttpContext.User));

        [HttpGet("GetAllNav")]
        public IActionResult GetAllNav() => Ok(_PermisionService.GetAllNavs(HttpContext.User));
        [HttpGet("GetRootNav")]
        public IActionResult GetRootNav() => Ok(_PermisionService.GetRootNav(HttpContext.User));
    }
}