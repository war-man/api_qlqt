﻿using Api.ManagerGift.DTO;
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
            Refuse
        }
        public static string GetFullName(List<User> lstUser, Guid? Id)
        {
            if (Id != null)
                return lstUser.SingleOrDefault(p => p.Id == Id)?.FullName ?? "";
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

        public static string GetPromotionName(List<Promotion> lst, Guid? Id)
        {
            if (Id != null)
                return lst.SingleOrDefault(p => p.Id == Id)?.Name ?? "";
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
        public static string GetOrganizationCode(List<Organization> lst, Guid? Id)
        {
            if (Id != null)
                return lst.SingleOrDefault(p => p.Id == Id)?.Code ?? "";
            else
                return "";
        }
        #endregion
    }
}