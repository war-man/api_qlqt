using Api.ManagerGift.DTO;
using Api.ManagerGift.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net.Http;

namespace Api.ManagerGift.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        AccountService _accountService;
        public AccountController(IConfiguration config)
        {
            _accountService = new AccountService(config);
        }
        [AllowAnonymous, HttpPost]
        public IActionResult Login([FromBody] UserDTO user)
        {
            dynamic result = new ExpandoObject();
            IActionResult response = Unauthorized();
            result = _accountService.Login(user.Email, user.Password);
            if ((((IDictionary<string, Object>)result).ContainsKey("JWT")
                && ((IDictionary<string, Object>)result).ContainsKey("FullName"))
                    || ((IDictionary<string, Object>)result).ContainsKey("IsResetPass"))
            {
                response = Ok(result);
            }
            return response;
        }
        /// <summary>
        /// Thay đổi mật khẩu.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpPost("ChangePassword/{password}")]
        public IActionResult ChangePassword(string password)
        {
            return Ok(_accountService.ChangePassword(HttpContext.User, password));
        }

        /// <summary>
        /// Reset mat khau. Danh cho nguoi dung
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        [HttpPut("ResetPassword/{user}/{passwordOld}/{passwordNew}")]
        public IActionResult ResetPassword(string user, string passwordOld, string passwordNew)
        {
            dynamic result = new ExpandoObject();
            result = _accountService.ResetPassword(user, passwordOld, passwordNew);
            return Ok(result);
        }
    }
}