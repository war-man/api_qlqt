using System;

namespace Api.ManagerGift.Entities
{
    public class TransferUserLog
    {
        public virtual Guid Id { get; set; }
        public virtual string  UserName { get; set; }
        public virtual string OldDepartmentCode { get; set; }
        public virtual string OldPositionCode { get; set; }
        public virtual string NewDepartmentCode { get; set; }
        public virtual string NewPositionCode { get; set; }
        public virtual string Note { get; set; }
        public virtual DateTime TransferDate { get; set; }
        public virtual Guid UserTransfer { get; set; }
        public virtual bool IsTransfer { get; set; }

    }
}
