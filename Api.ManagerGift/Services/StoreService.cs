using Api.ManagerGift.DTO;
using Api.ManagerGift.Entities;
using Api.ManagerGift.Sessions;
using Newtonsoft.Json;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Api.ManagerGift.Services
{
    public class StoreService
    {
        public string ValidateData(ISession ss, Guid departmentId, object data)
        {
            var result = string.Empty;
            try
            {
                var dicItem = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(data.ToString());

                // Lấy sp trong store - (sp của giao dịch trạng thái khởi tạo + gd chưa hoàn thành như dcnb).
                var lstStore = ss.Query<Store>()
                        .Where(p => p.DepartmentId == departmentId).ToList();
                var lstInitTransferId = ss.Query<TransferGift>()
                        .Where(p => p.Status == (int)ContextProvider.statusTransfer.Initialize || (p.Status == (int)ContextProvider.statusTransfer.Approve && !p.IsComplete)).Select(p => p.Id).ToList();
                var lstTransferDetail = ss.Query<TransferDetail>()
                        .Where(p => lstInitTransferId.Contains(p.TransferGift.Id)).ToList();
                foreach (var item in dicItem)
                {
                    var itemInStore = lstStore
                        .Where(p => p.GiftId == Guid.Parse(item["GiftId"]) && p.PromotionId == Guid.Parse(item["PromotionId"]))
                        .Select(p => p.Amount).DefaultIfEmpty(0).Sum();
                    var itemInTransferDetail = lstTransferDetail
                        .Where(p => p.GiftId == Guid.Parse(item["GiftId"]))
                        .Select(p => p.Amount).DefaultIfEmpty(0).Sum();
                    var itemCanUser = itemInStore - itemInTransferDetail;
                    if (itemCanUser < int.Parse(item["Amount"]))
                    {
                        result = $"{item["GiftCode"]} còn lại {itemCanUser} sản phẩm có thể sử dụng";
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }
        public string HandlerInput(ISession ss, Guid transferId, Guid departmentId)
        {
            var result = string.Empty;
            try
            {
                var lstStore = ss.Query<Store>()
                    .Where(p => p.DepartmentId == departmentId).ToList();
                var lstTransferDetail = ss.Query<TransferDetail>()
                    .Where(p => p.TransferGift.Id == transferId).ToList();

                foreach (var item in lstTransferDetail)
                {
                    var itemInStore = lstStore.SingleOrDefault(p => p.PromotionId == item.ReceivingPromotion && p.GiftId == item.GiftId);
                    if (itemInStore == null)
                    {
                        ss.Save(new Store
                        {
                            Id = Guid.NewGuid(),
                            DepartmentId = departmentId,
                            ManagerType = null,
                            PromotionId = item.ReceivingPromotion,
                            GiftId = item.GiftId,
                            Amount = item.Amount,
                            UpdatedDate = DateTime.ParseExact(DateTime.Now.ToString("u"), "u", CultureInfo.InvariantCulture),
                            LogTransfer = null
                        });
                    }
                    else
                    {
                        itemInStore.Amount += item.Amount;
                        itemInStore.UpdatedDate = DateTime.ParseExact(DateTime.Now.ToString("u"), "u", CultureInfo.InvariantCulture);
                    }
                }
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }
        public string HandlerOutput(ISession ss, Guid transferId, Guid departmentId)
        {
            var result = string.Empty;
            try
            {
                var lstStore = ss.Query<Store>()
                    .Where(p => p.DepartmentId == departmentId).ToList();
                var lstTransferDetail = ss.Query<TransferDetail>()
                    .Where(p => p.TransferGift.Id == transferId).ToList();

                foreach (var item in lstTransferDetail)
                {
                    var itemInStore = lstStore.Single(p => p.PromotionId == item.ReceivingPromotion && p.GiftId == item.GiftId);
                    itemInStore.Amount -= item.Amount;
                    itemInStore.UpdatedDate = DateTime.ParseExact(DateTime.Now.ToString("u"), "u", CultureInfo.InvariantCulture);
                }
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }
        public string HandlerPromotion(ISession ss, Guid transferId, Guid departmentId, Guid promotionId, Guid receivingPromotionId)
        {
            var result = string.Empty;
            try
            {
                var lstStore = ss.Query<Store>()
                    .Where(p => p.DepartmentId == departmentId).ToList();
                var lstTransferDetail = ss.Query<TransferDetail>()
                    .Where(p => p.TransferGift.Id == transferId).ToList();
                foreach(var item in lstTransferDetail)
                {
                    var sendItem = lstStore.Single(p => p.PromotionId == promotionId && p.GiftId == item.GiftId);
                    sendItem.Amount -= item.Amount;
                    sendItem.UpdatedDate = DateTime.ParseExact(DateTime.Now.ToString("u"), "u", CultureInfo.InvariantCulture);
                    var receiveItem = lstStore.Single(p => p.PromotionId == receivingPromotionId && p.GiftId == item.GiftId);
                    receiveItem.Amount += item.Amount;
                    receiveItem.UpdatedDate = DateTime.ParseExact(DateTime.Now.ToString("u"), "u", CultureInfo.InvariantCulture);
                }
            }
            catch(Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }
        public string HandlerOrganization(ISession ss, Guid transferId , Guid departmentId, Guid receiveDepartmentId)
        {
            var result = string.Empty;
            try
            {
                var lstSendStore = ss.Query<Store>()
                    .Where(p => p.DepartmentId == departmentId).ToList();
                var lstReceiveStore = ss.Query<Store>()
                    .Where(p => p.DepartmentId == receiveDepartmentId).ToList();
                var lstTransferDetail = ss.Query<TransferDetail>()
                    .Where(p => p.TransferGift.Id == transferId).ToList();
                foreach (var item in lstTransferDetail)
                {
                    var itemInSendStore = lstSendStore.Single(p => p.PromotionId == item.ReceivingPromotion && p.GiftId == item.GiftId);
                    itemInSendStore.Amount -= item.Amount;
                    itemInSendStore.UpdatedDate = DateTime.ParseExact(DateTime.Now.ToString("u"), "u", CultureInfo.InvariantCulture);

                    var itemInReceiveStore = lstSendStore.Single(p => p.PromotionId == item.ReceivingPromotion && p.GiftId == item.GiftId);
                    itemInReceiveStore.Amount += item.Amount;
                    itemInReceiveStore.UpdatedDate = DateTime.ParseExact(DateTime.Now.ToString("u"), "u", CultureInfo.InvariantCulture);
                }
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }
    }
}
