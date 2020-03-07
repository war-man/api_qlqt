using System;

namespace Api.ManagerGift.Abstracts
{
    public abstract class GeneralGiftAbstract
    {
        public virtual Guid Id { get; set; }
        public virtual string Code { get; set; }
        public virtual string Name { get; set; }
        public virtual Guid? CreatedBy { get; set; }
        public virtual DateTime? CreatedDate { get; set; }
        public virtual Guid? UpdatedBy { get; set; }
        public virtual DateTime? UpdatedDate { get; set; }
        public virtual bool Status { get; set; }
    }
}
