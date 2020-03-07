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
    public class PositionController : ControllerBase
    {
        PositionService _positionService = new PositionService();

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(_positionService.Get());
        }

        [HttpPost]
        public IActionResult Post([FromBody] Position obj)
        {
            return Ok(_positionService.Post(obj));
        }

        [HttpPut]
        public IActionResult Put([FromBody] Position obj)
        {
            return Ok(_positionService.Put(obj));
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            return Ok(_positionService.Delete(id));
        }
        [HttpGet("{code}")]
        public IActionResult Get(string code)
        {
            return Ok(_positionService.IsDuplicate(code));
        }
    }
}