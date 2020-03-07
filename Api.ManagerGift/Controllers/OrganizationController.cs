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
    public class OrganizationController : ControllerBase
    {
        OrganizationService _organizationService = new OrganizationService();

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(_organizationService.Get());
        }

        [HttpPost]
        public IActionResult Post([FromBody] Organization obj)
        {
            return Ok(_organizationService.Post(obj));
        }

        [HttpPut]
        public IActionResult Put([FromBody] Organization obj)
        {
            return Ok(_organizationService.Put(obj));
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            return Ok(_organizationService.Delete(id));
        }
        [HttpGet("{code}")]
        public IActionResult Get(string code)
        {
            return Ok(_organizationService.IsDuplicate(code));
        }

        [HttpGet("GetAllBranchs")]
        public IActionResult GetAllBranchs()
        {
            return Ok(_organizationService.GetAllBranchs());
        }
        [HttpGet("GetDetailById/{id}")]
        public IActionResult GetDetailById(Guid id)
        {
            return Ok(_organizationService.GetDetailById(id));
        }

        [HttpGet("GetAll")]
        public IActionResult GetAll()
        {
            return Ok(_organizationService.GetAll());
        }
    }
}