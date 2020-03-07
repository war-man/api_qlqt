using System;

namespace Api.ManagerGift.Entities
{
    public class Store
    {
        public virtual Guid Id { get; set; }
        public virtual Guid DepartmentId { get; set; }
        public virtual string ManagerType { get; set; }
        public virtual Guid? PromotionId { get; set; }
        public virtual Guid GiftId { get; set; }
        public virtual int Amount { get; set; }
        public virtual DateTime UpdatedDate { get; set; }
        public virtual string LogTransfer { get; set; }
        //public virtual Guid TransferId { get; set; }

    }
}
