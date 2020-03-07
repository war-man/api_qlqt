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
    public class TransferOrganization
    {
        StoreService _storeService = new StoreService();
        TransferDetailService _transferDetailService = new TransferDetailService();
        public string BrowseStaff(ISession ss, TransferGiftDTO obj, Stage stage, ClaimsPrincipal principal)
        {
            var result = string.Empty;
            var userinfo = ContextProvider.GetUserInfo(principal);
            try
            {
                SessionManager.DoWork(ss =>
                {
                    var workflow = ss.Query<Workflow>().Single(p => p.ProductId == obj.ProductId && p.UserId == userinfo.Id && p.Stage == "First Stage");
                    var assignUserId = workflow.AssignUserId;
                    result = _storeService.ValidateData(ss, userinfo.Organization.Id, obj.Data);
                    if (string.IsNullOrEmpty(result))
                    {
                        var transfer = ss.Get<TransferGift>(obj.Id);
                        var transferlog = ss.Query<TransferGiftLog>().Single(p => p.TransferGift.Id == obj.Id && p.Status == (int)ContextProvider.statusTransfer.Draft);
                        transfer.Status = obj.Status;
                        transfer.DepartmentId = obj.DepartmentId;
                        //transfer.PromotionId = obj.PromotionId;
                        transfer.CreatedDate = DateTime.ParseExact(DateTime.Now.ToString("u"), "u", CultureInfo.InvariantCulture);
                        transferlog.Status = obj.Status;
                        if (!string.IsNullOrEmpty(obj.Data.ToString()))
                            transferlog.Data = JsonConvert.SerializeObject(obj.Data);
                        transferlog.UpdateDate = transfer.CreatedDate;
                        //ss.CreateSQLQuery($"delete ReceivingPromotion where TransferId = '{ obj.Id}'").UniqueResult();
                        //ss.CreateSQLQuery($"delete TransferDetail where TransferId = '{ obj.Id}'").UniqueResult();
                        result = _transferDetailService.Post(ss, transfer, obj);
                        if (string.IsNullOrEmpty(result))
                        {

                            var newtransferlog = new TransferGiftLog
                            {
                                Id = Guid.NewGuid(),
                                TransferGift = transferlog.TransferGift,
                                AssignUserId = assignUserId,
                                AssignDeaprtmentId = userinfo.Organization.Id,
                                Comment = obj.Comment,
                                Data = transferlog.Data,
                                Status = obj.Status,
                                UpdateDate = DateTime.ParseExact(DateTime.Now.ToString("u"), "u", CultureInfo.InvariantCulture),
                                StageId = obj.StageId,
                                Dealine = null
                            };
                            ss.Save(newtransferlog);
                            result = "Browse Success";
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }
        public string BrowseLeaderQLBH(Guid transferId, ClaimsPrincipal principal)
        {
            var result = string.Empty;
            var userinfo = ContextProvider.GetUserInfo(principal);
            try
            {
                SessionManager.DoWork(ss => {
                    var transfer = ss.Get<TransferGift>(transferId);
                    var workflow = ss.Query<Workflow>().Single(p => p.ProductId == transfer.Product.Id && p.UserId == userinfo.Id);
                    var assignUserId = workflow.AssignUserId;
                    var assignDepartmentId = ss.Get<User>(assignUserId).Organization.Id;
                    transfer.Status = (int)ContextProvider.statusTransfer.Approve;
                    var transferlog = ss.Query<TransferGiftLog>()
                        .Where(p => p.TransferGift.Id == transferId && p.AssignUserId == userinfo.Id)
                        .OrderByDescending(p => p.UpdateDate).First();
                    transferlog.Status = transfer.Status;
                    transferlog.UpdateDate = DateTime.ParseExact(DateTime.Now.ToString("u"), "u", CultureInfo.InvariantCulture);
                    var newtransferlog = new TransferGiftLog
                    {
                        Id = Guid.NewGuid(),
                        TransferGift = transferlog.TransferGift,
                        AssignUserId = assignUserId,
                        AssignDeaprtmentId = assignDepartmentId,
                        Comment = transferlog.Comment,
                        Data = transferlog.Data,
                        Status = transfer.Status,
                        UpdateDate = DateTime.ParseExact(DateTime.Now.ToString("u"), "u", CultureInfo.InvariantCulture),
                        StageId = transferlog.StageId,
                        Dealine = null
                    };
                    ss.Save(newtransferlog);
                    result = "Browse Success";
                });
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }
        public string BrowseLeaderCNC(Guid transferId, ClaimsPrincipal principal)
        {
            var result = string.Empty;
            var userinfo = ContextProvider.GetUserInfo(principal);
            try
            {
                SessionManager.DoWork(ss => {
                    var transfer = ss.Get<TransferGift>(transferId);
                    var workflow = ss.Query<Workflow>().Single(p => p.ProductId == transfer.Product.Id && p.UserId == userinfo.Id);
                    var assignUserId = workflow.AssignUserId;
                    var assignDepartmentId = ss.Get<User>(assignUserId).Organization.Id;
                    var transferlog = ss.Query<TransferGiftLog>()
                        .Where(p => p.TransferGift.Id == transferId && p.AssignUserId == userinfo.Id)
                        .OrderByDescending(p => p.UpdateDate).First();
                    transferlog.UpdateDate = DateTime.ParseExact(DateTime.Now.ToString("u"), "u", CultureInfo.InvariantCulture);
                    var newtransferlog = new TransferGiftLog
                    {
                        Id = Guid.NewGuid(),
                        TransferGift = transferlog.TransferGift,
                        AssignUserId = assignUserId,
                        AssignDeaprtmentId = assignDepartmentId,
                        Comment = transferlog.Comment,
                        Data = transferlog.Data,
                        Status = transfer.Status,
                        UpdateDate = DateTime.ParseExact(DateTime.Now.ToString("u"), "u", CultureInfo.InvariantCulture),
                        StageId = transferlog.StageId,
                        Dealine = null
                    };
                    ss.Save(newtransferlog);
                    result = "Browse Success";
                });
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }
        public string BrowseLeaderCNN(Guid transferId, ClaimsPrincipal principal)
        {
            var result = string.Empty;
            var userinfo = ContextProvider.GetUserInfo(principal);
            try
            {
                SessionManager.DoWork(ss =>
                {
                    var transfer = ss.Get<TransferGift>(transferId);
                    var stage = ss.Query<Workflow>().Single(p => p.ProductId == transfer.Product.Id && p.UserId == userinfo.Id).Stage;
                    if (stage == "Last Stage")
                        transfer.IsComplete = true;
                    var transferlog = ss.Query<TransferGiftLog>()
                        .Where(p => p.TransferGift.Id == transferId && p.AssignUserId == userinfo.Id)
                        .OrderByDescending(p => p.UpdateDate).First();
                    transferlog.UpdateDate = DateTime.ParseExact(DateTime.Now.ToString("u"), "u", CultureInfo.InvariantCulture);
                    var receivingDepartmentId = ss.Query<ReceivingDepartment>().Single(p => p.TransferGift.Id == transferId).DepartmentId;
                    result = _storeService.HandlerOrganization(ss, transferId, transfer.DepartmentId.GetValueOrDefault(), receivingDepartmentId);
                    if (string.IsNullOrEmpty(result))
                    {
                        result = "Browse Success";
                    }
                });
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }
    }
}