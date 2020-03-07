using System;

namespace Api.ManagerGift.Entities
{
    public class GiftPromotion
    {
        public virtual Guid Id { get; set; }
        public virtual Guid GiftPromotionId { get; set; }
        //public virtual string CodeGift { get; set; }
        //public virtual string NameGift { get; set; }
        //public virtual decimal Price { get; set; }
        //public virtual string UnitName { get; set; }
        //public virtual string GiftGroupName { get; set; }
        public virtual int Amount { get; set; }
        public virtual Guid GiftId { get; set; }

    }
}
