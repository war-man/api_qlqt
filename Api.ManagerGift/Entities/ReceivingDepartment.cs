using System;

namespace Api.ManagerGift.Entities
{
    public class ReceivingDepartment
    {
        public virtual Guid Id { get; set; }
        public virtual Guid DepartmentId { get; set; }
        public virtual TransferGift TransferGift { get; set; }
    }
}
