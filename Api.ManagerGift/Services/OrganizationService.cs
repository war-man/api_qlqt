﻿using Api.ManagerGift.DTO;
using Api.ManagerGift.Entities;
using Api.ManagerGift.Sessions;
using System;
using AutoMapper;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Api.ManagerGift.Services
{
    public class OrganizationService
    {
        // dung cho OrganizationDTO
        //public List<OrganizationDTO> Get()
        //{
        //    var result = new List<OrganizationDTO>();
        //    SessionManager.DoWork(ss =>
        //    {
        //        try
        //        {
        //            var lstOrgs = ss.Query<Organization>().ToList();
        //            var lstRoots = lstOrgs.Where(p => p.ParentId == null).ToList();
        //            foreach (var item in lstRoots)
        //            {
        //                var currentItem = Mapper.Map<OrganizationDTO>(item);
        //                Recursion(lstOrgs, currentItem);
        //                result.Add(currentItem);
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine(ex.Message);
        //        }
        //    });

        //    return result;
        //}
        public List<NewOrganizationDTO> Get()
        {
            var result = new List<NewOrganizationDTO>();
            SessionManager.DoWork(ss =>
            {
                try
                {
                    var lstOrgs = ss.Query<Organization>().OrderByDescending(o=>o.CreateDate).ToList();
                    var lstRoots = lstOrgs.Where(p => p.ParentId == null).OrderByDescending(o => o.CreateDate).ToList();
                    foreach (var item in lstRoots)
                    {
                        var currentItem = Mapper.Map<NewOrganizationDTO>(item);
                        NewRecursion(lstOrgs, currentItem);
                        result.Add(currentItem);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            });

            return result;
        }
        public string Post(Organization obj)
        {
            var result = string.Empty;
            SessionManager.DoWork(ss =>
            {
                try
                {
                    if (ss.Query<Organization>().SingleOrDefault(p => p.Code == obj.Code) == null)
                    {
                        if (obj.ParentId != null)
                        {
                            var parent = ss.Get<Organization>(obj.ParentId);
                            obj.ManageCode = GetManagerCode(parent.ManageCode);
                        }
                        else
                        {
                            obj.ManageCode = "HO";
                        }
                        ss.Save(new Organization
                        {
                            Id = Guid.NewGuid(),
                            Name = obj.Name,
                            Code = obj.Code,
                            ParentId = obj.ParentId,
                            ManageCode = obj.ManageCode,
                            Address = obj.Address,
                            Region = obj.Region,
                            CreateDate = DateTime.Now
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
        public string Put(Organization obj)
        {
            var result = string.Empty;
            SessionManager.DoWork(ss =>
            {
                try
                {
                    var organization = ss.Query<Organization>().SingleOrDefault(p => p.Id == obj.Id);
                    if (organization != null)
                    {
                        var parent = ss.Get<Organization>(organization.ParentId);
                        organization.Code = obj.Code;
                        organization.Name = obj.Name;
                        organization.ParentId = obj.ParentId;
                        organization.ManageCode = GetManagerCode(parent.ManageCode);
                        organization.Address = obj.Address;
                        organization.Region = obj.Region;
                        ss.Update(organization);
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
                    var obj = ss.Get<Organization>(id);
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
                    if (ss.Query<Organization>().SingleOrDefault(p => p.Code == code) != null)
                        result = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            });
            return result;
        }
        private string GetManagerCode(string pmg)
        {
            var result = string.Empty;
            if (pmg == "HO")
                result = "CN";
            else if (pmg == "CN")
                result = "PGD";
            else if (pmg == "PGD")
                result = null;
            return result;
        }
        //private void Recursion(List<Organization> lstOrg, OrganizationDTO currentItem)
        //{
        //    var lstChilds = Mapper.Map<List<OrganizationDTO>>(lstOrg.Where(p => p.ParentId == currentItem.Id).ToList());
        //    currentItem.lstChilds = lstChilds;
        //    foreach (var item in lstChilds)
        //    {
        //        Recursion(lstOrg, item);
        //    }
        //}
        private void NewRecursion(List<Organization> lstOrg, NewOrganizationDTO currentItem)
        {
            var items = Mapper.Map<List<NewOrganizationDTO>>(lstOrg.Where(p => p.ParentId == currentItem.Id).OrderByDescending(o=>o.CreateDate).ToList());
            currentItem.items = items;
            foreach (var item in items)
            {
                NewRecursion(lstOrg, item);
            }
        }
        public void RecursionGetLstIds(List<Organization> lstOrg, List<Guid> lstResult, Guid currentId)
        {
            lstResult.Add(currentId);
            var lstChilds = Mapper.Map<List<OrganizationDTO>>(lstOrg.Where(p => p.ParentId == currentId).ToList());
            foreach (var item in lstChilds)
            {
                RecursionGetLstIds(lstOrg, lstResult, item.Id);
            }
        }
        public List<dynamic> GetAllBranchs()
        {
            var lstResults = new List<dynamic>();
            try
            {
                SessionManager.DoWork(ss => {
                    lstResults = ss.Query<Organization>().Where(p => p.ManageCode == "CN")
                        .Select(p => (dynamic)new {
                            p.Id,
                            p.Name,
                            p.Code,
                            p.ManageCode,
                            p.ParentId,
                            p.Address,
                            p.Region,
                            value = p.Id,
                            label = p.Name
                        }).ToList();
                });
            }
            catch(Exception ex)
            {

            }
            return lstResults;
        }
        public Organization GetDetailById(Guid id)
        {
            var result = new Organization();
            try
            {
                SessionManager.DoWork(ss =>
                {
                    result = ss.Get<Organization>(id);
                });
            }
            catch (Exception ex)
            {

            }
            return result;
        }

        public List<dynamic> GetAll(ClaimsPrincipal principal)
        {
            var lstResults = new List<dynamic>();
            var userinfo = ContextProvider.GetUserInfo(principal);
            var isTypeUser = ContextProvider.CheckPermission(userinfo.PermisionId);
            try
            {
                SessionManager.DoWork(ss => {
                    var orgs = ss.Query<Organization>().ToList();
                    if (isTypeUser == 3)
                        orgs = orgs.Where(w => w.ParentId == userinfo.Organization.Id).ToList();
                    lstResults = orgs.Select(p => (dynamic)new
                        {
                            p.Id,
                            p.Name,
                            p.Code,
                            p.ManageCode,
                            p.ParentId,
                            p.Address,
                            p.Region,
                            value = p.Id,
                            label = p.Name
                        }).ToList();
                });
            }
            catch (Exception ex)
            {

            }
            return lstResults;
        }

        public List<OrganizationDetailDTO> GetInfoDetail()
        {
            var lstResults = new List<OrganizationDetailDTO>();
            try
            {
                SessionManager.DoWork(ss => {
                    var lstOrganizations = ss.Query<Organization>().OrderBy(w => w.Name).ToList();

                    foreach (var item in lstOrganizations)
                    {
                        var parent = lstOrganizations.Where(w => w.Id == item.ParentId).FirstOrDefault();

                        var obj = Mapper.Map<OrganizationDetailDTO>(item);
                        obj.ParentName = parent != null ? parent.Name : "";
                        obj.RegionName = GetVungMien(item.Region);

                        if (item.ManageCode == "PGD")
                        {
                            obj.RegionName = GetVungMien(parent.Region);
                        }
                        lstResults.Add(obj);
                    }
                });
            }
            catch (Exception ex)
            {

            }
            return lstResults.OrderByDescending(od=>od.CreateDate).ToList();
        }
        public string GetVungMien(string code)
        {
            string regionName = "";
            if (code == "BAC")
                regionName = "Miền Bắc";
            else if (code == "TRUNG")
                regionName = "Miền Trung";
            else if (code == "NAM")
                regionName = "Miền Nam";

            return regionName;
        }

        public List<dynamic> GetBranchReport(ClaimsPrincipal principal)
        {
            var lstResults = new List<dynamic>();
            var userinfo = ContextProvider.GetUserInfo(principal);
            var isTypeUser = ContextProvider.CheckPermission(userinfo.PermisionId);
            try
            {
                SessionManager.DoWork(ss => {
                    var list = ss.Query<Organization>().Where(p => p.ManageCode == "CN").ToList();
                    if (isTypeUser != 1 && isTypeUser != 2)
                        list = list.Where(w => w.Id==userinfo.Organization.Id).ToList();
                    lstResults = list.Select(p => (dynamic)new
                        {
                            p.Id,
                            p.Name,
                            p.Code,
                            p.ManageCode,
                            p.ParentId,
                            p.Address,
                            p.Region,
                            value = p.Id,
                            label = p.Name
                        }).ToList();
                });
            }
            catch (Exception ex)
            {

            }
            return lstResults;
        }

        public List<dynamic> GetDepartmentReport(ClaimsPrincipal principal,string id)
        {
            var lstResults = new List<dynamic>();
            var userinfo = ContextProvider.GetUserInfo(principal);
            var isTypeUser = ContextProvider.CheckPermission(userinfo.PermisionId);
            try
            {
                SessionManager.DoWork(ss => {
                    var list = ss.Query<Organization>().Where(p => p.ManageCode == "PGD" && p.ParentId == new Guid(id)).ToList();
                    if (isTypeUser == 3)
                    {
                        if(userinfo.Position.IsLeader)
                            list = list.Where(w => w.ParentId == userinfo.Organization.Id || w.Id == userinfo.Organization.Id).ToList();
                        else
                            list = list.Where(w => w.Id == userinfo.Organization.Id).ToList();
                    }
                    lstResults = list
                        .Select(p => (dynamic)new
                        {
                            p.Id,
                            p.Name,
                            p.Code,
                            p.ManageCode,
                            p.ParentId,
                            p.Address,
                            p.Region,
                            value = p.Id,
                            label = p.Name
                        }).ToList();
                });
            }
            catch (Exception ex)
            {

            }
            return lstResults;
        }

        public List<dynamic> GetInUserLogin(ClaimsPrincipal principal)
        {
            var lstResults = new List<dynamic>();
            var userinfo = ContextProvider.GetUserInfo(principal);
            var isTypeUser = ContextProvider.CheckPermission(userinfo.PermisionId);
            try
            {
                SessionManager.DoWork(ss => {
                    var list = ss.Query<Organization>().ToList();
                    if (isTypeUser == 1 || isTypeUser == 2)
                        list = list.Where(p => p.ManageCode == "CN").ToList();
                    else
                        list = list.Where(p => p.ManageCode == "PGD" && (p.ParentId==userinfo.Organization.Id || p.Id == userinfo.Organization.Id)).ToList();
                    lstResults = list.Select(p => (dynamic)new
                    {
                        p.Id,
                        p.Name,
                        p.Code,
                        p.ManageCode,
                        p.ParentId,
                        p.Address,
                        p.Region,
                        value = p.Id,
                        label = p.Name
                    }).ToList();
                });
            }
            catch (Exception ex)
            {

            }
            return lstResults;
        }
    }
}
