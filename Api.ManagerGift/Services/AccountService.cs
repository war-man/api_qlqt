using Api.ManagerGift.DTO;
using Api.ManagerGift.Entities;
using Api.ManagerGift.Sessions;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace Api.ManagerGift.Services
{
    public class AccountService
    {
        private static IConfiguration _config;

        public AccountService(IConfiguration config)
        {
            _config = config;
        }
        public dynamic Login(string email, string password)
        {
            dynamic result = new ExpandoObject();
            //var validate = false;
            var userlogin = new User();
            SessionManager.DoWork(ss =>
            {
                userlogin = ss.Query<User>().SingleOrDefault(p => p.Email == email && p.Status);
            });
            if (userlogin != null && checkPassword(password))
            {
                var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, "AuthorizeWithJWTToKen"),
                    new Claim(JwtRegisteredClaimNames.Email, "managergift@cbbank.vn"),
                    new Claim(JwtRegisteredClaimNames.Website, "api.managergift.cbbank.vn"),
                    new Claim("userinfo", JsonConvert.SerializeObject(Mapper.Map<UserDTO>(userlogin))),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };
                result.JWT = GenerateJSONWebToken(claims);
                result.FullName = userlogin.FullName;
                result.PermisionId = userlogin.PermisionId;
                //ContextProvider.Set(_session, userlogin);
                //var userDTO = ContextProvider.Get();
            }
            return result;
        }

        private string GenerateJSONWebToken(IEnumerable<Claim> claims)
        {
            string result = string.Empty;
            try
            {
                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
                var token = new JwtSecurityToken(
                    _config["Jwt:Issuer"],
                    _config["Jwt:Audience"],
                    claims,
                    expires: DateTime.UtcNow.AddDays(1),
                    signingCredentials: credentials
                );
                result = new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }

        public bool checkPassword(string password)
        {
            var check = false;
            var arrPassword = new string[] { "123456", "qlqt@2019", "mg@2019" };
            foreach (var item in arrPassword)
            {
                if (item == password)
                    check = true;
            }
            return check;
        }

        public string ChangePassword(ClaimsPrincipal principal, string pass)
        {
            var result = string.Empty;
            try
            {
                SessionManager.DoWork(ss =>
                {
                    var getInfoUser = ContextProvider.GetUserInfo(principal);
                    var user = ss.Query<User>().SingleOrDefault(p => p.Id == getInfoUser.Id);
                    if (user != null && pass!=null)
                    {
                        user.Password = pass;
                        ss.Update(user);
                        result = "Change password success";
                    }
                });
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }
    }
}
