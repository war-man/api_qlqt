using Api.ManagerGift.Entities;
using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.ManagerGift.Maps
{
    public class GiftGroupMap : ClassMap<GiftGroup>
    {
        public GiftGroupMap()
        {
            Table("GiftGroup");
            Id(p => p.Id);
            Map(P => P.Code);
            Map(p => p.Name);
            References(p => p.OptionGift).Column("OptionGiftId").Not.LazyLoad();
            Map(p => p.CreatedBy);
            Map(p => p.CreatedDate);
            Map(p => p.UpdatedBy);
            Map(p => p.UpdatedDate);
            Map(p => p.Status);
        }
    }
}
