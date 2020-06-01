using System;

namespace Api.ManagerGift.DTO
{
    public class PromotionsDTO
    {
        public string Code { get; set; }
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Status { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public Guid UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public int NumberOdEdit { get; set; }
        public string GiftPromotion { get; set; }
        public string ConfigPromotion { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime FinishDate { get; set; }
        public string Description { get; set; }
        public int MaxGiftWithCustomer { get; set; }
        public int MaxGiftInDay { get; set; }
        public bool IsChange { get; set; }
        public int SoLanHPB { get; set; }
        public string Comment { get; set; }
    }
}
