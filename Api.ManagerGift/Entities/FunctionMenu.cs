using System;

namespace Api.ManagerGift.Entities
{
    public class FunctionMenu
    {
        public virtual Guid Id { get; set; }
        public virtual string Name { get; set; }
        public virtual string URL { get; set; }
        public virtual string IConCss { get; set; }
        public virtual Guid ParentId { get; set; }
        public virtual int SortOrder { get; set; }
        public virtual bool Status { get; set; }
    }
}
