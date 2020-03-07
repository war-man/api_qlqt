using Api.ManagerGift.Entities;
using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.ManagerGift.Maps
{
    public class PromotionLogMap : ClassMap<PromotionLog>
    {
        public PromotionLogMap()
        {
            Table("PromotionLog");
            Id(p => p.Id);
            References(p => p.Promotion).Not.LazyLoad().Column("PromotionId");
            Map(p => p.AssignUserId);
            Map(p => p.AssignDeaprtmentId);
            Map(p => p.Comment);
            Map(p => p.DataPromotion).CustomSqlType("CLOB");
            Map(p => p.GiftPromotion).CustomSqlType("CLOB");
            Map(p => p.ConfigPromotion);
            Map(p => p.Status);
            Map(p => p.UpdateDate);
            //Map(p => p.StageId);
        }
    }
}
