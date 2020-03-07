using Api.ManagerGift.Entities;
using FluentNHibernate.Mapping;

namespace Api.ManagerGift.Maps
{
    public class PermisionMap : ClassMap<Permision>
    {
        public PermisionMap()
        {
            Table("[SysPermision]");
            Id(p => p.PermisionId);
            Map(p => p.PermisionName);
            Map(p => p.Navigation);
        }
    }
}
