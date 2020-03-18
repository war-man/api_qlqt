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
    [Route("api/typegift")]
    [ApiController]
    [Authorize]
    public class OptionGiftController : ControllerBase
    {
        OptionGiftService _optionGiftService = new OptionGiftService();

        [HttpGet("{pageNo}/{pageSize}/{textSearch}/{sltPermisionId}/{dateSearch}")]
        public IActionResult Get(int pageNo, int pageSize, string textSearch, int sltPermisionId, string dateSearch)
        {
            return Ok(_optionGiftService.Get(pageNo, pageSize, textSearch, sltPermisionId, dateSearch));
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(_optionGiftService.Get());
        }

        [HttpPost]
        public IActionResult Post([FromBody] OptionGift obj)
        {

            return Ok(_optionGiftService.Post(obj, HttpContext.User));
        }

        [HttpPut]
        public IActionResult Put([FromBody] OptionGift obj)
        {
            return Ok(_optionGiftService.Put(obj, HttpContext.User));
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            return Ok(_optionGiftService.Delete(id));
        }

        [HttpGet("{code}")]
        public IActionResult Get(string code)
        {
            return Ok(_optionGiftService.IsDuplicate(code));
        }
    }
}