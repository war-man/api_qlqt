using System;

namespace Api.ManagerGift.Entities
{
    public class Organization
    {
        public virtual Guid Id { get; set; }
        public virtual string Name { get; set; }
        public virtual string Code { get; set; }
        public virtual Guid? ParentId { get; set; }
        public virtual string ManageCode { get; set; }
        public virtual string Address { get; set; }
        public virtual string Region { get; set; }
        public virtual DateTime CreateDate { get; set; }
    }
}
