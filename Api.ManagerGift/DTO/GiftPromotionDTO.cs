using System;

namespace Api.ManagerGift.DTO
{
    public class GiftPromotionDTO
    {
        public Guid Id { get; set; }
        public Guid GiftPromotionId { get; set; }
        public Guid CodeGift { get; set; }
        public string NameGift { get; set; }
        public virtual decimal Price { get; set; }
        public Guid GiftGroupId { get; set; }
        public Guid UnitId { get; set; }
        public string UnitName { get; set; }
        public string GiftGroupName { get; set; }
        public int Amount { get; set; }
    }
}
