using Api.ManagerGift.Abstracts;
using System;

namespace Api.ManagerGift.Entities
{
    public class TransferGiftDraftDTO
    {
        public string Code { get; set; }
        public Guid ProductId { get; set; }
        public string Data { get; set; }
        public Guid ? SentDepartment { get; set; }
        public Guid ? ReceivingDepartment { get; set; }
        public Guid ? SentPromotion { get; set; }
        public Guid ? ReceivingPromotion { get; set; }
        public Guid ? CreatedBy { get; set; }
    }
}
