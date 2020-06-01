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
        public List<BaoCaoQuaTangDTO> GetDataReport(ClaimsPrincipal principal, string productId, string idPromotion, string fromDate, string toDate)
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
                    var userinfo = ContextProvider.GetUserInfo(principal);
                    var promotion = ss.Query<Promotion>().ToList();
                    var organization = ss.Query<Organization>().ToList();

                    var tranfer = ss.Query<TransferGift>().Where(s => s.Status == 2 && s.CreatedDate <= _toDate.AddDays(1) && s.CreatedDate >= _fromDate).ToList();

                    if (productId != null)
                        tranfer = tranfer.Where(s => s.Product.Id == new Guid(productId)).ToList();
                    if (userinfo.OrganizationCode != "QLBH" && userinfo.UserName != "admin" && userinfo.UserName != "nva")
                    {
                        //Nếu là LD CN/PGD
                        if (userinfo.Position.IsLeader)
                            tranfer = tranfer.Where(s => s.DepartmentId == userinfo.Organization.Id).ToList();
                        else//CV CN/PGD
                            tranfer = tranfer.Where(s => s.CreatedBy == userinfo.Id).ToList();
                    }
                    if (idPromotion != null && productId.ToUpper() != "7A452975-E667-41CB-9B32-5875D357FF37")
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
                                       Name = _gifts.GiftGroup.OptionGift.Name +" - "+ _gifts.GiftGroup.Name+" - "+ _gifts.Name,
                                       UnitName = _gifts.Unit.Name,
                                       Price = _gifts.Price.ToString("f0"),
                                       GiftGroupId = _gifts.GiftGroup.Id.ToString(),
                                       GroupName = _gifts.GiftGroup.Name,
                                       OptionGiftId = _gifts.GiftGroup.OptionGift.Id.ToString(),
                                       OptionGiftName = _gifts.GiftGroup.OptionGift.Name,
                                       OrderByDate = ContextProvider.GetOrderDatetime(_tranfer.CreatedDate)
                                   }).OrderBy(pp => pp.Name).OrderBy(p => p.GroupName).OrderBy(pp => pp.OptionGiftName).OrderByDescending(pp => pp.OrderByDate);
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
                    if (userinfo.OrganizationCode != "QLBH" && userinfo.UserName != "admin" && userinfo.UserName != "nva")
                    {
                        //Nếu là LD CN/PGD
                        if (userinfo.Position.IsLeader)
                            lstPromotions = lstPromotions.Where(s => s.NguoiDuyet == userinfo.Organization.Id).ToList();
                        else//CV CN/PGD
                            lstPromotions = lstPromotions.Where(s => s.CreatedBy == userinfo.Id).ToList();
                    }
                    var idPromotions = lstPromotions.Select(s => s.Id).ToList();
                    var idGiftPromotions = lstPromotions.Select(s => s.GiftPromotionId).ToList();
                    var giftPromotions = ss.Query<GiftPromotion>().Where(s => idGiftPromotions.Contains(s.GiftPromotionId)).ToList();

                    var lstGiftsId = giftPromotions.Select(s => s.GiftId).ToList();
                    var gifts = ss.Query<Gift>().Where(s => lstGiftsId.Contains(s.Id)).ToList();
                    var transferGift = ss.Query<TransferGift>().Where(s => s.Status == 2).ToList();
                    var transferDetails = ss.Query<TransferDetail>().Where(s => lstGiftsId.Contains(s.GiftId)).ToList();
                    var stores = ss.Query<Store>().Where(s => lstGiftsId.Contains(s.GiftId)).ToList();


                    #region SL Nhap kho
                    var idTranNK = transferGift.Where(w => w.Product.Id.ToString().ToUpper() == Constants.PARAM_NHAP_KHO && w.Status == 2).Select(s => s.Id);
                    var gbNKDetails = transferDetails.Where(s => idTranNK.Contains(s.TransferGift.Id)).GroupBy(g => new { g.GiftId, g.ReceivingDepartment, productId }).Select(s => new
                    {
                        s.Key.GiftId,
                        DepartmentId = s.Key.ReceivingDepartment,
                        Total = s.Sum(t => t.Amount)
                    }).ToList();
                    #endregion

                    #region SL Xuất kho
                    var idTranXK = transferGift.Where(w => w.Product.Id.ToString().ToUpper() == Constants.PARAM_XUAT_KHO && w.Status == 2).Select(s => s.Id);
                    var gbXKDetails = transferDetails.Where(s => idTranXK.Contains(s.TransferGift.Id)).GroupBy(g => new { g.GiftId, g.ReceivingDepartment, productId }).Select(s => new
                    {
                        s.Key.GiftId,
                        DepartmentId = s.Key.ReceivingDepartment,
                        Total = s.Sum(t => t.Amount)
                    }).ToList();
                    #endregion

                    #region SL Dieu chuyen ngang
                    var idTranDCN = transferGift.Where(w => w.Product.Id.ToString().ToUpper() == Constants.PARAM_DIEU_CHUYEN_NGANG && w.Status == 2).Select(s => s.Id);
                    var gbDCNDetails = transferDetails.Where(s => idTranDCN.Contains(s.TransferGift.Id)).GroupBy(g => new { g.GiftId, g.ReceivingDepartment, productId }).Select(s => new
                    {
                        s.Key.GiftId,
                        DepartmentId = s.Key.ReceivingDepartment,
                        Total = s.Sum(t => t.Amount)
                    }).ToList();
                    #endregion

                    #region SL Dieu chuyen noi bo
                    var idTranDCNB = transferGift.Where(w => w.Product.Id.ToString().ToUpper() == Constants.PARAM_DIEU_CHUYEN_NOI_BO && w.Status == 2).Select(s => s.Id);
                    var gbDCNBDetails = transferDetails.Where(s => idTranDCNB.Contains(s.TransferGift.Id)).GroupBy(g => new { g.GiftId, g.ReceivingDepartment, productId }).Select(s => new
                    {
                        s.Key.GiftId,
                        DepartmentId = s.Key.ReceivingDepartment,
                        Total = s.Sum(t => t.Amount)
                    }).ToList();
                    #endregion

                    #region SL Phan Bo
                    var idTranPBo = transferGift.Where(w => w.Product.Id.ToString().ToUpper() == Constants.PARAM_PHAN_BO && w.Status == 2).Select(s => s.Id);
                    var gbPBoDetails = transferDetails.Where(s => idTranPBo.Contains(s.TransferGift.Id)).GroupBy(g => new { g.GiftId, g.ReceivingDepartment, productId }).Select(s => new
                    {
                        s.Key.GiftId,
                        DepartmentId = s.Key.ReceivingDepartment,
                        Total = s.Sum(t => t.Amount)
                    }).ToList();
                    #endregion

                    //So luong ton kho
                    var groupByStores = stores.GroupBy(g => new { g.GiftId, g.DepartmentId }).Select(s => new { s.Key.GiftId, s.Key.DepartmentId, Total = s.Sum(t => t.Amount), }).ToList();
                    var lstOrgan = ss.Query<Organization>().ToList();
                    groupByStores.ForEach(store =>
                    {
                        var amountTotal = gbNKDetails.Where(w => w.GiftId == store.GiftId && w.DepartmentId == store.DepartmentId).Sum(s => s.Total);
                        var amountAttribution = gbPBoDetails.Where(w => w.GiftId == store.GiftId && w.DepartmentId == store.DepartmentId).Sum(s => s.Total);
                        //gbDCNDetails.Where(w => w.GiftId == store.GiftId && w.DepartmentId == store.DepartmentId).Sum(s => s.Total)
                        //+ gbDCNBDetails.Where(w => w.GiftId == store.GiftId && w.DepartmentId == store.DepartmentId).Sum(s => s.Total);
                        var depCode = ContextProvider.GetOrganizationCode(lstOrgan, store.DepartmentId);
                        var saveItem = new StoreDTO
                        {
                            GiftId = store.GiftId,
                            GiftName = ContextProvider.GiftName(gifts, store.GiftId),
                            GiftCode = ContextProvider.GiftCode(gifts, store.GiftId),
                            Price = (ContextProvider.GiftPrice(gifts, store.GiftId) * store.Total).ToString("N", CultureInfo.CurrentCulture),
                            DepartmentId = store.DepartmentId,
                            DepartmentName = ContextProvider.GetOrganizationName(lstOrgan, store.DepartmentId),
                            AmountInventory = store.Total,//amountTotal - amountAttribution,
                            AmountUse = depCode == "QLBH" ? 0 : amountAttribution - amountTotal,
                            AmountAttribution = depCode == "QLBH" ? 0 : amountAttribution
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

        public List<StoreDTO> GetDataReportInventoryBak(ClaimsPrincipal principal, string productId, string idPromotion, string toDate)
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

        public List<BC06_DTO> GetDataReport07(ClaimsPrincipal principal, string productId, string idPromotion, string idGift, string idBranch, string idDepartment, string fromDate, string toDate)
        {
            var result = new List<BC06_DTO>();
            SessionManager.DoWork(ss =>
            {
                try
                {
                    var _fromDate = DateTime.ParseExact(fromDate + " 00:00:00,000", "yyyy-MM-dd HH:mm:ss,fff",
                                       System.Globalization.CultureInfo.InvariantCulture);
                    var _toDate = DateTime.ParseExact(toDate + " 00:00:00,000", "yyyy-MM-dd HH:mm:ss,fff",
                                       System.Globalization.CultureInfo.InvariantCulture);
                    //var _toDate = Convert.ToDateTime(DateTime.ParseExact(toDate, "dd-MM-yyyy", CultureInfo.InvariantCulture));
                    var userinfo = ContextProvider.GetUserInfo(principal);
                    var promotions = ss.Query<Promotion>().ToList();
                    var organization = ss.Query<Organization>().ToList();
                    var branchs = organization.Where(w => w.ManageCode == "CN").ToList();
                    var dept = organization.Where(w => w.ManageCode == "PGD").ToList();
                    var gifts = ss.Query<Gift>().ToList();
                    var data = ss.Query<CustomerGift>().Where(s => s.Status == 2 && s.CREATEDDATE <= _toDate.AddDays(1) && s.CREATEDDATE >= _fromDate).ToList();
                    if (!string.IsNullOrEmpty(idPromotion))
                        data = data.Where(s => s.Promotion.Id == new Guid(idPromotion)).ToList();
                    if (!string.IsNullOrEmpty(idDepartment))
                    {
                        var dep = dept.FirstOrDefault(w => w.Id == new Guid(idDepartment));
                        data = data.Where(s => s.SUBBRID == dep.Code).ToList();
                    }
                    if (!string.IsNullOrEmpty(idBranch))
                    {
                        var branch = branchs.FirstOrDefault(w => w.Id == new Guid(idBranch));
                        data = data.Where(s => s.BRANCHID == branch.Code).ToList();
                    }
                    if (!string.IsNullOrEmpty(idGift))
                        data = data.Where(s => s.Gift.Id == new Guid(idGift)).ToList();
                    if (userinfo.OrganizationCode != "QLBH" && userinfo.UserName != "admin" && userinfo.UserName != "nva")
                    {
                        //Nếu là LD CN/PGD
                        if (userinfo.Position.IsLeader)
                            data = data.Where(s => s.SUBBRID == userinfo.Organization.Code).ToList();
                        else//CV CN/PGD
                            data = data.Where(s => s.CREATEDBy == userinfo.Id).ToList();
                    }
                    switch (productId.ToUpper())
                    {
                        case "BC_07":
                            var lstGift = (from _data in data
                                           join _gift in gifts on _data.Gift.Id equals _gift.Id
                                           join _promotion in promotions on _data.Promotion.Id equals _promotion.Id
                                           select new BC06_DTO
                                           {
                                               DepartmentName = _data.SUBBRNAME,
                                               BranchName = _data.BRNAME,
                                               SoTK = _data.Acctno,
                                               CIF = _data.CusId,
                                               CustomerName = _data.CusName,
                                               GhiChu = "",
                                               GiaTriQuaTang = _gift.Price,
                                               KyHan = _data.TERM,
                                               LoaiQua = _data.BRNAME,
                                               NgayGui = _data.TODATE.ToString("dd-MM-yyyy"),
                                               SoDu = _data.BALANCE,
                                               GiftName = _gift.Name,
                                               PromotionName = _promotion.Name,
                                               PhanHe = _data.PhanHe,
                                               LoaiTien = _data.CCYCD,
                                               TenLoaiHinh = _data.TENLOAIHINH,
                                               OrderByDate = ContextProvider.GetOrderDatetime(_data.CREATEDDATE)
                                           }).OrderBy(pp => pp.BranchName).OrderBy(p => p.DepartmentName).OrderByDescending(pp => pp.OrderByDate);
                            result = lstGift.ToList();
                            break;
                        case "BC_08":
                            var lstBC_08 = new List<BC06_DTO>();
                            var groupPGD = data.GroupBy(g => new { g.SUBBRID, GifiId = g.Gift.Id }).Select(s => new
                            {
                                SUBBRID = s.Key.SUBBRID,
                                GifiId = s.Key.GifiId,
                                Amount = s.Count(),
                                Balance = s.Sum(f => f.BALANCE)
                            }).ToList();
                            groupPGD.ForEach(code =>
                            {
                                var depBC08 = organization.FirstOrDefault(f => f.Code == code.SUBBRID);
                                var branchBC08 = depBC08.ManageCode == "CN" ? depBC08 : organization.FirstOrDefault(f => f.Id == depBC08?.ParentId);
                                var giftBC08 = gifts.FirstOrDefault(f => f.Id == code.GifiId);
                                var save = new BC06_DTO
                                {
                                    BranchName = branchBC08.Name,
                                    DepartmentName = depBC08.Name,
                                    SoDu = code.Balance,
                                    SoLuong = code.Amount.ToString(),
                                    GiaTriQuaTang = (giftBC08.Price * code.Amount),
                                    GiftCode = giftBC08.Code,
                                    GiftName = giftBC08.Name,
                                    GhiChu = ""
                                };
                                lstBC_08.Add(save);
                            });
                            lstBC_08 = lstBC_08.OrderBy(pp => pp.BranchName).OrderBy(p => p.DepartmentName).ToList();
                            result = lstBC_08.ToList();
                            break;
                        default:
                            break;
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            });
            return result;
        }

        public List<BC06_DTO> GetDataReport09(ClaimsPrincipal principal, string productId, string idPromotion, string idGiftStore, string idGiftUse, string idBranch, string idDepartment, string fromDate, string toDate)
        {
            var result = new List<BC06_DTO>();
            SessionManager.DoWork(ss =>
            {
                try
                {
                    var _fromDate = DateTime.ParseExact(fromDate + " 00:00:00,000", "yyyy-MM-dd HH:mm:ss,fff",
                                       System.Globalization.CultureInfo.InvariantCulture);
                    var _toDate = DateTime.ParseExact(toDate + " 00:00:00,000", "yyyy-MM-dd HH:mm:ss,fff",
                                       System.Globalization.CultureInfo.InvariantCulture);
                    //var _toDate = Convert.ToDateTime(DateTime.ParseExact(toDate, "dd-MM-yyyy", CultureInfo.InvariantCulture));
                    var userinfo = ContextProvider.GetUserInfo(principal);
                    var promotions = ss.Query<Promotion>().ToList();
                    var organization = ss.Query<Organization>().ToList();
                    var branchs = organization.Where(w => w.ManageCode == "CN").ToList();
                    var dept = organization.Where(w => w.ManageCode == "PGD").ToList();
                    var gifts = ss.Query<Gift>().ToList();
                    var data = ss.Query<CustomerGift>().Where(s => s.Status == 2 && s.CREATEDDATE <= _toDate.AddDays(1) && s.CREATEDDATE >= _fromDate).ToList();
                    if (!string.IsNullOrEmpty(idPromotion))
                        data = data.Where(s => s.Promotion.Id == new Guid(idPromotion)).ToList();
                    if (!string.IsNullOrEmpty(idDepartment))
                    {
                        var dep = dept.FirstOrDefault(w => w.Id == new Guid(idDepartment));
                        data = data.Where(s => s.SUBBRID == dep.Code).ToList();
                    }
                    if (!string.IsNullOrEmpty(idBranch))
                    {
                        var branch = branchs.FirstOrDefault(w => w.Id == new Guid(idBranch));
                        data = data.Where(s => s.BRANCHID == branch.Code).ToList();
                    }
                    if (!string.IsNullOrEmpty(idGiftStore))
                        data = data.Where(s => s.Gift.Id == new Guid(idGiftStore)).ToList();
                    if (!string.IsNullOrEmpty(idGiftUse))
                        data = data.Where(s => s.Gift.Id == new Guid(idGiftUse)).ToList();
                    if (userinfo.OrganizationCode != "QLBH" && userinfo.UserName != "admin" && userinfo.UserName != "nva")
                    {
                        //Nếu là LD CN/PGD
                        if (userinfo.Position.IsLeader)
                            data = data.Where(s => s.SUBBRID == userinfo.Organization.Code).ToList();
                        else//CV CN/PGD
                            data = data.Where(s => s.CREATEDBy == userinfo.Id).ToList();
                    }
                    switch (productId.ToUpper())
                    {
                        case "BC_09":
                            var lstBC_09 = new List<BC06_DTO>();
                            var groupPGD = data.GroupBy(g => new { g.SUBBRID, PromotionId = g.Promotion.Id, GiftId = g.Gift.Id }).Select(s => new
                            {
                                SUBBRID = s.Key.SUBBRID,
                                GiftId = s.Key.GiftId,
                                PromotionId = s.Key.PromotionId,
                                NumGift = s.Sum(f => f.NumGift)
                            }).ToList();
                            var idGifts = groupPGD.Select(s => s.GiftId).ToList();
                            var idPromotions = groupPGD.Select(s => s.PromotionId).ToList();
                            var tranfsDetail = ss.Query<TransferDetail>().Where(w=> idPromotions.Contains(w.ReceivingPromotion??Guid.NewGuid()) && idGifts.Contains(w.GiftId)).ToList();
                            groupPGD.ForEach(code =>
                            {
                                var depBC09 = organization.FirstOrDefault(f => f.Code == code.SUBBRID);
                                var giftBC09 = gifts.FirstOrDefault(f => f.Id == code.GiftId);
                                var slPB = tranfsDetail.Where(f => f.ReceivingDepartment == depBC09.Id && f.GiftId == code.GiftId && f.ReceivingPromotion == code.PromotionId && f.TransferGift.Status==2).ToList().Sum(s=>s.Amount);
                                var save = new BC06_DTO
                                {
                                    DepartmentName = depBC09.Name,
                                    GiftCode = giftBC09.Code,
                                    GiftName = giftBC09.Name,
                                    GiaTri = giftBC09.Price,
                                    SoLuongNhapKho = slPB.ToString(),
                                    SoLuongSuDung = code.NumGift.ToString(),
                                    SoLuongCuoiKy = (slPB- code.NumGift).ToString(),
                                    //ChenhLech = (slPB - code.NumGift).ToString(),
                                    ThanhTien = (giftBC09.Price * (slPB - code.NumGift)),
                                    GhiChu = ""
                                };
                                lstBC_09.Add(save);
                            });
                            lstBC_09 = lstBC_09.OrderBy(pp => pp.BranchName).OrderBy(p => p.DepartmentName).ToList();
                            result = lstBC_09.ToList();
                            break;
                        default:
                            break;
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            });
            return result;
        }
    }
}
