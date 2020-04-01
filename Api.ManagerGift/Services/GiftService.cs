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
    public class GiftService
    {
        public dynamic Get(int pageNo, int pageSize, string textSearch, string typeGift, string groupGift)
        {
            dynamic lstResults = new ExpandoObject();
            textSearch = string.IsNullOrWhiteSpace(textSearch) ? "" : textSearch;
            SessionManager.DoWork(ss =>
            {
                try
                {
                    var lstUser = ss.Query<User>().ToList();
                    var lstGift = ss.QueryOver<Gift>()
                        .Where(p => p.Name.IsLike(textSearch, MatchMode.Anywhere)).OrderBy(p => p.CreatedDate).Desc.List();
                    if (!string.IsNullOrEmpty(typeGift) && lstGift.Count > 0)
                        lstGift = lstGift.Where(p => p.GiftGroup.OptionGift.Name == typeGift).ToList();
                    if (!string.IsNullOrEmpty(groupGift) && lstGift.Count > 0)
                        lstGift = lstGift.Where(p => p.GiftGroup.Name == groupGift).ToList();
                    lstResults.ListGiftOutput =
                        lstGift.Skip((pageNo - 1) * pageSize).Take(pageSize)
                        .Select(p => new
                        {
                            p.Id,
                            p.Code,
                            p.Name,
                            GiftGroupId = p.GiftGroup.Id,
                            GiftGroupCode = p.GiftGroup.Code,
                            GiftGroupName = p.GiftGroup.Name,
                            OptionGiftId = p.GiftGroup.OptionGift.Id,
                            OptionGiftCode = p.GiftGroup.OptionGift.Code,
                            OptionGiftName = p.GiftGroup.OptionGift.Name,
                            UnitId = p.Unit.Id,
                            UnitCode = p.Unit.Code,
                            UnitName = p.Unit.Name,
                            p.Price,
                            CreatedBy = ContextProvider.GetFullName(lstUser, p.CreatedBy),
                            CreatedDate = ContextProvider.GetConvertDatetime(p.CreatedDate),
                            UpdatedBy = ContextProvider.GetFullName(lstUser, p.UpdatedBy),
                            UpdatedDate = ContextProvider.GetConvertDatetime(p.UpdatedDate)
                        }).ToList();
                    var total = lstGift.Count();
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
                    lstResults = ss.Query<Gift>()
                                    .Select(p => (dynamic)new
                                    {
                                        p.Id,
                                        p.Code,
                                        p.Name,
                                        GiftGroupId = p.GiftGroup.Id,
                                        GiftGroupCode = p.GiftGroup.Code,
                                        GiftGroupName = p.GiftGroup.Name,
                                        OptionGiftId = p.GiftGroup.OptionGift.Id,
                                        OptionGiftCode = p.GiftGroup.OptionGift.Code,
                                        OptionGiftName = p.GiftGroup.OptionGift.Name,
                                        UnitId = p.Unit.Id,
                                        UnitCode = p.Unit.Code,
                                        UnitName = p.Unit.Name,
                                        p.Price,
                                        value = p.Name,
                                        label = p.Code
                                    }).ToList();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            });
            return lstResults;
        }
        public string Post(GiftDTO obj, ClaimsPrincipal principal)
        {
            var result = string.Empty;
            SessionManager.DoWork(ss =>
            {
                try
                {

                    if (ss.Query<Gift>().SingleOrDefault(p => p.Code == obj.Code) == null)
                    {
                        var userDTO = ContextProvider.GetUserInfo(principal);
                        var giftgroup = ss.Get<GiftGroup>(obj.GiftGroupId);
                        var unit = ss.Get<Unit>(obj.UnitId);
                        ss.Save(new Gift
                        {
                            Id = Guid.NewGuid(),
                            Code = obj.Code,
                            Name = obj.Name,
                            GiftGroup = giftgroup,
                            Unit = unit,
                            Price = obj.Price,
                            CreatedBy = userDTO.Id,
                            CreatedDate = DateTime.ParseExact(DateTime.Now.ToString("u"), "u", CultureInfo.InvariantCulture),
                            Status = true
                        });
                        result = "Thành công";
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
        public string Put(GiftDTO obj, ClaimsPrincipal principal)
        {
            var result = string.Empty;
            SessionManager.DoWork(ss =>
            {
                try
                {
                    var gift = ss.Query<Gift>().SingleOrDefault(p => p.Id == obj.Id);
                    if (gift != null)
                    {
                        var userDTO = ContextProvider.GetUserInfo(principal);
                        var giftgroup = ss.Get<GiftGroup>(obj.GiftGroupId);
                        var unit = ss.Get<Unit>(obj.UnitId);
                        gift.Code = obj.Code;
                        gift.Name = obj.Name;
                        gift.GiftGroup = giftgroup;
                        gift.Unit = unit;
                        gift.Price = obj.Price;
                        gift.UpdatedBy = userDTO.Id;
                        gift.UpdatedDate = DateTime.ParseExact(DateTime.Now.ToString("u"), "u", CultureInfo.InvariantCulture);
                        ss.Update(gift);
                        result = "Cập nhật thành công";
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
            SessionManager.DoWork(ss =>
            {
                try
                {
                    var obj = ss.Get<Gift>(id);
                    ss.Delete(obj);
                    result = "Đã xóa";
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    result = ex.Message;
                }
            });
            return result;
        }
        public bool IsDuplicate(string code)
        {
            var result = false;
            SessionManager.DoWork(ss =>
            {
                try
                {
                    if (ss.Query<Gift>().SingleOrDefault(p => p.Code == code) != null)
                        result = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            });
            return result;
        }

        public dynamic GetDetail(Guid id)
        {
            dynamic result = new ExpandoObject();
            try
            {
                SessionManager.DoWork(ss =>
                {
                    var lstUser = ss.Query<User>().ToList();
                    var gift = ss.Get<Gift>(id);
                    result.Id = gift.Id;
                    result.Code = gift.Code;
                    result.Name = gift.Name;
                    result.GiftGroupId = gift.GiftGroup.Id;
                    result.GiftGroupCode = gift.GiftGroup.Code;
                    result.GiftGroupName = gift.GiftGroup.Name;
                    result.OptionGiftId = gift.GiftGroup.OptionGift.Id;
                    result.OptionGiftCode = gift.GiftGroup.OptionGift.Code;
                    result.OptionGiftName = gift.GiftGroup.OptionGift.Name;
                    result.Price = gift.Price;
                    result.UnitId = gift.Unit.Id;
                    result.UnitCode = gift.Unit.Code;
                    result.UnitName = gift.Unit.Name;
                    result.CreatedBy = ContextProvider.GetFullName(lstUser, gift.CreatedBy);
                    result.CreatedDate = gift.CreatedDate;
                    result.UpdatedDate = gift.UpdatedDate;
                    result.UpdatedBy = ContextProvider.GetFullName(lstUser, gift.UpdatedBy);
                });
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }

        public dynamic GetGift()
        {
            dynamic result = new ExpandoObject();
            try
            {
                SessionManager.DoWork(ss =>
                {
                    var giftIdInStore = ss.Query<Store>().Where(s => s.DepartmentId == new Guid(Constants.ID_PHONG_QUAN_LY_BAN_HANG)
                        && s.PromotionId == null
                        && s.Amount > 0).Select(p => new
                        {
                            p.GiftId
                        }).ToList();
                    if (giftIdInStore.Count > 0)
                    {
                        var gift = ss.Query<Gift>().ToList();
                        var query = (from p in gift
                                     join _giftIdInStore in giftIdInStore
                                        on p.Id equals _giftIdInStore.GiftId
                                        select new
                                        {
                                            p.Id,
                                            p.Code,
                                            p.Name,
                                            GiftGroupId = p.GiftGroup.Id,
                                            GiftGroupCode = p.GiftGroup.Code,
                                            GiftGroupName = p.GiftGroup.Name,
                                            OptionGiftId = p.GiftGroup.OptionGift.Id,
                                            OptionGiftCode = p.GiftGroup.OptionGift.Code,
                                            OptionGiftName = p.GiftGroup.OptionGift.Name,
                                            UnitId = p.Unit.Id,
                                            UnitCode = p.Unit.Code,
                                            UnitName = p.Unit.Name,
                                            p.Price,
                                            value = p.Name,
                                            label = p.Code
                                        }
                                     ).ToList();
                        result = query;
                    }
                });
            }
            catch (Exception ex)
            {
                result = ex;
            }
            return result;
        }
    }
}
