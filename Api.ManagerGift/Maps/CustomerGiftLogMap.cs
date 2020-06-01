using Api.ManagerGift.Entities;
using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.ManagerGift.Maps
{
    public class CustomerGiftLogMap : ClassMap<CustomerGiftLog>
    {
        public CustomerGiftLogMap()
        {
            Table("CustomerGiftLog");
            Id(p => p.Id);
            //References(p => p.CustomerGift).Not.LazyLoad().Column("CustomerGiftId");
            Map(p => p.CustomerGiftId);
            Map(p => p.AssignUserId);
            Map(p => p.AssignDeaprtmentId);
            Map(p => p.Comment);
            Map(p => p.Status);
            Map(p => p.UpdateDate);
            Map(p => p.StageId);
        }
    }
}
