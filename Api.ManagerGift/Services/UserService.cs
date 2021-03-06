﻿using Api.ManagerGift.DTO;
using Api.ManagerGift.Entities;
using Api.ManagerGift.Sessions;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Api.ManagerGift.Services
{
    public class UserService
    {
        public List<dynamic> Get()
        {
            var result = new List<dynamic>();
            SessionManager.DoWork(ss =>
            {
                try
                {
                    result = ss.Query<User>()
                        .Select(p => (dynamic)new
                        {
                            p.Id,
                            p.UserName,
                            PositionCode = p.Position.Code,
                            PositionName = p.Position.Name,
                            OrganizationCode = p.Organization.Code,
                            OrganizationName = p.Organization.Name,
                            p.Email,
                            p.FullName,
                            p.Status,
                            p.MonthId
                        }).ToList();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            });

            return result;
        }

        public string Post(UserDTO obj)
        {
            var result = string.Empty;
            SessionManager.DoWork(ss =>
            {
                try
                {
                    if (!ss.Query<User>().Any(p => p.Email.ToString().ToLower() == obj.Email.ToString().ToLower()))
                    {
                        if (!ss.Query<User>().Any(p => p.UserName.ToString().ToLower() == obj.UserName.ToString().ToLower()))
                        {
                            var position = ss.Get<Position>((obj.PositionId == "nhan-vien" ? Guid.Parse("0179CD27-F7AC-4042-8685-6D3B694A6954") : Guid.Parse("803424CE-EA9C-4503-8B28-AAC000910ADB")));
                            var organization = ss.Query<Organization>().Single(p => p.Id == obj.OrganizationId);
                            ss.Save(new User
                            {
                                Id = Guid.NewGuid(),
                                UserName = obj.UserName,
                                Password = obj.Password,
                                Position = position,
                                Organization = organization,
                                Email = obj.Email,
                                FullName = obj.FullName,
                                Status = obj.Status,
                                MonthId = int.Parse(DateTime.UtcNow.ToString("yyyyMM")),
                                PermisionId = obj.PermisionId
                            });
                            result = "Thành công";
                        }
                        else
                        {
                            result = $"{obj.UserName} đã được sử dụng!\nAnh/Chị vui lòng kiểm tra lại.";
                        }

                    }
                    else
                    {
                        result = $"{obj.Email} đã được sử dụng!\nAnh/Chị vui lòng kiểm tra lại.";
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    result = ex.Message;
                }
            });
            return result;
        }

        public string Put(UserDTO obj)
        {
            var result = string.Empty;
            SessionManager.DoWork(ss =>
            {
                try
                {
                    var user = ss.Query<User>().SingleOrDefault(p => p.Id == obj.Id);
                    if (user != null)
                    {
                        var position = ss.Get<Position>((obj.PositionId == "nhan-vien" ? Guid.Parse("0179CD27-F7AC-4042-8685-6D3B694A6954") : Guid.Parse("803424CE-EA9C-4503-8B28-AAC000910ADB")));
                        var organization = ss.Query<Organization>().Single(p => p.Id == obj.OrganizationId);
                        user.UserName = obj.UserName;
                        user.Position = position;
                        user.Organization = organization;
                        user.FullName = obj.FullName;
                        user.Email = obj.Email;
                        user.Status = obj.Status;
                        user.PermisionId = obj.PermisionId;
                        ss.Update(user);
                        result = "Cập nhật thành công";
                    }
                    else
                    {
                        result = $"{obj.UserName} không tồn tại!\nAnh/Chị vui lòng kiểm tra lại.";
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    result = ex.Message;
                }
            });
            return result;
        }

        public string Delete(Guid id)
        {
            var result = string.Empty;
            SessionManager.DoWork(ss =>
            {
                try
                {
                    var obj = ss.Get<User>(id);
                    if (obj.UserName == "nva" || obj.UserName == "admin")
                    {
                        result = "Bạn không thể xóa. Vì user này là user hệ thống.";
                    }
                    else
                    {
                        if (obj.IsUser)
                        {
                            result = "Bạn không thể xóa. Vì user này đã có thao tác trên hệ thống.";
                        }
                        else
                        {
                            if (IsUser(obj.Id))
                            {
                                obj.IsUser = true;
                                result = "Bạn không thể xóa. Vì user này đã có thao tác trên hệ thống.";
                            }
                            else
                            {
                                ss.Delete(obj);
                                //Xóa tất cả các log pass theo user này
                                var userPassLog = ss.Query<UserLogPassword>().Where(w => w.UserId == id).ToList();
                                userPassLog.ForEach(itm =>
                                {
                                    ss.Delete(itm);
                                });
                                result = "Đã xóa";
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    result = ex.Message;
                }
            });
            return result;
        }
        /// <summary>
        /// Check user đó đã có thao tác trên hệ thống chưa?
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool IsUser(Guid id)
        {
            var result = false;
            SessionManager.DoWork(ss =>
            {
                try
                {
                    if (ss.Query<Gift>().FirstOrDefault(p => p.CreatedBy == id || p.UpdatedBy == id) != null)
                        result = true;
                    else if (ss.Query<GiftGroup>().FirstOrDefault(p => p.CreatedBy == id || p.UpdatedBy == id) != null)
                        result = true;
                    else if (ss.Query<OptionGift>().FirstOrDefault(p => p.CreatedBy == id || p.UpdatedBy == id) != null)
                        result = true;
                    else if (ss.Query<OptionGift>().FirstOrDefault(p => p.CreatedBy == id || p.UpdatedBy == id) != null)
                        result = true;
                    else if (ss.Query<Promotion>().FirstOrDefault(p => p.CreatedBy == id || p.NguoiDuyet == id) != null)
                        result = true;
                    else if (ss.Query<TransferGift>().FirstOrDefault(p => p.CreatedBy == id || p.NguoiDuyet == id) != null)
                        result = true;
                    else if (ss.Query<TransferUserLog>().FirstOrDefault(p => p.UserTransfer == id) != null)
                        result = true;
                    else if (ss.Query<Unit>().FirstOrDefault(p => p.CreatedBy == id || p.UpdatedBy == id) != null)
                        result = true;
                    else
                        result = false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            });
            return result;
        }
        public bool IsDuplicate(string stringName, string type)
        {
            var result = false;
            SessionManager.DoWork(ss =>
            {
                try
                {
                    if (ss.Query<User>().SingleOrDefault(p => p.UserName == stringName && type == "username") != null)
                        result = true;
                    if (ss.Query<User>().SingleOrDefault(p => p.UserName == stringName && type == "email") != null)
                        result = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            });
            return result;
        }

        public string RefreshPassword(string username)
        {
            var result = string.Empty;
            SessionManager.DoWork(ss =>
            {
                try
                {
                    var user = ss.Query<User>().Single(p => p.UserName == username);
                    user.Password = "Abcd@1234";
                    ss.Update(user);
                    result = "Refresh password success: Abcd@1234";
                }
                catch (Exception ex)
                {
                    result = ex.Message;
                }
            });
            return result;
        }

        public string ChangePassword(string username, string password)
        {
            var result = string.Empty;
            SessionManager.DoWork(ss =>
            {
                try
                {
                    var user = ss.Query<User>().Single(p => p.UserName == username);
                    user.Password = password;
                    ss.Update(user);
                    result = "Change password success";
                }
                catch (Exception ex)
                {
                    result = ex.Message;
                }
            });
            return result;
        }

        public List<dynamic> GetAllUseOfOrg(Guid organizationId)
        {
            var _organizationService = new OrganizationService();
            var lstResult = new List<dynamic>();
            try
            {
                SessionManager.DoWork(ss =>
                {
                    var lstOrgs = ss.Query<Organization>().ToList();
                    var lstIds = new List<Guid>();
                    _organizationService.RecursionGetLstIds(lstOrgs, lstIds, organizationId);
                    lstResult = ss.Query<User>()
                        .Where(p => lstIds.Contains(p.Organization.Id))
                        .Select(p => (dynamic)new
                        {
                            p.Id,
                            p.MonthId,
                            p.UserName,
                            p.FullName,
                            p.Email,
                            PositionCode = p.Position.Code,
                            PositionName = p.Position.Name,
                            OrganizationCode = p.Organization.Code,
                            OrganizationName = p.Organization.Name,
                            p.Status
                        }).ToList();
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return lstResult;
        }

        public bool SetStatus(string username, bool status)
        {
            var result = false;
            try
            {
                SessionManager.DoWork(ss =>
                {
                    var use = ss.Query<User>().Single(p => p.UserName == username);
                    use.Status = status;
                    ss.Update(use);
                });
            }
            catch (Exception ex)
            {

            }
            return result;
        }
        public dynamic GetUserFromId(Guid id)
        {
            dynamic result = new ExpandoObject();
            SessionManager.DoWork(ss =>
            {
                try
                {
                    var permissions = ss.Query<SysPermision>().ToList();
                    var users = ss.Query<User>().Where(w => w.Id == id).ToList();
                    var item = (from user in users
                             join permission in permissions on user.PermisionId equals permission.PermisionId
                             select new
                             {
                                 user.Id,
                                 user.UserName,
                                 PositionCode = user.Position.Code,
                                 PositionName = user.Position.Name,
                                 OrganizationId = user.Organization.Id,
                                 OrganizationCode = user.Organization.Code,
                                 OrganizationName = user.Organization.Name,
                                 user.PermisionId,
                                 PermisionName = permission.PermisionName,
                                 user.Email,
                                 user.FullName,
                                 user.Status,
                                 IsLeader = user.Position.IsLeader,
                                 user.MonthId
                             });
                    result.User = item.ToList().FirstOrDefault();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            });

            return result;
        }

        public string UpdateUser(UserDTO obj)
        {
            var result = string.Empty;
            SessionManager.DoWork(ss =>
            {
                try
                {
                    var user = ss.Query<User>().SingleOrDefault(p => p.Id == obj.Id);
                    if (user != null)
                    {
                        var position = ss.Get<Position>((obj.PositionCode == "Staff" ? Guid.Parse("0179CD27-F7AC-4042-8685-6D3B694A6954") : Guid.Parse("803424CE-EA9C-4503-8B28-AAC000910ADB")));
                        var organization = ss.Query<Organization>().Single(p => p.Id == obj.OrganizationId);
                        user.Position = position;
                        user.Organization = organization;
                        user.FullName = obj.FullName;
                        user.Status = obj.Status;
                        user.PermisionId = obj.PermisionId;
                        ss.Update(user);
                        result = "Cập nhật thành công";
                    }
                    else
                    {
                        result = $"{obj.UserName} không tồn tại!\nAnh/Chị vui lòng kiểm tra lại.";
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    result = ex.Message;
                }
            });
            return result;
        }
    }
}
