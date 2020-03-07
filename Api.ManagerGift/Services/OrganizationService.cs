using Api.ManagerGift.DTO;
using Api.ManagerGift.Entities;
using Api.ManagerGift.Sessions;
using System;
using AutoMapper;
using System.Collections.Generic;
using System.Linq;

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
                    var lstOrgs = ss.Query<Organization>().ToList();
                    var lstRoots = lstOrgs.Where(p => p.ParentId == null).ToList();
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
                            Region = obj.Region
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
            SessionManager.DoWork(ss =>
            {
                try
                {
                    var obj = ss.Get<Organization>(id);
                    ss.Delete(obj);
                    result = "Delete success";
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
            var items = Mapper.Map<List<NewOrganizationDTO>>(lstOrg.Where(p => p.ParentId == currentItem.Id).ToList());
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

        public List<dynamic> GetAll()
        {
            var lstResults = new List<dynamic>();
            try
            {
                SessionManager.DoWork(ss => {
                    lstResults = ss.Query<Organization>()
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
    }
}
