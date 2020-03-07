using Api.ManagerGift.DTO;
using Api.ManagerGift.Entities;
using Newtonsoft.Json;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.ManagerGift.Services
{
    public class TransferDetailService
    {
        public string Post(ISession ss, TransferGift transfer, TransferGiftDTO obj)
        {
            var result = string.Empty;
            //var ReceivingPromotionId = ss.Query<ReceivingPromotion>().SingleOrDefault(p => p.TransferGift.Id == transfer.Id);
            var ReceivingDepartmentId = ss.Query<ReceivingDepartment>().SingleOrDefault(p => p.TransferGift.Id == transfer.Id);
            try
            {
                var dicItem = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(obj.Data.ToString());
                foreach (var item in dicItem)
                {
                    var promotionId = Guid.Empty;
                    var transferDetail = new TransferDetail
                    {
                        Id = Guid.NewGuid(),
                        TransferGift = transfer,
                        GiftId = Guid.Parse(item["GiftId"]),
                        Amount = int.Parse(item["Amount"])
                    };

                    switch (obj.ProductId.ToString().ToUpper())
                    {
                        // NHẬP KHO
                        case "7A452975-E667-41CB-9B32-5875D357FF37":
                            transferDetail.ReceivingDepartment = ReceivingDepartmentId.DepartmentId;
                            transferDetail.ReceivingPromotion = null;
                            break;

                        //// XUẤT KHO
                        case "0AFC855F-5E19-4B2A-A296-E5E66BA3B17B":
                            promotionId = Guid.Parse(item["PromotionId"]);
                            transferDetail.ReceivingDepartment = null;
                            transferDetail.ReceivingPromotion = promotionId;
                            break;

                        // ĐIỀU CHUYỂN NGANG
                        case "4E0F159C-1B09-4FAB-8D2D-FA38DF55006A":
                            promotionId = Guid.Parse(item["PromotionId"]);
                            transferDetail.ReceivingPromotion = promotionId;
                            transferDetail.ReceivingDepartment = obj.ReceivingDepartmentId;
                            break;
                        // ĐIỀU CHUYỂN NỘI BỘ
                        case "81A05F45-9BE2-4754-A5D1-D0F8632AC8F8":
                            transferDetail.ReceivingDepartment = ReceivingDepartmentId.DepartmentId;
                            transferDetail.ReceivingPromotion = obj.ReceivingPromotionId.GetValueOrDefault();
                            break;
                    }
                    if(promotionId != Guid.Empty)
                    {
                        ss.Save(new ReceivingPromotion {
                            Id = Guid.NewGuid(),
                            PromotionId = promotionId,
                            TransferGift = transfer
                        });
                    }
                    ss.Save(transferDetail);
                }
                if(obj.ProductId.ToString().ToUpper() == "81A05F45-9BE2-4754-A5D1-D0F8632AC8F8")
                {
                    ss.Save(new ReceivingPromotion
                    {
                        Id = Guid.NewGuid(),
                        PromotionId = obj.ReceivingPromotionId.GetValueOrDefault(),
                        TransferGift = transfer
                    });
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
