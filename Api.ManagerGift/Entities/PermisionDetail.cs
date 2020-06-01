using System;

namespace Api.ManagerGift.Entities
{
    public class PermisionDetail
    {
        public virtual Guid Id { get; set; }
        public virtual Guid ParentId { get; set; }
        public virtual int PermisionId { get; set; }
        public virtual string ActionName { get; set; }
        public virtual string ActionCode { get; set; }
        public virtual bool CheckAction { get; set; }
        public virtual Guid NavId { get; set; }
    }
}
