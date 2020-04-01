using Api.ManagerGift.DTO;
using Api.ManagerGift.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Api.ManagerGift.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        UserService _userService = new UserService();

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(_userService.Get());
        }

        [HttpPost]
        public IActionResult Post([FromBody] UserDTO obj)
        {
            return Ok(_userService.Post(obj));
        }

        [HttpPut]
        public IActionResult Put([FromBody] UserDTO obj)
        {
            return Ok(_userService.Put(obj));
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            return Ok(_userService.Delete(id));
        }

        [HttpGet("{username}")]
        public IActionResult Get(string username)
        {
            return Ok(_userService.IsDuplicate(username));
        }

        [HttpPut("{username}")]
        public IActionResult Put(string username)
        {
            return Ok(_userService.RefreshPassword(username));
        }

        [HttpPut("{username}/{password}")]
        public IActionResult Put(string username, string password)
        {
            return Ok(_userService.ChangePassword(username, password));
        }

        [HttpGet("GetListUserByOrganizationId/{organizationId}")]
        public IActionResult Get(Guid organizationId)
        {
            return Ok(_userService.GetAllUseOfOrg(organizationId));
        }

        [HttpPut("BlockAndActive/{username}/{status}")]
        public IActionResult BlockAndActive(string username, bool status)
        {
            return Ok(_userService.SetStatus(username, status));
        }
    }
}