using System;

namespace Api.ManagerGift.DTO
{
    public class GiftDTO
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public Guid GiftGroupId { get; set; }
        public Guid UnitId { get; set; }
        public decimal Price { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public Guid UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public bool Status { get; set; }
    }
}
