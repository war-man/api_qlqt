using System;

namespace Api.ManagerGift.Entities
{
    public class TransferDetail
    {
        public virtual Guid Id { get; set; }
        public virtual TransferGift TransferGift { get; set; }
        public virtual Guid? ReceivingPromotion { get; set; }
        public virtual Guid? ReceivingDepartment { get; set; }
        public virtual Guid? TransferDepartment { get; set; }
        public virtual Guid GiftId { get; set; }
        public virtual int Amount { get; set; }
        public virtual Guid FlagDieuChuyen { get; set; }

    }
}
