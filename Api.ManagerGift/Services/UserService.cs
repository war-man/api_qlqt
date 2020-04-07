using Api.ManagerGift.DTO;
using Api.ManagerGift.Entities;
using Api.ManagerGift.Sessions;
using System;
using System.Collections.Generic;
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
                                MonthId = int.Parse(DateTime.UtcNow.ToString("yyyyMM"))
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
                            result = "Đã xóa";
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
                    if (ss.Query<Gift>().SingleOrDefault(p => p.CreatedBy == id || p.UpdatedBy == id) != null)
                        result = true;
                    else if (ss.Query<GiftGroup>().SingleOrDefault(p => p.CreatedBy == id || p.UpdatedBy == id) != null)
                        result = true;
                    else if (ss.Query<OptionGift>().SingleOrDefault(p => p.CreatedBy == id || p.UpdatedBy == id) != null)
                        result = true;
                    else if (ss.Query<OptionGift>().SingleOrDefault(p => p.CreatedBy == id || p.UpdatedBy == id) != null)
                        result = true;
                    else if (ss.Query<Promotion>().SingleOrDefault(p => p.CreatedBy == id || p.NguoiDuyet == id) != null)
                        result = true;
                    else if (ss.Query<TransferGift>().SingleOrDefault(p => p.CreatedBy == id || p.NguoiDuyet == id) != null)
                        result = true;
                    else if (ss.Query<TransferUserLog>().SingleOrDefault(p => p.UserTransfer == id) != null)
                        result = true;
                    else if (ss.Query<Unit>().SingleOrDefault(p => p.CreatedBy == id || p.UpdatedBy == id) != null)
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
                    user.Password = "123456";
                    ss.Update(user);
                    result = "Refresh password success";
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
        public object GetUserFromId(Guid id)
        {
            var result = new object();
            SessionManager.DoWork(ss =>
            {
                try
                {
                    result = ss.Query<User>().Where(w => w.Id == id)
                        .Select(p => (object)new
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
                            IsLeader = p.Position.IsLeader,
                            p.MonthId
                        }).FirstOrDefault();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            });

            return result;
        }
    }
}
