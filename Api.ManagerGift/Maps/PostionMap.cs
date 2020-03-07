using Api.ManagerGift.Entities;
using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.ManagerGift.Maps
{
    public class PostionMap : ClassMap<Position>
    {
        public PostionMap()
        {
            Table("Position");
            Id(p => p.Id);
            Map(p => p.Name);
            Map(p => p.Code);
            Map(p => p.IsLeader);
        }
    }
}
