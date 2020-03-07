using Api.ManagerGift.Entities;
using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.ManagerGift.Maps
{
    public class OptionGiftMap : ClassMap<OptionGift>
    {
        public OptionGiftMap()
        {
            Table("OptionGift");
            Id(p => p.Id);
            Map(P => P.Code);
            Map(p => p.Name);
            Map(p => p.CreatedBy);
            Map(p => p.CreatedDate);
            Map(p => p.UpdatedBy);
            Map(p => p.UpdatedDate);
            Map(p => p.Status);
        }
    }
}
