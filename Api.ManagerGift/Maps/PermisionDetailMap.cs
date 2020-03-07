using Api.ManagerGift.Entities;
using FluentNHibernate.Mapping;

namespace Api.ManagerGift.Maps
{
    public class PermisionDetailMap : ClassMap<PermisionDetail>
    {
        public PermisionDetailMap()
        {
            Table("[SysPermisionDetail]");
            Id(p => p.Id);
            Map(p => p.ParentId);
            Map(p => p.ActionCode);
            Map(p => p.ActionName);
            Map(p => p.CheckAction);
            Map(p => p.Url);
            Map(p => p.PermisionId);
            Map(p => p.NavName);
        }
    }
}