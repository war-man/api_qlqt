using Api.ManagerGift.Entities;
using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.ManagerGift.Maps
{
    public class StageMap : ClassMap<Stage>
    {
        public StageMap()
        {
            Table("Stage");
            Id(p => p.Id);
            Map(p => p.Name);
            Map(p => p.NextStage);
            Map(p => p.ProductId);
            Map(p => p.PositionId);
        }
    }
}
