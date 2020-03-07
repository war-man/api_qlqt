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
    public class TransferInputService
    {
        TransferDetailService _transferDetailService = new TransferDetailService();
        StoreService _storeService = new StoreService();
        public string BrowseStaff(ISession ss, TransferGiftDTO obj, Stage stage, ClaimsPrincipal principal)
        {
            var result = string.Empty;
            var userinfo = ContextProvider.GetUserInfo(principal);
            try
            {
                var nextStage = ss.Get<Stage>(stage.NextStage);
                var transfer = ss.Get<TransferGift>(obj.Id);
                var transferlog = ss.Query<TransferGiftLog>().Single(p => p.TransferGift.Id == obj.Id && p.Status == (int)ContextProvider.statusTransfer.Draft);
                //transfer.Status = obj.Status;
                transfer.Status = (int)ContextProvider.statusTransfer.Initialize;
                transfer.CreatedDate = DateTime.ParseExact(DateTime.Now.ToString("u"), "u", CultureInfo.InvariantCulture);
                transferlog.Status = (int)ContextProvider.statusTransfer.Initialize;
                if (!string.IsNullOrEmpty(obj.Data.ToString()))
                    transferlog.Data = JsonConvert.SerializeObject(obj.Data);
                transferlog.UpdateDate = transfer.CreatedDate;
                //ss.CreateSQLQuery($"delete TransferDetail where TransferId = '{ obj.Id}'").UniqueResult();

                // xử lý cục Data (phi cấu trúc => có cấu trúc)
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
                        Status = (int)ContextProvider.statusTransfer.Initialize,
                        UpdateDate = DateTime.ParseExact(DateTime.Now.ToString("u"), "u", CultureInfo.InvariantCulture),
                        Stage = nextStage,
                        Dealine = null
                    };
                    ss.Save(newtransferlog);
                    result = "Gửi duyệt thành công!";
                }
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }

        // ông lãnh đạo đồng ý.
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
                var receivingDepartmentId = ss.Query<ReceivingDepartment>().Single(p => p.TransferGift.Id == transferId).DepartmentId;


                result = _storeService.HandlerInput(ss, transferId, receivingDepartmentId);
                if (string.IsNullOrEmpty(result))
                    result = "Duyệt thành công!";
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }
    }
}
