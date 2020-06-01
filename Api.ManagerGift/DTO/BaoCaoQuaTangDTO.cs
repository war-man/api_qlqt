using System;

namespace Api.ManagerGift.DTO
{
    public class BaoCaoQuaTangDTO
    {
        public int Amount { get; set; }
        public string ReceivingDepartment { get; set; }
        public string ReceivingPromotion { get; set; }
        public string CreatedDate { get; set; }
        public string TranferDepartment { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string UnitName { get; set; }
        public string Price { get; set; }
        public string GiftGroupId { get; set; }
        public string GroupName { get; set; }
        public string OptionGiftId { get; set; }
        public string OptionGiftName { get; set; }
        public DateTime OrderByDate { get; set; }

    }
    public class BC06_DTO
    {
        public string BranchName { get; set; }
        public string DepartmentName { get; set; }
        public string SoTK { get; set; }
        public string CustomerName { get; set; }
        public string CIF { get; set; }
        public decimal KyHan { get; set; }
        public decimal SoDu { get; set; }
        public string NgayGui { get; set; }
        public string LoaiQua { get; set; }
        public string GiftName { get; set; }
        public string PromotionName { get; set; }
        public string PhanHe { get; set; }
        public string LoaiTien { get; set; }
        public string TenLoaiHinh { get; set; }
        public decimal GiaTriQuaTang { get; set; }
        public decimal GiaTri { get; set; }
        public string GhiChu { get; set; }
        public string NgayXuatQT { get; set; }
        public string GiftCode { get; set; }
        public string SoLuong { get; set; }
        public string SoLuongNhapKho { get; set; }
        public string SoLuongSuDung { get; set; }
        public string SoLuongCuoiKy { get; set; }
        public string ChenhLech { get; set; }
        public decimal ThanhTien { get; set; }
        public DateTime OrderByDate { get; set; }
        

    }
}
