using Api.ManagerGift.Entities;
using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.ManagerGift.Maps
{
    public class UnitMap : ClassMap<Unit>
    {
        public UnitMap()
        {
            Table("Unit");
            Id(p => p.Id);
            Map(p => p.Code);
            Map(p => p.Name);
            Map(p => p.CreatedBy);
            Map(p => p.CreatedDate);
            Map(p => p.UpdatedBy);
            Map(p => p.UpdatedDate);
        }
    }
}
