using System;

namespace Api.ManagerGift.DTO
{
    public class TransferGiftDTO
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public bool IsFinished { get; set; }
        public bool IsComplete { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? PromotionId { get; set; }
        public int Status { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public Guid ? ApprovedBy { get; set; }
        public DateTime ? ApprovedDate { get; set; }
        public object Data { get; set; }
        public Guid? ReceivingDepartmentId { get; set; }
        public Guid? ReceivingPromotionId { get; set; }
        public Guid? StageId { get; set; }
        public string Comment { get; set; }
    }
}
