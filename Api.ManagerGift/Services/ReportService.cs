using Api.ManagerGift.DTO;
using Api.ManagerGift.Entities;
using Api.ManagerGift.Sessions;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;

namespace Api.ManagerGift.Services
{
    public class ReportService
    {
        public List<BaoCaoQuaTangDTO> GetDataReport(string productId, string idPromotion, string fromDate, string toDate)
        {
            var result = new List<BaoCaoQuaTangDTO>();
            SessionManager.DoWork(ss =>
            {
                try
                {
                    var _fromDate = DateTime.ParseExact(fromDate + " 00:00:00,000", "yyyy-MM-dd HH:mm:ss,fff",
                                       System.Globalization.CultureInfo.InvariantCulture);
                    var _toDate = DateTime.ParseExact(toDate + " 00:00:00,000", "yyyy-MM-dd HH:mm:ss,fff",
                                       System.Globalization.CultureInfo.InvariantCulture);

                    var promotion = ss.Query<Promotion>().ToList();
                    var organization = ss.Query<Organization>().ToList();

                    var tranfer = ss.Query<TransferGift>().Where(s => s.Status == 2 && s.CreatedDate <= _toDate && s.CreatedDate >= _fromDate).ToList();

                    if (productId != null)
                        tranfer = tranfer.Where(s => s.Product.Id == new Guid(productId)).ToList();
                    if (idPromotion != null)
                        tranfer = tranfer.Where(s => s.PromotionId == new Guid(idPromotion)).ToList();

                    var lstTranferId = tranfer.Select(s => s.Id);
                    var tranferDetail = ss.Query<TransferDetail>().Where(s => lstTranferId.Contains(s.TransferGift.Id)).ToList();
                    var gifts = ss.Query<Gift>().ToList();
                    var lstGift = (from _tranfer in tranfer
                                   join _tranferDetail in tranferDetail on _tranfer.Id equals _tranferDetail.TransferGift.Id
                                   join _gifts in gifts on _tranferDetail.GiftId equals _gifts.Id
                                   select new BaoCaoQuaTangDTO
                                   {
                                       Amount = _tranferDetail.Amount,
                                       ReceivingDepartment = ContextProvider.GetOrganizationName(organization, _tranferDetail.ReceivingDepartment),
                                       ReceivingPromotion = ContextProvider.GetPromotionName(promotion, _tranferDetail.ReceivingPromotion),
                                       CreatedDate = ContextProvider.GetConvertDatetime(_tranfer.CreatedDate),
                                       TranferDepartment = ContextProvider.GetOrganizationName(organization, _tranfer.DepartmentId),
                                       Code = _gifts.Code,
                                       Name = _gifts.Name,
                                       UnitName = _gifts.Unit.Name,
                                       Price = _gifts.Price.ToString(),
                                       GiftGroupId = _gifts.GiftGroup.Id.ToString(),
                                       GroupName = _gifts.GiftGroup.Name,
                                       OptionGiftId = _gifts.GiftGroup.OptionGift.Id.ToString(),
                                       OptionGiftName = _gifts.GiftGroup.OptionGift.Name
                                   }).OrderBy(p => p.GiftGroupId).OrderBy(pp => pp.OptionGiftId);
                    result = lstGift.ToList();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            });
            return result;
        }


        public List<StoreDTO> GetDataReportInventory(ClaimsPrincipal principal, string productId, string idPromotion, string toDate)
        {
            var result = new List<StoreDTO>();
            var userinfo = ContextProvider.GetUserInfo(principal);
            SessionManager.DoWork(ss =>
            {
                try
                {
                    var _toDate = DateTime.ParseExact(toDate.Replace("-", "/") + " 23:59:59,000", "dd/MM/yyyy HH:mm:ss,fff",
                                       System.Globalization.CultureInfo.InvariantCulture);
                    var lstPromotions = ss.Query<Promotion>().Where(w => w.CreatedDate <= _toDate && w.Status == 2).ToList();
                    if (!string.IsNullOrEmpty(idPromotion))
                        lstPromotions = lstPromotions.Where(n => n.Id == Guid.Parse(idPromotion)).ToList();
                    var idPromotions = lstPromotions.Select(s => s.Id).ToList();
                    var idGiftPromotions = lstPromotions.Select(s => s.GiftPromotionId).ToList();
                    var giftPromotions = ss.Query<GiftPromotion>().Where(s => idGiftPromotions.Contains(s.GiftPromotionId)).ToList();
                    var lstGiftsId = giftPromotions.Select(s => s.GiftId).ToList();
                    var gifts = ss.Query<Gift>().Where(s => lstGiftsId.Contains(s.Id)).ToList();
                    var transferGift = ss.Query<TransferGift>().Where(s => s.Status == 2).ToList();
                    var transferDetails = ss.Query<TransferDetail>().Where(s => lstGiftsId.Contains(s.GiftId)).ToList();
                    var stores = ss.Query<Store>().Where(s => lstGiftsId.Contains(s.GiftId)).ToList();
                    #region SL Nhap kho
                    var idTranNK = transferGift.Where(w => w.Product.Id.ToString().ToUpper() == Constants.PARAM_NHAP_KHO).Select(s => s.Id);
                    var gbNKDetails = transferDetails.Where(s => idTranNK.Contains(s.TransferGift.Id)).GroupBy(g => new { g.GiftId, g.ReceivingDepartment, productId }).Select(s => new
                    {
                        s.Key.GiftId,
                        DepartmentId = s.Key.ReceivingDepartment,
                        Total = s.Sum(t => t.Amount)
                    }).ToList();
                    #endregion

                    #region SL Xuất kho
                    var idTranXK = transferGift.Where(w => w.Product.Id.ToString().ToUpper() == Constants.PARAM_XUAT_KHO).Select(s => s.Id);
                    var gbXKDetails = transferDetails.Where(s => idTranXK.Contains(s.TransferGift.Id)).GroupBy(g => new { g.GiftId, g.ReceivingDepartment, productId }).Select(s => new
                    {
                        s.Key.GiftId,
                        DepartmentId = s.Key.ReceivingDepartment,
                        Total = s.Sum(t => t.Amount)
                    }).ToList();
                    #endregion

                    #region SL Dieu chuyen ngang
                    var idTranDCN = transferGift.Where(w => w.Product.Id.ToString().ToUpper() == Constants.PARAM_DIEU_CHUYEN_NGANG).Select(s => s.Id);
                    var gbDCNDetails = transferDetails.Where(s => idTranDCN.Contains(s.TransferGift.Id)).GroupBy(g => new { g.GiftId, g.ReceivingDepartment, productId }).Select(s => new
                    {
                        s.Key.GiftId,
                        DepartmentId = s.Key.ReceivingDepartment,
                        Total = s.Sum(t => t.Amount)
                    }).ToList();
                    #endregion

                    #region SL Dieu chuyen noi bo
                    var idTranDCNB = transferGift.Where(w => w.Product.Id.ToString().ToUpper() == Constants.PARAM_DIEU_CHUYEN_NOI_BO).Select(s => s.Id);
                    var gbDCNBDetails = transferDetails.Where(s => idTranDCNB.Contains(s.TransferGift.Id)).GroupBy(g => new { g.GiftId, g.ReceivingDepartment, productId }).Select(s => new
                    {
                        s.Key.GiftId,
                        DepartmentId = s.Key.ReceivingDepartment,
                        Total = s.Sum(t => t.Amount)
                    }).ToList();
                    #endregion

                    //So luong ton kho
                    var groupByStores = stores.GroupBy(g => new { g.GiftId, g.DepartmentId }).Select(s => new { s.Key.GiftId, s.Key.DepartmentId, Total = s.Sum(t => t.Amount), }).ToList();
                    //So luong
                    //var groupByTFDetails = transferDetails.GroupBy(g => new{g.GiftId,g.ReceivingDepartment,productId}).Select(s => new
                    //{
                    //    s.Key.GiftId, DepartmentId = s.Key.ReceivingDepartment,
                    //    ProductId = s.Key.productId, Total = s.Sum(t => t.Amount)
                    //}).ToList();

                    //So luong da su dung
                    //var customerGifts = ss.Query<CustomerGift>().Where(s => lstGiftsId.Contains(s.Gift.Id)).ToList();
                    //var groupByCustomerGifts = customerGifts.GroupBy(g => new{GiftId = g.Gift.Id}).Select(s => new{s.Key.GiftId,Total = s.Count(),}).ToList();
                    var lstOrgan = ss.Query<Organization>().ToList();
                    groupByStores.ForEach(store =>
                    {
                        var amountTotal = gbNKDetails.Where(w => w.GiftId == store.GiftId && w.DepartmentId == store.DepartmentId).Sum(s => s.Total);
                        var amountAttribution = gbDCNDetails.Where(w => w.GiftId == store.GiftId && w.DepartmentId == store.DepartmentId).Sum(s => s.Total)
                                                    + gbDCNBDetails.Where(w => w.GiftId == store.GiftId && w.DepartmentId == store.DepartmentId).Sum(s => s.Total);
                        var saveItem = new StoreDTO
                        {
                            GiftId = store.GiftId,
                            GiftName = ContextProvider.GiftName(gifts, store.GiftId),
                            GiftCode = ContextProvider.GiftCode(gifts, store.GiftId),
                            Price = (ContextProvider.GiftPrice(gifts, store.GiftId) * (amountTotal - amountAttribution)).ToString(),
                            DepartmentId = store.DepartmentId,
                            DepartmentName = ContextProvider.GetOrganizationName(lstOrgan, store.DepartmentId),
                            AmountInventory = amountTotal - amountAttribution,
                            AmountAttribution = amountAttribution
                        };
                        if (!string.IsNullOrEmpty(saveItem.GiftCode))
                            result.Add(saveItem);
                    });

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            });
            return result;
        }
        public dynamic TonKho(ClaimsPrincipal principal)
        {
            dynamic result = new ExpandoObject();
            var userinfo = ContextProvider.GetUserInfo(principal);
            try
            {
                SessionManager.DoWork(ss =>
                {
                    var lstOrgan = ss.Query<Organization>().ToList();
                    var lstPromotion = ss.Query<Promotion>().ToList();
                    var lstGift = ss.Query<Gift>().ToList();
                    result = ss.Query<Store>().Where(s => s.DepartmentId == userinfo.OrganizationId)
                        .Select(p => new
                        {
                            DepartmentName = ContextProvider.GetOrganizationName(lstOrgan, p.DepartmentId),
                            PromotionName = ContextProvider.GetPromotionName(lstPromotion, p.PromotionId),
                            GiftName = ContextProvider.GiftName(lstGift, p.GiftId),
                            p.Amount,
                            p.UpdatedDate
                        }).ToList();
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return result;
        }
    }
}
