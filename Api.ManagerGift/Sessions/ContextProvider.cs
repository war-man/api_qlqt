using Api.ManagerGift.DTO;
using Api.ManagerGift.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Api.ManagerGift.Sessions
{
    public static class ContextProvider
    {
        public enum statusTransfer
        {
            Draft,
            Initialize,
            Approve,
            Refuse,
            ApproveCN
        }
        public static int CheckPermission(int value)
        {
            int result = 0;
            if (value == 1)
                result = 1;//Admin
            else if (value == 2 || value == 3)
                result = 2;//Quản Lý BH
            else if (value == 6 || value == 7)
                result = 3;//CN
            else if (value == 8 || value == 9)
                result = 6;//PGD
            else if (value == 4)
                result = 4;//KSV
            else
                result = 5;//GDV
            return result;
        }
        public static string GetFullName(List<User> lstUser, Guid? Id)
        {
            if (Id != null)
                return lstUser.SingleOrDefault(p => p.Id == Id)?.FullName ?? "";
            else
                return "";
        }
        public static string GetNameNav(List<SysNav> lst, Guid Id)
        {
            if (Id != Guid.Empty)
                return lst.FirstOrDefault(p => p.Id == Id)?.Name ?? "";
            else
                return "";
        }
        public static string GetDonViTang(List<User> lstUser, Guid? Id)
        {
            if (Id != null)
                return lstUser.FirstOrDefault(p => p.Id == Id)?.Organization.Name ?? "";
            else
                return "";
        }

        public static string GiftName(List<Gift> lstGift, Guid? Id)
        {
            if (Id != null)
                return lstGift.SingleOrDefault(p => p.Id == Id)?.Name ?? "";
            else
                return "";
        }

        public static string GetOrganizationName(List<Organization> lst, Guid? Id)
        {
            if (Id != null)
                return lst.SingleOrDefault(p => p.Id == Id)?.Name ?? "";
            else
                return "";
        }
        public static string GetBranchParentName(List<Organization> lst, Guid? Id)
        {
            if (Id != null)
            {
                var parentid = lst.SingleOrDefault(p => p.Id == Id)?.ParentId ?? Guid.NewGuid();
                return lst.SingleOrDefault(p => p.Id == parentid)?.Name ?? "";
            }
            else
                return "";
        }

        public static string GetPromotionName(List<Promotion> lst, Guid? Id)
        {
            if (Id != null)
                return lst.SingleOrDefault(p => p.Id == Id)?.Name ?? "";
            else
                return "";
        }
        public static string GetPromotionCode(List<Promotion> lst, Guid? Id)
        {
            if (Id != null)
                return lst.SingleOrDefault(p => p.Id == Id)?.Code ?? "";
            else
                return "";
        }
        public static string GetConvertDatetime(DateTime? date)
        {
            if (date != null)
            {
                var d = date.GetValueOrDefault(DateTime.Now);
                return d.ToString("dd-MM-yyyy");
            }
            return "";
        }
        public static DateTime GetOrderDatetime(DateTime? date)
        {
            if (date != null)
            {
                var d = date.GetValueOrDefault(DateTime.Now);
                return d;
            }
            return DateTime.Now;
        }
        public static string GetConvertDatetimeDDMMYYYHHmm(DateTime? date)
        {
            if (date != null)
            {
                var d = date.GetValueOrDefault(DateTime.Now);
                return d.ToString("dd-MM-yyyy HH:mm");
            }
            return "";
        }
        public static UserDTO GetUserInfo(ClaimsPrincipal principal)
        {
            var result = new UserDTO();
            if(principal.HasClaim(p => p.Type == "userinfo"))
            {
                var userinfo = principal.Claims.Single(p => p.Type == "userinfo").Value;
                result = JsonConvert.DeserializeObject<UserDTO>(userinfo);
            }
            return result;
        }

        public static string GetStateName(List<Stage> lstStage, Guid? Id)
        {
            if (Id != null)
                return lstStage.SingleOrDefault(p => p.Id == Id)?.Name ?? "";
            else
                return "";
        }
        #region Nguyen
        public static int OrderBy(int status, bool isLeadder, int isPermission)
        {
            var result = 0;
            switch (isPermission)
            {
                case 1://user Admin
                    if (status == 2)
                        result = 1;
                    else if (status == 1)
                        result = 2;
                    else
                        result = 3;
                    break;
                case 2://user QLBH
                    if (isLeadder)
                    {
                        if (status == 1)
                            result = 1;
                        else if (status == 99 || status == 4)
                            result = 2;
                        else if (status == 98 || status == 97)
                            result = 3;
                        else
                            result = 4;
                    }
                    else
                    {
                        if (status == 0)
                            result = 1;
                        else if (status == 1)
                            result = 2;
                        else if (status == 98 || status == 97)
                            result = 3;
                        else if (status == 99 || status == 4)
                            result = 4;
                        else
                            result = 5;
                    }
                    break;
                case 3://user CN/PGD
                    if (isLeadder)
                    {
                        if (status == 97)
                            result = 1;
                        else if (status == 1)
                            result = 2;
                        else
                            result = 3;
                    }
                    else
                    {
                        if (status == 0)
                            result = 1;
                        else if (status == 97)
                            result = 2;
                        else if (status == 1)
                            result = 3;
                        else if (status == 98 || status == 97)
                            result = 4;
                        else
                            result = 5;
                    }
                    break;
                default:
                    break;
            }

            return result;
        }
        public static string GetOrganizationCode(List<Organization> lst, Guid? Id)
        {
            if (Id != null)
                return lst.SingleOrDefault(p => p.Id == Id)?.Code ?? "";
            else
                return "";
        }

        public static string GiftCode(List<Gift> lstGift, Guid? Id)
        {
            if (Id != null)
                return lstGift.SingleOrDefault(p => p.Id == Id)?.Code ?? "";
            else
                return "";
        }
        public static decimal GiftPrice(List<Gift> lstGift, Guid? Id)
        {
            if (Id != null)
                return lstGift.SingleOrDefault(p => p.Id == Id)?.Price ?? 0;
            else
                return 0;
        }
        #endregion
    }
}
