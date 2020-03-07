using Api.ManagerGift.DTO;
using Api.ManagerGift.Entities;
using Api.ManagerGift.Sessions;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace Api.ManagerGift.Services
{
    public class PhanBoQuaTangService
    {

        public dynamic Get(int pageNo, string organizationId, string promotionId, ClaimsPrincipal principal)
        {
            int pageSize = 20;
            dynamic result = new ExpandoObject();
            var userinfo = ContextProvider.GetUserInfo(principal);
            SessionManager.DoWork(ss =>
            {
                try
                {
                    var lstUser = ss.Query<User>().ToList();
                    var lstOrgan = ss.Query<Organization>().ToList();

                    var tranfers = ss.Query<TransferGift>().Where(p => p.FlagDieuChuyen != new Guid(Constants.GUIDE_TYPE_NULL)).
                    Select(s => new
                    {
                        s.FlagDieuChuyen,
                        s.CreatedBy,
                        s.PromotionId,
                        ProductId = s.Product.Id,
                        s.Status,
                        s.DepartmentId,
                        s.CreatedDate
                    }).Distinct().ToList();
                    // .OrderBy(s => new { s.CreatedDate, s.Status })

                    if (organizationId != null)
                        tranfers = tranfers.Where(s => s.DepartmentId == new Guid(organizationId)).ToList();

                    IEnumerable<Promotion> Promotion = ss.Query<Promotion>();

                    if (promotionId != null)
                        Promotion = Promotion.Where(s => s.Id == new Guid(promotionId)).ToList();

                    var status = 100; // giá trị này không có ý nghĩa gì cả, mục đích lọc bản nháp không cho lãnh đạo thấy.
                    if (userinfo.PositionId == Constants.ID_LANH_DAO)
                        status = 0;

                    var lstTranfers = (from _tranfers in tranfers
                                       join _promotion in Promotion on _tranfers.PromotionId equals _promotion.Id
                                       where _tranfers.Status != status
                                       select new
                                       {
                                           _tranfers.ProductId,
                                           _tranfers.Status,
                                           _tranfers.FlagDieuChuyen,
                                           CreatedBy = ContextProvider.GetFullName(lstUser, _tranfers.CreatedBy),
                                           CreatedDate = ContextProvider.GetConvertDatetime(_tranfers.CreatedDate),
                                           MaCTKM = _promotion.Code,
                                           TenCTKM = _promotion.Name,
                                           DonViThucHien = ContextProvider.GetOrganizationName(lstOrgan, _tranfers.DepartmentId),
                                       }).OrderBy(s => s.Status).OrderBy(s=>s.CreatedDate);
                    result.LstPhanBo = lstTranfers.Skip((pageNo - 1) * pageSize).Take(pageSize).ToList();

                    var total = lstTranfers.Count();
                    result.TotalPage = total % pageSize == 0 ? total / pageSize : total / pageSize + 1;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    throw;
                }
            });
            return result;
        }

        public string InitPhanBoQuaTang(List<PhanBoQuaTang> obj, ClaimsPrincipal principal, string flag, string promotionId)
        {
            var result = string.Empty;
            try
            {
                var userinfo = ContextProvider.GetUserInfo(principal);
                var departmentId = userinfo.OrganizationId;
                var _promotionId = new Guid(promotionId);
                var checkAmount = false;
                var _productId = new Guid(Constants.ID_PRODUCT_PHAN_BO_QUA_TANG);
                SessionManager.DoWork((Action<NHibernate.ISession>)(ss =>
                {
                    var product = ss.Get<Product>(_productId);
                    var stage = new Stage();

                    var status = 0;
                    var flagDieuChuyen = Guid.NewGuid();

                    // check valid amount
                    foreach (var itm in obj)
                    {
                        var giftId = new Guid(itm.GiftId);

                        // lấy quà tặng trong kho để check valid.
                        var amount = ss.Query<Store>()
                                        .Where(s =>
                                            s.DepartmentId == departmentId &&
                                            s.GiftId == giftId &&
                                            s.PromotionId == _promotionId
                                        ).Select(p => p.Amount)
                                        .FirstOrDefault();
                        var giftPhanBo = itm.chinhanh_pgd.Length * itm.Amount;
                        if (giftPhanBo < amount)
                        {
                            checkAmount = true;
                        }
                        else
                        {
                            checkAmount = false;
                            result = $"Tổng số quà tặng phân bổ là: {giftPhanBo} > số quà tặng trong kho: {amount}!\nAnh/Chị vui lòng kiểm tra lại.";
                            break;
                        }
                    }
                    if (checkAmount)
                    {

                        if (flag == Constants.DRAFT)
                        {
                            status = (int)ContextProvider.statusTransfer.Draft;
                            stage = ss.Query<Stage>().SingleOrDefault(p => p.ProductId == product.Id && p.PositionId == userinfo.Position.Id && p.Name == Constants.TAO_NHAP);
                            result = Constants.LUU_THANH_CONG;
                        }

                        if (flag == Constants.INITIALIZE)
                        {
                            status = (int)ContextProvider.statusTransfer.Initialize;
                            stage = ss.Query<Stage>().SingleOrDefault(p => p.ProductId == product.Id && p.PositionId == userinfo.Position.Id && p.Name == Constants.GUI_DUYET);
                            result = Constants.GUI_DUYET_THANH_CONG;
                        }
                        if (stage != null)
                        {
                            foreach (var itm in obj)
                            {
                                foreach (var itmChiNhanh in itm.chinhanh_pgd)
                                {
                                    var newTransfer = new TransferGift
                                    {
                                        Id = Guid.NewGuid(),
                                        Code = CreateTranferCode(product.Code),
                                        Product = product,
                                        IsFinished = false,
                                        IsComplete = false, //lanh dao duyet => true
                                        DepartmentId = userinfo.Organization.Id,
                                        PromotionId = new Guid(promotionId),
                                        Status = status,
                                        CreatedBy = userinfo.Id,
                                        CreatedDate = DateTime.ParseExact(DateTime.Now.ToString("u"), "u", CultureInfo.InvariantCulture),
                                        Deadline = null,
                                        StageCurrent = stage.Id,
                                        FlagDieuChuyen = flagDieuChuyen
                                    };

                                    var newTransferlog = new TransferGiftLog
                                    {
                                        Id = Guid.NewGuid(),
                                        TransferGift = newTransfer,
                                        AssignUserId = userinfo.Id,
                                        AssignDeaprtmentId = userinfo.Organization.Id,
                                        Comment = null,
                                        Data = null,
                                        Status = status,
                                        UpdateDate = newTransfer.CreatedDate,
                                        Stage = stage,
                                        Dealine = null,
                                        FlagDieuChuyen = flagDieuChuyen
                                    };

                                    var newTranferDetail = new TransferDetail
                                    {
                                        Id = Guid.NewGuid(),
                                        GiftId = new Guid(itm.GiftId),
                                        Amount = itm.Amount,
                                        TransferGift = newTransfer,
                                        ReceivingDepartment = new Guid(itmChiNhanh),
                                        ReceivingPromotion = _promotionId,
                                        FlagDieuChuyen = flagDieuChuyen
                                    };

                                    ss.Save(newTransfer);
                                    ss.Save(newTransferlog);
                                    ss.Save(newTranferDetail);
                                };
                            }
                        }
                        else
                            result = Constants.CHUC_NANG_NHAN_VIEN;
                    }

                }));
            }
            catch (Exception ex)
            {
                result = ex.Message;
                Console.WriteLine(result);
                throw;
            }
            return result;
        }

        public dynamic DetailPhanBoQuaTang(Guid flagDieuChuyen)
        {
            dynamic result = new ExpandoObject();
            //var userinfo = ContextProvider.GetUserInfo(principal);
            SessionManager.DoWork(ss =>
            {
                try
                {
                    var lstUser = ss.Query<User>().ToList();
                    var lstOrgan = ss.Query<Organization>().ToList();
                    var detailTranfer = ss.Query<TransferDetail>().Where(p => p.FlagDieuChuyen == flagDieuChuyen)
                    .Select(s => new
                    {
                        s.FlagDieuChuyen,
                        s.GiftId,
                        s.Amount
                    }).Distinct().ToList();

                    var tranfer = ss.Query<TransferGift>().Where(p => p.FlagDieuChuyen == flagDieuChuyen)
                        .Select(s => new
                        {
                            s.FlagDieuChuyen,
                            s.PromotionId,
                            s.Status,
                            s.DepartmentId,
                            s.CreatedBy,
                            s.CreatedDate,
                            s.NguoiDuyet,
                            s.NgayDuyet
                        }).Distinct().ToList();

                    IEnumerable<Promotion> promotion = ss.Query<Promotion>();


                    var gift = ss.Query<Gift>();

                    var detail = (from _detailTranfer in detailTranfer
                                  join _gift in gift on _detailTranfer.GiftId equals _gift.Id
                                  select new
                                  {
                                      _detailTranfer.Amount,
                                      _detailTranfer.FlagDieuChuyen,
                                      _detailTranfer.GiftId,
                                      _gift.Price,
                                      NameGift = _gift.Name,
                                      CodeGift = _gift.Code,
                                      UnitName = _gift.Unit.Name,
                                  });
                    result.ListGift = detail.ToList();

                    //
                    var ttc = (from _tranfer in tranfer
                               join _promotion in promotion on _tranfer.PromotionId equals _promotion.Id
                               select new
                               {
                                   _tranfer.Status,
                                   _tranfer.FlagDieuChuyen,
                                   CreatedBy = ContextProvider.GetFullName(lstUser, _tranfer.CreatedBy),
                                   NguoiDuyet = ContextProvider.GetFullName(lstUser, _tranfer.NguoiDuyet),
                                   NgayDuyet = ContextProvider.GetConvertDatetime(_tranfer.NgayDuyet),
                                   CreatedDate = ContextProvider.GetConvertDatetime(_tranfer.CreatedDate),
                                   MaCTKM = _promotion.Code,
                                   TenCTKM = _promotion.Name,
                                   DonViThucHien = ContextProvider.GetOrganizationName(lstOrgan, _tranfer.DepartmentId),
                               });
                    result.ThongTinChung = ttc.ToList();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            });
            return result;
        }

        public dynamic GetBranch(Guid flagDieuChuyen, Guid idGift)
        {
            dynamic result = new ExpandoObject();
            SessionManager.DoWork(ss =>
            {
                try
                {
                    result.arrBranch = ss.Query<TransferDetail>().Where(p =>
                                                p.FlagDieuChuyen == flagDieuChuyen &&
                                                p.GiftId == idGift
                                            ).
                                            Select(s => new
                                            { s.ReceivingDepartment }).ToArray();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    result.Message = ex.Message;
                    throw;
                }
            });
            return result;
        }

        public string Duyet(Guid flagDieuChuyen, string flag, ClaimsPrincipal principal)
        {
            var result = string.Empty;
            SessionManager.DoWork((Action<NHibernate.ISession>)(ss =>
            {
                try
                {
                    var _productId = new Guid(Constants.ID_PRODUCT_PHAN_BO_QUA_TANG);
                    var product = ss.Get<Product>(_productId);
                    var userinfo = ContextProvider.GetUserInfo(principal);
                    var date = DateTime.ParseExact(DateTime.Now.ToString("u"), "u", CultureInfo.InvariantCulture);
                    var tranfer = ss.Query<TransferGift>().Where(p => p.FlagDieuChuyen == flagDieuChuyen).ToList();
                    var promotionId = new Guid();

                    if (flag == Constants.REFUSE)
                    {
                        var stage = ss.Query<Stage>().SingleOrDefault(p =>
                                    p.ProductId == product.Id &&
                                    p.PositionId == userinfo.Position.Id &&
                                    p.Name == Constants.TU_CHOI_DUYET);
                        if (stage != null)
                        {
                            var status = (int)ContextProvider.statusTransfer.Refuse;
                            result = Constants.TU_CHOI_DUYET_THANH_CONG;

                            foreach (var itm in tranfer)
                            {
                                itm.Status = status;
                                itm.StageCurrent = stage.Id;
                                itm.NguoiDuyet = userinfo.Id;
                                itm.NgayDuyet = date;

                                var newTranferLog = new TransferGiftLog
                                {
                                    Id = Guid.NewGuid(),
                                    TransferGift = itm,
                                    AssignUserId = userinfo.Id,
                                    AssignDeaprtmentId = userinfo.Organization.Id,
                                    Comment = null,
                                    Data = null,
                                    Status = status,
                                    UpdateDate = date,
                                    Stage = stage,
                                    Dealine = null,
                                    FlagDieuChuyen = flagDieuChuyen
                                };
                                ss.Save(newTranferLog);
                            }
                        }
                        else
                            result = Constants.CHUC_NANG_LANH_DAO;
                    }

                    else
                    {
                        // check valid amount
                        var checkAmount = false;

                        var tranferDetailDisTinct = ss.Query<TransferDetail>()
                            .Where(p => p.FlagDieuChuyen == flagDieuChuyen)
                            .Select(s => new { s.ReceivingPromotion, s.Amount, s.GiftId }).Distinct().ToList();

                        foreach (var itm in tranferDetailDisTinct)
                        {
                            var count = ss.Query<TransferDetail>()
                                .Where(p => p.FlagDieuChuyen == flagDieuChuyen && p.GiftId == itm.GiftId).Count();
                            var giftPhanBo = itm.Amount * count;

                            promotionId = tranferDetailDisTinct.FirstOrDefault().ReceivingPromotion.Value;
                            var amountInStore = ss.Query<Store>()
                                .Where(s => s.PromotionId == promotionId && s.DepartmentId == userinfo.OrganizationId && s.GiftId == itm.GiftId)
                                .FirstOrDefault().Amount;
                            if (giftPhanBo < amountInStore)
                                checkAmount = true;

                            else
                            {
                                checkAmount = false;
                                result = $"Tổng số quà tặng điều chuyển là: {giftPhanBo} > số quà tặng trong kho: {amountInStore}!\nAnh/Chị vui lòng kiểm tra lại.";
                                break;
                            }
                        }
                        if (checkAmount)
                        {
                            var stage = new Stage();
                            var status = 99;

                            if (flag == Constants.INITIALIZE)
                            {
                                stage = ss.Query<Stage>().SingleOrDefault(p =>
                                            p.ProductId == product.Id &&
                                            p.PositionId == userinfo.Position.Id &&
                                            p.Name == Constants.GUI_DUYET);
                                if (stage != null)
                                {
                                    status = (int)ContextProvider.statusTransfer.Initialize;
                                    result = Constants.GUI_DUYET_THANH_CONG;
                                }
                                else
                                    result = Constants.CHUC_NANG_NHAN_VIEN;
                            }

                            if (flag == Constants.APPROVE)
                            {
                                stage = ss.Query<Stage>().SingleOrDefault(p =>
                                            p.ProductId == product.Id &&
                                            p.PositionId == userinfo.Position.Id &&
                                            p.Name == Constants.DUYET);
                                if (stage != null)
                                {
                                    status = (int)ContextProvider.statusTransfer.Approve;
                                    result = Constants.DUYET_THANH_CONG;

                                    foreach (var itm in tranferDetailDisTinct)
                                    {
                                        // update store trong kho HO.
                                        var store = ss.Query<Store>()
                                            .Single(s => s.PromotionId == promotionId && s.GiftId == itm.GiftId && s.DepartmentId == userinfo.OrganizationId);
                                        var count = ss.Query<TransferDetail>()
                                            .Where(p => p.FlagDieuChuyen == flagDieuChuyen && p.GiftId == itm.GiftId).Count();
                                        store.Amount -= itm.Amount * count;
                                        store.UpdatedDate = date;
                                    }

                                    // update store chi nhánh, phòng giao dịch.
                                    var tranferDetail = ss.Query<TransferDetail>().Where(s => s.FlagDieuChuyen == flagDieuChuyen).ToList();
                                    foreach (var itm in tranferDetail)
                                    {
                                        var updateStore = ss.Query<Store>().Where(s =>
                                            s.DepartmentId == itm.ReceivingDepartment &&
                                            s.PromotionId == itm.ReceivingPromotion &&
                                            s.GiftId == itm.GiftId).FirstOrDefault();

                                        if (updateStore != null)
                                        {
                                            updateStore.Amount += itm.Amount;
                                            updateStore.UpdatedDate = date;
                                        }

                                        else
                                        {
                                            var newStore = new Store
                                            {
                                                Id = new Guid(),
                                                DepartmentId = itm.ReceivingDepartment.Value,
                                                PromotionId = itm.ReceivingPromotion,
                                                GiftId = itm.GiftId,
                                                Amount = itm.Amount,
                                                UpdatedDate = date
                                            };
                                            ss.Save(newStore);
                                        }
                                    }
                                }
                                else
                                    result = Constants.CHUC_NANG_LANH_DAO;
                            }

                            if (stage != null)
                            {
                                foreach (var itm in tranfer)
                                {
                                    itm.Status = status;
                                    itm.StageCurrent = stage.Id;
                                    itm.NguoiDuyet = userinfo.Id;
                                    itm.NgayDuyet = date;

                                    var newTranferLog = new TransferGiftLog
                                    {
                                        Id = Guid.NewGuid(),
                                        TransferGift = itm,
                                        AssignUserId = userinfo.Id,
                                        AssignDeaprtmentId = userinfo.Organization.Id,
                                        Comment = null,
                                        Data = null,
                                        Status = status,
                                        UpdateDate = date,
                                        Stage = stage,
                                        Dealine = null,
                                        FlagDieuChuyen = flagDieuChuyen
                                    };
                                    ss.Save(newTranferLog);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    result = ex.Message;
                }
            }));
            return result;
        }

        #region Hoan Phan Bo
        public string HoanPhanBo_LuuHoacGuiDuyet(List<PhanBoQuaTang> obj, string flag, Guid promotionId, ClaimsPrincipal principal)
        {
            var result = string.Empty;
            var userinfo = ContextProvider.GetUserInfo(principal);
            var departmentId = userinfo.OrganizationId;
            var checkAmount = false;
            var _productId = new Guid(Constants.ID_PRODUCT_HOAN_PHAN_BO_QUA_TANG);
            SessionManager.DoWork((Action<NHibernate.ISession>)(ss =>
            {
                try
                {
                    var product = ss.Get<Product>(_productId);
                    var stage = new Stage();

                    var status = 0;
                    var stageCurrent = new Guid();
                    var flagDieuChuyen = Guid.NewGuid();
                    stage = ss.Query<Stage>().Where(p =>
                                       p.ProductId == product.Id &&
                                       p.PositionId == userinfo.Position.Id &&
                                       (p.Name == Constants.TAO_NHAP || p.Name == Constants.GUI_DUYET)).FirstOrDefault();
                    if (stage != null)
                    {
                        // valid amount gift
                        foreach (var itm in obj)
                        {
                            var giftId = new Guid(itm.GiftId);

                            // lấy quà tặng trong kho để check valid.
                            var amount = ss.Query<Store>()
                                            .Single(s => s.DepartmentId == departmentId && s.GiftId == giftId && s.PromotionId == promotionId
                                            ).Amount;
                            if (itm.Amount < amount)
                            {
                                checkAmount = true;
                            }
                            else
                            {
                                checkAmount = false;
                                result = $"{itm.NameGift} Trong kho còn {amount} < {itm.Amount}!\nAnh/Chị vui lòng kiểm tra lại.";
                                break;
                            }
                        }

                        if (checkAmount)
                        {
                            if (flag == Constants.DRAFT)
                            {
                                status = (int)ContextProvider.statusTransfer.Draft;
                                stageCurrent = new Guid(Constants.ID_HOAN_PHAN_BO_TAO_NHAP);
                                stage = ss.Query<Stage>().Single(p =>
                                           p.ProductId == product.Id &&
                                           p.PositionId == userinfo.Position.Id &&
                                           p.Name == Constants.TAO_NHAP);
                                result = Constants.LUU_THANH_CONG;
                            }

                            if (flag == Constants.INITIALIZE)
                            {
                                status = (int)ContextProvider.statusTransfer.Initialize;
                                stageCurrent = new Guid(Constants.ID_HOAN_PHAN_BO_GUI_DUYET);
                                stage = ss.Query<Stage>().Single(p =>
                                            p.ProductId == product.Id &&
                                            p.PositionId == userinfo.Position.Id &&
                                            p.Name == Constants.GUI_DUYET);
                                result = Constants.GUI_DUYET_THANH_CONG;
                            }

                            var newTransfer = new TransferGift
                            {
                                Id = Guid.NewGuid(),
                                Code = CreateTranferCode(product.Code),
                                Product = product,
                                IsFinished = false,
                                IsComplete = false, //lanh dao duyet => true
                                DepartmentId = userinfo.Organization.Id,
                                PromotionId = promotionId,
                                Status = status,
                                CreatedBy = userinfo.Id,
                                CreatedDate = DateTime.ParseExact(DateTime.Now.ToString("u"), "u", CultureInfo.InvariantCulture),
                                Deadline = null,
                                StageCurrent = stageCurrent,
                                FlagDieuChuyen = flagDieuChuyen
                            };

                            var newTransferlog = new TransferGiftLog
                            {
                                Id = Guid.NewGuid(),
                                TransferGift = newTransfer,
                                AssignUserId = userinfo.Id,
                                AssignDeaprtmentId = userinfo.Organization.Id,
                                Comment = null,
                                Data = null,
                                Status = status,
                                UpdateDate = newTransfer.CreatedDate,
                                Stage = stage,
                                Dealine = null,
                                FlagDieuChuyen = flagDieuChuyen
                            };
                            ss.Save(newTransfer);
                            ss.Save(newTransferlog);

                            foreach (var itm in obj)
                            {
                                var newTranferDetail = new TransferDetail
                                {
                                    Id = Guid.NewGuid(),
                                    GiftId = new Guid(itm.GiftId),
                                    Amount = itm.Amount,
                                    TransferGift = newTransfer,
                                    ReceivingDepartment = new Guid(Constants.ID_PHONG_QUAN_LY_BAN_HANG),
                                    ReceivingPromotion = promotionId,
                                    FlagDieuChuyen = flagDieuChuyen
                                };
                                ss.Save(newTranferDetail);
                            }
                        }
                    }
                    else
                        result = Constants.CHUC_NANG_NHAN_VIEN;
                }
                catch (Exception ex)
                {
                    result = ex.Message;
                    throw;
                }
            }));
            return result;
        }

        public string HoanPhanBo_Duyet(Guid flagDieuChuyen, string flag, ClaimsPrincipal principal)
        {
            var result = string.Empty;
            SessionManager.DoWork((Action<NHibernate.ISession>)(ss =>
            {
                try
                {
                    var _productId = new Guid(Constants.ID_PRODUCT_HOAN_PHAN_BO_QUA_TANG);
                    var product = ss.Get<Product>(_productId);
                    var userinfo = ContextProvider.GetUserInfo(principal);
                    var date = DateTime.ParseExact(DateTime.Now.ToString("u"), "u", CultureInfo.InvariantCulture);
                    var tranfer = ss.Query<TransferGift>().Where(p => p.FlagDieuChuyen == flagDieuChuyen).ToList();
                    var promotionId = new Guid();

                    if (flag == Constants.REFUSE) // lãnh đạo từ chối duyệt
                    {
                        var stage = ss.Query<Stage>().Where(p =>
                                    p.ProductId == product.Id &&
                                    p.PositionId == userinfo.Position.Id &&
                                    p.Name == Constants.TU_CHOI_DUYET).FirstOrDefault();
                        var stageCurrent = new Guid(Constants.ID_HOAN_PHAN_BO_TU_CHOI_DUYET);
                        var status = (int)ContextProvider.statusTransfer.Refuse;
                        result = Constants.TU_CHOI_DUYET_THANH_CONG;

                        foreach (var itm in tranfer)
                        {
                            itm.Status = status;
                            itm.StageCurrent = stageCurrent;
                            itm.CreatedBy = userinfo.OrganizationId;
                            itm.CreatedDate = date;

                            var newTranferLog = new TransferGiftLog
                            {
                                Id = Guid.NewGuid(),
                                TransferGift = itm,
                                AssignUserId = userinfo.Id,
                                AssignDeaprtmentId = userinfo.Organization.Id,
                                Comment = null,
                                Data = null,
                                Status = status,
                                UpdateDate = date,
                                Stage = stage,
                                Dealine = null,
                                FlagDieuChuyen = flagDieuChuyen
                            };
                            ss.Save(newTranferLog);
                        }
                    }

                    else
                    {
                        // check valid amount
                        var checkAmount = false;

                        var tranferDetailDisTinct = ss.Query<TransferDetail>()
                            .Where(p => p.FlagDieuChuyen == flagDieuChuyen)
                            .Select(s => new { s.ReceivingPromotion, s.Amount, s.GiftId }).Distinct().ToList();

                        foreach (var itm in tranferDetailDisTinct)
                        {
                            var amountGift = itm.Amount;

                            promotionId = tranferDetailDisTinct.FirstOrDefault().ReceivingPromotion.Value;
                            var amountInStore = ss.Query<Store>()
                                .Where(s => s.PromotionId == promotionId && s.DepartmentId == userinfo.OrganizationId && s.GiftId == itm.GiftId)
                                .FirstOrDefault().Amount;

                            if (amountGift < amountInStore)
                                checkAmount = true;

                            else
                            {
                                checkAmount = false;
                                result = $"Tổng số quà tặng hoàn phân bổ là: {amountGift} > số quà tặng trong kho: {amountInStore}!\nAnh/Chị vui lòng kiểm tra lại.";
                                break;
                            }
                        }
                        if (checkAmount)
                        {
                            var stageCurrent = new Guid();
                            var stage = new Stage();
                            var status = 99;

                            if (flag == Constants.INITIALIZE)
                            {
                                stage = ss.Query<Stage>().Where((System.Linq.Expressions.Expression<Func<Stage, bool>>)(p =>
                                            p.ProductId == product.Id &&
                                            p.PositionId == userinfo.Position.Id &&
                                            p.Name == Constants.GUI_DUYET)).FirstOrDefault();
                                stageCurrent = new Guid(Constants.ID_HOAN_PHAN_BO_GUI_DUYET);
                                status = (int)ContextProvider.statusTransfer.Initialize;
                                result = Constants.GUI_DUYET_THANH_CONG;
                            }

                            if (flag == Constants.APPROVE)
                            {
                                stage = ss.Query<Stage>().Where(p =>
                                            p.ProductId == product.Id &&
                                            p.PositionId == userinfo.Position.Id &&
                                            p.Name == Constants.DUYET).FirstOrDefault();
                                stageCurrent = new Guid(Constants.ID_HOAN_PHAN_BO_DUYET);
                                status = (int)ContextProvider.statusTransfer.Approve;
                                result = Constants.DUYET_THANH_CONG;

                                foreach (var itm in tranferDetailDisTinct)
                                {
                                    // update store chi nhánh, phòng giao dịch
                                    var store = ss.Query<Store>()
                                        .Single(s => s.PromotionId == promotionId && s.GiftId == itm.GiftId && s.DepartmentId == userinfo.OrganizationId);
                                    store.Amount -= itm.Amount;
                                    store.UpdatedDate = date;
                                }

                                // update store HO.
                                var tranferDetail = ss.Query<TransferDetail>().Where(s => s.FlagDieuChuyen == flagDieuChuyen).ToList();
                                foreach (var itm in tranferDetail)
                                {
                                    var updateStore = ss.Query<Store>().Where(s =>
                                        s.DepartmentId == itm.ReceivingDepartment &&
                                        s.PromotionId == itm.ReceivingPromotion &&
                                        s.GiftId == itm.GiftId).FirstOrDefault();

                                    if (updateStore != null)
                                    {
                                        updateStore.Amount += itm.Amount;
                                        updateStore.UpdatedDate = date;
                                    }

                                    //else
                                    //{
                                    //    var newStore = new Store
                                    //    {
                                    //        Id = new Guid(),
                                    //        DepartmentId = itm.ReceivingDepartment.Value,
                                    //        PromotionId = itm.ReceivingPromotion,
                                    //        GiftId = itm.GiftId,
                                    //        Amount = itm.Amount,
                                    //        UpdatedDate = date
                                    //    };
                                    //    ss.Save(newStore);
                                    //}
                                }
                            }

                            foreach (var itm in tranfer)
                            {
                                itm.Status = status;
                                itm.StageCurrent = stageCurrent;
                                itm.CreatedBy = userinfo.OrganizationId;
                                itm.CreatedDate = date;

                                var newTranferLog = new TransferGiftLog
                                {
                                    Id = Guid.NewGuid(),
                                    TransferGift = itm,
                                    AssignUserId = userinfo.Id,
                                    AssignDeaprtmentId = userinfo.Organization.Id,
                                    Comment = null,
                                    Data = null,
                                    Status = status,
                                    UpdateDate = date,
                                    Stage = stage,
                                    Dealine = null,
                                    FlagDieuChuyen = flagDieuChuyen
                                };
                                ss.Save(newTranferLog);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    result = ex.Message;
                }
            }));
            return result;
        }
        #endregion

        #region private
        private string ValidAmount()
        {
            var result = string.Empty;
            return result;
        }
        private string CreateTranferCode(string type)
        {
            var dateTime = DateTime.Now.ToString("ddMMyyyy");
            StringBuilder builder = new StringBuilder();
            builder.Append(dateTime).Append("/").Append(type).Append("/");
            builder.Append(RandomString(3));
            builder.Append(RandomNumber(100, 999));

            return builder.ToString();
        }

        private int RandomNumber(int min, int max)
        {
            Random random = new Random();
            return random.Next(min, max);
        }

        private string RandomString(int size)
        {
            StringBuilder builder = new StringBuilder();
            Random random = new Random();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }
            return builder.ToString().ToUpper();
        }
        #endregion
    }
}
