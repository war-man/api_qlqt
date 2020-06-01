using Api.ManagerGift.DTO;
using Api.ManagerGift.Entities;
using Api.ManagerGift.Sessions;
using Newtonsoft.Json;
using NHibernate.Criterion;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;

namespace Api.ManagerGift.Services
{
    public class PromotionsService
    {
        public string LuuHoacGuiDuyet(PromotionsDTO obj, ClaimsPrincipal principal, string flag)
        {
            var result = string.Empty;
            try
            {
                SessionManager.DoWork(ss =>
                {
                    var userinfo = ContextProvider.GetUserInfo(principal);
                    var productId = new Guid(Constants.PARAM_TAO_CHUONG_TRINH_KHUYEN_MAI);
                    var stage = ss.Query<Stage>().Where(p => p.ProductId == productId && p.PositionId == userinfo.Position.Id && (p.Name == Constants.TAO_NHAP || p.Name == Constants.GUI_DUYET)).FirstOrDefault();
                    if (stage != null)
                    {
                        if (ss.Query<Promotion>().SingleOrDefault(p => p.Code == obj.Code) == null)
                        {
                            var giftPromotionId = Guid.NewGuid();
                            var giftPromotion = ConvertJsonToObject(obj.GiftPromotion);
                            var endDate = DateTime.ParseExact(new DateTime(obj.FinishDate.Year, obj.FinishDate.Month, obj.FinishDate.Day, 23, 59, 59).ToString("yyyy-MM-dd hh:mm:ss tt"), "yyyy-MM-dd hh:mm:ss tt", CultureInfo.InvariantCulture);
                            if (flag == Constants.DRAFT)
                            {
                                var newPromotion = new Promotion
                                {
                                    Id = Guid.NewGuid(),
                                    GiftPromotionId = giftPromotionId,
                                    Code = obj.Code,
                                    Name = obj.Name,
                                    Status = (int)ContextProvider.statusTransfer.Draft,
                                    CreatedBy = userinfo.Id,
                                    CreatedDate = DateTime.ParseExact(DateTime.Now.ToString("u"), "u", CultureInfo.InvariantCulture),
                                    NumberOdEdit = 0,
                                    ConfigPromotion = obj.ConfigPromotion,
                                    StartDate = obj.StartDate,
                                    FinishDate = endDate,
                                    Description = obj.Description,
                                    MaxGiftWithCustomer = obj.MaxGiftWithCustomer,
                                    MaxGiftInDay = obj.MaxGiftInDay,
                                    IsChange = obj.IsChange,
                                };
                                var newPromotionLog = new PromotionLog
                                {
                                    Id = Guid.NewGuid(),
                                    Status = (int)ContextProvider.statusTransfer.Draft,
                                    AssignUserId = userinfo.Id,
                                    AssignDeaprtmentId = userinfo.OrganizationId,
                                    UpdateDate = DateTime.ParseExact(DateTime.Now.ToString("u"), "u", CultureInfo.InvariantCulture),
                                    Promotion = newPromotion
                                };

                                ss.Save(newPromotion);
                                ss.Save(newPromotionLog);

                                foreach (var itm in giftPromotion)
                                {
                                    var newGiftPromotion = new GiftPromotion
                                    {
                                        Id = new Guid(),
                                        GiftId = itm.GiftId,
                                        GiftPromotionId = giftPromotionId,
                                        Amount = itm.Amount,
                                        //Price = itm.Price,
                                        //CodeGift = itm.CodeGift,
                                        //NameGift = itm.NameGift,
                                        //UnitName = itm.UnitName,
                                    };

                                    ss.Save(newGiftPromotion);
                                }
                                result = Constants.LUU_THANH_CONG;
                            }

                            if (flag == Constants.INITIALIZE)
                            {
                                var checkAmount = false;

                                // valid Amount Gift
                                if (giftPromotion.Count() > 0)
                                {
                                    var departmentId = userinfo.OrganizationId;
                                    foreach (var itm in giftPromotion)
                                    {
                                        var giftId = itm.GiftId;
                                        //var giftName = itm.NameGift;
                                        var giftAmount = itm.Amount;

                                        // số lượng quà tặng trong Store.
                                        var amount = ss.Query<Store>()
                                            .Where(s => s.DepartmentId == departmentId && s.GiftId == giftId && s.PromotionId == null)
                                            .Select(p => p.Amount)
                                            .FirstOrDefault();

                                        if (amount >= giftAmount)
                                            checkAmount = true;

                                        else
                                        {
                                            checkAmount = false;
                                            result = $"Trong kho chỉ còn {amount} < {giftAmount} !\nAnh/Chị vui lòng kiểm tra lại.";
                                            break;
                                        }
                                    }
                                }

                                if (checkAmount)
                                {
                                    var newPromotion = new Promotion
                                    {
                                        Id = Guid.NewGuid(),
                                        GiftPromotionId = giftPromotionId,
                                        Code = obj.Code,
                                        Name = obj.Name,
                                        Status = (int)ContextProvider.statusTransfer.Initialize,
                                        CreatedBy = userinfo.Id,
                                        CreatedDate = DateTime.ParseExact(DateTime.Now.ToString("u"), "u", CultureInfo.InvariantCulture),
                                        NumberOdEdit = 0,
                                        ConfigPromotion = obj.ConfigPromotion,
                                        StartDate = obj.StartDate,
                                        FinishDate = endDate,//obj.FinishDate,
                                        Description = obj.Description,
                                        MaxGiftWithCustomer = obj.MaxGiftWithCustomer,
                                        MaxGiftInDay = obj.MaxGiftInDay,
                                        IsChange = obj.IsChange,
                                    };
                                    var newPromotionLog = new PromotionLog
                                    {
                                        Id = Guid.NewGuid(),
                                        Status = (int)ContextProvider.statusTransfer.Initialize,
                                        AssignUserId = userinfo.Id,
                                        AssignDeaprtmentId = userinfo.OrganizationId,
                                        UpdateDate = DateTime.ParseExact(DateTime.Now.ToString("u"), "u", CultureInfo.InvariantCulture),
                                        Promotion = newPromotion
                                    };

                                    ss.Save(newPromotion);
                                    ss.Save(newPromotionLog);

                                    foreach (var itm in giftPromotion)
                                    {
                                        var newGiftPromotion = new GiftPromotion
                                        {
                                            Id = new Guid(),
                                            GiftId = itm.GiftId,
                                            GiftPromotionId = giftPromotionId,
                                            Amount = itm.Amount,
                                            //Price = itm.Price,
                                            //CodeGift = itm.CodeGift,
                                            //NameGift = itm.NameGift,
                                            //UnitName = itm.UnitName,
                                        };

                                        ss.Save(newGiftPromotion);
                                    }
                                    result = Constants.GUI_DUYET_THANH_CONG;
                                }
                            }
                        }
                        else
                            result = $"Mã {obj.Code} đã được sử dụng!\nAnh/Chị vui lòng kiểm tra lại.";
                    }
                    else
                        result = Constants.CHUC_NANG_NHAN_VIEN;
                });
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }

        public string GuiDuyet(Guid promotionId, ClaimsPrincipal principal)
        {
            var result = string.Empty;
            try
            {
                SessionManager.DoWork(ss =>
                {
                    var checkAmount = false;
                    var promotion = ss.Query<Promotion>().Where(p => p.Id == promotionId).First();
                    var giftPromotionId = promotion.GiftPromotionId;

                    var userDTO = ContextProvider.GetUserInfo(principal);
                    var departmentId = userDTO.OrganizationId;
                    var productId = new Guid(Constants.PARAM_TAO_CHUONG_TRINH_KHUYEN_MAI);
                    var stage = ss.Query<Stage>().SingleOrDefault(p => p.ProductId == productId && p.PositionId == userDTO.Position.Id && p.Name == Constants.TAO_NHAP);
                    if (stage != null)
                    {
                        // lấy quà tặng gắn với chương trình khuyến mãi.
                        var listGift = ss.Query<GiftPromotion>().Where(p => p.GiftPromotionId == giftPromotionId).ToList();
                        if (listGift.Count > 0)
                        {
                            foreach (var itm in listGift)
                            {
                                // số lượng quà tặng trong Store.
                                var amount = ss.Query<Store>()
                                    .Where(s => s.DepartmentId == departmentId && s.GiftId == itm.GiftId && s.PromotionId == null)
                                    .Select(p => p.Amount)
                                    .FirstOrDefault();
                                if (amount >= itm.Amount)
                                    checkAmount = true;

                                else
                                {
                                    checkAmount = false;
                                    result = $"Trong kho chỉ còn {amount} < {itm.Amount} !\nAnh/Chị vui lòng kiểm tra lại.";
                                    break;
                                }
                            }
                        }

                        if (checkAmount)
                        {
                            promotion.Status = (int)ContextProvider.statusTransfer.Initialize;
                            var newPromotionLog = new PromotionLog
                            {
                                Id = Guid.NewGuid(),
                                Status = (int)ContextProvider.statusTransfer.Initialize,
                                AssignUserId = userDTO.Id,
                                AssignDeaprtmentId = userDTO.OrganizationId,
                                UpdateDate = DateTime.ParseExact(DateTime.Now.ToString("u"), "u", CultureInfo.InvariantCulture),
                                Promotion = promotion
                            };
                            ss.Save(newPromotionLog);
                            result = "Gửi duyệt thành công!";
                        }
                    }
                    else
                        result = Constants.CHUC_NANG_NHAN_VIEN;
                });
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// get danh sách promotion cho hiển thị list.
        /// </summary>
        public dynamic Get(ClaimsPrincipal principal, int pageNo, string status, string namePromotion, int year)
        {
            int pageSize = 20;
            dynamic lstResults = new ExpandoObject();
            namePromotion = string.IsNullOrWhiteSpace(namePromotion) ? "" : namePromotion;
            status = string.IsNullOrWhiteSpace(status) ? "" : status;
            var user = ContextProvider.GetUserInfo(principal);
            var isTypeUser = ContextProvider.CheckPermission(user.PermisionId);
            var isLDCN_PGD = isTypeUser == 3 && user.Position.IsLeader ? true : false;
            SessionManager.DoWork(ss =>
            {
                try
                {
                    var lstUser = ss.Query<User>().ToList();
                    var listPromotions = ss.QueryOver<Promotion>()
                        .Where(p => p.Name.IsLike(namePromotion, MatchMode.Anywhere)
                            || p.Code.IsLike(namePromotion, MatchMode.Anywhere)).OrderBy(p => p.Name).Asc.List();
                    if (!string.IsNullOrEmpty(status))
                        listPromotions = listPromotions.Where(p => p.Status == int.Parse(status)).ToList();
                    if (year != 0)
                        listPromotions = listPromotions.Where(p => p.CreatedDate.Value.Year == year).ToList();
                    if (user.Position.IsLeader)
                        listPromotions = listPromotions.Where(p => p.Status != 0).ToList();
                    if(isTypeUser!=1&& isTypeUser!=2)
                    {
                        var ids = ss.Query<Store>().Where(w=>w.DepartmentId==user.Organization.Id).Select(s=>s.PromotionId).ToList();
                        listPromotions = listPromotions.Where(p => ids.Contains(p.Id) && p.Status == 2).ToList();
                    }
                    var PromotionIds = listPromotions.Select(s => s.Id).Distinct().ToList();
                    Guid defaultId = Guid.NewGuid();
                    var tranfers = ss.Query<TransferGift>().Where(p => PromotionIds.Contains(p.PromotionId ?? defaultId)).
                    Select(s => new
                    {
                        s.Id,
                        s.FlagDieuChuyen,
                        s.CreatedBy,
                        s.PromotionId,
                        ProductId = s.Product.Id,
                        s.Status,
                        s.DepartmentId,
                        s.CreatedDate
                    }).Distinct().ToList();
                    var list = listPromotions.Skip((pageNo - 1) * pageSize).Take(pageSize)
                        .Select(p => new
                        {
                            p.Id,
                            p.GiftPromotionId,
                            p.MaxGiftInDay,
                            p.MaxGiftWithCustomer,
                            p.ConfigPromotion,
                            p.Description,
                            p.IsChange,
                            p.Code,
                            p.Name,
                            p.Status,
                            StartDate = ContextProvider.GetConvertDatetime(p.StartDate),
                            FinishDate = ContextProvider.GetConvertDatetime(p.FinishDate),
                            CreatedBy = ContextProvider.GetFullName(lstUser, p.CreatedBy),
                            CreatedDate = ContextProvider.GetConvertDatetime(p.CreatedDate),
                            NguoiDuyet = ContextProvider.GetFullName(lstUser, p.NguoiDuyet),
                            NgayDuyet = ContextProvider.GetConvertDatetime(p.NgayDuyet),
                            Tranfers = tranfers.Where(w => w.PromotionId == p.Id && w.CreatedBy == p.CreatedBy)
                                            .OrderByDescending(o => o.CreatedDate),
                            OrderByCreateDate = p.CreatedDate,
                            StatusOrderBy = ContextProvider.OrderBy(p.Status, user.Position.IsLeader, isTypeUser),
                            UserCreateId = p.CreatedBy,
                        }).ToList();
                    lstResults.ListPromotions = list.OrderByDescending(o => o.OrderByCreateDate).OrderBy(p => p.StatusOrderBy).ToList();
                    var total = list.Count();
                    lstResults.TotalPage = total % pageSize == 0 ? total / pageSize : total / pageSize + 1;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            });
            return lstResults;
        }

        /// <summary>
        /// get danh sách promotion cho combobox.
        /// </summary>
        /// <returns>list promotion</returns>
        public List<dynamic> Get(ClaimsPrincipal principal)
        {
            var lstResults = new List<dynamic>();
            var user = ContextProvider.GetUserInfo(principal);
            SessionManager.DoWork(ss =>
            {
                try
                {
                    var lst = ss.Query<Promotion>().Where(s => s.Status == 2).ToList();
                    if (ContextProvider.CheckPermission(user.PermisionId) == 3 || ContextProvider.CheckPermission(user.PermisionId) == 6)
                    {
                        var ids = ss.Query<TransferDetail>().Where(s => s.ReceivingDepartment == user.OrganizationId && s.TransferGift.Status==2).Select(s => s.ReceivingPromotion).ToList();
                        lst = lst.Where(w => ids.Contains(w.Id)).ToList();
                        var idPromotions = lst.Select(s => s.Id).ToList();
                        var listDetails = ss.Query<TransferGift>().Where(s => idPromotions.Contains(s.PromotionId??Guid.NewGuid()) && s.DepartmentId== user.OrganizationId && s.Product.Id== Guid.Parse(Constants.ID_PRODUCT_HOAN_PHAN_BO_QUA_TANG)).ToList();
                        lst.ForEach(pb=> {
                            var count = 0;
                            count = listDetails.Where(w => w.PromotionId == pb.Id).ToList().Count();
                            pb.SoLanHPB = count;
                        });
                    }
                    lstResults = lst.Select(p => (dynamic)new
                                    {
                                        p.Id,
                                        p.GiftPromotionId,
                                        p.MaxGiftInDay,
                                        p.MaxGiftWithCustomer,
                                        p.ConfigPromotion,
                                        p.Description,
                                        p.IsChange,
                                        p.Code,
                                        p.Name,
                                        p.Status,
                                        p.NumberOdEdit,
                                        p.SoLanHPB,
                                        value = p.Code,
                                        label = p.Name
                                    }).ToList();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            });
            return lstResults;
        }

        /// <summary>
        /// get danh sách quà tặng ứng với chương trình khuyến mãi
        /// </summary>
        /// <param name="Id">id chuong trình khuyến mãi</param>
        /// <returns></returns>
        public List<dynamic> Get(ClaimsPrincipal principal, Guid Id)
        {
            var lstResults = new List<dynamic>();
            var user = ContextProvider.GetUserInfo(principal);
            SessionManager.DoWork(ss =>
            {
                try
                {
                    var giftPmomotion = ss.Query<GiftPromotion>().Where(sp => sp.GiftPromotionId == Id).ToList();
                    var gifts = ss.Query<Gift>();

                    lstResults = (from _giftPmomotion in giftPmomotion
                                  join _gifts in gifts on _giftPmomotion.GiftId equals _gifts.Id
                                  select (dynamic)new
                                  {
                                      _giftPmomotion.GiftId,
                                      _giftPmomotion.GiftPromotionId,
                                      _giftPmomotion.Amount,
                                      CodeGift = _gifts.Code,
                                      NameGift = _gifts.Name,
                                      _gifts.Price,
                                      UnitName = _gifts.Unit.Name,
                                      value = _gifts.Name,
                                      label = _gifts.Code
                                  }).ToList();
                    if (ContextProvider.CheckPermission(user.PermisionId) == 3)
                    {
                        var ids = ss.Query<TransferDetail>().Where(s => s.ReceivingDepartment == user.OrganizationId).Select(s => s.GiftId).ToList();
                        lstResults = lstResults.Where(w => ids.Contains(w.GiftId)).ToList();
                    }
                    //lstResults = ss.Query<GiftPromotion>().Where(sp => sp.GiftPromotionId == Id)
                    //                .Select(p => (dynamic)new
                    //                {
                    //                    p.GiftId,
                    //                    p.GiftPromotionId,
                    //                    //p.CodeGift,
                    //                    //p.NameGift,
                    //                    //p.Price,
                    //                    //p.UnitName,
                    //                    //p.Amount,
                    //                    //value = p.NameGift,
                    //                    //label = p.CodeGift
                    //                }).ToList();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            });
            return lstResults;
        }

        /// <summary>
        /// get chi tiet chuong trinh khuyen mai
        /// </summary>
        /// <param name="IdPromotion"></param>
        /// <returns></returns>
        public dynamic GetPromotion(ClaimsPrincipal principal, Guid IdPromotion)
        {
            dynamic lstResults = new ExpandoObject();
            SessionManager.DoWork(ss =>
            {
                try
                {
                    var userinfo = ContextProvider.GetUserInfo(principal);
                    var lstUser = ss.Query<User>().ToList();

                    #region promotion
                    var promotion = ss.Query<Promotion>().Where(sp => sp.Id == IdPromotion)
                                                .Select(p => new
                                                {
                                                    p.Id,
                                                    p.GiftPromotionId,
                                                    p.Code,
                                                    p.Name,
                                                    p.Status,
                                                    p.NumberOdEdit,
                                                    p.ConfigPromotion,
                                                    p.Description,
                                                    p.MaxGiftInDay,
                                                    p.MaxGiftWithCustomer,
                                                    p.IsChange,
                                                    //StartDate = ContextProvider.GetConvertDatetime(p.StartDate),
                                                    //FinishDate = ContextProvider.GetConvertDatetime(p.FinishDate),
                                                    StartDate = p.StartDate,
                                                    FinishDate = p.FinishDate,
                                                    CreatedBy = ContextProvider.GetFullName(lstUser, p.CreatedBy),
                                                    CreatedDate = ContextProvider.GetConvertDatetimeDDMMYYYHHmm(p.CreatedDate),
                                                    NguoiDuyet = ContextProvider.GetFullName(lstUser, p.NguoiDuyet),
                                                    NgayDuyet = ContextProvider.GetConvertDatetimeDDMMYYYHHmm(p.NgayDuyet)
                                                }).FirstOrDefault();
                    #endregion

                    var giftPromotionId = promotion.GiftPromotionId;
                    var giftPromotions = ss.Query<GiftPromotion>()
                                    .Where(sp => sp.GiftPromotionId == giftPromotionId)
                                    .Select(p => new
                                    {
                                        p.Id,
                                        p.Amount,
                                        p.GiftPromotionId,
                                        p.GiftId,
                                    }).ToList();
                    var giftIds = giftPromotions.Select(p => p.GiftId).ToList();
                    var gifts = ss.Query<Gift>().Where(w => giftIds.Contains(w.Id)).ToList();
                    var transferDetails = ss.Query<TransferDetail>().Where(w => giftIds.Contains(w.GiftId)).ToList();
                    var groupByTDs = transferDetails.GroupBy(g => new
                    {
                        g.GiftId,
                        g.TransferGift.Id,
                        g.TransferGift.Status
                    }).Select(s => new
                    {
                        s.Key.GiftId,
                        TransferGiftId = s.Key.Id,
                        Total = s.Sum(t => t.Amount)
                    }).ToList();
                    var transferGiftIds = transferDetails.Select(p => p.TransferGift.Id).Distinct().ToList();
                    var tranferGifts = ss.Query<TransferGift>().Where(w => transferGiftIds.Contains(w.Id) && w.Status == 2).ToList();

                    #region Kho
                    var stores = ss.Query<Store>().Where(s => giftIds.Contains(s.GiftId)).ToList();
                    var gbStore = stores.GroupBy(g => new { g.GiftId, g.DepartmentId }).Select(s => new
                    {
                        s.Key.GiftId,
                        s.Key.DepartmentId,
                        Total = s.Sum(t => t.Amount)
                    }).ToList();
                    #endregion

                    #region SL Xuất kho
                    var idTranXK = tranferGifts.Where(w => w.Product.Id.ToString().ToUpper() == Constants.PARAM_XUAT_KHO && w.Status == 2).Select(s => s.Id);
                    var gbXKDetails = transferDetails.Where(s => idTranXK.Contains(s.TransferGift.Id)).GroupBy(g => new { g.GiftId, g.ReceivingDepartment }).Select(s => new
                    {
                        s.Key.GiftId,
                        DepartmentId = s.Key.ReceivingDepartment,
                        Total = s.Sum(t => t.Amount)
                    }).ToList();
                    #endregion

                    #region SL Dieu chuyen ngang
                    var idTranDCN = tranferGifts.Where(w => w.Product.Id.ToString().ToUpper() == Constants.PARAM_DIEU_CHUYEN_NGANG && w.Status == 2).Select(s => s.Id);
                    var dieuChuyenN = transferDetails.Where(s => idTranDCN.Contains(s.TransferGift.Id) && !(s.ReceivingDepartment == userinfo.Organization.Id && s.TransferDepartment != userinfo.Organization.Id)).ToList();
                    var gbDCNDetails = dieuChuyenN.GroupBy(g => new { g.GiftId, g.ReceivingDepartment }).Select(s => new
                    {
                        s.Key.GiftId,
                        DepartmentId = s.Key.ReceivingDepartment,
                        Total = s.Sum(t => t.Amount)
                    }).ToList();

                    var dcNhan = transferDetails.Where(s => idTranDCN.Contains(s.TransferGift.Id) && s.ReceivingDepartment == userinfo.Organization.Id && s.TransferDepartment != userinfo.Organization.Id).ToList();
                    var gbDCNs = dcNhan.GroupBy(g => new { g.GiftId, g.ReceivingDepartment }).Select(s => new
                    {
                        s.Key.GiftId,
                        DepartmentId = s.Key.ReceivingDepartment,
                        Total = s.Sum(t => t.Amount)
                    }).ToList();
                    var dcNgangChuyen = transferDetails.Where(s => idTranDCN.Contains(s.TransferGift.Id) && s.TransferDepartment == userinfo.Organization.Id).ToList();
                    var gbDCNChuyens = dcNgangChuyen.GroupBy(g => new { g.GiftId, g.ReceivingDepartment }).Select(s => new
                    {
                        s.Key.GiftId,
                        DepartmentId = s.Key.ReceivingDepartment,
                        Total = s.Sum(t => t.Amount)
                    }).ToList();
                    #endregion

                    #region SL Dieu chuyen noi bo
                    var idTranDCNB = tranferGifts.Where(w => w.Product.Id.ToString().ToUpper() == Constants.PARAM_DIEU_CHUYEN_NOI_BO && w.Status == 2).Select(s => s.Id);
                    //var dieuChuyenNB = transferDetails.Where(s => idTranDCNB.Contains(s.TransferGift.Id) && !(s.ReceivingDepartment == userinfo.Organization.Id && s.TransferDepartment != userinfo.Organization.Id)).ToList();
                    var gbDCNBDetails = transferDetails.Where(s => idTranDCNB.Contains(s.TransferGift.Id)).GroupBy(g => new { g.GiftId, g.ReceivingDepartment }).Select(s => new
                    {
                        s.Key.GiftId,
                        DepartmentId = s.Key.ReceivingDepartment,
                        Total = s.Sum(t => t.Amount)
                    }).ToList();
                    #endregion

                    #region SL Phan Bo
                    var idTranPBo = tranferGifts.Where(w => w.Product.Id.ToString().ToUpper() == Constants.PARAM_PHAN_BO && w.Status == 2).Select(s => s.Id);
                    var gbPBoDetails = transferDetails.Where(s => idTranPBo.Contains(s.TransferGift.Id)).GroupBy(g => new { g.GiftId, g.ReceivingDepartment, g.TransferGift.Status }).Select(s => new
                    {
                        s.Key.GiftId,
                        DepartmentId = s.Key.ReceivingDepartment,
                        Status = s.Key.Status,
                        Total = s.Sum(t => t.Amount)
                    }).ToList();
                    #endregion

                    #region SL Hoan Phan Bo
                    var idTranHPBo = tranferGifts.Where(w => w.Product.Id.ToString().ToUpper() == Constants.PARAM_HOAN_PHAN_BO && w.Status == 2).Select(s => s.Id);
                    var gbHPBoDetails = transferDetails.Where(s => idTranHPBo.Contains(s.TransferGift.Id)).GroupBy(g => new { g.GiftId, g.ReceivingDepartment,g.TransferGift.Status }).Select(s => new
                    {
                        s.Key.GiftId,
                        DepartmentId = s.Key.ReceivingDepartment,
                        Status = s.Key.Status,
                        Total = s.Sum(t => t.Amount)
                    }).ToList();
                    #endregion
                    #region SL Qua da tang
                    var quaTang = ss.Query<CustomerGift>().Where(w => w.Promotion.Id == promotion.Id && w.Status == 2).ToList();
                    #endregion

                    var isHO = userinfo.OrganizationCode == "QLBH" ? true : false;
                    var listGift = new List<object>();
                    giftPromotions.ForEach(itm =>
                    {
                        var gift = gifts.FirstOrDefault(f => f.Id == itm.GiftId);
                        var xuatKhos = gbXKDetails.Where(f => f.GiftId == itm.GiftId).ToList();
                        var tonKhos = gbStore.Where(f => f.GiftId == itm.GiftId).ToList();
                        var phanbos = gbPBoDetails.Where(f => f.GiftId == itm.GiftId).ToList();
                        var hoanphanbos = gbHPBoDetails.Where(f => f.GiftId == itm.GiftId).ToList();
                        var dcNgangs = gbDCNDetails.Where(f => f.GiftId == itm.GiftId).ToList();
                        var dcNoiBos = gbDCNBDetails.Where(f => f.GiftId == itm.GiftId).ToList();
                        var slDaTang = quaTang.Where(w => w.Gift.Id == itm.GiftId).Sum(s=>s.NumGift);
                        var totalXK = xuatKhos.Select(s => s.Total).Sum();
                        //var totalTonKho = tonKhos.Select(s => s.Total).Sum();
                        var totalDCN = dcNgangs.Select(s => s.Total).Sum();
                        var totalDCNB = dcNoiBos.Select(s => s.Total).Sum();
                        var slPhanBo = itm.Amount;
                        var slHPhanBo = hoanphanbos.Select(s => s.Total).Sum();
                        if (!isHO)
                        {
                            totalXK = xuatKhos.Where(w => w.DepartmentId == userinfo.Organization.Id).Select(s => s.Total).Sum();
                            //totalTonKho = tonKhos.Where(w => w.DepartmentId == userinfo.Organization.Id).Select(s => s.Total).Sum();
                            var dcNgangNhans = gbDCNs.Where(f => f.GiftId == itm.GiftId).ToList();
                            var dcNgangChuyens = gbDCNChuyens.Where(f => f.GiftId == itm.GiftId).ToList();
                            slPhanBo = (phanbos.Where(w => w.DepartmentId == userinfo.Organization.Id && w.Status==2).Select(s => s.Total).Sum() + dcNgangNhans.Select(s => s.Total).Sum());
                            totalDCN = dcNgangs.Where(w => w.DepartmentId == userinfo.Organization.Id).Select(s => s.Total).Sum() + dcNgangChuyens.Select(s => s.Total).Sum();
                            totalDCNB = dcNoiBos.Where(w => w.DepartmentId == userinfo.Organization.Id).Select(s => s.Total).Sum();
                            slHPhanBo = hoanphanbos.Where(w => w.DepartmentId == userinfo.Organization.Id).Select(s => s.Total).Sum();
                        }
                        var totalTonKho = (slPhanBo - totalXK - totalDCN - totalDCNB) + (isHO ? (+slHPhanBo ): (-slHPhanBo));
                        var totalPricePB = slPhanBo * (gift == null ? 0 : gift.Price);
                        var item = new
                        {
                            Id = itm.Id,
                            Amount = itm.Amount,
                            GiftPromotionId = itm.GiftPromotionId,
                            GiftId = itm.GiftId,
                            NameGift = gift == null ? "" : gift.Name,
                            CodeGift = gift == null ? "" : gift.Code,
                            Price = gift == null ? 0 : gift.Price,
                            UnitName = gifts.FirstOrDefault(f => f.Id == itm.GiftId)?.Unit.Name,
                            SLPhanBo = slPhanBo,
                            SLHuHong = totalXK,
                            SLDCKhacDV = totalDCN,
                            SLDCNoiBo = totalDCNB,
                            SLTang = slDaTang,
                            SLHoanPB = slHPhanBo,
                            SLTon = totalTonKho,
                            TotalPricePB = totalPricePB,
                            TotalPriceStore = totalTonKho * (gift == null ? 0 : gift.Price)
                        };
                        listGift.Add(item);
                    });
                    //var listGift = (from _giftPromotions in giftPromotions
                    //                join _gifts in gifts on _giftPromotions.GiftId equals _gifts.Id
                    //                select new
                    //                {
                    //                    _giftPromotions.Id,
                    //                    _giftPromotions.Amount,
                    //                    _giftPromotions.GiftPromotionId,
                    //                    _giftPromotions.GiftId,
                    //                    NameGift = _gifts.Name,
                    //                    CodeGift = _gifts.Code,
                    //                    _gifts.Price,
                    //                    UnitName = _gifts.Unit.Name,
                    //                    SLPhanBo = isHO ? _giftPromotions.Amount : 0,
                    //                    SLHuHong = 0,
                    //                    SLDCKhacDV = 0,
                    //                    SLDCNoiBo=0,
                    //                    SLTang = 0,
                    //                    SLHoanPB = 0,
                    //                    SLTon = 0,
                    //                    TotalPricePB = isHO ? (_giftPromotions.Amount * _gifts.Price) : 0
                    //                }).ToList();

                    lstResults.Promotion = promotion;
                    lstResults.ListGift = listGift;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            });
            return lstResults;
        }

        public List<dynamic> GetPromotionDep(ClaimsPrincipal principal, Guid orgId)
        {
            var lstResults = new List<dynamic>();
            var user = ContextProvider.GetUserInfo(principal);
            SessionManager.DoWork(ss =>
            {
                try
                {
                    lstResults = ss.Query<Promotion>().Where(s => s.Status == 2)
                                    .Select(p => (dynamic)new
                                    {
                                        p.Id,
                                        p.GiftPromotionId,
                                        p.MaxGiftInDay,
                                        p.MaxGiftWithCustomer,
                                        p.ConfigPromotion,
                                        p.Description,
                                        p.IsChange,
                                        p.Code,
                                        p.Name,
                                        p.Status,
                                        p.NumberOdEdit,
                                        p.SoLanHPB,
                                        value = p.Code,
                                        label = p.Name
                                    }).ToList();
                    var ids = ss.Query<TransferDetail>().Where(s => s.ReceivingDepartment == orgId).Select(s => s.ReceivingPromotion).ToList();
                    lstResults = lstResults.Where(w => ids.Contains(w.Id)).ToList();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            });
            return lstResults;
        }

        public string ActionLanhDao(Guid idPromotion, string note, string flag, ClaimsPrincipal principal)
        {
            var result = string.Empty;
            var userDTO = ContextProvider.GetUserInfo(principal);

            try
            {
                SessionManager.DoWork(ss =>
                {
                    var productId = new Guid(Constants.PARAM_TAO_CHUONG_TRINH_KHUYEN_MAI);
                    var stage = ss.Query<Stage>().Where(p => p.ProductId == productId && p.PositionId == userDTO.Position.Id && (p.Name == Constants.DUYET || p.Name == Constants.TU_CHOI_DUYET)).FirstOrDefault();
                    if (stage != null)
                    {
                        var promotion = ss.Get<Promotion>(idPromotion);
                        var checkAmount = false;

                        if (flag == Constants.APPROVE)
                        {
                            var giftPromotionId = promotion.GiftPromotionId;
                            var departmentId = userDTO.OrganizationId;

                            // lấy danh sách quà tặng gắn với chương trình khuyến mãi.
                            var listGift = ss.Query<GiftPromotion>().Where(p => p.GiftPromotionId == giftPromotionId).ToList();
                            if (listGift.Count > 0)
                            {
                                foreach (var itm in listGift)
                                {
                                    // lấy quà tặng trong kho để check valid.
                                    var amount = ss.Query<Store>()
                                                    .Where(s => s.DepartmentId == departmentId && s.GiftId == itm.GiftId && s.PromotionId == null)
                                                    .Select(p => p.Amount)
                                                    .FirstOrDefault();

                                    if (amount >= itm.Amount)
                                        checkAmount = true;

                                    else
                                    {
                                        checkAmount = false;
                                        result = $"Trong kho chỉ còn {amount} < {itm.Amount} !\nAnh/Chị vui lòng kiểm tra lại.";
                                        break;
                                    }
                                }
                            }

                            if (checkAmount)
                            {
                                promotion.Status = (int)ContextProvider.statusTransfer.Approve;
                                promotion.NgayDuyet = DateTime.ParseExact(DateTime.Now.ToString("u"), "u", CultureInfo.InvariantCulture);
                                promotion.NguoiDuyet = userDTO.Id;

                                var newPromotionLog = new PromotionLog
                                {
                                    Id = Guid.NewGuid(),
                                    Status = (int)ContextProvider.statusTransfer.Approve,
                                    AssignUserId = userDTO.Id,
                                    AssignDeaprtmentId = userDTO.OrganizationId,
                                    UpdateDate = DateTime.ParseExact(DateTime.Now.ToString("u"), "u", CultureInfo.InvariantCulture),
                                    Promotion = promotion
                                };
                                ss.Save(newPromotionLog);

                                foreach (var itm in listGift)
                                {
                                    var store = ss.Query<Store>()
                                                    .Where(s => s.DepartmentId == departmentId && s.GiftId == itm.GiftId && s.PromotionId == null)
                                                    .FirstOrDefault();
                                    store.Amount -= itm.Amount;
                                    var newStore = new Store
                                    {
                                        Id = new Guid(),
                                        DepartmentId = userDTO.OrganizationId,
                                        PromotionId = idPromotion,
                                        GiftId = itm.GiftId,
                                        Amount = itm.Amount,
                                        UpdatedDate = DateTime.ParseExact(DateTime.Now.ToString("u"), "u", CultureInfo.InvariantCulture)
                                    };
                                    ss.Save(newStore);
                                }
                                result = Constants.DUYET_THANH_CONG;
                            }
                        }

                        if (flag == Constants.REFUSE)
                        {
                            promotion.Status = (int)ContextProvider.statusTransfer.Refuse;
                            promotion.NgayDuyet = DateTime.ParseExact(DateTime.Now.ToString("u"), "u", CultureInfo.InvariantCulture);
                            promotion.NguoiDuyet = userDTO.Id;

                            var newPromotionLog = new PromotionLog
                            {
                                Id = Guid.NewGuid(),
                                Status = (int)ContextProvider.statusTransfer.Refuse,
                                AssignUserId = userDTO.Id,
                                AssignDeaprtmentId = userDTO.OrganizationId,
                                UpdateDate = DateTime.ParseExact(DateTime.Now.ToString("u"), "u", CultureInfo.InvariantCulture),
                                Comment = note,
                                Promotion = promotion
                            };
                            ss.Save(newPromotionLog);
                            result = Constants.TU_CHOI_DUYET_THANH_CONG;
                        }
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

        #region Nguyen Code
        public string UpdatePromotion(PromotionsDTO obj, ClaimsPrincipal principal, string flag)
        {
            var result = string.Empty;
            SessionManager.DoWork(ss =>
            {
                try
                {
                    var promotion = ss.Query<Promotion>().SingleOrDefault(p => p.Id == obj.Id);
                    if (promotion == null
                        || (promotion != null && flag == "Update"))
                    {
                        var userDTO = ContextProvider.GetUserInfo(principal);
                        var giftPromotionId = Guid.NewGuid();
                        var giftPromotion = ConvertJsonToObjectPromotion(obj.GiftPromotion);
                        if (flag == "Update")
                        {
                            var endDate = DateTime.ParseExact(new DateTime(obj.FinishDate.Year, obj.FinishDate.Month, obj.FinishDate.Day, 23, 59, 59).ToString("yyyy-MM-dd hh:mm:ss tt"), "yyyy-MM-dd hh:mm:ss tt", CultureInfo.InvariantCulture);
                            promotion.Code = obj.Code;
                            promotion.Name = obj.Name;
                            //promotion.Status = (int)ContextProvider.statusTransfer.Draft;
                            promotion.NumberOdEdit = 0;
                            promotion.ConfigPromotion = obj.ConfigPromotion;
                            promotion.StartDate = obj.StartDate;
                            promotion.FinishDate = endDate;
                            promotion.Description = obj.Description;
                            promotion.MaxGiftWithCustomer = obj.MaxGiftWithCustomer;
                            promotion.MaxGiftInDay = obj.MaxGiftInDay;
                            promotion.IsChange = obj.IsChange;

                            var newPromotionLog = ss.Query<PromotionLog>().FirstOrDefault(f => f.Promotion.Id == promotion.Id);
                            newPromotionLog.UpdateDate = DateTime.ParseExact(DateTime.Now.ToString("u"), "u", CultureInfo.InvariantCulture);
                            newPromotionLog.Promotion = promotion;

                            ss.SaveOrUpdate(promotion);
                            ss.SaveOrUpdate(newPromotionLog);
                            ss.CreateSQLQuery($"delete [GiftPromotion] where GiftPromotionId = '{promotion.GiftPromotionId}'").UniqueResult();
                            foreach (var itm in giftPromotion)
                            {
                                var newGiftPromotion = new GiftPromotion
                                {
                                    Id = new Guid(),
                                    GiftId = itm.GiftId,
                                    GiftPromotionId = promotion.GiftPromotionId,
                                    Amount = itm.Amount,
                                };

                                ss.Save(newGiftPromotion);
                            }
                            result = "Cập nhật chương trình khuyến mãi thành công!";
                        }
                    }
                    else
                        result = $"{obj.Code} đã được sử dụng!\nAnh/Chị vui lòng kiểm tra lại.";
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    result = ex.Message;
                }
            });
            return result;
        }
        private List<GiftPromotion> ConvertJsonToObjectPromotion(string json)
        {
            var listGift = new List<GiftPromotion>();
            var objGift = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(json.ToString());
            foreach (var itmGift in objGift)
            {
                var giftPromotion = new GiftPromotion
                {
                    GiftId = new Guid(itmGift["idGift"]),
                    Amount = itmGift["Amount"] == null ? 0 : int.Parse(itmGift["Amount"]),
                };
                listGift.Add(giftPromotion);
            }
            return listGift;
        }

        public bool CheckMaCTKM(string codeCTKM)
        {
            var result = false;
            try
            {
                SessionManager.DoWork(ss =>
                {
                    result = ss.Query<Promotion>().Any(p => p.Code.ToUpper() == codeCTKM.ToUpper());
                });
            }
            catch (Exception ex)
            {

            }
            return result;
        }

        public string Delete(ClaimsPrincipal principal, Guid id)
        {
            var result = string.Empty;
            SessionManager.DoWork(ss =>
            {
                try
                {
                    var obj = ss.Get<Promotion>(id);
                    if (obj != null && obj.CreatedBy == ContextProvider.GetUserInfo(principal).Id)
                    {
                        var logs = ss.Query<PromotionLog>().Where(w => w.Promotion.Id == id).ToList();
                        logs.ForEach(itm =>
                        {
                            ss.Delete(itm);
                        });
                        ss.Delete(obj);
                        result = "Đã xóa";
                    }
                    else
                        result = "Bạn không đủ quyền xóa chương trình khuyến mãi này!";
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    result = ex.Message;
                }
            });
            return result;
        }
        #endregion

        #region private
        private List<GiftPromotion> ConvertJsonToObject(string json)
        {
            var listGift = new List<GiftPromotion>();
            var objGift = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(json.ToString());
            foreach (var itmGift in objGift)
            {
                var giftPromotion = new GiftPromotion
                {
                    GiftId = new Guid(itmGift["idGift"]),
                    //NameGift = itmGift["maquatang"],
                    //CodeGift = itmGift["tenquatang"],
                    //UnitName = itmGift["donvitinh"],
                    //Price = itmGift["price"] == null ? 0 : decimal.Parse(itmGift["price"]),
                    Amount = itmGift["soluong"] == null ? 0 : int.Parse(itmGift["soluong"]),
                };
                listGift.Add(giftPromotion);
            }
            return listGift;
        }
        #endregion

        public List<dynamic> GetIsUser(ClaimsPrincipal principal)
        {
            var lstResults = new List<dynamic>();
            var user = ContextProvider.GetUserInfo(principal);
            var isTypeUser = ContextProvider.CheckPermission(user.PermisionId);
            SessionManager.DoWork(ss =>
            {
                try
                {
                    lstResults = ss.Query<Promotion>().Where(s => s.Status == 2)
                                    .Select(p => (dynamic)new
                                    {
                                        p.Id,
                                        p.GiftPromotionId,
                                        p.MaxGiftInDay,
                                        p.MaxGiftWithCustomer,
                                        p.ConfigPromotion,
                                        p.Description,
                                        p.IsChange,
                                        p.Code,
                                        p.Name,
                                        p.Status,
                                        p.NumberOdEdit,
                                        p.SoLanHPB,
                                        value = p.Code,
                                        label = p.Name
                                    }).ToList();
                    if (ContextProvider.CheckPermission(user.PermisionId) == 3)
                    {
                        var ids = ss.Query<TransferDetail>().Where(s => s.ReceivingDepartment == user.OrganizationId).Select(s => s.ReceivingPromotion).ToList();
                        lstResults = lstResults.Where(w => ids.Contains(w.Id)).ToList();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            });
            return lstResults;
        }

        /// <summary>
        /// get danh sách promotion cho combobox Điều chuyển nội bộ.
        /// </summary>
        /// <returns>list promotion</returns>
        public List<dynamic> GetDCNB(ClaimsPrincipal principal)
        {
            var lstResults = new List<dynamic>();
            var user = ContextProvider.GetUserInfo(principal);
            SessionManager.DoWork(ss =>
            {
                try
                {
                    var lst = ss.Query<Promotion>().Where(s => s.Status == 2).ToList();
                    if (ContextProvider.CheckPermission(user.PermisionId)!= 1 && ContextProvider.CheckPermission(user.PermisionId) != 2)
                    {
                        var ids = ss.Query<TransferDetail>().Where(s => s.ReceivingDepartment == user.OrganizationId &&s.TransferGift.Status==2).Select(s => s.ReceivingPromotion).ToList();
                        lst = lst.Where(w => ids.Contains(w.Id)).ToList();
                    }
                    lstResults = lst.Select(p => (dynamic)new
                    {
                        p.Id,
                        p.GiftPromotionId,
                        p.MaxGiftInDay,
                        p.MaxGiftWithCustomer,
                        p.ConfigPromotion,
                        p.Description,
                        p.IsChange,
                        p.Code,
                        p.Name,
                        p.Status,
                        p.NumberOdEdit,
                        p.SoLanHPB,
                        value = p.Code,
                        label = p.Name
                    }).ToList();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            });
            return lstResults;
        }
    }
}