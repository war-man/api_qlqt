using System;

namespace Api.ManagerGift.Entities
{
    public class SysNav
    {
        public virtual Guid Id { get; set; }
        public virtual string Name { get; set; }
        public virtual string Icon { get; set; }
        public virtual string Url { get; set; }
        public virtual bool Title { get; set; }
        public virtual Guid ParentId { get; set; }
        public virtual int Position { get; set; }
        public virtual bool Active { get; set; }
        public virtual int Type { get; set; }

    }
}
