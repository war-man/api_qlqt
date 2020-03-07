using System;

namespace Api.ManagerGift.DTO
{
    public class GiftGroupDTO
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public Guid UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public bool Status { get; set; }
        public Guid OptionGiftId { get; set; }
    }
}
