using Api.ManagerGift.Entities;
using FluentNHibernate.Mapping;

namespace Api.ManagerGift.Maps
{
    public class GiftMap : ClassMap<Gift>
    {
        public GiftMap()
        {
            Table("Gift");
            Id(p => p.Id);
            Map(P => P.Code);
            Map(p => p.Name);
            References(p => p.GiftGroup).Column("GiftGroupId").Not.LazyLoad();
            References(p => p.Unit).Column("UnitId").Not.LazyLoad();
            Map(p => p.Price);
            Map(p => p.CreatedBy);
            Map(p => p.CreatedDate);
            Map(p => p.UpdatedBy);
            Map(p => p.UpdatedDate);
            Map(p => p.Status);
        }
    }
}
