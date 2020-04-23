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

        public dynamic Get(int pageNo, int pageSize, string organizationId, string promotionId, ClaimsPrincipal principal)
        {
            //int pageSize = 20;
            dynamic result = new ExpandoObject();
            var userinfo = ContextProvider.GetUserInfo(principal);
            var isTypeUser = ContextProvider.CheckPermission(userinfo.PermisionId);
            var isLDCN_PGD = isTypeUser == 3 && userinfo.Position.IsLeader ? true : false;
            SessionManager.DoWork(ss =>
            {
                try
                {
                    var lstUser = ss.Query<User>().ToList();
                    var lstOrgan = ss.Query<Organization>().ToList();

                    var tranfers = ss.Query<TransferGift>().Where(p => p.FlagDieuChuyen != new Guid(Constants.GUIDE_TYPE_NULL)).
                    Select(s => new
                    {
                        s.Id,
                        s.Code,
                        s.FlagDieuChuyen,
                        s.CreatedBy,
                        s.PromotionId,
                        ProductId = s.Product.Id,
                        s.Status,
                        s.DepartmentId,
                        s.CreatedDate,
                        s.NguoiDuyet,
                        s.NgayDuyet,
                        s.NumberOdEdit
                    }).Distinct().ToList();

                    List<Guid> Ids = tranfers.Select(s => s.Id).ToList();
                    if (isTypeUser == 3)
                    {
                        var tranfersIds = ss.Query<TransferDetail>().Where(p => Ids.Contains(p.TransferGift.Id) && p.ReceivingDepartment == userinfo.OrganizationId)
                        .Select(s => s.TransferGift.Id).Distinct().ToList();
                        tranfers = tranfers.Where(w => (tranfersIds.Contains(w.Id) && (w.Status == 2 || w.Status == 99)) || w.DepartmentId == userinfo.OrganizationId).ToList();
                    }
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
                                       orderby _tranfers.CreatedDate
                                       select new
                                       {
                                           Id = _tranfers.Id,
                                           _tranfers.ProductId,
                                           _tranfers.Status,
                                           _tranfers.FlagDieuChuyen,
                                           CreatedBy = ContextProvider.GetFullName(lstUser, _tranfers.CreatedBy),
                                           _tranfers.DepartmentId,
                                           CreatedDate = ContextProvider.GetConvertDatetime(_tranfers.CreatedDate),
                                           MaCTKM = _promotion.Code,
                                           TenCTKM = _promotion.Name,
                                           Code = _tranfers.Code,
                                           SoLanPBo = _tranfers.NumberOdEdit,
                                           DonViThucHien = ContextProvider.GetOrganizationName(lstOrgan, _tranfers.DepartmentId),
                                           NguoiDuyet = _tranfers.NguoiDuyet != null ? ContextProvider.GetFullName(lstUser, _tranfers.NguoiDuyet) : "",
                                           NgayDuyet = ContextProvider.GetConvertDatetime(_tranfers.NgayDuyet),
                                           OrderByCreateDate = _tranfers.CreatedDate
                                       }).ToList();//.OrderBy(s => s.Status).OrderBy(s => s.CreatedDate);
                    if (isTypeUser == 2)
                    {
                        if (userinfo.Position.IsLeader)
                            lstTranfers = lstTranfers.Where(w => (w.Status == 1 && w.DepartmentId == userinfo.OrganizationId) || w.Status == 4 || w.Status == 2 || w.Status == 99).ToList();
                    }
                    result.LstPhanBo = lstTranfers.OrderByDescending(od => od.OrderByCreateDate).Skip((pageNo - 1) * pageSize).Take(pageSize).ToList();

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

        public dynamic InitPhanBoQuaTang(List<PhanBoQuaTang> obj, ClaimsPrincipal principal, string flag, string promotionId)
        {
            var result = string.Empty;
            var checkAmount = false;
            try
            {
                var userinfo = ContextProvider.GetUserInfo(principal);
                var departmentId = userinfo.OrganizationId;
                var _promotionId = new Guid(promotionId);
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
                        var countAmount = ss.Query<TransferDetail>()
                                             .Where(s =>
                                                 s.GiftId == giftId &&
                                                 s.ReceivingPromotion == _promotionId &&
                                                 s.TransferGift.Status != 2
                                             ).Select(p => p.Amount).ToList()
                                             .Sum();
                        var giftPhanBo = itm.chinhanh_pgd.Length * itm.Amount;
                        if (giftPhanBo <= (amount - countAmount))
                        {
                            checkAmount = true;
                        }
                        else
                        {
                            checkAmount = false;
                            result = $"Tổng số quà tặng phân bổ là: {giftPhanBo} > số quà tặng đã khai báo cho CTKM: {(amount - countAmount)}!\nAnh/Chị vui lòng kiểm tra lại.";
                            break;
                        }
                    }
                    if (checkAmount)
                    {
                        var promotion = ss.Get<Promotion>(_promotionId);
                        int numberOdEdit = promotion.NumberOdEdit + 1;
                        if (flag == Constants.DRAFT)
                        {
                            status = (int)ContextProvider.statusTransfer.Draft;
                            stage = ss.Query<Stage>().SingleOrDefault(p => p.ProductId == product.Id && p.PositionId == userinfo.Position.Id && p.Name == Constants.TAO_NHAP);
                            promotion.NumberOdEdit = numberOdEdit;
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
                                        FlagDieuChuyen = flagDieuChuyen,
                                        NumberOdEdit = numberOdEdit,
                                        UpdateDate = DateTime.ParseExact(DateTime.Now.ToString("u"), "u", CultureInfo.InvariantCulture)
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
                checkAmount = false;
                Console.WriteLine(result);
                throw;
            }
            return new { result, checkAmount };
        }
        public string UpdatePhanBoQuaTang(List<TransferDetail> obj, ClaimsPrincipal principal, string tranferId)
        {
            var result = string.Empty;
            try
            {
                var userinfo = ContextProvider.GetUserInfo(principal);
                var departmentId = userinfo.OrganizationId;
                var _tranferId = new Guid(tranferId);
                var checkAmount = false;
                var _productId = new Guid(Constants.ID_PRODUCT_PHAN_BO_QUA_TANG);
                SessionManager.DoWork((Action<NHibernate.ISession>)(ss =>
                {
                    var tranferGift = ss.Get<TransferGift>(_tranferId);
                    var tranferDetail = ss.Query<TransferDetail>().Where(w => w.TransferGift.Id == _tranferId).ToList();
                    var product = ss.Get<Product>(_productId);
                    var stage = new Stage();

                    var status = 0;
                    var flagDieuChuyen = Guid.NewGuid();

                    // check valid amount
                    foreach (var itm in obj)
                    {
                        var giftId = itm.GiftId;

                        // lấy quà tặng trong kho để check valid.
                        var amount = ss.Query<Store>()
                                        .Where(s =>
                                            s.DepartmentId == departmentId &&
                                            s.GiftId == giftId &&
                                            s.PromotionId == tranferGift.PromotionId
                                        ).Select(p => p.Amount)
                                        .FirstOrDefault();
                        var countAmount = ss.Query<TransferDetail>()
                                        .Where(s =>
                                            s.GiftId == giftId &&
                                            s.ReceivingPromotion == tranferGift.PromotionId &&
                                            s.Id != itm.Id
                                        ).Select(p => p.Amount).ToList()
                                        .Sum();
                        var giftPhanBo = itm.Amount;
                        if (giftPhanBo <= (amount - countAmount))
                        {
                            checkAmount = true;
                        }
                        else
                        {
                            checkAmount = false;
                            result = $"Tổng số quà tặng phân bổ là: {giftPhanBo} > số quà tặng đã khai báo cho CTKM: {(amount - countAmount)}!\nAnh/Chị vui lòng kiểm tra lại.";
                            break;
                        }
                    }
                    if (checkAmount)
                    {

                        status = (int)ContextProvider.statusTransfer.Draft;
                        stage = ss.Query<Stage>().SingleOrDefault(p => p.ProductId == product.Id && p.PositionId == userinfo.Position.Id && p.Name == Constants.TAO_NHAP);
                        result = Constants.LUU_THANH_CONG;

                        if (stage != null)
                        {
                            tranferGift.UpdateDate = DateTime.Now;
                            foreach (var itm in obj)
                            {
                                var itemSave = tranferDetail.FirstOrDefault(f => f.Id == itm.Id);
                                if (itemSave != null)
                                {
                                    itemSave.Amount = itm.Amount;
                                    ss.SaveOrUpdate(itemSave);
                                }
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
        public dynamic DetailPhanBoQuaTang(ClaimsPrincipal principal, Guid _tranferId)//Guid flagDieuChuyen)
        {
            dynamic result = new ExpandoObject();
            var userinfo = ContextProvider.GetUserInfo(principal);
            var isTypeUser = ContextProvider.CheckPermission(userinfo.PermisionId);
            var isLDCN_PGD = isTypeUser == 3 && userinfo.Position.IsLeader ? true : false;
            SessionManager.DoWork(ss =>
            {
                try
                {
                    var lstUser = ss.Query<User>().ToList();
                    var lstOrgan = ss.Query<Organization>().ToList();
                    var detailTranfer = ss.Query<TransferDetail>().Where(p => p.TransferGift.Id == _tranferId)// p.FlagDieuChuyen == flagDieuChuyen)
                    .Select(s => new
                    {
                        s.Id,
                        TransferId = s.TransferGift.Id,
                        s.FlagDieuChuyen,
                        s.GiftId,
                        s.ReceivingDepartment,
                        s.ReceivingPromotion,
                        s.Amount
                    }).Distinct().ToList();

                    var tranfer = ss.Query<TransferGift>().Where(p => p.Id == _tranferId)// p.FlagDieuChuyen == flagDieuChuyen)
                        .Select(s => new
                        {
                            s.Id,
                            s.FlagDieuChuyen,
                            s.PromotionId,
                            s.Status,
                            s.DepartmentId,
                            s.CreatedBy,
                            s.CreatedDate,
                            s.NguoiDuyet,
                            s.NgayDuyet,
                            s.UpdateDate,
                            s.NumberOdEdit
                        }).Distinct().ToList();

                    IEnumerable<Promotion> promotion = ss.Query<Promotion>();


                    var gift = ss.Query<Gift>();

                    var detail = (from _detailTranfer in detailTranfer
                                  join _gift in gift on _detailTranfer.GiftId equals _gift.Id
                                  select new
                                  {
                                      _detailTranfer.Id,
                                      _detailTranfer.Amount,
                                      _detailTranfer.FlagDieuChuyen,
                                      _detailTranfer.GiftId,
                                      _gift.Price,
                                      DepartmentName = ContextProvider.GetOrganizationName(lstOrgan, _detailTranfer.ReceivingDepartment),
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
                                   UpdateDate = ContextProvider.GetConvertDatetime(_tranfer.UpdateDate),
                                   NumberOdEdit = _tranfer.NumberOdEdit,
                                   MaCTKM = _promotion.Code,
                                   TenCTKM = _promotion.Name,
                                   DonViThucHien = ContextProvider.GetOrganizationName(lstOrgan, _tranfer.DepartmentId),
                                   IsTypeUser = isTypeUser,//1-admin/2-QLBH/3-CN-PGD
                                   IsLeader = userinfo.Position.IsLeader,
                                   IsDep = userinfo.OrganizationId == _tranfer.DepartmentId
                               });
                    result.ThongTinChung = ttc.ToList();
                    result.AssignTransfer = ss.Query<TransferGiftLog>().Any(a => a.TransferGift.Id == _tranferId && a.AssignUserId == userinfo.Id && a.Status != 3);
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

        public string Duyet(Guid flagDieuChuyen, string flag, Guid id, ClaimsPrincipal principal)
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
                    var itm = ss.Query<TransferGift>().FirstOrDefault(p => p.FlagDieuChuyen == flagDieuChuyen && p.Id == id);
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

                            //foreach (var itm in tranfer)
                            // {
                            itm.Status = status;
                            itm.StageCurrent = stage.Id;
                            itm.NguoiDuyet = userinfo.Id;
                            itm.NgayDuyet = date;
                            itm.UpdateDate = date;

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
                            //}
                        }
                        else
                            result = Constants.CHUC_NANG_LANH_DAO;
                    }

                    else
                    {
                        // check valid amount
                        var checkAmount = false;

                        var tranferDetailDisTinct = ss.Query<TransferDetail>()
                            .Where(p => p.FlagDieuChuyen == flagDieuChuyen && p.TransferGift.Id == id)
                            .Select(s => new { s.ReceivingPromotion, s.Amount, s.GiftId }).Distinct().ToList();

                        foreach (var item in tranferDetailDisTinct)
                        {
                            //var count = ss.Query<TransferDetail>()
                            //    .Where(p => p.FlagDieuChuyen == flagDieuChuyen && p.TransferGift.Id == id && p.GiftId == itm.GiftId && p.ReceivingDepartment== userinfo.OrganizationId).Count();
                            var giftPhanBo = item.Amount;// * count;

                            promotionId = tranferDetailDisTinct.FirstOrDefault().ReceivingPromotion.Value;
                            var amountInStore = ss.Query<Store>()
                                .Where(s => s.PromotionId == promotionId && s.DepartmentId == userinfo.OrganizationId && s.GiftId == item.GiftId)
                                .FirstOrDefault()?.Amount;
                            var countAmount = ss.Query<TransferDetail>()
                                            .Where(s =>
                                                s.GiftId == item.GiftId &&
                                                s.ReceivingDepartment == userinfo.Organization.Id &&
                                                s.ReceivingPromotion == promotionId && s.FlagDieuChuyen == flagDieuChuyen
                                            ).Select(p => p.Amount).ToList()
                                            .Sum();
                            if (giftPhanBo <= (amountInStore - countAmount) || itm.Status == 99)
                                checkAmount = true;

                            else
                            {
                                checkAmount = false;
                                result = $"Tổng số quà tặng điều chuyển là: {giftPhanBo} > số quà tặng trong kho: {(amountInStore - countAmount)}!\nAnh/Chị vui lòng kiểm tra lại.";
                                break;
                            }
                        }
                        if (checkAmount)
                        {
                            var stage = new Stage();
                            var status = 99;

                            var newTranferLog = new TransferGiftLog();
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
                                    newTranferLog = new TransferGiftLog
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
                                    itm.Status = status;
                                    itm.StageCurrent = stage.Id;
                                    itm.NguoiDuyet = null;
                                    itm.NgayDuyet = null;
                                    itm.UpdateDate = date;
                                    newTranferLog.Status = status;
                                    ss.Save(newTranferLog);
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

                                result = Constants.DUYET_THANH_CONG;
                                newTranferLog = new TransferGiftLog
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
                                if (itm.Status == 99)//Dang tai LDHO
                                {
                                    if (stage != null)
                                    {
                                        //status = (int)ContextProvider.statusTransfer.Approve;

                                        foreach (var item in tranferDetailDisTinct)
                                        {
                                            // update store trong kho HO.
                                            var store = ss.Query<Store>()
                                                .FirstOrDefault(s => s.PromotionId == promotionId && s.GiftId == item.GiftId && s.DepartmentId == itm.DepartmentId);
                                            //var count = ss.Query<TransferDetail>()
                                            //    .Where(p => p.FlagDieuChuyen == flagDieuChuyen && p.TransferGift.Id == id && p.GiftId == itm.GiftId).Count();
                                            store.Amount -= item.Amount;// * count;
                                            store.UpdatedDate = date;
                                        }

                                        // update store chi nhánh, phòng giao dịch.
                                        var tranferDetail = ss.Query<TransferDetail>().Where(s => s.FlagDieuChuyen == flagDieuChuyen && s.TransferGift.Id == id).ToList();
                                        foreach (var item in tranferDetail)
                                        {
                                            var updateStore = ss.Query<Store>().Where(s =>
                                                s.DepartmentId == item.ReceivingDepartment &&
                                                s.PromotionId == item.ReceivingPromotion &&
                                                s.GiftId == item.GiftId).FirstOrDefault();

                                            if (updateStore != null)
                                            {
                                                updateStore.Amount += item.Amount;
                                                updateStore.UpdatedDate = date;
                                            }

                                            else
                                            {
                                                var newStore = new Store
                                                {
                                                    Id = new Guid(),
                                                    DepartmentId = item.ReceivingDepartment.Value,
                                                    PromotionId = item.ReceivingPromotion,
                                                    GiftId = item.GiftId,
                                                    Amount = item.Amount,
                                                    UpdatedDate = date
                                                };
                                                ss.Save(newStore);
                                            }
                                        }

                                        itm.Status = (int)ContextProvider.statusTransfer.Approve;
                                        itm.StageCurrent = stage.Id;
                                        itm.NguoiDuyet = userinfo.Id;
                                        itm.NgayDuyet = date;
                                        itm.UpdateDate = date;

                                        newTranferLog.Status = 2;
                                        ss.Save(newTranferLog);
                                    }
                                    else
                                        result = Constants.CHUC_NANG_LANH_DAO;

                                }
                                else if (itm.Status == 1)
                                {
                                    itm.Status = 99;
                                    newTranferLog.Status = 99;
                                    ss.Save(newTranferLog);
                                }
                                else
                                    result = Constants.CHUC_NANG_LANH_DAO;
                            }

                            //if (stage != null)
                            //{
                            //    //foreach (var itm in tranfer)
                            //    //{
                            //    itm.Status = status;
                            //    itm.StageCurrent = stage.Id;
                            //    itm.NguoiDuyet = userinfo.Id;
                            //    itm.NgayDuyet = date;
                            //    itm.UpdateDate = date;

                            //    newTranferLog.Status = 2;
                            //    ss.Save(newTranferLog);
                            //}
                            //}
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
            var isTypeUser = ContextProvider.CheckPermission(userinfo.PermisionId);
            var isLDCN_PGD = isTypeUser == 3 && userinfo.Position.IsLeader ? true : false;
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
                            var countAmount = ss.Query<TransferDetail>()
                                            .Where(s =>
                                                s.GiftId == giftId &&
                                                s.ReceivingDepartment == userinfo.Organization.Id &&
                                                s.ReceivingPromotion == promotionId &&
                                                s.TransferGift.Product.Id == Guid.Parse(Constants.ID_PRODUCT_HOAN_PHAN_BO_QUA_TANG)
                                            ).Select(p => p.Amount).ToList()
                                            .Sum();
                            if (itm.Amount <= (amount - countAmount))
                            {
                                checkAmount = true;
                            }
                            else
                            {
                                checkAmount = false;
                                result = $"{itm.NameGift} Trong kho còn {amount - countAmount} < {itm.Amount}!\nAnh/Chị vui lòng kiểm tra lại.";
                                break;
                            }
                        }

                        if (checkAmount)
                        {
                            var promotion = ss.Get<Promotion>(promotionId);
                            int numberOdEdit = promotion.SoLanHPB + 1;
                            promotion.SoLanHPB = numberOdEdit;
                            if (flag == Constants.DRAFT)
                            {
                                status = (int)ContextProvider.statusTransfer.Draft;
                                stageCurrent = new Guid(Constants.ID_HOAN_PHAN_BO_TAO_NHAP);
                                stage = ss.Query<Stage>().Single(p =>
                                           p.ProductId == product.Id &&
                                           p.PositionId == userinfo.Position.Id &&
                                           p.Name == Constants.TAO_NHAP);
                                promotion.NumberOdEdit = numberOdEdit;
                                result = Constants.LUU_THANH_CONG;
                            }

                            if (flag == Constants.INITIALIZE)
                            {
                                status = (int)ContextProvider.statusTransfer.Initialize;
                                if (isTypeUser == 3)
                                {
                                    status = 97;
                                }
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
                                NumberOdEdit = numberOdEdit,
                                IsComplete = false, //lanh dao duyet => true
                                DepartmentId = userinfo.Organization.Id,
                                PromotionId = promotionId,
                                Status = status,
                                CreatedBy = userinfo.Id,
                                CreatedDate = DateTime.ParseExact(DateTime.Now.ToString("u"), "u", CultureInfo.InvariantCulture),
                                Deadline = null,
                                StageCurrent = stageCurrent,
                                FlagDieuChuyen = flagDieuChuyen,
                                UpdateDate = DateTime.ParseExact(DateTime.Now.ToString("u"), "u", CultureInfo.InvariantCulture)
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
                                    ReceivingDepartment = userinfo.Organization.Id,//new Guid(Constants.ID_PHONG_QUAN_LY_BAN_HANG),
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
        public string UpdateHoanPhanBoQuaTang(List<TransferDetail> obj, ClaimsPrincipal principal, string tranferId)
        {
            var result = string.Empty;
            var userinfo = ContextProvider.GetUserInfo(principal);
            var departmentId = userinfo.OrganizationId;
            var checkAmount = false; var _tranferId = new Guid(tranferId);
            var _productId = new Guid(Constants.ID_PRODUCT_HOAN_PHAN_BO_QUA_TANG);
            SessionManager.DoWork((Action<NHibernate.ISession>)(ss =>
            {
                try
                {
                    var product = ss.Get<Product>(_productId);
                    var stage = new Stage();
                    var tranferGift = ss.Get<TransferGift>(_tranferId);
                    var listGiftIds = obj.ToList().Select(s => s.GiftId).ToList();
                    var gifts = ss.Query<Gift>().Where(w => listGiftIds.Contains(w.Id)).ToList();
                    var tranferDetail = ss.Query<TransferDetail>().Where(w => w.TransferGift.Id == _tranferId).ToList();
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
                            var giftId = itm.GiftId;
                            var gift = gifts.FirstOrDefault(w => w.Id == giftId);
                            // lấy quà tặng trong kho để check valid.
                            var amount = ss.Query<Store>()
                                            .Single(s => s.DepartmentId == departmentId && s.GiftId == giftId && s.PromotionId == tranferGift.PromotionId
                                            ).Amount;
                            var countAmount = ss.Query<TransferDetail>()
                                            .Where(s =>
                                                s.GiftId == giftId &&
                                                s.ReceivingPromotion == tranferGift.PromotionId &&
                                                s.Id != itm.Id
                                            ).Select(p => p.Amount).ToList()
                                            .Sum();
                            if (itm.Amount < (amount - countAmount))
                            {
                                checkAmount = true;
                            }
                            else
                            {
                                checkAmount = false;
                                result = $"{gift.Name} Trong kho còn {amount} < {(amount - countAmount)}!\nAnh/Chị vui lòng kiểm tra lại.";
                                break;
                            }
                        }

                        if (checkAmount)
                        {

                            status = (int)ContextProvider.statusTransfer.Draft;
                            stageCurrent = new Guid(Constants.ID_HOAN_PHAN_BO_TAO_NHAP);
                            stage = ss.Query<Stage>().FirstOrDefault(p =>
                                       p.ProductId == product.Id &&
                                       p.PositionId == userinfo.Position.Id &&
                                       p.Name == Constants.TAO_NHAP);
                            result = Constants.LUU_THANH_CONG;
                            if (stage != null)
                            {
                                tranferGift.UpdateDate = DateTime.Now;
                                foreach (var itm in obj)
                                {
                                    var itemSave = tranferDetail.FirstOrDefault(f => f.Id == itm.Id);
                                    if (itemSave != null)
                                    {
                                        itemSave.Amount = itm.Amount;
                                        ss.SaveOrUpdate(itemSave);
                                    }
                                }
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
                    var isTypeUser = ContextProvider.CheckPermission(userinfo.PermisionId);
                    var isLDCN_PGD = isTypeUser == 3 && userinfo.Position.IsLeader ? true : false;
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
                            //itm.CreatedBy = userinfo.OrganizationId;
                            itm.NguoiDuyet = userinfo.Id;
                            itm.NgayDuyet = date;
                            //itm.CreatedDate = date;
                            itm.UpdateDate = date;

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
                                status = 97;// (int)ContextProvider.statusTransfer.Initialize;
                                result = Constants.GUI_DUYET_THANH_CONG;
                                saveDataHPB(ss, tranfer, userinfo, status, date, stage, stageCurrent, flagDieuChuyen);
                            }

                            if (flag == Constants.APPROVE)
                            {
                                stage = ss.Query<Stage>().Where(p =>
                                            p.ProductId == product.Id &&
                                            p.PositionId == userinfo.Position.Id &&
                                            p.Name == Constants.DUYET).FirstOrDefault();
                                stageCurrent = new Guid(Constants.ID_HOAN_PHAN_BO_DUYET);
                                status = isLDCN_PGD ? (int)ContextProvider.statusTransfer.ApproveCN : (int)ContextProvider.statusTransfer.Approve;
                                result = Constants.DUYET_THANH_CONG;

                                if (tranfer.FirstOrDefault().Status == 4)
                                {
                                    foreach (var itm in tranferDetailDisTinct)
                                    {
                                        // update store chi nhánh, phòng giao dịch
                                        var store = ss.Query<Store>()
                                            .Single(s => s.PromotionId == promotionId && s.GiftId == itm.GiftId && s.DepartmentId == userinfo.OrganizationId);
                                        store.Amount += itm.Amount;
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
                                            updateStore.Amount -= itm.Amount;
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
                                saveDataHPB(ss, tranfer, userinfo, status, date, stage, stageCurrent, flagDieuChuyen);

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

        public void saveDataHPB(NHibernate.ISession ss, List<TransferGift> tranfer, UserDTO userinfo, int status, DateTime date, Stage stage, Guid stageCurrent, Guid flagDieuChuyen)
        {

            try
            {
                foreach (var itm in tranfer)
                {
                    if (status == 97)
                    {
                        itm.CreatedBy = userinfo.Id;
                        itm.CreatedDate = date;
                    }
                    else if (status == 2)
                    {

                        itm.NguoiDuyet = userinfo.Id;
                        itm.NgayDuyet = date;
                    }
                    itm.Status = status;
                    itm.StageCurrent = stageCurrent;
                    itm.UpdateDate = date;

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
            catch (Exception ex)
            {
            }
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
