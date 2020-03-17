using Api.ManagerGift.Entities;
using FluentNHibernate.Mapping;

namespace Api.ManagerGift.Maps
{
    public class SysPermisionMap : ClassMap<SysPermision>
    {
        public SysPermisionMap()
        {
            Table("SysPermision");
            Id(p => p.PermisionId);
            Map(p => p.PermisionName);
        }
    }
}
