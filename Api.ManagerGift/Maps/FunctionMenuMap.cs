using Api.ManagerGift.Entities;
using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.ManagerGift.Maps
{
    public class FunctionMenuMap : ClassMap<FunctionMenu>
    {
        public FunctionMenuMap()
        {
            Table("FUNCTIONMenu");
            Id(p => p.Id);
            Map(p => p.Name);
            Map(p => p.URL);
            Map(p => p.IConCss);
            Map(p => p.ParentId);
            Map(p => p.SortOrder);
            Map(p => p.Status);
        }
    }
}
