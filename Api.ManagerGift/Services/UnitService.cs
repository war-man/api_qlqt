using Api.ManagerGift.Entities;
using Api.ManagerGift.Sessions;
using NHibernate.Criterion;
using NHibernate.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Api.ManagerGift.Services
{
    public class UnitService
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
                    var lstUnit = ss.QueryOver<Unit>()
                        .Where(p => p.Name.IsLike(textSearch, MatchMode.Anywhere) 
                            || p.Code.IsLike(textSearch, MatchMode.Anywhere)).OrderBy(p => p.Name).Asc.List();
                    lstResults.ListUnitOutput =
                        lstUnit.Skip((pageNo - 1) * pageSize).Take(pageSize)
                        .Select(p => new
                        {
                            p.Id,
                            p.Code,
                            p.Name,
                            CreatedBy = ContextProvider.GetFullName(lstUser, p.CreatedBy),
                            CreatedDate = ContextProvider.GetConvertDatetime(p.CreatedDate),
                            UpdatedBy = ContextProvider.GetFullName(lstUser, p.UpdatedBy),
                            UpdatedDate = ContextProvider.GetConvertDatetime(p.UpdatedDate)
                        }).ToList();
                    lstResults.Total = lstUnit.Count();
                    var total = lstUnit.Count();
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
                    lstResults = ss.Query<Unit>()
                                    .Select(p => (dynamic)new
                                    {
                                        p.Id,
                                        p.Code,
                                        p.Name,
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
        public string Post(Unit obj, ClaimsPrincipal principal)
        {
            var result = string.Empty;
            SessionManager.DoWork(ss =>
            {
                try
                {
                    //var userDTO = ContextProvider.Get();
                    var userDTO = ContextProvider.GetUserInfo(principal);
                    if (ss.Query<Unit>().SingleOrDefault(p => p.Code == obj.Code) == null)
                    {
                        ss.Save(new Unit
                        {
                            Id = Guid.NewGuid(),
                            Code = obj.Code,
                            Name = obj.Name,
                            CreatedBy = userDTO.Id,
                            CreatedDate = DateTime.ParseExact(DateTime.Now.ToString("u"), "u", CultureInfo.InvariantCulture),
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
        public string Put(Unit obj, ClaimsPrincipal principal)
        {
            var result = string.Empty;
            SessionManager.DoWork(ss =>
            {
                try
                {

                    var unit = ss.Query<Unit>().SingleOrDefault(p => p.Id == obj.Id);
                    if (unit != null)
                    {
                        //var userDTO = ContextProvider.Get();
                        var userDTO = ContextProvider.GetUserInfo(principal);
                        unit.Code = obj.Code;
                        unit.Name = obj.Name;
                        unit.UpdatedBy = userDTO.Id;
                        unit.UpdatedDate = DateTime.ParseExact(DateTime.Now.ToString("u"), "u", CultureInfo.InvariantCulture);
                        ss.Update(unit);
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
            try
            {
                SessionManager.DoWork(ss =>
                {
                    var obj = ss.Get<Unit>(id);
                    ss.Delete(obj);
                    result = "Đã xóa";
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                result = "Không cho phép xóa đơn vị tính đã gắn với quà tặng";
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
                    if (ss.Query<Unit>().SingleOrDefault(p => p.Code == code) != null)
                        result = true;
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
