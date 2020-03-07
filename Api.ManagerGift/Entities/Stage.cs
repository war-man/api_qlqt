using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.ManagerGift.Entities
{
    public class Stage
    {
        public virtual Guid Id { get; set; }
        public virtual string Name { get; set; }
        public virtual Guid NextStage { get; set; }
        public virtual Guid ProductId { get; set; }
        public virtual Guid PositionId { get; set; }
    }
}
