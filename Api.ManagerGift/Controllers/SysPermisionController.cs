using Api.ManagerGift.DTO;
using Api.ManagerGift.Entities;
using Api.ManagerGift.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Api.ManagerGift.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SysPermisionController : ControllerBase
    {
        SysPermisionService _sysPermisionService = new SysPermisionService();

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(_sysPermisionService.Get());
        }

        [HttpPost]
        public IActionResult Post([FromBody] Position obj)
        {
            return Ok(_sysPermisionService.Post(obj));
        }

        [HttpPut]
        public IActionResult Put([FromBody] Position obj)
        {
            return Ok(_sysPermisionService.Put(obj));
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            return Ok(_sysPermisionService.Delete(id));
        }
        [HttpGet("{code}")]
        public IActionResult Get(string code)
        {
            return Ok(_sysPermisionService.IsDuplicate(code));
        }
    }
}