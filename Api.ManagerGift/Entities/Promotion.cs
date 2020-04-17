using Api.ManagerGift.Abstracts;
using System;

namespace Api.ManagerGift.Entities
{
    public class Promotion : GeneralGiftAbstract
    {
        public virtual int Status { get; set; }
        public virtual int NumberOdEdit { get; set; }
        public virtual Guid GiftPromotionId { get; set; }
        public virtual string ConfigPromotion { get; set; }
        public virtual DateTime StartDate { get; set; }
        public virtual DateTime FinishDate { get; set; }
        public virtual string Description { get; set; }
        public virtual int MaxGiftWithCustomer { get; set; }
        public virtual int MaxGiftInDay { get; set; }
        public virtual bool IsChange { get; set; }
        public virtual Guid NguoiDuyet { get; set; }
        public virtual DateTime? NgayDuyet { get; set; }
        public virtual int SoLanHPB { get; set; }
    }
}
