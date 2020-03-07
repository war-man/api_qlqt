using Api.ManagerGift.Entities;
using FluentNHibernate.Mapping;

namespace Api.ManagerGift.Maps
{
    public class GiftPromotionMap : ClassMap<GiftPromotion>
    {
        public GiftPromotionMap()
        {
            Table("GiftPromotion");
            Id(p => p.Id);
            Map(p => p.GiftPromotionId);
            //Map(P => P.CodeGift);
            //Map(p => p.NameGift);
            //Map(p => p.Price);
            //Map(p => p.UnitName);
            //Map(p => p.GiftGroupName);
            Map(p => p.Amount);
            Map(p => p.GiftId);
        }
    }
}
