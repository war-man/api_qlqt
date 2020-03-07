using Api.ManagerGift.Entities;
using Api.ManagerGift.Sessions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.ManagerGift.ClassT
{
    public abstract class TestTClass <T> where T : new()
    {
        public virtual List<T> Get(int pageNo, int pageSize, string textSearch)
        {
            var lstResults = new List<T>();
            SessionManager.DoWork(ss => {
                //lstResults = ss.Query<T>().Where(p => p.Name == textSearch).Skip((pageNo - 1) * pageSize).Take(10).ToList();
            });
            return lstResults;
        }
    }
}
