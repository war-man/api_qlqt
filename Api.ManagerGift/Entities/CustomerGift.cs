using System;

namespace Api.ManagerGift.Entities
{
    public class CustomerGift
    {
        public virtual Guid Id { get; set; }
        public virtual string Acctno { get; set; }
        public virtual string TENLOAIHINH { get; set; }
        public virtual string CusName { get; set; }
        public virtual string CusId { get; set; }
        public virtual decimal TERM { get; set; }
        public virtual string TERMCD { get; set; }
        public virtual decimal BALANCE { get; set; }
        public virtual DateTime FRDATE { get; set; }
        public virtual DateTime TODATE { get; set; }
        public virtual string CHEQUENO { get; set; }
        public virtual decimal INTRATE { get; set; }
        public virtual string RATECD { get; set; }
        public virtual string LICENSE { get; set; }
        public virtual string SUBBRID { get; set; }
        public virtual string SUBBRNAME { get; set; }
        public virtual string BRANCHID { get; set; }
        public virtual string BRNAME { get; set; }
        public virtual DateTime? CREATEDDATE { get; set; }
        public virtual Guid CREATEDBy { get; set; }
        public virtual Guid USERID { get; set; }
        public virtual string ACTYPE { get; set; }
        public virtual Gift Gift { get; set; }
        public virtual Promotion Promotion { get; set; }
        public virtual int Status { get; set; }
        public virtual string PhanHe { get; set; }
        public virtual DateTime? NgayDuyet { get; set; }
        public virtual Guid NguoiDuyet { get; set; }
        public virtual int NumGift { get; set; }
        public virtual string CCYCD { get; set; } 
    }
}
