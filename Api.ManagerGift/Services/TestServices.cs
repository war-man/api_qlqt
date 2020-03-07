using Api.ManagerGift.Entities;
using Api.ManagerGift.Sessions;
using System.Collections.Generic;
using System.Linq;

namespace Api.ManagerGift.Services
{
    public class TestServices
    {
        public List<Unit> GetAllUnits()
        {
            var lstResults = new List<Unit>();
            SessionManager.DoWork(ss => {
                lstResults = ss.Query<Unit>().ToList();
            });
            return lstResults;
        }

        public List<User> GetAllUsers()
        {
            var lstResults = new List<User>();
            SessionManager.DoWork(ss => {
                lstResults = ss.Query<User>().ToList();
            });
            return lstResults;
        }
    }
}
