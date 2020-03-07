using Api.ManagerGift.Abstracts;
using System;

namespace Api.ManagerGift.Entities
{
    public class TransferGift : GeneralGiftAbstract
    {
        public virtual Product Product { get; set; }
        public virtual bool IsFinished { get; set; }
        public virtual bool IsComplete { get; set; }
        public virtual Guid? DepartmentId { get; set; }
        public virtual Guid? PromotionId { get; set; }
        public virtual int Status { get; set; }
        public virtual DateTime? Deadline { get; set; }
        public virtual Guid StageCurrent { get; set; }
        public virtual Guid FlagDieuChuyen { get; set; }
        public virtual Guid? NguoiDuyet { get; set; }
        public virtual DateTime? NgayDuyet { get; set; }
    }
}
