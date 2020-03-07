using Api.ManagerGift.DTO;
using Api.ManagerGift.Entities;
using Api.ManagerGift.Sessions;
using Newtonsoft.Json;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Api.ManagerGift.Services
{
    public class TransferOutputService
    {
        StoreService _storeService = new StoreService();
        TransferDetailService _transferDetailService = new TransferDetailService();
        public string BrowseStaff(ISession ss, TransferGiftDTO obj, Stage stage, ClaimsPrincipal principal)
        {
            var result = string.Empty;
            var userinfo = ContextProvider.GetUserInfo(principal);
            try
            {
                result = _storeService.ValidateData(ss, userinfo.Organization.Id, obj.Data);
                if (string.IsNullOrEmpty(result))
                {
                    var nextStage = ss.Get<Stage>(stage.NextStage);
                    var transfer = ss.Get<TransferGift>(obj.Id);
                    var transferlog = ss.Query<TransferGiftLog>().Single(p => p.TransferGift.Id == obj.Id && p.Status == (int)ContextProvider.statusTransfer.Draft);
                    transfer.Status = (int)ContextProvider.statusTransfer.Initialize;
                    transfer.CreatedDate = DateTime.ParseExact(DateTime.Now.ToString("u"), "u", CultureInfo.InvariantCulture);
                    transferlog.Status = (int)ContextProvider.statusTransfer.Initialize;
                    if (!string.IsNullOrEmpty(obj.Data.ToString()))
                        transferlog.Data = JsonConvert.SerializeObject(obj.Data);
                    transferlog.UpdateDate = transfer.CreatedDate;
                    result = _transferDetailService.Post(ss, transfer, obj);
                    if (string.IsNullOrEmpty(result))
                    {
                        var newtransferlog = new TransferGiftLog
                        {
                            Id = Guid.NewGuid(),
                            TransferGift = transferlog.TransferGift,
                            AssignUserId = null,
                            AssignDeaprtmentId = userinfo.Organization.Id,
                            Comment = obj.Comment,
                            Data = transferlog.Data,
                            Status = obj.Status,
                            UpdateDate = DateTime.ParseExact(DateTime.Now.ToString("u"), "u", CultureInfo.InvariantCulture),
                            Stage = nextStage,
                            Dealine = null
                        };
                        ss.Save(newtransferlog);
                        result = "Browse Success";
                    }
                }
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }
        public string BrowseLeader(ISession ss, Guid transferId, Stage stage, ClaimsPrincipal principal)
        {
            var result = string.Empty;
            var userinfo = ContextProvider.GetUserInfo(principal);
            try
            {
                var transfer = ss.Get<TransferGift>(transferId);
                transfer.Status = (int)ContextProvider.statusTransfer.Approve;
                if (stage.Name == "End Stage")
                    transfer.IsComplete = true;
                var transferlog = ss.Query<TransferGiftLog>()
                    .Single(p => p.TransferGift.Id == transferId && p.Stage.Id == stage.Id && p.AssignDeaprtmentId == userinfo.Organization.Id);
                transferlog.AssignUserId = userinfo.Id;
                transferlog.Status = transfer.Status;
                transferlog.UpdateDate = DateTime.ParseExact(DateTime.Now.ToString("u"), "u", CultureInfo.InvariantCulture);
                result = _storeService.HandlerOutput(ss, transferId, transfer.DepartmentId.GetValueOrDefault());
                if (string.IsNullOrEmpty(result))
                    result = "Browse Success";
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }
    }
}
