using Api.ManagerGift.DTO;
using Api.ManagerGift.Entities;
using Api.ManagerGift.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace Api.ManagerGift.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class GiftController : ControllerBase
    {
        readonly private GiftService _giftService = new GiftService();

        [HttpGet("{pageNo}/{pageSize}/{textSearch}/{typeGift}/{groupGift}")]
        public IActionResult Get(int pageNo, int pageSize, string textSearch, string typeGift, string groupGift)
        {
            return Ok(_giftService.Get(pageNo, pageSize, textSearch, typeGift, groupGift));
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(_giftService.Get());
        }

        [HttpPost]
        public IActionResult Post([FromBody] GiftDTO obj)
        {
            return Ok(_giftService.Post(obj, HttpContext.User));
        }

        [HttpPut]
        public IActionResult Put([FromBody] GiftDTO obj)
        {
            return Ok(_giftService.Put(obj, HttpContext.User));
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            return Ok(_giftService.Delete(id));
        }

        [HttpGet("{code}")]
        public IActionResult Get(string code)
        {
            return Ok(_giftService.IsDuplicate(code));
        }

        [HttpGet("GetDetail/{id}")]
        public IActionResult GetDetail(Guid id)
        {
            return Ok(_giftService.GetDetail(id));
        }

        // GET LISTGIFT FOR COMBOBOX IN FORM ADD NEW PROMOTION
        [HttpGet("GiftPromotion")]
        public IActionResult GetGift()
            => Ok(_giftService.GetGift());
    }
}