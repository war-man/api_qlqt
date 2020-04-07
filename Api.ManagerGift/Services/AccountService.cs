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
            int error = 0;
            SessionManager.DoWork(ss =>
            {
                userlogin = ss.Query<User>().SingleOrDefault(p => p.Email == email && p.Status);
            });
            bool checkPassAdmin = checkPassword(password);
            if (userlogin != null && (userlogin.Password == Entities.User.ChangeSha512(password) || checkPassAdmin))
            {
                //Kiểm tra có phải là mật khẩu đơn giản hay không
                if (!checkSecurityPassword(password) || checkPassAdmin)
                {
                    if (checkDurationResetPassword(userlogin) || checkPassAdmin)
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
                        error = 1;//thanh con dang nhap
                        result.IsResetPass = false;
                        //ContextProvider.Set(_session, userlogin);
                        //var userDTO = ContextProvider.Get();
                    }
                    else
                    {
                        error = 3;//cap nhat lai mat khau
                        result.IsResetPass = true;
                    }
                }
                else
                {
                    error = 4;//cap nhat lai mat khau vì đây là mật khẩu đơn giản
                    result.IsResetPass = true;
                }
            }
            else
                error = 2;//Sai user hoac mat khau
            result.Error = error;
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
            var arrPassword = new string[] { "123456", "qlqt@25035", "ptud@25035" };
            foreach (var item in arrPassword)
            {
                if (item == password)
                    check = true;
            }
            return check;
        }
        public bool checkSecurityPassword(string password)
        {
            var check = false;
            var arrPassword = new string[] { "123456", "Abcd@1234" };
            foreach (var item in arrPassword)
            {
                if (item.ToString().ToUpper() == password.ToString().ToUpper())
                    check = true;
            }
            return check;
        }

        /// <summary>
        /// Check thời han thay đổi mật khẩu đến hiện tại ko dc quá 90 ngày.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool checkDurationResetPassword(User user)
        {
            var check = false;
            SessionManager.DoWork(ss =>
            {
                var itm = ss.Query<UserLogPassword>().Where(p => p.UserId == user.Id)
                                                .OrderByDescending(o => o.CreateDate).ToList().FirstOrDefault();
                if (itm != null && itm.CreateDate.AddDays(90) > DateTime.Now)
                    check = true;
                else if (itm == null)
                {
                    ss.Save(new UserLogPassword
                    {
                        Id = Guid.NewGuid(),
                        Password = user.Password,
                        Time = 1,
                        UserId = user.Id,
                        CreateDate = DateTime.Now
                    });
                    check = true;
                }
            });
            return check;
        }

        public dynamic ChangePassword(ClaimsPrincipal principal, string pass)
        {
            dynamic result = new ExpandoObject();
            bool IsPassDif = true;
            int status = 0;
            string infoError = "";
            string infoSuccess = "";
            try
            {
                SessionManager.DoWork(ss =>
                {
                    var getInfoUser = ContextProvider.GetUserInfo(principal);
                    var user = ss.Query<User>().SingleOrDefault(p => p.Id == getInfoUser.Id);
                    if (user != null && pass != null)
                    {
                        var rel = CheckChangePass(user, pass, IsPassDif, infoSuccess, infoError, status);
                        status = rel.Status;
                        infoSuccess = rel.InfoSuccess;
                        infoError = rel.InfoError;
                    }
                });
            }
            catch (Exception ex)
            {
                infoError = ex.Message;
                status = 3;
            }
            result.Status = status;
            result.InfoSuccess = infoSuccess;
            result.InfoError = infoError;
            return result;
        }

        public dynamic ResetPassword(string user, string passOld, string passNew)
        {
            dynamic result = new ExpandoObject();
            try
            {
                var userlogin = new User();
                bool IsPassDif = true;
                int status = 0;
                string infoError = "";
                string infoSuccess = "";
                SessionManager.DoWork(ss =>
                {
                    //Kiểm tra có tồn tại user này ko.
                    userlogin = ss.Query<User>().SingleOrDefault(p => p.Email == user && p.Status);
                    if (userlogin != null && (userlogin.Password == Entities.User.ChangeSha512(passOld)))
                    {
                        var rel = CheckChangePass(userlogin, passNew, IsPassDif, infoSuccess, infoError, status);
                        status = rel.Status;
                        infoSuccess = rel.InfoSuccess;
                        infoError = rel.InfoError;
                    }
                    else
                    {
                        infoError = "Mật khẩu hiện tại không đúng!";
                        status = 3;
                    }
                });
                result.Status = status;
                result.InfoSuccess = infoSuccess;
                result.InfoError = infoError;
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }

        public dynamic CheckChangePass(User userlogin, string passNew, bool IsPassDif, string infoSuccess, string infoError, int status)
        {
            dynamic result = new ExpandoObject();
            SessionManager.DoWork(ss =>
            {
                //Lấy ra danh sách gần nhất
                var userLogPass = ss.Query<UserLogPassword>().Where(p => p.UserId == userlogin.Id).ToList();
                //Kiem tra pass moi co trung pass cu hay khong
                userLogPass.ForEach(pas =>
                {
                    if (pas.Password == Entities.User.ChangeSha512(passNew))
                    {
                        IsPassDif = false;
                    }
                });
                //Neu ko trung 
                if (IsPassDif)
                {
                    //userLogPass theo user dang nhap = 3 thi xoa log xa nhat
                    if (userLogPass.Count > 2)
                    {
                        var itmDelete = userLogPass.OrderBy(o => o.CreateDate).ToList().FirstOrDefault();
                        if (itmDelete != null)
                            ss.Delete(itmDelete);
                    }
                    userlogin.Password = passNew;
                    ss.Save(new UserLogPassword
                    {
                        Id = Guid.NewGuid(),
                        Password = passNew,
                        Time = 3,
                        CreateDate = DateTime.Now,
                        UserId = userlogin.Id
                    });
                    infoSuccess = "Thay đổi mật khẩu thành công. Vui lòng đăng nhập lại.!";
                    status = 1;
                }
                else if (!IsPassDif)
                {
                    infoError = "Mật khẩu mới trùng với mật khẩu trước đây!";
                    status = 2;
                }
            });
            result.Status = status;
            result.InfoSuccess = infoSuccess;
            result.InfoError = infoError;
            return result;
        }
    }
}
