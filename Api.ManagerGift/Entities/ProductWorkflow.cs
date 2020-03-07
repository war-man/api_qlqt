using System;

namespace Api.ManagerGift.Entities
{
    public class ProductWorkflow
    {
        public virtual Guid Id { get; set; }
        public virtual string ContentData { get; set; }
        public virtual string DesignData { get; set; }
    }
}
