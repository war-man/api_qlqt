using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.ManagerGift.Entities;
using Api.ManagerGift.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.ManagerGift.Controllers
{
    [Route("api/unitgift")]
    [ApiController]
    [Authorize]
    public class UnitController : ControllerBase
    {
        UnitService _unitService = new UnitService();

        [HttpGet("{pageNo}/{pageSize}/{textSearch}")]
        public IActionResult Get(int pageNo, int pageSize, string textSearch)
        {
            return Ok(_unitService.Get(pageNo, pageSize, textSearch));
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(_unitService.Get());
        }

        [HttpPost]
        public IActionResult Post([FromBody] Unit obj)
        {
            return Ok(_unitService.Post(obj, HttpContext.User));
        }

        [HttpPut]
        public IActionResult Put([FromBody] Unit obj)
        {
            return Ok(_unitService.Put(obj, HttpContext.User));
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            return Ok(_unitService.Delete(id));
        }

        [HttpGet("{code}")]
        public IActionResult Get(string code)
        {
            return Ok(_unitService.IsDuplicate(code));
        }
    }
}