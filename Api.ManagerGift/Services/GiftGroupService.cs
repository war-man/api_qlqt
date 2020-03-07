using Api.ManagerGift.DTO;
using Api.ManagerGift.Entities;
using Api.ManagerGift.Sessions;
using NHibernate.Criterion;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;

namespace Api.ManagerGift.Services
{
    public class GiftGroupService
    {
        public dynamic Get(int pageNo, int pageSize, string textSearch)
        {
            dynamic lstResults = new ExpandoObject();
            textSearch = string.IsNullOrWhiteSpace(textSearch) ? "" : textSearch;
            SessionManager.DoWork(ss =>
            {
                try
                {
                    var lstUser = ss.Query<User>().ToList();
                    var lstGG = ss.QueryOver<GiftGroup>()
                        .Where(p => p.Name.IsLike(textSearch, MatchMode.Anywhere)
                            || p.Code.IsLike(textSearch, MatchMode.Anywhere)).OrderBy(p => p.Name).Asc.List();

                    lstResults.ListGiftGroupOutput =
                        lstGG.Skip((pageNo - 1) * pageSize).Take(pageSize)
                        .Select(p => new
                        {
                            p.Id,
                            p.Code,
                            p.Name,
                            OptionGiftId = p.OptionGift.Id,
                            OptionGiftCode = p.OptionGift.Code,
                            OptionGiftName = p.OptionGift.Name,
                            CreatedBy = ContextProvider.GetFullName(lstUser, p.CreatedBy),
                            CreatedDate = ContextProvider.GetConvertDatetime(p.CreatedDate),
                            UpdatedBy = ContextProvider.GetFullName(lstUser, p.UpdatedBy),
                            UpdatedDate = ContextProvider.GetConvertDatetime(p.UpdatedDate),
                            p.Status
                        }).ToList();
                    var total = lstGG.Count();
                    lstResults.TotalPage = total % pageSize == 0 ? total / pageSize : total / pageSize + 1;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            });
            return lstResults;
        }
        public List<dynamic> Get()
        {
            var lstResults = new List<dynamic>();
            SessionManager.DoWork(ss =>
            {
                try
                {
                    lstResults = ss.Query<GiftGroup>()
                                    .Select(p => (dynamic)new
                                    {
                                        p.Id,
                                        p.Code,
                                        p.Name,
                                        OptionGiftId = p.OptionGift.Id,
                                        OptionGiftCode = p.OptionGift.Code,
                                        OptionGiftName = p.OptionGift.Name,
                                        value = p.Id,
                                        label = p.Name
                                    }).ToList();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            });
            return lstResults;
        }
        public string Post(GiftGroupDTO obj, ClaimsPrincipal principal)
        {
            var result = string.Empty;
            SessionManager.DoWork(ss =>
            {
                try
                {
                    if (ss.Query<GiftGroup>().SingleOrDefault(p => p.Code == obj.Code) == null)
                    {
                        var userDTO = ContextProvider.GetUserInfo(principal);
                        var optiongift = ss.Get<OptionGift>(obj.OptionGiftId);
                        ss.Save(new GiftGroup
                        {
                            Id = Guid.NewGuid(),
                            Code = obj.Code,
                            Name = obj.Name,
                            OptionGift = optiongift,
                            CreatedBy = userDTO.Id,
                            CreatedDate = DateTime.ParseExact(DateTime.Now.ToString("u"), "u", CultureInfo.InvariantCulture),
                            Status = true
                        });
                        result = "Add success";
                    }
                    else
                    {
                        result = $"{obj.Code} đã được sử dụng!\nAnh/Chị vui lòng kiểm tra lại.";
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
        public string Put(GiftGroupDTO obj, ClaimsPrincipal principal)
        {
            var result = string.Empty;
            SessionManager.DoWork(ss =>
            {
                try
                {
                    var giftgroup = ss.Query<GiftGroup>().SingleOrDefault(p => p.Id == obj.Id);
                    if (giftgroup != null)
                    {
                        var userDTO = ContextProvider.GetUserInfo(principal);
                        var optiongift = ss.Get<OptionGift>(obj.OptionGiftId);
                        giftgroup.Code = obj.Code;
                        giftgroup.Name = obj.Name;
                        giftgroup.OptionGift = optiongift;
                        giftgroup.UpdatedBy = userDTO.Id;
                        giftgroup.UpdatedDate = DateTime.ParseExact(DateTime.Now.ToString("u"), "u", CultureInfo.InvariantCulture);
                        ss.Update(giftgroup);
                        result = "Edit success";
                    }
                    else
                    {
                        result = $"{obj.Code} không tồn tại!\nAnh/Chị vui lòng kiểm tra lại.";
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
            try
            {
                SessionManager.DoWork(ss =>
                {
                    var obj = ss.Get<GiftGroup>(id);
                    ss.Delete(obj);
                    result = "Delete success";
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                result = "Bạn không thể xóa nhóm quà tặng đã gắn với quà tặng.";
            }
            return result;
        }
        public bool IsDuplicate(string code)
        {
            var result = false;
            SessionManager.DoWork(ss =>
            {
                try
                {
                    if (ss.Query<GiftGroup>().SingleOrDefault(p => p.Code == code) != null)
                        result = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            });
            return result;
        }
        public List<dynamic> GetGroupOfOption(Guid OptionGiftId)
        {
            var lstResult = new List<dynamic>();
            try
            {
                SessionManager.DoWork(ss =>
                {
                    lstResult = ss.Query<GiftGroup>()
                        .Where(p => p.OptionGift.Id == OptionGiftId)
                        .Select(p => (dynamic)new
                        {
                            p.Id,
                            p.Code,
                            p.Name,
                            OptionGiftId = p.OptionGift.Id,
                            OptionGiftCode = p.OptionGift.Code,
                            OptionGiftName = p.OptionGift.Name,
                            value = p.Id,
                            label = p.Name
                        }).ToList();
                });
            }
            catch (Exception ex)
            {

            }
            return lstResult;
        }
    }
}
