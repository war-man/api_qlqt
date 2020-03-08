using Api.ManagerGift.Entities;
using Api.ManagerGift.Sessions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Security.Claims;

namespace Api.ManagerGift.Services
{
    public class PermisionService
    {
        public dynamic Get(string url, ClaimsPrincipal principal)
        {
            dynamic result = new ExpandoObject();
            try
            {
                SessionManager.DoWork(ss =>
                {
                    var userinfo = ContextProvider.GetUserInfo(principal);
                    var permisionDetail = ss.Query<PermisionDetail>().Where(s => s.Url == url && s.CheckAction == true).ToList();
                    var user = ss.Query<User>().Where(s => s.Id == userinfo.Id).ToList();
                    var permision = (from _permisionDetail in permisionDetail
                                     join _user in user on _permisionDetail.PermisionId equals _user.PermisionId
                                     select new
                                     {
                                         _permisionDetail.CheckAction,
                                         _permisionDetail.ActionName,
                                         _permisionDetail.ActionCode
                                     });
                    result = permision.ToList();
                });
            }
            catch (Exception)
            {

            }
            return result;
        }

        public DataTable GetAll()
        {
            var data = new DataTable();
            try
            {
                SessionManager.DoWork(ss =>
                {
                    var allPermision = ss.Query<PermisionDetail>().ToList();
                    var lstUrl = allPermision.Select(s => (dynamic)new { s.Url }).Distinct().ToList();

                    ConvertToDataTable(allPermision, lstUrl, out data);
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return data;
        }

        public DataTable GetPer(string url) 
        {
            var data = new DataTable();
            try
            {
                SessionManager.DoWork(ss =>
                {
                    var idParend = ss.Query<PermisionDetail>().Where(s=>s.Url == url).First().Id;
                    var permisionChild = ss.Query<PermisionDetail>().Where(s => s.ParentId == idParend).ToList();
                    var lstUrl = permisionChild.Select(s => (dynamic)new { s.Url }).Distinct().ToList();

                    ConvertToDataTable(permisionChild, lstUrl, out data);
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return data;
        }

        public string EditPermision(PermisionDetail per, ClaimsPrincipal principal)
        {
            var result = string.Empty;
            var userinfo = ContextProvider.GetUserInfo(principal);
            if (userinfo.PermisionId == 1)
            {
                try
                {
                    SessionManager.DoWork(ss =>
                    {
                        var permision = ss.Query<PermisionDetail>().Where(s => s.Id == per.Id).FirstOrDefault();
                        if (permision != null)
                        {
                            permision.CheckAction = !per.CheckAction;
                            result = "Cập nhật thành công";
                        }
                    });
                }
                catch (Exception)
                {
                    throw;
                }
            }
            else
            {
                result = "Bạn không có quyền 'phân quyền chức năng.'";
            }
            
            return result;
        }

        #region private
        private static void ConvertToDataTable(List<PermisionDetail> dataInput, List<dynamic> lstUrl, out DataTable data)
        {
            var tbl = new DataTable();
            tbl.Columns.Add(new DataColumn("Id", typeof(string)));
            tbl.Columns.Add(new DataColumn("ParentId", typeof(string)));
            tbl.Columns.Add(new DataColumn("NavName", typeof(string)));
            tbl.Columns.Add(new DataColumn("Permision1", typeof(List<PermisionDetail>)));
            tbl.Columns.Add(new DataColumn("Permision2", typeof(List<PermisionDetail>)));
            tbl.Columns.Add(new DataColumn("Permision3", typeof(List<PermisionDetail>)));
            tbl.Columns.Add(new DataColumn("Permision4", typeof(List<PermisionDetail>)));
            tbl.Columns.Add(new DataColumn("Permision5", typeof(List<PermisionDetail>)));
            tbl.Columns.Add(new DataColumn("Permision6", typeof(List<PermisionDetail>)));
            tbl.Columns.Add(new DataColumn("Permision7", typeof(List<PermisionDetail>)));

            foreach (var itm in lstUrl)
            {
                var _dataInput = dataInput.Where(s => s.Url == itm.Url).ToList();
                var navName = _dataInput.FirstOrDefault()?.NavName ?? "";
                var id = _dataInput.FirstOrDefault().Id;
                var parentId = _dataInput.FirstOrDefault().ParentId;
                AddToDataTable(tbl, _dataInput, navName, id, parentId);

            }

            data = tbl;
        }

        private static void AddToDataTable(DataTable tbl, List<PermisionDetail> dataInput, string navName, int id, int parentId)
        {
            var newRow = tbl.NewRow();

            var lst1 = new List<PermisionDetail>();
            var lst2 = new List<PermisionDetail>();
            var lst3 = new List<PermisionDetail>();
            var lst4 = new List<PermisionDetail>();
            var lst5 = new List<PermisionDetail>();
            var lst6 = new List<PermisionDetail>();
            var lst7 = new List<PermisionDetail>();

            foreach (var itm in dataInput)
            {
                switch (itm.PermisionId)
                {
                    case 1:
                        lst1.Add(itm);
                        break;
                    case 2:
                        lst2.Add(itm);
                        break;
                    case 3:
                        lst3.Add(itm);
                        break;
                    case 4:
                        lst4.Add(itm);
                        break;
                    case 5:
                        lst5.Add(itm);
                        break;
                    case 6:
                        lst6.Add(itm);
                        break;
                    case 7:
                        lst7.Add(itm);
                        break;

                    default:
                        break;
                }
            }
            newRow["Id"] = id;
            newRow["ParentId"] = parentId;
            newRow["NavName"] = navName;
            newRow["Permision1"] = lst1;
            newRow["Permision2"] = lst2;
            newRow["Permision3"] = lst3;
            newRow["Permision4"] = lst4;
            newRow["Permision5"] = lst5;
            newRow["Permision6"] = lst6;
            newRow["Permision7"] = lst7;

            tbl.Rows.Add(newRow);
        }
        #endregion
    }
}