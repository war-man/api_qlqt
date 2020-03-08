using Api.ManagerGift.Entities;
using Api.ManagerGift.Sessions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.ManagerGift.Services
{
    public class PositionService
    {
        public List<Position> Get()
        {
            var result = new List<Position>();
            SessionManager.DoWork(ss => {
                try {
                    result = ss.Query<Position>().ToList();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            });
            
            return result;
        }

        public string Post(Position obj)
        {
            var result = string.Empty;
            SessionManager.DoWork(ss =>
            {
                try
                {
                    if (ss.Query<Position>().SingleOrDefault(p => p.Code == obj.Code) == null)
                    {
                        ss.Save(new Position
                        {
                            Id = Guid.NewGuid(),
                            Code = obj.Code,
                            Name = obj.Name,
                            IsLeader = obj.IsLeader
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

        public string Put(Position obj)
        {
            var result = string.Empty;
            SessionManager.DoWork(ss => {
                try
                {
                    var position = ss.Query<Position>().SingleOrDefault(p => p.Id == obj.Id);
                    if (position != null)
                    {
                        position.Code = obj.Code;
                        position.Name = obj.Name;
                        position.IsLeader = obj.IsLeader;
                        ss.Update(position);
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
            SessionManager.DoWork(ss => {
                try
                {
                    var obj = ss.Get<Position>(id);
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
            SessionManager.DoWork(ss => {
                try
                {
                    if (ss.Query<Position>().SingleOrDefault(p => p.Code == code) != null)
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
