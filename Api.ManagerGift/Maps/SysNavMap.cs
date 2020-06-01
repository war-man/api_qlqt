using Api.ManagerGift.Entities;
using FluentNHibernate.Mapping;

namespace Api.ManagerGift.Maps
{
    public class SysNavMap : ClassMap<SysNav>
    {
        public SysNavMap()
        {
            Table("[SysNav]");
            Id(p => p.Id);
            Map(p => p.ParentId);
            Map(p => p.Name);
            Map(p => p.Url);
            Map(p => p.Icon);
            Map(p => p.Position);
            Map(p => p.Title);
            Map(p => p.Type);
            Map(p => p.Active);
        }
    }
}