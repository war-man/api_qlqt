﻿using Api.ManagerGift.DTO;
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

namespace Api.ManagerGift.Services
{
    public class TangQuaKhachHangService
    {
        public dynamic GetAcc(string accNo)
        {
            dynamic result = new ExpandoObject();
            SessionManager.DoWork(ss =>
            {
                try
                {
                    var cus = ss.Query<CustomerGift>().SingleOrDefault(s => s.Acctno == accNo);
                    if (cus != null)
                    {
                        result.InfoCus = cus;

                        switch (cus.Status)
                        {
                            case 1:
                                result.Status = "Đang chờ duyệt tặng quà";
                                break;
                            case 2:
                                result.Status = "Đã tặng quà";
                                break;
                            case 3:
                                result.Status = "Từ chối duyệt";
                                break;
                            default:
                                break;
                        }
                    }
                    else
                    {
                        result = null;
                    }
                }
                catch (Exception)
                {

                    throw;
                }
            });
            return result;
        }

        public dynamic Check(string accNo, string json, string phanhe, ClaimsPrincipal principal)
        {
            dynamic result = new ExpandoObject();
            try
            {
                var lstPromotion = new List<PromotionOut>();
                var lstCusGift = new List<CustomerGift>();
                var cusGift = new CustomerGift();

                var userinfo = ContextProvider.GetUserInfo(principal);

                var infoCusFromCoreBanking = ConvertJson(json);
                //var infoCusFromCoreBanking = JsonConvert.DeserializeObject<CustomerDTO>(json);

                SessionManager.DoWork(ss =>
                {
                    var lstUser = ss.Query<User>().ToList();
                    var now = DateTime.Now;
                    var timeNow = DateTime.ParseExact(new DateTime(now.Year, now.Month, now.Day, 0, 0, 0).ToString("yyyy-MM-dd hh:mm:ss tt"), "yyyy-MM-dd hh:mm:ss tt", CultureInfo.InvariantCulture);
                    var idPromotions = ss.Query<Store>().Where(s => s.DepartmentId == userinfo.Organization.Id && s.Amount > 0).Select(s => s.PromotionId).ToList();
                    if (idPromotions.Count == 0)
                        result.MesError = "Hiện không triển khai chương trình khuyến mãi nào.";
                    var promotions = ss.Query<Promotion>().Where(s =>
                                                            s.StartDate <= timeNow
                                                        && s.FinishDate >= timeNow
                                                        && s.Status == 2).ToList();
                    if (promotions.Count == 0)
                    {
                        result.MesError = "Hiện không triển khai chương trình khuyến mãi nào.";
                    }
                    else
                    {

                        var status = false;
                        foreach (var itm in promotions)
                        {
                            var quaTangKH = new QuaTangKH();
                            var promotionIdUsed = new Guid();

                            cusGift = ss.Query<CustomerGift>().SingleOrDefault(s => s.Acctno == accNo && s.Promotion.Id == itm.Id && s.PhanHe == phanhe);
                            if (cusGift != null)
                            {
                                status = true;
                                lstCusGift.Add(cusGift);

                                var gifts = ss.Query<Gift>().Where(s => s.Id == cusGift.Gift.Id).ToList();
                                promotionIdUsed = cusGift.Promotion.Id;

                                quaTangKH.GiftId = cusGift.Gift.Id.ToString();
                                quaTangKH.GiftName = ContextProvider.GiftName(gifts, cusGift.Gift.Id);
                                quaTangKH.Num = cusGift.NumGift;

                                var lstQuaTangKH = new List<QuaTangKH>();
                                lstQuaTangKH.Add(quaTangKH);

                                var promotionOut = new PromotionOut
                                {
                                    Id = promotionIdUsed.ToString(),
                                    Code = ContextProvider.GetPromotionCode(promotions, promotionIdUsed),
                                    Name = ContextProvider.GetPromotionName(promotions, promotionIdUsed),
                                    label = ContextProvider.GetPromotionCode(promotions, promotionIdUsed),
                                    value = promotionIdUsed.ToString(),
                                    FlagTangQua = cusGift.Status,
                                    QuaTangKH = lstQuaTangKH
                                };
                                lstPromotion.Add(promotionOut);
                            }
                            if (idPromotions.Any(a => a == itm.Id))
                            {
                                var idGP = promotions.Select(s => s.GiftPromotionId).ToList();
                                var giftIds = ss.Query<GiftPromotion>().Where(s => idGP.Contains(s.GiftPromotionId)).Select(s => s.GiftId).ToList();
                                var gift = ss.Query<Gift>().Where(s => giftIds.Contains(s.Id)).ToList();
                                if (promotionIdUsed.ToString() != Constants.GUIDE_TYPE_NULL)
                                {
                                    if (itm.Id != promotionIdUsed)
                                    {
                                        var lstQuaTangKH = new List<QuaTangKH>();

                                        if (infoCusFromCoreBanking.FRDATE >= itm.StartDate && infoCusFromCoreBanking.FRDATE <= itm.FinishDate)
                                        {
                                            var configPromotion = JsonConvert.DeserializeObject<List<ConfigPromotion>>(itm.ConfigPromotion.ToString());
                                            foreach (var itmConfigPromotion in configPromotion)
                                            {
                                                if (string.IsNullOrEmpty(phanhe))
                                                    phanhe = "DEFAULT";
                                                if (infoCusFromCoreBanking.TERM >= decimal.Parse(itmConfigPromotion.kyhantoithieu)
                                                    && infoCusFromCoreBanking.BALANCE >= decimal.Parse(itmConfigPromotion.sodutoithieu)
                                                    && itmConfigPromotion.phanhe.ToUpper() == phanhe.ToUpper())
                                                {
                                                    infoCusFromCoreBanking.PhanHe = phanhe.ToUpper();

                                                    var dataGift = itmConfigPromotion.dataKhaiBaoQuaTang;
                                                    if (dataGift == null)
                                                        result.MesError = "Chưa thiết lập nguyên tắc tặng quà.";
                                                    else
                                                        lstQuaTangKH = CreateLstGift(dataGift, gift, infoCusFromCoreBanking);
                                                }
                                            }
                                        }
                                        if (lstQuaTangKH.Count > 0)
                                            lstPromotion.Add(CreateCardPromotion(itm, lstQuaTangKH));
                                    }
                                }

                                else
                                {
                                    var lstQuaTangKH = new List<QuaTangKH>();

                                    if (infoCusFromCoreBanking.FRDATE >= itm.StartDate && infoCusFromCoreBanking.FRDATE <= itm.FinishDate)
                                    {
                                        var configPromotion = JsonConvert.DeserializeObject<List<ConfigPromotion>>(itm.ConfigPromotion.ToString());
                                        foreach (var itmConfigPromotion in configPromotion)
                                        {
                                            if (string.IsNullOrEmpty(phanhe))
                                                phanhe = "DEFAULT";
                                            if (infoCusFromCoreBanking.TERM >= decimal.Parse(itmConfigPromotion.kyhantoithieu)
                                            && infoCusFromCoreBanking.BALANCE >= decimal.Parse(itmConfigPromotion.sodutoithieu)
                                            && itmConfigPromotion.phanhe.ToUpper() == phanhe.ToUpper())
                                            {
                                                infoCusFromCoreBanking.PhanHe = phanhe.ToUpper();

                                                var dataGift = itmConfigPromotion.dataKhaiBaoQuaTang;
                                                if (dataGift == null)
                                                    result.MesError = "Chưa thiết lập nguyên tắc tặng quà.";

                                                else
                                                    lstQuaTangKH = CreateLstGift(dataGift, gift, infoCusFromCoreBanking);
                                            }
                                        }
                                    }
                                    if (lstQuaTangKH.Count > 0)
                                        lstPromotion.Add(CreateCardPromotion(itm, lstQuaTangKH));
                                }
                            }
                        }

                        if (lstCusGift.Count == 0) lstCusGift.Add(infoCusFromCoreBanking);

                        result.InfoCus = LstCustomer(lstCusGift, phanhe, lstUser);

                        result.LstPromotion = lstPromotion.ToList().OrderByDescending(o => o.CountPrice).ToList();
                        result.Status = status;
                    }
                });
            }
            catch (Exception ex)
            {
                throw;
            }
            return result;
        }

        public string TangQuaKhachHang(CustomerDTO obj, string giftId, string promotionId, int soluong, ClaimsPrincipal principal)
        {
            var result = string.Empty;
            var _promotionId = new Guid(promotionId);
            var _giftId = new Guid(giftId);
            try
            {
                var userinfo = ContextProvider.GetUserInfo(principal);
                SessionManager.DoWork(ss =>
                {
                    var newCus = new CustomerGift();
                    var newCustomerGiftLog = new CustomerGiftLog();

                    var gift = ss.Query<Gift>().SingleOrDefault(s => s.Id == _giftId);
                    var promotion = ss.Query<Promotion>().SingleOrDefault(s => s.Id == _promotionId);

                    if (CheckMaxGiftInDay(_promotionId, promotion.MaxGiftInDay))
                    {
                        if (CheckMaxGiftWithCustomer(promotion.MaxGiftWithCustomer, _promotionId, obj.Acctno, soluong))
                        {
                            var store = ss.Query<Store>().FirstOrDefault(s => s.DepartmentId == userinfo.OrganizationId && s.PromotionId == _promotionId && s.GiftId == _giftId);
                            if (store != null && store.Amount >= soluong)
                            {
                                var parentId = Guid.NewGuid();
                                newCus.Id = parentId;
                                newCus.ParentId = parentId;
                                newCus.USERID = userinfo.Id;
                                newCus.Gift = gift;
                                newCus.Status = (int)ContextProvider.statusTransfer.Initialize;
                                newCus.Promotion = promotion;
                                newCus.CREATEDBy = userinfo.Id;
                                newCus.CREATEDDATE = DateTime.ParseExact(DateTime.Now.ToString("u"), "u", CultureInfo.InvariantCulture);
                                newCus.NumGift = soluong;

                                newCus.PhanHe = obj.PhanHe;
                                newCus.Acctno = obj.Acctno;
                                newCus.TENLOAIHINH = obj.TENLOAIHINH;
                                newCus.CusName = obj.CusName;
                                newCus.CusId = obj.CusId;
                                newCus.TERM = obj.TERM;
                                newCus.TERMCD = obj.TERMCD;
                                newCus.BALANCE = obj.BALANCE;
                                newCus.FRDATE = obj.FRDATE;
                                newCus.TODATE = obj.TODATE;
                                newCus.CHEQUENO = obj.CHEQUENO;
                                newCus.INTRATE = obj.INTRATE;
                                newCus.RATECD = obj.RATECD;
                                newCus.LICENSE = obj.LICENSE;
                                newCus.SUBBRID = obj.SUBBRID;
                                newCus.SUBBRNAME = obj.SUBBRNAME;
                                newCus.BRANCHID = obj.BRANCHID;
                                newCus.BRNAME = obj.BRNAME;
                                newCus.ACTYPE = obj.ACTYPE;
                                newCus.CCYCD = obj.CCYCD;

                                var _productId = new Guid(Constants.ID_PRODUCT_TANG_QUA_KHACH_HANG);
                                var product = ss.Get<Product>(_productId);
                                var stage = ss.Query<Stage>().SingleOrDefault(p => p.ProductId == product.Id && p.PositionId == userinfo.Position.Id && p.Name == Constants.GUI_DUYET);
                                if (stage != null)
                                {
                                    ss.Save(newCus);
                                    ss.Save(new CustomerGiftLog
                                    {
                                        Id = new Guid(),
                                        //CustomerGift = newCus,
                                        CustomerGiftId = parentId,
                                        AssignUserId = userinfo.Id,
                                        AssignDeaprtmentId = userinfo.Organization.Id,
                                        Comment = "",
                                        Status = (int)ContextProvider.statusTransfer.Initialize,
                                        UpdateDate = DateTime.ParseExact(DateTime.Now.ToString("u"), "u", CultureInfo.InvariantCulture),
                                        StageId = stage.Id
                                    });

                                    result = Constants.GUI_DUYET_THANH_CONG;
                                }
                                else
                                    result = "Chức năng này chỉ dành cho GDV";
                            }
                            else
                                result = "Số lượng quà trong kho không đủ, xin hãy kiểm tra lại";
                        }
                        else
                            result = "Mỗi khách hàng không được nhận quá số lượng: " + promotion.MaxGiftWithCustomer;
                    }
                    else
                        result = "Mỗi ngày không được tặng quá " + promotion.MaxGiftInDay + " quà tặng";
                });

                return result;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public dynamic LstKhachHangNhanQua(string phanhe, string acctno, string promotionId, int pageNo, int pageSize, ClaimsPrincipal principal)
        {
            dynamic result = new ExpandoObject();
            try
            {
                SessionManager.DoWork(s =>
                {
                    var userinfo = ContextProvider.GetUserInfo(principal);
                    var typeUser = ContextProvider.CheckPermission(userinfo.PermisionId);
                    var lstPromotion = s.Query<Promotion>().ToList();
                    var lstUser = s.Query<User>().ToList();
                    var idUsers = lstUser.Where(w => w.Organization.Id == userinfo.Organization.Id).Select(u=>u.Id).ToList();
                    var list = s.Query<CustomerGift>().Where(w => (idUsers.Contains(w.USERID) && typeUser != 1 && typeUser != 2) || typeUser == 1 || typeUser == 2).ToList();
                    if (!string.IsNullOrEmpty(phanhe))
                        list = list.Where(p => p.PhanHe == phanhe).ToList();

                    if (!string.IsNullOrEmpty(acctno))
                        list = list.Where(p => p.Acctno == acctno).ToList();

                    if (!string.IsNullOrEmpty(promotionId))
                        list = list.Where(p => p.Promotion.Id == new Guid(promotionId)).ToList();
                    var cus = list.GroupBy(p => new
                    {
                        PromotionId = p.Promotion.Id,
                        p.ParentId,
                        p.CusId,
                        p.PhanHe,
                        p.Acctno,
                        p.CusName,
                        p.TERM,
                        p.TERMCD,
                        p.BALANCE,
                        p.CCYCD,
                        p.FRDATE,
                        p.NgayDuyet,
                        p.NguoiDuyet,
                        p.Status,
                        p.CREATEDBy,
                        p.CREATEDDATE
                    }).Select(p => new
                    {
                        Id = p.Key.ParentId,
                        p.Key.CusId,
                        p.Key.PhanHe,
                        p.Key.Acctno,
                        p.Key.PromotionId,
                        PromotionName = ContextProvider.GetPromotionName(lstPromotion, p.Key.PromotionId),
                        NumGift = p.Sum(t => t.NumGift),
                        p.Key.CusName,
                        p.Key.TERM,
                        p.Key.TERMCD,
                        p.Key.BALANCE,
                        p.Key.CCYCD,
                        FRDATE = ContextProvider.GetConvertDatetime(p.Key.FRDATE),
                        NgayDuyet = ContextProvider.GetConvertDatetime(p.Key.NgayDuyet),
                        NguoiDuyet = ContextProvider.GetFullName(lstUser, p.Key.NguoiDuyet),
                        p.Key.Status,
                        CREATEDBy = ContextProvider.GetFullName(lstUser, p.Key.CREATEDBy),
                        CREATEDDATE = ContextProvider.GetConvertDatetime(p.Key.CREATEDDATE),
                    }).ToList();

                    result.Cus = cus.Skip((pageNo - 1) * pageSize).Take(pageSize).ToList();
                    var total = cus.Count();
                    result.TotalPage = total % pageSize == 0 ? total / pageSize : total / pageSize + 1;
                });
            }
            catch (Exception)
            {
                throw;
            }
            return result;
        }

        public string Approve(string id, string param, string idGift,
            string idPromotion, int numGift, ClaimsPrincipal principal)
        {
            var result = string.Empty;
            try
            {
                var userinfo = ContextProvider.GetUserInfo(principal);
                var isTypeUser = ContextProvider.CheckPermission(userinfo.PermisionId);
                var idCustomerGift = new Guid(id);
                var _productId = new Guid(Constants.ID_PRODUCT_TANG_QUA_KHACH_HANG);

                SessionManager.DoWork(s =>
                {
                    var customerGift = s.Query<CustomerGift>().SingleOrDefault(p => p.Id == idCustomerGift);
                    if (customerGift != null)
                    {
                        var product = s.Get<Product>(_productId);
                        var stage = s.Query<Stage>().SingleOrDefault(p => p.ProductId == product.Id && p.PositionId == userinfo.Position.Id && p.Name == Constants.DUYET);
                        if (stage != null)
                        {
                            if (param == Constants.APPROVE)
                            {
                                // check số lượng quà tặng trong kho.
                                var store = s.Query<Store>().SingleOrDefault(p => p.DepartmentId == userinfo.Organization.Id
                                    && p.PromotionId == new Guid(idPromotion) && p.GiftId == new Guid(idGift));
                                if (store.Amount >= numGift)
                                {
                                    int status = (int)ContextProvider.statusTransfer.Approve;
                                    UpdateCustomerGift(customerGift, userinfo, status);
                                    SaveCustomerGiftLog(s, stage, customerGift, userinfo);

                                    // update Store
                                    store.Amount -= numGift;
                                    result = Constants.DUYET_THANH_CONG;
                                }
                                else
                                    result = "Quà tặng trong kho không đủ";

                            }
                            if (param == Constants.REFUSE)
                            {
                                int status = (int)ContextProvider.statusTransfer.Refuse;
                                UpdateCustomerGift(customerGift, userinfo, status);
                                SaveCustomerGiftLog(s, stage, customerGift, userinfo);
                                result = Constants.TU_CHOI_DUYET_THANH_CONG;
                            }
                        }
                        else
                            result = "Chức năng này dành cho KSV";
                    }
                });
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }

        public dynamic DetailTangQua(string id)
        {
            dynamic result = new ExpandoObject();
            try
            {
                SessionManager.DoWork(ss =>
                {
                    var idCusGift = new Guid(id);
                    var lstUser = ss.Query<User>().ToList();
                    result = ss.Query<CustomerGift>().Where(s => s.ParentId == idCusGift).ToList().GroupBy(p => new
                    {
                        PromotionId = p.Promotion.Id,
                        p.ParentId,
                        p.CusId,
                        p.PhanHe,
                        p.Acctno,
                        p.TENLOAIHINH,
                        p.CusName,
                        p.TERM,
                        p.TERMCD,
                        p.BALANCE,
                        p.CCYCD,
                        p.FRDATE,
                        p.TODATE,
                        p.NgayDuyet,
                        p.NguoiDuyet,
                        p.Status,
                        p.CREATEDBy,
                        p.CREATEDDATE
                    }).Select(p => new
                    {
                        Id = p.Key.ParentId,
                        p.Key.CusId,
                        p.Key.CusName,
                        p.Key.Acctno,
                        p.Key.TERM,
                        p.Key.TERMCD,
                        p.Key.TENLOAIHINH,
                        p.Key.BALANCE,
                        p.Key.PhanHe,
                        p.Key.CCYCD,
                        p.Key.FRDATE,
                        p.Key.TODATE,
                        PromotionId = p.Key.PromotionId,
                        PromotionName = ss.Get<Promotion>(p.Key.PromotionId)?.Name,
                        PromotionCode = ss.Get<Promotion>(p.Key.PromotionId)?.Code,
                        GiftKH = p.ToList().Select(s => new
                        {
                            GiftCode = s.Gift.Code,
                            GiftName = s.Gift.Name,
                            GiftId = s.Gift.Id,
                            UnitName = s.Gift.Unit.Name,
                            Price = s.Gift.Price,
                            TotalPrice = (s.Gift.Price * s.NumGift),
                            NumGift = s.NumGift,
                        }).ToList(),
                        CreatedDate = p.Key.CREATEDDATE != null ? p.Key.CREATEDDATE.Value.ToString("yyyy-MM-dd hh:mm") : "",
                        CreatedBy = ContextProvider.GetFullName(lstUser, p.Key.CREATEDBy),
                        DonViTang = ContextProvider.GetDonViTang(lstUser, p.Key.CREATEDBy),
                        NgayDuyet = p.Key.NgayDuyet != null ? p.Key.NgayDuyet.Value.ToString("yyyy-MM-dd hh:mm") : "",
                        NguoiDuyet = ContextProvider.GetFullName(lstUser, p.Key.NguoiDuyet),
                        p.Key.Status
                    }).FirstOrDefault();
                });
            }
            catch (Exception ex)
            {
                result = ex;
            }
            return result;
        }

        private List<CustomerDTO> LstCustomer(List<CustomerGift> lst, string phanhe, List<User> lstUser)
        {
            return lst.Select(p => new CustomerDTO
            {
                Id = p.Id,
                Status = p.Status,
                CusId = p.CusId,
                Acctno = p.Acctno,
                CusName = p.CusName,
                TERM = p.TERM,
                TERMCD = p.TERMCD,
                INTRATE = p.INTRATE,
                RATECD = p.RATECD,
                BALANCE = p.BALANCE,
                CCYCD = p.CCYCD,
                FRDATE = p.FRDATE,
                TODATE = p.TODATE,
                CREATEDDATE = p.CREATEDDATE,
                CREATEDBy = ContextProvider.GetFullName(lstUser, p.CREATEDBy),
                TENLOAIHINH = p.TENLOAIHINH,
                ACTYPE = p.ACTYPE,
                CHEQUENO = p.CHEQUENO,
                LICENSE = p.LICENSE,
                SUBBRID = p.SUBBRID,
                SUBBRNAME = p.SUBBRNAME,
                BRANCHID = p.BRANCHID,
                BRNAME = p.BRNAME,
                PhanHe = phanhe.ToUpper()
            }).ToList();
        }
        private bool CheckMaxGiftInDay(Guid promotionId, int maxGiftInDay)
        {
            var flag = false;
            var curr = DateTime.Today;
            var curr_1 = DateTime.Today.AddDays(1);
            try
            {
                SessionManager.DoWork(ss =>
                {
                    var query = ss.Query<CustomerGift>().Where(s => s.Promotion.Id == promotionId && s.CREATEDDATE >= curr && s.CREATEDDATE <= curr_1).Select(p => new { p.NumGift }).ToList();
                    if (query.Count > 0)
                    {
                        var totalNumGift = 0;
                        foreach (var itm in query)
                        {
                            totalNumGift += itm.NumGift;
                        }
                        if (totalNumGift <= maxGiftInDay)
                            flag = true;
                        else
                            flag = false;
                    }
                    else flag = true;
                });
            }
            catch (Exception)
            {
                return false;
            }
            return flag;
        }

        private bool CheckMaxGiftWithCustomer(int maxGiftWithCustomer, Guid promotionId, string acctno, int numGift)
        {
            var flag = false;
            try
            {
                SessionManager.DoWork(ss =>
                {
                    var query = ss.Query<CustomerGift>().Where(s => s.Promotion.Id == promotionId && s.Acctno == acctno).ToList();
                    var countGift = 0;
                    if (query.Count > 0)
                    {
                        foreach (var itm in query)
                        {
                            countGift += itm.NumGift;
                        }
                    }
                    if (countGift + numGift <= maxGiftWithCustomer)
                        flag = true;

                    else
                        flag = false;
                });
                return flag;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void UpdateCustomerGift(CustomerGift c, UserDTO u, int status)
        {
            c.Status = status;
            c.NguoiDuyet = u.Id;
            c.NgayDuyet = DateTime.ParseExact(DateTime.Now.ToString("u"), "u", CultureInfo.InvariantCulture);
        }
        private void SaveCustomerGiftLog(ISession s, Stage st, CustomerGift c, UserDTO u)
        {
            s.Save(new CustomerGiftLog
            {
                Id = new Guid(),
                //CustomerGift = c,
                CustomerGiftId = c.ParentId,
                AssignUserId = u.Id,
                AssignDeaprtmentId = u.Organization.Id,
                Status = c.Status,
                UpdateDate = DateTime.ParseExact(DateTime.Now.ToString("u"), "u", CultureInfo.InvariantCulture),
                StageId = st.Id,
                Comment = "",
            });
        }

        private PromotionOut CreateCardPromotion(Promotion itm, List<QuaTangKH> lstQuaTangKH)
        {
            var i = new PromotionOut
            {
                Id = itm.Id.ToString(),
                Name = itm.Name,
                IsChange = itm.IsChange,
                FlagTangQua = 0,
                QuaTangKH = lstQuaTangKH,
                CountPrice = lstQuaTangKH.Sum(s => s.Price),
                Code = itm.Code,
                label = itm.Code,
                value = itm.Id.ToString(),
            };
            return i;
        }
        private List<QuaTangKH> CreateLstGift(List<DataKhaiBaoQuaTang> dataGift, List<Gift> gifts, CustomerGift infoCusFromCoreBanking)
        {
            var lstQuaTangKH = new List<QuaTangKH>();
            foreach (var itmGift in dataGift)
            {
                var minsotien = decimal.Parse(itmGift.minsotien);
                var minkyhan = decimal.Parse(itmGift.minkyhan);
                var maxsotien = decimal.Parse(itmGift.maxsotien);
                var maxkyhan = decimal.Parse(itmGift.maxkyhan);

                if (infoCusFromCoreBanking.BALANCE >= minsotien && infoCusFromCoreBanking.TERM >= minkyhan)
                {
                    var newGift = NewGift(itmGift);
                    var gift = gifts.FirstOrDefault(f => f.Id == Guid.Parse(newGift.GiftId));
                    newGift.UnitName = gift.Unit.Name;
                    newGift.GiftCode = gift.Code;
                    newGift.Price = gift.Price;
                    newGift.TotalPrice = gift.Price * newGift.Num;
                    if (maxsotien == 0 && maxkyhan == 0)
                        lstQuaTangKH.Add(newGift);

                    else if (maxsotien != 0 && maxkyhan == 0)
                    {
                        if (infoCusFromCoreBanking.BALANCE <= maxsotien)
                            lstQuaTangKH.Add(newGift);
                    }
                    else if (maxsotien == 0 && maxkyhan != 0)
                    {
                        if (infoCusFromCoreBanking.TERM <= maxkyhan)
                            lstQuaTangKH.Add(newGift);
                    }
                    else
                    {
                        if (infoCusFromCoreBanking.BALANCE <= maxsotien && infoCusFromCoreBanking.TERM <= maxkyhan)
                            lstQuaTangKH.Add(newGift);
                    }
                }
            }
            return lstQuaTangKH;
        }

        private CustomerGift ConvertJson(string json)
        {
            var data = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(json.ToString());
            var d = data[0];
            return new CustomerGift
            {
                Acctno = d["ACCTNO"],
                TENLOAIHINH = d["TENLOAIHINH"],
                ACTYPE = d["ACTYPE"],
                CusName = d["TENKH"],
                CusId = d["CUSTID"],
                TERM = decimal.Parse(d["TERM"]),
                TERMCD = d["TERMCD"],
                BALANCE = decimal.Parse(d["BALANCE"]),
                FRDATE = Convert.ToDateTime(d["FRDATE"]),
                TODATE = Convert.ToDateTime(d["TODATE"]),
                CHEQUENO = d["CHEQUENO"],
                INTRATE = decimal.Parse(d["INTRATE"]),
                RATECD = d.ContainsKey("RATECD") ? d["RATECD"] : "",
                LICENSE = d["LICENSE"],
                SUBBRID = d["SUBBRID"],
                SUBBRNAME = d["SUBBRNAME"],
                BRANCHID = d["BRANCHID"],
                BRNAME = d["BRNAME"],
                CCYCD = d["CCYCD"]
            };
        }

        private QuaTangKH NewGift(DataKhaiBaoQuaTang d)
        {
            return new QuaTangKH
            {
                GiftId = d.giftId,
                GiftName = d.giftName,
                Num = int.Parse(d.soluong)
            };
        }

        public string Temp(string accno, string phanhe)
        {
            var result = string.Empty;
            SessionManager.DoWork(ss =>
            {
                result = ss.Query<Draft>().SingleOrDefault(s => s.Id == accno && s.PhanHe == phanhe).Data;
            });
            return result;
        }

        public string TangQuaKhachHangQuyDoi(CustomerDTO obj, string promotionId, ClaimsPrincipal principal)
        {
            var result = string.Empty;
            var _promotionId = new Guid(promotionId);
            try
            {
                var userinfo = ContextProvider.GetUserInfo(principal);
                SessionManager.DoWork(ss =>
                {
                    var lst = obj.QuaTangKH.ToList();
                    var idGifts = lst.Select(s => Guid.Parse(s.GiftId)).ToList();
                    var gifts = ss.Query<Gift>().Where(s => idGifts.Contains(s.Id)).ToList();
                    var promotion = ss.Query<Promotion>().SingleOrDefault(s => s.Id == _promotionId);

                    if (CheckMaxGiftInDay(_promotionId, promotion.MaxGiftInDay))
                    {
                        var parentId = Guid.NewGuid();
                        lst.ForEach(itm =>
                        {
                            if (CheckMaxGiftWithCustomer(promotion.MaxGiftWithCustomer, _promotionId, obj.Acctno, itm.NumEdit))
                            {
                                var store = ss.Query<Store>().FirstOrDefault(s => s.DepartmentId == userinfo.OrganizationId && s.PromotionId == _promotionId && s.GiftId == Guid.Parse(itm.GiftId));
                                if (store != null && store.Amount >= itm.NumEdit)
                                {
                                    var _productId = new Guid(Constants.ID_PRODUCT_TANG_QUA_KHACH_HANG);
                                    var product = ss.Get<Product>(_productId);
                                    var stage = ss.Query<Stage>().SingleOrDefault(p => p.ProductId == product.Id && p.PositionId == userinfo.Position.Id && p.Name == Constants.GUI_DUYET);
                                    if (stage != null)
                                    {
                                        itm.Error = Constants.GUI_DUYET_THANH_CONG;
                                    }
                                    else
                                    {
                                        itm.Error = "Chức năng này chỉ dành cho GDV";
                                    }
                                }
                                else
                                    itm.Error = "Số lượng quà trong kho không đủ, xin hãy kiểm tra lại";
                            }
                            else
                                itm.Error = "Mỗi khách hàng không được nhận quá số lượng: " + promotion.MaxGiftWithCustomer;

                        });
                        if (lst.Any(a => a.Error != Constants.GUI_DUYET_THANH_CONG))
                        {
                            result = lst.FirstOrDefault(f => f.Error != Constants.GUI_DUYET_THANH_CONG).Error;
                        }
                        else
                        {
                            lst.ForEach(itm =>
                            {
                                var newCus = new CustomerGift();
                                newCus.Id = new Guid();
                                newCus.USERID = userinfo.Id;
                                newCus.Gift = gifts.FirstOrDefault(f => f.Id == Guid.Parse(itm.GiftId));
                                newCus.Status = (int)ContextProvider.statusTransfer.Initialize;
                                newCus.Promotion = promotion;
                                newCus.CREATEDBy = userinfo.Id;
                                newCus.CREATEDDATE = DateTime.ParseExact(DateTime.Now.ToString("u"), "u", CultureInfo.InvariantCulture);
                                newCus.NumGift = itm.NumEdit;
                                newCus.PhanHe = obj.PhanHe;
                                newCus.Acctno = obj.Acctno;
                                newCus.TENLOAIHINH = obj.TENLOAIHINH;
                                newCus.CusName = obj.CusName;
                                newCus.CusId = obj.CusId;
                                newCus.TERM = obj.TERM;
                                newCus.TERMCD = obj.TERMCD;
                                newCus.BALANCE = obj.BALANCE;
                                newCus.FRDATE = obj.FRDATE;
                                newCus.TODATE = obj.TODATE;
                                newCus.CHEQUENO = obj.CHEQUENO;
                                newCus.INTRATE = obj.INTRATE;
                                newCus.RATECD = obj.RATECD;
                                newCus.LICENSE = obj.LICENSE;
                                newCus.SUBBRID = obj.SUBBRID;
                                newCus.SUBBRNAME = obj.SUBBRNAME;
                                newCus.BRANCHID = obj.BRANCHID;
                                newCus.BRNAME = obj.BRNAME;
                                newCus.ACTYPE = obj.ACTYPE;
                                newCus.CCYCD = obj.CCYCD;
                                newCus.ParentId = parentId;
                                ss.Save(newCus);
                            });
                            var _productId = new Guid(Constants.ID_PRODUCT_TANG_QUA_KHACH_HANG);
                            var product = ss.Get<Product>(_productId);
                            var stage = ss.Query<Stage>().SingleOrDefault(p => p.ProductId == product.Id && p.PositionId == userinfo.Position.Id && p.Name == Constants.GUI_DUYET);
                            if (stage != null)
                            {
                                ss.Save(new CustomerGiftLog
                                {
                                    Id = new Guid(),
                                    //CustomerGift = newCus,
                                    CustomerGiftId = parentId,
                                    AssignUserId = userinfo.Id,
                                    AssignDeaprtmentId = userinfo.Organization.Id,
                                    Comment = "",
                                    Status = (int)ContextProvider.statusTransfer.Initialize,
                                    UpdateDate = DateTime.ParseExact(DateTime.Now.ToString("u"), "u", CultureInfo.InvariantCulture),
                                    StageId = stage.Id
                                });
                            }
                            result = Constants.GUI_DUYET_THANH_CONG;
                        }
                    }
                    else
                        result = "Mỗi ngày không được tặng quá " + promotion.MaxGiftInDay + " quà tặng";
                });

                return result;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }

    public class QuaTangKH
    {
        public bool IsChecked { get; set; }
        public string GiftId { get; set; }
        public string GiftCode { get; set; }
        public string GiftName { get; set; }
        public string Error { get; set; }
        public int Num { get; set; }
        public int NumEdit { get; set; }
        public string UnitName { get; set; }
        public decimal Price { get; set; }
        public decimal TotalPriceEdit { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public class PromotionOut
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string value { get; set; }
        public string label { get; set; }
        public bool IsChange { get; set; }
        public decimal CountPrice { get; set; }
        public int FlagTangQua { get; set; } // 0: chưa tặng, 1: đã chuyển cho lãnh đạo duyệt tặng quà.
        public List<QuaTangKH> QuaTangKH { get; set; }
    }

    public class ConfigPromotion
    {
        public List<DataKhaiBaoQuaTang> dataKhaiBaoQuaTang { get; set; }
        public string idCard { get; set; }
        public string loaitien { get; set; }
        public string phanhe { get; set; }
        public string kyhantoithieu { get; set; }
        public string sodutoithieu { get; set; }
        public string tichsotoithieu { get; set; }
    }

    public class DataKhaiBaoQuaTang
    {
        public string id { get; set; }
        public string minkyhan { get; set; }
        public string maxkyhan { get; set; }
        public string minsotien { get; set; }
        public string maxsotien { get; set; }
        public string soluong { get; set; }
        public string giftId { get; set; }
        public string giftName { get; set; }
    }
}
