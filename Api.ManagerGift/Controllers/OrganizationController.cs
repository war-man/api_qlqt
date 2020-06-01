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
            return Ok(_organizationService.GetAll(HttpContext.User));
        }
        [HttpGet("GetInfoDetail")]
        public IActionResult GetInfoDetail()
        {
            return Ok(_organizationService.GetInfoDetail());
        }
        [HttpGet("GetBranchReport")]
        public IActionResult GetBranchReport()
        {
            return Ok(_organizationService.GetBranchReport(HttpContext.User));
        }
        [HttpGet("GetDepartmentReport/{id}")]
        public IActionResult GetDepartmentReport(string id)
        {
            return Ok(_organizationService.GetDepartmentReport(HttpContext.User, id));
        }
        [HttpGet("GetInUserLogin")]
        public IActionResult GetInUserLogin()
        {
            return Ok(_organizationService.GetInUserLogin(HttpContext.User));
        }
    }
}