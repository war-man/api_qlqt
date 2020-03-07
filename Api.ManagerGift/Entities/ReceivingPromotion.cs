using System;

namespace Api.ManagerGift.Entities
{
    public class ReceivingPromotion
    {
        public virtual Guid Id { get; set; }
        public virtual Guid PromotionId { get; set; }
        public virtual TransferGift TransferGift { get; set; }
    }
}
