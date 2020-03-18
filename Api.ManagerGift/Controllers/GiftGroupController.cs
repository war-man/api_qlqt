using Api.ManagerGift.DTO;
using Api.ManagerGift.Entities;
using Api.ManagerGift.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Api.ManagerGift.Controllers
{
    [Route("api/groupgift")]
    [ApiController]
    [Authorize]
    public class GiftGroupController : ControllerBase
    {
        GiftGroupService _giftGroupService = new GiftGroupService();

        [HttpGet("{pageNo}/{pageSize}/{textSearch}/{sltPermisionId}/{dateSearch}")]
        public IActionResult Get(int pageNo, int pageSize, string textSearch, int sltPermisionId, string dateSearch)
        {
            return Ok(_giftGroupService.Get(pageNo, pageSize, textSearch, sltPermisionId, dateSearch));
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(_giftGroupService.Get());
        }

        [HttpPost]
        public IActionResult Post([FromBody] GiftGroupDTO obj)
        {
            return Ok(_giftGroupService.Post(obj, HttpContext.User));
        }

        [HttpPut]
        public IActionResult Put([FromBody] GiftGroupDTO obj)
        {
            return Ok(_giftGroupService.Put(obj, HttpContext.User));
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            return Ok(_giftGroupService.Delete(id));
        }

        [HttpGet("{code}")]
        public IActionResult Get(string code)
        {
            return Ok(_giftGroupService.IsDuplicate(code));
        }

        [HttpGet("GetListGroupGiftByOptionGiftId/{OptionGiftId}")]
        public IActionResult GetGroupOfOption(Guid OptionGiftId)
        {
            return Ok(_giftGroupService.GetGroupOfOption(OptionGiftId));
        }
    }
}