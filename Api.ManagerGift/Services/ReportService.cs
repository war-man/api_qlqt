using Api.ManagerGift.DTO;
using Api.ManagerGift.Entities;
using Api.ManagerGift.Sessions;
using System;
using System.Collections.Generic;
using System.Dynamic;
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
