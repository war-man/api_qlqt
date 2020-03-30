using Api.ManagerGift.DTO;
using Api.ManagerGift.Entities;
using Api.ManagerGift.Sessions;
using Newtonsoft.Json;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace Api.ManagerGift.Services
{
    public class TransferService
    {
        public dynamic Get(int pageNo, ClaimsPrincipal principal,
            string idProduct, string maQuaTang, string donViThucHien, string donViChuyen, string donViNhan)
        {
            int pageSize = 20;
            dynamic lstResults = new ExpandoObject();
            var userinfo = ContextProvider.GetUserInfo(principal);
            SessionManager.DoWork(ss =>
            {
                try
                {
                    var lstUser = ss.Query<User>().ToList();
                    var organization = ss.Query<Organization>().ToList();

                    IEnumerable<TransferGiftLog> tranfersLog = ss.Query<TransferGiftLog>().Where(p => p.FlagDieuChuyen == new Guid(Constants.GUIDE_TYPE_NULL));

                    if (!string.IsNullOrEmpty(donViThucHien))
                        tranfersLog = tranfersLog.Where(s => s.AssignDeaprtmentId == new Guid(donViThucHien));

                    var grpTransfersLog = tranfersLog.GroupBy(p => p.TransferGift.Id).Select(grp => new { TransferGiftId = grp.Key, Status = grp.Max(m => m.Status) });

                    var tranfersLog1 = (from grp in grpTransfersLog
                                        join log in tranfersLog
                                        on new { grp.TransferGiftId, grp.Status } equals new { TransferGiftId = log.TransferGift.Id, log.Status }
                                        select log);

                    IEnumerable<TransferGift> tranfers = ss.Query<TransferGift>().Where(p => p.FlagDieuChuyen == new Guid(Constants.GUIDE_TYPE_NULL));

                    if (!string.IsNullOrEmpty(idProduct))
                        tranfers = tranfers.Where(p => p.Product.Id == new Guid(idProduct));

                    if (!string.IsNullOrEmpty(donViChuyen))
                        tranfers = tranfers.Where(p => p.DepartmentId == new Guid(donViChuyen));

                    var transferDetailDistinct = ss.Query<TransferDetail>().Where(p => p.FlagDieuChuyen == new Guid(Constants.GUIDE_TYPE_NULL))
                        .Select(s => new { TransferId = s.TransferGift.Id, s.ReceivingDepartment, s.GiftId }).ToList();

                    if (!string.IsNullOrEmpty(maQuaTang))
                        transferDetailDistinct = transferDetailDistinct.Where(p => p.GiftId == new Guid(maQuaTang)).ToList();

                    var transferDetail = transferDetailDistinct.Select(s => new { s.TransferId, s.ReceivingDepartment }).Distinct().ToList();

                    if (!string.IsNullOrEmpty(donViNhan))
                        transferDetail = transferDetail.Where(p => p.ReceivingDepartment == new Guid(donViNhan)).ToList();

                    var status = 100; // giá trị này không có ý nghĩa gì cả, mục đích lọc bản nháp không cho lãnh đạo thấy.
                    if (userinfo.PositionId == Constants.ID_LANH_DAO)
                        status = 0;


                    var lstTranfers = (from _tranfers in tranfers
                                       join _tranferLog in tranfersLog1 on _tranfers.Id equals _tranferLog.TransferGift.Id
                                       join _transferDetail in transferDetail on _tranfers.Id equals _transferDetail.TransferId
                                       where (_tranferLog.AssignDeaprtmentId == userinfo.Organization.Id ||
                                            _transferDetail.ReceivingDepartment == userinfo.Organization.Id) && _tranfers.Status != status
                                       select new
                                       {
                                           _tranfers.Id,
                                           _tranfers.Code,
                                           _tranfers.Name,
                                           _tranfers.Status,
                                           _tranfers.IsFinished,
                                           _tranfers.IsComplete,
                                           //_tranfers.DepartmentId,
                                           DepartmentCreate = ContextProvider.GetOrganizationName(organization, _tranfers.DepartmentId),
                                           _tranfers.Deadline,
                                           ProductId = _tranfers.Product.Id,
                                           //ReceiveDepartment = _recDepartment.DepartmentId,
                                           ReceiveDepartment = ContextProvider.GetOrganizationName(organization, _transferDetail.ReceivingDepartment),
                                           StageId = _tranferLog.Stage.Id,
                                           TransferGiftId = _tranferLog.TransferGift.Id,
                                           _tranfers.StageCurrent,
                                           _tranfers.CreatedBy,
                                           CreatedDate = ContextProvider.GetConvertDatetime(_tranfers.CreatedDate),
                                       }).OrderBy(v => v.Status).OrderByDescending(v=>v.CreatedDate);


                    lstResults.ListTranfers = lstTranfers.Skip((pageNo - 1) * pageSize).Take(pageSize).ToList();

                    var total = lstTranfers.Count();
                    lstResults.TotalPage = total % pageSize == 0 ? total / pageSize : total / pageSize + 1;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            });
            return lstResults;
        }

        public dynamic GetDetailTranfer(Guid tranferId)
        {
            dynamic lstResults = new ExpandoObject();
            SessionManager.DoWork(ss =>
            {
                try
                {
                    var lstUser = ss.Query<User>().ToList();
                    var organization = ss.Query<Organization>().ToList();
                    var promotion = ss.Query<Promotion>().ToList();
                    IEnumerable<TransferGift> tranfers = ss.Query<TransferGift>().Where(s => s.Id == tranferId);
                    IEnumerable<TransferGiftLog> tranferlogs = ss.Query<TransferGiftLog>().Where(s => s.TransferGift.Id == tranferId);
                    var tranferDetail = ss.Query<TransferDetail>().Where(s => s.TransferGift.Id == tranferId);
                    IEnumerable<Product> product = ss.Query<Product>();
                    lstResults.InfoTrans = (from _tranfers in tranfers
                                            join _product in product on _tranfers.Product.Id equals _product.Id
                                            join _tranferDetail in tranferDetail on _tranfers.Id equals _tranferDetail.TransferGift.Id
                                            select new
                                            {
                                                _tranfers.Code,
                                                _tranfers.Status,
                                                _tranfers.PromotionId,
                                                Note = tranferlogs.FirstOrDefault().Comment,
                                                CreatedBy = ContextProvider.GetFullName(lstUser, _tranfers.CreatedBy),
                                                _tranfers.CreatedDate,
                                                NguoiDuyet = ContextProvider.GetFullName(lstUser, _tranfers.NguoiDuyet),
                                                _tranfers.NgayDuyet,
                                                CTKMChuyen = ContextProvider.GetPromotionName(promotion, _tranfers.PromotionId),
                                                PhongBanChuyen = ContextProvider.GetOrganizationName(organization, _tranfers.DepartmentId),
                                                DepartmentCreate = ContextProvider.GetOrganizationName(organization, _tranfers.DepartmentId),
                                                LoaiGiaoDich = _product.Name,
                                                ProductId = _product.Id,
                                                CTKMNhan = ContextProvider.GetPromotionName(promotion, _tranferDetail.ReceivingPromotion),
                                                PhongBanNhan = ContextProvider.GetOrganizationName(organization, _tranferDetail.ReceivingDepartment),
                                                BranchInboxId = _tranferDetail.ReceivingDepartment,
                                                BranchSendId = _tranfers.DepartmentId,
                                                CTKMChuyenId = _tranfers.PromotionId,//them ngay 18/11/2019 dung cho dieu chuyen noi bo
                                                CTKMNhanId = _tranferDetail.ReceivingPromotion,//them ngay 18/11/2019 dung cho dieu chuyen noi bo
                                            }).ToList();

                    IEnumerable<TransferDetail> tranferDetails = ss.Query<TransferDetail>().Where(s => s.TransferGift.Id == tranferId);
                    IEnumerable<Gift> gifts = ss.Query<Gift>();

                    var listGift = (from _tranferDetails in tranferDetails
                                    join _gifts in gifts on _tranferDetails.GiftId equals _gifts.Id
                                    select new
                                    {
                                        GiftId = _gifts.Id,//nguyen sua ngay 18/11/2019
                                        GiftCode = _gifts.Code,
                                        GiftName = _gifts.Name,
                                        _gifts.Price,
                                        UnitName = _gifts.Unit.Name,
                                        _tranferDetails.Amount,
                                    });
                    lstResults.ListGift = listGift.ToList();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            });
            return lstResults;
        }

        /// <summary>
        /// Khởi tạo nhập kho và xuất kho
        /// </summary>
        public string InitTransfer(List<DataNhapKho> lst, string productId, ClaimsPrincipal principal, string flag)
        {
            var result = string.Empty;
            var _productId = new Guid(productId);
            try
            {
                var userinfo = ContextProvider.GetUserInfo(principal);
                SessionManager.DoWork(ss =>
                {
                    var product = ss.Get<Product>(_productId);
                    var stage = new Stage();
                    var dateNow = DateTime.ParseExact(DateTime.Now.ToString("u"), "u", CultureInfo.InvariantCulture);

                    var status = 100;
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
                        switch (_productId.ToString().ToUpper())
                        {
                            case Constants.PARAM_NHAP_KHO:

                                var newTransfer = new TransferGift
                                {
                                    Id = Guid.NewGuid(),
                                    Code = CreateTranferCode(product.Code),
                                    Product = product,
                                    IsFinished = false, // chua dung
                                    IsComplete = false, //lanh dao duyet => true
                                    DepartmentId = userinfo.Organization.Id,
                                    PromotionId = null,
                                    Status = status,
                                    CreatedBy = userinfo.Id,
                                    CreatedDate = dateNow,
                                    Deadline = null,
                                    StageCurrent = stage.Id,
                                };

                                var newTransferLog = new TransferGiftLog
                                {
                                    Id = Guid.NewGuid(),
                                    TransferGift = newTransfer,
                                    AssignUserId = userinfo.Id,
                                    AssignDeaprtmentId = userinfo.Organization.Id,
                                    Comment = null,
                                    Data = null,
                                    Status = newTransfer.Status,
                                    UpdateDate = newTransfer.CreatedDate,
                                    Stage = stage,
                                    Dealine = null
                                };

                                ss.Save(newTransfer);
                                ss.Save(newTransferLog);

                                foreach (var itm in lst)
                                {
                                    ss.Save(new TransferDetail
                                    {
                                        Id = Guid.NewGuid(),
                                        GiftId = new Guid(itm.GiftId),
                                        Amount = itm.Amount,
                                        TransferGift = newTransfer,
                                        ReceivingDepartment = userinfo.Organization.Id,
                                        ReceivingPromotion = null
                                    });
                                }
                                break;

                            case Constants.PARAM_XUAT_KHO:

                                // valid amount gift
                                var checkAmount = false;
                                foreach (var itm in lst)
                                {
                                    var amount = ss.Query<Store>().Where(s => s.DepartmentId == userinfo.OrganizationId &&
                                        s.GiftId == new Guid(itm.GiftId) &&
                                        s.PromotionId == new Guid(itm.PromotionId)).FirstOrDefault().Amount;
                                    if (amount >= itm.Amount)
                                        checkAmount = true;

                                    else
                                    {
                                        checkAmount = false;
                                        result = $"{itm.GiftName} trong kho còn: {amount} < {itm.Amount}!\nAnh/Chị vui lòng kiểm tra lại.";
                                        break;
                                    }
                                }
                                if (checkAmount)
                                {
                                    foreach (var itm in lst)
                                    {
                                        var newTransfer1 = new TransferGift
                                        {
                                            Id = Guid.NewGuid(),
                                            Code = CreateTranferCode(product.Code),
                                            Product = product,
                                            IsFinished = false, // chua dung
                                            IsComplete = false, //lanh dao duyet => true
                                            DepartmentId = userinfo.Organization.Id,
                                            PromotionId = new Guid(itm.PromotionId),
                                            Status = status,
                                            CreatedBy = userinfo.Id,
                                            CreatedDate = dateNow,
                                            Deadline = null,
                                            StageCurrent = stage.Id,
                                        };

                                        var newTransferLog1 = new TransferGiftLog
                                        {
                                            Id = Guid.NewGuid(),
                                            TransferGift = newTransfer1,
                                            AssignUserId = userinfo.Id,
                                            AssignDeaprtmentId = userinfo.Organization.Id,
                                            Comment = itm.Note,
                                            Data = null,
                                            Status = newTransfer1.Status,
                                            UpdateDate = newTransfer1.CreatedDate,
                                            Stage = stage,
                                            Dealine = null
                                        };

                                        ss.Save(newTransfer1);
                                        ss.Save(newTransferLog1);

                                        ss.Save(new TransferDetail
                                        {
                                            Id = Guid.NewGuid(),
                                            GiftId = new Guid(itm.GiftId),
                                            Amount = itm.Amount,
                                            TransferGift = newTransfer1,
                                            ReceivingDepartment = userinfo.Organization.Id,
                                            ReceivingPromotion = new Guid(itm.PromotionId)
                                        });
                                    }
                                }
                                break;
                        }
                    }
                    else
                        result = Constants.CHUC_NANG_NHAN_VIEN;
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return result;
        }

        public string InitDieuChuyenNgang(List<DataNhapKho> lst, string flag, string fromOrganId, string toOrganId, ClaimsPrincipal principal)
        {
            var result = string.Empty;
            var userinfo = ContextProvider.GetUserInfo(principal);
            if (userinfo.OrganizationId == new Guid(fromOrganId) || userinfo.OrganizationId == new Guid(Constants.ID_PHONG_QUAN_LY_BAN_HANG))
            {
                var _productId = new Guid(Constants.ID_PRODUCT_DIEU_CHUYEN_NGANG);
                var checkAmount = false;
                try
                {
                    SessionManager.DoWork(ss =>
                    {
                        var product = ss.Get<Product>(_productId);
                        var stage = new Stage();

                        var status = 0;
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
                            // valid amount gift
                            foreach (var itm in lst)
                            {
                                // get amount in store:
                                var amount = ss.Query<Store>().Where(s => s.DepartmentId == userinfo.OrganizationId &&
                                    s.GiftId == new Guid(itm.GiftId) &&
                                    s.PromotionId == new Guid(itm.PromotionId)).FirstOrDefault().Amount;
                                if (amount >= itm.Amount)
                                    checkAmount = true;

                                else
                                {
                                    checkAmount = false;
                                    result = $"{itm.GiftName} trong kho còn: {amount} < {itm.Amount}!\nAnh/Chị vui lòng kiểm tra lại.";
                                    break;
                                }
                            }
                            if (checkAmount)
                            {
                                foreach (var itm in lst)
                                {
                                    var newTransfer = new TransferGift
                                    {
                                        Id = Guid.NewGuid(),
                                        Code = CreateTranferCode(product.Code),
                                        Product = product,
                                        IsFinished = false, // chua dung
                                        IsComplete = false, //lanh dao duyet => true
                                        DepartmentId = userinfo.Organization.Id,
                                        PromotionId = new Guid(itm.PromotionId),
                                        Status = status,
                                        CreatedBy = userinfo.Id,
                                        CreatedDate = DateTime.ParseExact(DateTime.Now.ToString("u"), "u", CultureInfo.InvariantCulture),
                                        Deadline = null,
                                        StageCurrent = stage.Id,
                                    };

                                    var newTransferLog = new TransferGiftLog
                                    {
                                        Id = Guid.NewGuid(),
                                        TransferGift = newTransfer,
                                        AssignUserId = userinfo.Id,
                                        AssignDeaprtmentId = userinfo.Organization.Id,
                                        Comment = null,
                                        Data = null,
                                        Status = newTransfer.Status,
                                        UpdateDate = newTransfer.CreatedDate,
                                        Stage = stage,
                                        Dealine = null
                                    };

                                    ss.Save(newTransfer);
                                    ss.Save(newTransferLog);

                                    ss.Save(new TransferDetail
                                    {
                                        Id = Guid.NewGuid(),
                                        GiftId = new Guid(itm.GiftId),
                                        Amount = itm.Amount,
                                        TransferGift = newTransfer,
                                        ReceivingDepartment = new Guid(toOrganId),
                                        ReceivingPromotion = new Guid(itm.PromotionId)
                                    });
                                }
                            }
                        }
                        else
                            result = Constants.CHUC_NANG_NHAN_VIEN;
                    });
                }
                catch (Exception ex)
                {
                    result = ex.Message;
                    Console.WriteLine(result);
                }
            }
            else
                result = "Bạn không được điều chuyển ngang từ đơn vị khác!";
            return result;
        }

        public string InitDieuChuyenNoiBo(List<DataNhapKho> lst, string flag, string fromPromotionId, string toPromotionId, ClaimsPrincipal principal)
        {
            var result = string.Empty;
            var userinfo = ContextProvider.GetUserInfo(principal);
            var _productId = new Guid(Constants.ID_PRODUCT_DIEU_CHUYEN_NOI_BO);
            var checkAmount = false;
            try
            {
                SessionManager.DoWork(ss =>
                {
                    var product = ss.Get<Product>(_productId);
                    var stage = new Stage();

                    var status = 0;
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
                        // valid amount gift
                        foreach (var itm in lst)
                        {
                            // get amount in store:
                            var amount = ss.Query<Store>().Where(s => s.DepartmentId == userinfo.OrganizationId &&
                            s.GiftId == new Guid(itm.GiftId) &&
                            s.PromotionId == new Guid(fromPromotionId)).FirstOrDefault().Amount;
                            if (amount >= itm.Amount)
                                checkAmount = true;

                            else
                            {
                                checkAmount = false;
                                result = $"{itm.GiftName} trong kho còn: {amount} < {itm.Amount}!\nAnh/Chị vui lòng kiểm tra lại.";
                                break;
                            }
                        }
                        if (checkAmount)
                        {
                            // tạo tranfer
                            var transfer = new TransferGift
                            {
                                Id = Guid.NewGuid(),
                                Code = CreateTranferCode(product.Code),
                                Product = product,
                                IsFinished = false, // chua dung
                                IsComplete = false, //lanh dao duyet => true
                                DepartmentId = userinfo.Organization.Id,
                                PromotionId = new Guid(fromPromotionId),
                                Status = status,
                                CreatedBy = userinfo.Id,
                                CreatedDate = DateTime.ParseExact(DateTime.Now.ToString("u"), "u", CultureInfo.InvariantCulture),
                                Deadline = null,
                                StageCurrent = stage.Id,
                            };

                            // tạo tranfer log
                            var transferlog = new TransferGiftLog
                            {
                                Id = Guid.NewGuid(),
                                TransferGift = transfer,
                                AssignUserId = userinfo.Id,
                                AssignDeaprtmentId = userinfo.Organization.Id,
                                Comment = null,
                                Data = null,
                                Status = transfer.Status,
                                UpdateDate = transfer.CreatedDate,
                                Stage = stage,
                                Dealine = null
                            };

                            ss.Save(transfer);
                            ss.Save(transferlog);
                            foreach (var itm in lst)
                            {
                                ss.Save(new TransferDetail
                                {
                                    Id = Guid.NewGuid(),
                                    GiftId = new Guid(itm.GiftId),
                                    Amount = itm.Amount,
                                    TransferGift = transfer,
                                    ReceivingDepartment = userinfo.Organization.Id,
                                    ReceivingPromotion = new Guid(toPromotionId)
                                });
                            }
                        }
                    }
                    else
                        result = Constants.CHUC_NANG_NHAN_VIEN;
                });
            }
            catch (Exception ex)
            {
                result = ex.Message;
                Console.WriteLine(result);
            }
            return result;
        }

        public string NhanVienGuiDuyet(Guid transferId, string productId, ClaimsPrincipal principal)
        {
            var result = string.Empty;
            var userinfo = ContextProvider.GetUserInfo(principal);
            try
            {
                SessionManager.DoWork(ss =>
                {
                    var product = ss.Get<Product>(new Guid(productId));

                    var stage = ss.Query<Stage>().SingleOrDefault(p => p.ProductId == product.Id && p.PositionId == userinfo.Position.Id && p.Name == Constants.GUI_DUYET);
                    if (stage != null)
                    {
                        var transfer = ss.Get<TransferGift>(transferId);
                        var tranferDetail = ss.Query<TransferDetail>().Where(p => p.TransferGift.Id == transferId).ToList();
                        foreach (var itm in tranferDetail)
                        {
                            transfer.Status = (int)ContextProvider.statusTransfer.Initialize;
                            transfer.StageCurrent = stage.Id;

                            var transferlog = new TransferGiftLog
                            {
                                Id = Guid.NewGuid(),
                                TransferGift = transfer,
                                AssignUserId = userinfo.Id,
                                AssignDeaprtmentId = userinfo.Organization.Id,
                                Comment = null,
                                Data = null,
                                Status = (int)ContextProvider.statusTransfer.Initialize,
                                UpdateDate = DateTime.ParseExact(DateTime.Now.ToString("u"), "u", CultureInfo.InvariantCulture),
                                Stage = stage,
                                Dealine = null
                            };
                            ss.Save(transferlog);
                            result = Constants.GUI_DUYET_THANH_CONG;
                        }
                    }
                    else
                        result = Constants.CHUC_NANG_NHAN_VIEN;
                });
                return result;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public string LanhDaoDuyet(Guid transferId, string productId, ClaimsPrincipal principal)
        {
            var result = string.Empty;
            var userinfo = ContextProvider.GetUserInfo(principal);
            try
            {
                SessionManager.DoWork(ss =>
                {
                    var dateNow = DateTime.ParseExact(DateTime.Now.ToString("u"), "u", CultureInfo.InvariantCulture);

                    var product = ss.Get<Product>(new Guid(productId));

                    var stage = ss.Query<Stage>().SingleOrDefault(p => p.ProductId == product.Id && p.PositionId == userinfo.Position.Id && p.Name == Constants.DUYET);
                    if (stage != null)
                    {
                        var transfer = ss.Get<TransferGift>(transferId);
                        transfer.StageCurrent = stage.Id;
                        transfer.NguoiDuyet = userinfo.Id;
                        transfer.NgayDuyet = dateNow;

                        var transferlog = new TransferGiftLog
                        {
                            Id = Guid.NewGuid(),
                            TransferGift = transfer,
                            AssignUserId = userinfo.Id,
                            AssignDeaprtmentId = userinfo.Organization.Id,
                            Comment = null,
                            Data = null,
                            Status = (int)ContextProvider.statusTransfer.Approve,
                            UpdateDate = dateNow,
                            Stage = stage,
                            Dealine = null
                        };

                        var tranferDetail = ss.Query<TransferDetail>().Where(p => p.TransferGift.Id == transferId);

                        // CTKM nhận và Department nhận.
                        var receivingDepartmentId = tranferDetail.First().ReceivingDepartment;
                        var receivingPromotionId = tranferDetail.First().ReceivingPromotion;

                        // CTKM chuyển và Department chuyển:
                        var promotionIdTranfer = transfer.PromotionId;
                        var departmentIdTranfer = transfer.DepartmentId;

                        var lstStore = ss.Query<Store>().ToList(); //.Where(p => p.DepartmentId == receivingDepartmentId)

                        var lstTransferDetail = tranferDetail.ToList();

                        // update in store
                        switch (productId.ToUpper())
                        {
                            case Constants.PARAM_NHAP_KHO:

                                transfer.Status = (int)ContextProvider.statusTransfer.Approve;
                                ss.Save(transferlog);

                                foreach (var itm in lstTransferDetail)
                                {
                                    var store = lstStore.SingleOrDefault(p => p.DepartmentId == itm.ReceivingDepartment && p.GiftId == itm.GiftId && p.PromotionId == null);
                                    if (store == null)
                                    {
                                        ss.Save(new Store
                                        {
                                            Id = Guid.NewGuid(),
                                            DepartmentId = itm.ReceivingDepartment.Value,
                                            ManagerType = null,
                                            PromotionId = null,
                                            GiftId = itm.GiftId,
                                            Amount = itm.Amount,
                                            UpdatedDate = dateNow,
                                            LogTransfer = null
                                        });
                                    }
                                    else
                                    {
                                        store.Amount += itm.Amount;
                                        store.UpdatedDate = dateNow;
                                    }
                                }
                                break;

                            case Constants.PARAM_XUAT_KHO:

                                transfer.Status = (int)ContextProvider.statusTransfer.Approve;
                                ss.Save(transferlog);

                                foreach (var itm in lstTransferDetail)
                                {
                                    var store = lstStore.SingleOrDefault(p => p.DepartmentId == itm.ReceivingDepartment && p.GiftId == itm.GiftId && p.PromotionId == itm.ReceivingPromotion);
                                    if (store != null)
                                    {
                                        // check valid amount
                                        if (store.Amount >= itm.Amount)
                                        {
                                            store.Amount -= itm.Amount;
                                            store.UpdatedDate = dateNow;
                                        }
                                        else
                                        {
                                            result = "Trong kho không còn!";
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        result = "Trong kho không còn!";
                                        break;
                                    }
                                }
                                break;

                            case Constants.PARAM_DIEU_CHUYEN_NGANG:

                                if (transfer.Status == 99)
                                {
                                    transfer.Status = (int)ContextProvider.statusTransfer.Approve;
                                    transferlog.Status = (int)ContextProvider.statusTransfer.Approve;
                                    foreach (var itm in lstTransferDetail)
                                    {
                                        // department tranfer
                                        var storeTranfer = lstStore.SingleOrDefault(p => p.DepartmentId == departmentIdTranfer && p.GiftId == itm.GiftId && p.PromotionId == itm.ReceivingPromotion);

                                        // department receive
                                        var storeReceive = lstStore.SingleOrDefault(p => p.DepartmentId == receivingDepartmentId && p.GiftId == itm.GiftId && p.PromotionId == itm.ReceivingPromotion);

                                        // check valid amount
                                        if (storeTranfer.Amount >= itm.Amount)
                                        {
                                            storeTranfer.Amount -= itm.Amount;
                                            storeTranfer.UpdatedDate = dateNow;

                                            if (storeReceive != null)
                                            {
                                                storeReceive.Amount += itm.Amount;
                                                storeReceive.UpdatedDate = dateNow;
                                            }
                                            else
                                            {
                                                ss.Save(new Store
                                                {
                                                    Id = Guid.NewGuid(),
                                                    DepartmentId = receivingDepartmentId.Value,
                                                    ManagerType = null,
                                                    PromotionId = itm.ReceivingPromotion,
                                                    GiftId = itm.GiftId,
                                                    Amount = itm.Amount,
                                                    UpdatedDate = dateNow,
                                                    LogTransfer = null
                                                });
                                            }
                                        }
                                        else
                                        {
                                            result = "Trong kho không còn!";
                                            break;
                                        }
                                    }
                                }
                                if (transfer.Status == 1)
                                {
                                    transfer.Status = 99;
                                    transferlog.Status = 99;
                                }
                                ss.Save(transferlog);

                                break;

                            case Constants.PARAM_DIEU_CHUYEN_NOI_BO:

                                transfer.Status = (int)ContextProvider.statusTransfer.Approve;
                                ss.Save(transferlog);

                                foreach (var itm in lstTransferDetail)
                                {
                                    // promotion tranfer
                                    var storeTranfer = lstStore.SingleOrDefault(p => p.DepartmentId == itm.ReceivingDepartment && p.GiftId == itm.GiftId && p.PromotionId == promotionIdTranfer);

                                    // promotion receive
                                    var storeReceive = lstStore.SingleOrDefault(p => p.DepartmentId == itm.ReceivingDepartment && p.GiftId == itm.GiftId && p.PromotionId == receivingPromotionId);


                                    if (storeTranfer != null)
                                    {
                                        // check valid amount
                                        if (storeTranfer.Amount >= itm.Amount)
                                        {
                                            storeTranfer.Amount -= itm.Amount;
                                            storeTranfer.UpdatedDate = dateNow;

                                            if (storeReceive != null)
                                            {
                                                storeReceive.Amount += itm.Amount;
                                                storeReceive.UpdatedDate = dateNow;
                                            }
                                            else
                                            {
                                                ss.Save(new Store
                                                {
                                                    Id = Guid.NewGuid(),
                                                    DepartmentId = receivingDepartmentId.Value,
                                                    ManagerType = null,
                                                    PromotionId = itm.ReceivingPromotion,
                                                    GiftId = itm.GiftId,
                                                    Amount = itm.Amount,
                                                    UpdatedDate = dateNow,
                                                    LogTransfer = null
                                                });
                                            }
                                        }
                                        else
                                        {
                                            result = "Trong kho không còn!";
                                            break;
                                        }
                                    }
                                    else
                                        result = "Không có quà tặng trong kho";
                                }
                                break;

                            default:
                                break;
                        }

                        result = Constants.DUYET_THANH_CONG;
                    }
                    else
                        result = Constants.CHUC_NANG_LANH_DAO;
                });
            }
            catch (Exception ex)
            {
                result = ex.Message;
                Console.WriteLine(result);
            }
            return result;
        }

        public string LanhDaoTuChoiDuyet(Guid transferId, string productId, ClaimsPrincipal principal)
        {
            var result = string.Empty;
            var userinfo = ContextProvider.GetUserInfo(principal);
            try
            {
                SessionManager.DoWork(ss =>
                {
                    var dateNow = DateTime.ParseExact(DateTime.Now.ToString("u"), "u", CultureInfo.InvariantCulture);

                    var product = ss.Get<Product>(new Guid(productId));
                    var stage = ss.Query<Stage>().SingleOrDefault(p => p.ProductId == product.Id && p.PositionId == userinfo.Position.Id && p.Name == Constants.TU_CHOI_DUYET);
                    if (stage != null)
                    {
                        var transfer = ss.Get<TransferGift>(transferId);
                        transfer.Status = (int)ContextProvider.statusTransfer.Refuse;
                        transfer.NguoiDuyet = userinfo.Organization.Id;
                        transfer.NgayDuyet = dateNow;
                        transfer.StageCurrent = stage.Id;

                        var newTransferLog = new TransferGiftLog
                        {
                            Id = Guid.NewGuid(),
                            TransferGift = transfer,
                            AssignUserId = userinfo.Id,
                            AssignDeaprtmentId = userinfo.Organization.Id,
                            Comment = null,
                            Data = null,
                            Status = transfer.Status,
                            UpdateDate = dateNow,
                            Stage = stage,
                            Dealine = null
                        };
                        ss.Save(newTransferLog);

                        result = Constants.TU_CHOI_DUYET_THANH_CONG;
                    }
                    else
                        result = Constants.CHUC_NANG_LANH_DAO;
                });
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }

        public dynamic TranferHistory(string _idTranfer)
        {
            dynamic result = new ExpandoObject();

            try
            {
                var idTranfer = new Guid(_idTranfer);
                SessionManager.DoWork(ss =>
                {
                    var lstStage = ss.Query<Stage>().ToList();
                    var lstUser = ss.Query<User>().ToList();
                    var organization = ss.Query<Organization>().ToList();
                    var logTranfer = ss.Query<TransferGiftLog>().Where(s => s.TransferGift.Id == idTranfer)
                        .Select(p => new
                        {
                            TenBuoc = ContextProvider.GetStateName(lstStage, p.Stage.Id),
                            DonViThucHien = ContextProvider.GetOrganizationName(organization, p.AssignDeaprtmentId),
                            NguoiThucHien = ContextProvider.GetFullName(lstUser, p.AssignUserId),
                            p.Status,
                            p.UpdateDate
                        }).ToList();
                    result = logTranfer;
                });
            }
            catch (Exception ex)
            {
                result = ex;
            }
            return result;
        }

        #region privete
        private List<TransferDetail> ConvertJsonToObject(string json)
        {
            var lstTransferDetail = new List<TransferDetail>();
            var objGift = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(json.ToString());
            foreach (var itm in objGift)
            {
                var detail = new TransferDetail
                {
                    Id = new Guid(itm["idGift"]),
                    Amount = itm["soluong"] == null ? 0 : int.Parse(itm["soluong"]),
                };
                lstTransferDetail.Add(detail);
            }
            return lstTransferDetail;
        }
        private string CreateTranferCode(string type)
        {
            var dateTime = DateTime.Now.ToString("ddMMyyyy");
            StringBuilder builder = new StringBuilder();
            builder.Append(dateTime).Append("/").Append(type).Append("/");
            builder.Append(RandomString(4));
            builder.Append(RandomNumber(1000, 9999));

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

        #region Tamnt4
        //public string PostDraft(TransferGiftDTO obj, ClaimsPrincipal principal)
        //{
        //    var result = string.Empty;
        //    try
        //    {
        //        //var userDTO = ContextProvider.GetUserInfo(principal);
        //        SessionManager.DoWork(ss =>
        //        {
        //            var transfer = ss.Get<TransferGift>(obj.Id);
        //            transfer.Status = (int)ContextProvider.statusTransfer.Draft;
        //            var transferlog = ss.Query<TransferGiftLog>().Single(p => p.TransferGift.Id == obj.Id);
        //            if (!string.IsNullOrEmpty(obj.Data.ToString()))
        //                transferlog.Data = JsonConvert.SerializeObject(obj.Data);
        //            transferlog.Status = transfer.Status;
        //            switch (obj.ProductId.ToString().ToUpper())
        //            {
        //                // NHẬP KHO
        //                //case "7A452975-E667-41CB-9B32-5875D357FF37":
        //                //// XUẤT KHO
        //                //case "0AFC855F-5E19-4B2A-A296-E5E66BA3B17B":
        //                //    ss.Update(transferlog);
        //                //    break;
        //                // ĐIỀU CHUYỂN NGANG
        //                case "4E0F159C-1B09-4FAB-8D2D-FA38DF55006A":
        //                    transfer.DepartmentId = obj.DepartmentId;

        //                    var receivingDepartment = ss.Query<ReceivingDepartment>().SingleOrDefault(p => p.TransferGift.Id == obj.Id);
        //                    if (receivingDepartment == null)
        //                        ss.Save(new ReceivingDepartment
        //                        {
        //                            Id = Guid.NewGuid(),
        //                            DepartmentId = obj.ReceivingDepartmentId.GetValueOrDefault(),
        //                            TransferGift = transfer
        //                        });
        //                    else
        //                    {
        //                        receivingDepartment.DepartmentId = obj.ReceivingDepartmentId.GetValueOrDefault();

        //                    }
        //                    break;
        //                // ĐIỀU CHUYỂN NỘI BỘ
        //                case "81A05F45-9BE2-4754-A5D1-D0F8632AC8F8":
        //                    transfer.PromotionId = obj.PromotionId;
        //                    var receivingPromotion = ss.Query<ReceivingPromotion>().SingleOrDefault(p => p.TransferGift.Id == obj.Id);
        //                    if (receivingPromotion == null)
        //                        ss.Save(new ReceivingPromotion
        //                        {
        //                            Id = Guid.NewGuid(),
        //                            PromotionId = obj.ReceivingDepartmentId.GetValueOrDefault(),
        //                            TransferGift = transfer
        //                        });
        //                    else
        //                    {
        //                        receivingPromotion.PromotionId = obj.ReceivingDepartmentId.GetValueOrDefault();
        //                    }
        //                    break;
        //            }
        //        });
        //        result = " Lưu nháp thành công!";
        //    }
        //    catch (Exception ex)
        //    {
        //        result = ex.Message;
        //    }
        //    return result;
        //}

        //public string Refuse(Guid transferId, ClaimsPrincipal principal)
        //{
        //    var result = string.Empty;
        //    var userinfo = ContextProvider.GetUserInfo(principal);
        //    try
        //    {
        //        SessionManager.DoWork(ss =>
        //        {
        //            var transfer = ss.Get<TransferGift>(transferId);
        //            transfer.Status = (int)ContextProvider.statusTransfer.Refuse;
        //            var transferlog = ss.Query<TransferGiftLog>()
        //                .Where(p => p.TransferGift.Id == transferId && p.AssignUserId == userinfo.Id)
        //                .OrderByDescending(p => p.UpdateDate).First();
        //            transferlog.Status = transfer.Status;
        //            transferlog.UpdateDate = DateTime.ParseExact(DateTime.Now.ToString("u"), "u", CultureInfo.InvariantCulture);
        //            result = "Đã từ chối duyệt thành công";
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        result = ex.Message;
        //    }
        //    return result;
        //}

        //public string Browse(TransferGiftDTO obj, ClaimsPrincipal principal)
        //{
        //    var result = string.Empty;
        //    try
        //    {
        //        var userinfo = ContextProvider.GetUserInfo(principal);
        //        SessionManager.DoWork(ss =>
        //        {
        //            var stage = ss.Get<Stage>(obj.StageId);
        //            switch (obj.ProductId.ToString().ToUpper())
        //            {
        //                // NHẬP KHO
        //                case "7A452975-E667-41CB-9B32-5875D357FF37":

        //                    // nhân viên gửi duyệt
        //                    if (stage.Name == "Start Stage")
        //                    {
        //                        result = _transferInputService.BrowseStaff(ss, obj, stage, principal);
        //                    }

        //                    // lãnh đạo duyệt
        //                    else if (stage.Name == "End Stage")
        //                    {
        //                        result = _transferInputService.BrowseLeader(ss, obj.Id, stage, principal);
        //                    }
        //                    break;

        //                // XUẤT KHO
        //                case "0AFC855F-5E19-4B2A-A296-E5E66BA3B17B":
        //                    if (stage.Name == "Start Stage")
        //                    {
        //                        result = _transferOutputService.BrowseStaff(ss, obj, stage, principal);
        //                    }
        //                    else if (stage.Name == "End Stage")
        //                    {
        //                        result = _transferOutputService.BrowseLeader(ss, obj.Id, stage, principal);
        //                    }
        //                    break;

        //                // ĐIỀU CHUYỂN NGANG
        //                case "4E0F159C-1B09-4FAB-8D2D-FA38DF55006A":
        //                    if (stage.Name == "Start Stage")
        //                    {

        //                    }
        //                    else if (stage.Name == "Leader QLBH Approve Stage")
        //                    {

        //                    }
        //                    else if (stage.Name == "Leader Branch Send Approve Stage")
        //                    {

        //                    }
        //                    else if (stage.Name == "End Stage")
        //                    {

        //                    }
        //                    break;

        //                // ĐIỀU CHUYỂN NỘI BỘ
        //                case "81A05F45-9BE2-4754-A5D1-D0F8632AC8F8":
        //                    if (stage.Name == "Start Stage")
        //                    {
        //                        result = _transferPromotionService.BrowseStaff(ss, obj, stage, principal);
        //                    }
        //                    else if (stage.Name == "Leader Branch Approve Stage")
        //                    {
        //                        result = _transferPromotionService.BrowseLeader(ss, obj.Id, stage, principal);
        //                    }
        //                    else if (stage.Name == "End Stage")
        //                    {
        //                        result = _transferPromotionService.BrowseLeaderQLBH(ss, obj.Id, stage, principal);
        //                    }
        //                    break;
        //            }
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        result = ex.Message;
        //    }
        //    return result;
        //}
        #endregion

        #region nguyen
        public string InitTransferUpdate(List<DataNhapKho> lst, string productId, ClaimsPrincipal principal, string flag, string Id)
        {
            var result = string.Empty;
            try
            {
                var _id = new Guid(Id);
                var _productId = new Guid(productId);
                var userinfo = ContextProvider.GetUserInfo(principal);
                SessionManager.DoWork(ss =>
                {
                    var dateNow = DateTime.ParseExact(DateTime.Now.ToString("u"), "u", CultureInfo.InvariantCulture);

                    if (flag == Constants.DRAFT)
                    {
                        switch (_productId.ToString().ToUpper())
                        {
                            case Constants.PARAM_NHAP_KHO:
                                var newTransfer = ss.Query<TransferGift>().SingleOrDefault(p => p.Id == _id);
                                ss.CreateSQLQuery($"delete [TransferDetail] where TransferId = '{_id}'").UniqueResult();
                                foreach (var itm in lst)
                                {
                                    ss.Save(new TransferDetail
                                    {
                                        Id = Guid.NewGuid(),
                                        GiftId = new Guid(itm.GiftId),
                                        Amount = itm.Amount,
                                        TransferGift = newTransfer,
                                        ReceivingDepartment = userinfo.Organization.Id,
                                        ReceivingPromotion = null
                                    });
                                }
                                break;

                            case Constants.PARAM_XUAT_KHO:

                                // valid amount gift
                                var checkAmount = false;
                                foreach (var itm in lst)
                                {
                                    var amount = ss.Query<Store>().Where(s => s.DepartmentId == userinfo.OrganizationId &&
                                        s.GiftId == new Guid(itm.GiftId) &&
                                        s.PromotionId == new Guid(itm.PromotionId)).FirstOrDefault().Amount;
                                    if (amount >= itm.Amount)
                                        checkAmount = true;

                                    else
                                    {
                                        checkAmount = false;
                                        result = $"{itm.GiftName} trong kho còn: {amount} < {itm.Amount}!\nAnh/Chị vui lòng kiểm tra lại.";
                                        break;
                                    }
                                }
                                if (checkAmount)
                                {
                                    foreach (var itm in lst)
                                    {
                                        var newTransfer1 = ss.Query<TransferGift>().SingleOrDefault(p => p.Id == _id);
                                        ss.CreateSQLQuery($"delete [TransferDetail] where TransferId = '{_id}'").UniqueResult();
                                        ss.Save(new TransferDetail
                                        {
                                            Id = Guid.NewGuid(),
                                            GiftId = new Guid(itm.GiftId),
                                            Amount = itm.Amount,
                                            TransferGift = newTransfer1,
                                            ReceivingDepartment = userinfo.Organization.Id,
                                            ReceivingPromotion = new Guid(itm.PromotionId)
                                        });
                                    }
                                }
                                break;
                        }
                        result = Constants.LUU_THANH_CONG;
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return result;
        }
        public string InitTransferDelete(Guid id)
        {
            var result = string.Empty;
            SessionManager.DoWork(ss =>
            {
                try
                {
                    ss.CreateSQLQuery($"delete [TransferDetail] where TransferId = '{id}'").UniqueResult();
                    ss.CreateSQLQuery($"delete [[TransferGiftLog]] where TransferGiftId = '{id}'").UniqueResult();
                    ss.CreateSQLQuery($"delete [TransferGift] where Id = '{id}'").UniqueResult();
                    result = "Đã xóa";
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    result = ex.Message;
                }
            });
            return result;
        }

        public string InitDieuChuyenNgangUpdate(List<DataNhapKho> lst, string flag, string fromOrganId, string toOrganId, string Id, ClaimsPrincipal principal)
        {
            var result = string.Empty;
            var userinfo = ContextProvider.GetUserInfo(principal);
            if (userinfo.OrganizationId == new Guid(fromOrganId))
            {
                var _productId = new Guid(Constants.ID_PRODUCT_DIEU_CHUYEN_NGANG);
                var _id = new Guid(Id);
                var checkAmount = false;
                try
                {
                    SessionManager.DoWork(ss =>
                    {
                        var product = ss.Get<Product>(_productId);
                        var stage = new Stage();

                        if (flag == Constants.DRAFT)
                        {
                            stage = ss.Query<Stage>().Single(p => p.ProductId == product.Id && p.PositionId == userinfo.Position.Id && p.Name == Constants.TAO_NHAP);
                            result = Constants.LUU_THANH_CONG;
                        }

                        if (flag == Constants.INITIALIZE)
                        {
                            stage = ss.Query<Stage>().Single(p => p.ProductId == product.Id && p.PositionId == userinfo.Position.Id && p.Name == Constants.GUI_DUYET);
                            result = Constants.GUI_DUYET_THANH_CONG;
                        }

                        // valid amount gift
                        foreach (var itm in lst)
                        {
                            // get amount in store:
                            var amount = ss.Query<Store>().Where(s => s.DepartmentId == userinfo.OrganizationId &&
                                s.GiftId == new Guid(itm.GiftId) &&
                                s.PromotionId == new Guid(itm.PromotionId)).FirstOrDefault().Amount;
                            if (amount >= itm.Amount)
                                checkAmount = true;

                            else
                            {
                                checkAmount = false;
                                result = $"{itm.GiftName} trong kho còn: {amount} < {itm.Amount}!\nAnh/Chị vui lòng kiểm tra lại.";
                                break;
                            }
                        }
                        if (checkAmount)
                        {
                            foreach (var itm in lst)
                            {
                                var newTransfer = ss.Query<TransferGift>().SingleOrDefault(p => p.Id == _id);
                                ss.CreateSQLQuery($"delete [TransferGiftLog] where TransferGiftId = '{_id}'").UniqueResult();
                                ss.CreateSQLQuery($"delete [TransferDetail] where TransferId = '{_id}'").UniqueResult();

                                var newTransferLog = new TransferGiftLog
                                {
                                    Id = Guid.NewGuid(),
                                    TransferGift = newTransfer,
                                    AssignUserId = userinfo.Id,
                                    AssignDeaprtmentId = userinfo.Organization.Id,
                                    Comment = null,
                                    Data = null,
                                    Status = newTransfer.Status,
                                    UpdateDate = newTransfer.CreatedDate,
                                    Stage = stage,
                                    Dealine = null
                                };

                                ss.Save(newTransferLog);
                                ss.Save(new TransferDetail
                                {
                                    Id = Guid.NewGuid(),
                                    GiftId = new Guid(itm.GiftId),
                                    Amount = itm.Amount,
                                    TransferGift = newTransfer,
                                    ReceivingDepartment = new Guid(toOrganId),
                                    ReceivingPromotion = new Guid(itm.PromotionId)
                                });
                            }
                        }
                    });
                }
                catch (Exception ex)
                {
                    result = ex.Message;
                    Console.WriteLine(result);
                }
            }
            else
                result = "Bạn không được điều chuyển ngang từ đơn vị khác!";
            return result;
        }

        public string InitDieuChuyenNoiBoUpdate(List<DataNhapKho> lst, string flag, string fromPromotionId, string toPromotionId, string Id, ClaimsPrincipal principal)
        {
            var result = string.Empty;
            var userinfo = ContextProvider.GetUserInfo(principal);
            var _productId = new Guid(Constants.ID_PRODUCT_DIEU_CHUYEN_NOI_BO);
            var checkAmount = false;
            try
            {
                SessionManager.DoWork(ss =>
                {
                    var product = ss.Get<Product>(_productId);
                    var _id = new Guid(Id);
                    var stage = new Stage();
                    var status = 0;
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
                        // valid amount gift
                        foreach (var itm in lst)
                        {
                            // get amount in store:
                            var amount = ss.Query<Store>().Where(s => s.DepartmentId == userinfo.OrganizationId &&
                            s.GiftId == new Guid(itm.GiftId) &&
                            s.PromotionId == new Guid(fromPromotionId)).FirstOrDefault().Amount;
                            if (amount >= itm.Amount)
                                checkAmount = true;

                            else
                            {
                                checkAmount = false;
                                result = $"{itm.GiftName} trong kho còn: {amount} < {itm.Amount}!\nAnh/Chị vui lòng kiểm tra lại.";
                                break;
                            }
                        }
                        if (checkAmount)
                        {
                            var transfer = ss.Query<TransferGift>().SingleOrDefault(p => p.Id == _id);

                            ss.CreateSQLQuery($"delete [TransferGiftLog] where TransferGiftId = '{_id}'").UniqueResult();
                            ss.CreateSQLQuery($"delete [TransferDetail] where TransferId = '{_id}'").UniqueResult();
                            transfer.PromotionId = new Guid(fromPromotionId);
                            ss.SaveOrUpdate(transfer);
                            var newTransferLog = new TransferGiftLog
                            {
                                Id = Guid.NewGuid(),
                                TransferGift = transfer,
                                AssignUserId = userinfo.Id,
                                AssignDeaprtmentId = userinfo.Organization.Id,
                                Comment = null,
                                Data = null,
                                Status = transfer.Status,
                                UpdateDate = transfer.CreatedDate,
                                Stage = stage,
                                Dealine = null
                            };
                            ss.Save(newTransferLog);
                            foreach (var itm in lst)
                            {
                                ss.Save(new TransferDetail
                                {
                                    Id = Guid.NewGuid(),
                                    GiftId = new Guid(itm.GiftId),
                                    Amount = itm.Amount,
                                    TransferGift = transfer,
                                    ReceivingDepartment = userinfo.Organization.Id,
                                    ReceivingPromotion = new Guid(toPromotionId)
                                });
                            }

                        }
                    }
                    else
                        result = Constants.CHUC_NANG_NHAN_VIEN;
                    
                });
            }
            catch (Exception ex)
            {
                result = ex.Message;
                Console.WriteLine(result);
            }
            return result;
        }
        #endregion
    }
}