using Api.ManagerGift.Entities;
using System;

namespace Api.ManagerGift.DTO
{
    public class StoreDTO
    {
        public Guid Id { get; set; }
        public Guid DepartmentId { get; set; }
        public string ManagerType { get; set; }
        public Guid? PromotionId { get; set; }
        public Guid GiftId { get; set; }
        public int Amount { get; set; }
        public int AmountAttribution { get; set; }
        public int AmountUse { get; set; }
        public int AmountInventory { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string LogTransfer { get; set; }
        public string DepartmentName { get; set; }
        public string DepartmentCode { get; set; }
        public string PromotionName { get; set; }
        public string PromotionCode { get; set; }
        public string GiftName { get; set; }
        public string GiftCode { get; set; }
        public string Note { get; set; }
        public string Price { get; set; }

    }
}
