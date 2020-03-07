using Api.ManagerGift.Entities;
using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.ManagerGift.Maps
{
    public class ReceivingPromotionMap : ClassMap<ReceivingPromotion>
    {
        public ReceivingPromotionMap()
        {
            Table("ReceivingPromotion");
            Id(p => p.Id);
            Map(p => p.PromotionId);
            References(p => p.TransferGift).Not.LazyLoad().Column("TransferId");
        }
    }
}
