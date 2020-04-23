using System;

namespace Api.ManagerGift.DTO
{
    public class TranferDetailDTO
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int Status { get; set; }
        public bool IsFinished { get; set; }
        public bool IsComplete { get; set; }
        public Guid DepartmentId { get; set; }
        public DateTime Deadline { get; set; }
        public Guid ProductId { get; set; }
        public Guid CreatedBy { get; set; }
        public string CreatedDate { get; set; }
        //public string GiftCode { get; set; }
        //public string GiftName { get; set; }
        //public string Price { get; set; }
        //public string UnitName { get; set; }
        //public string Amount { get; set; }
        public Guid ReceiveDepartment { get; set; }
        public Guid TransferDepartment { get; set; }
        public Guid StageId { get; set; }
        public DetailGiftTranfer DetailGiftTranfer { get; set; }
    }

    public class DetailGiftTranfer
    {
        public string GiftCode { get; set; }
        public string GiftName { get; set; }
        public int Amount { get; set; }
        public string UnitName { get; set; }
        public string Price { get; set; }
    }
}
