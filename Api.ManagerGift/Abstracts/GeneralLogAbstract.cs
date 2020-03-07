using System;

namespace Api.ManagerGift.Abstracts
{
    public abstract class GeneralLogAbstract
    {
        public virtual Guid Id { get; set; }
        public virtual Guid? AssignUserId { get; set; }
        public virtual Guid AssignDeaprtmentId { get; set; }
        public virtual string Comment { get; set; }
        public virtual int? Status { get; set; }
        public virtual DateTime? UpdateDate { get; set; }
        public virtual Guid? StageId { get; set; }

    }
}
