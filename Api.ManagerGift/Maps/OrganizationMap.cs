using Api.ManagerGift.Entities;
using FluentNHibernate.Mapping;

namespace Api.ManagerGift.Maps
{
    public class OrganizationMap : ClassMap<Organization>
    {
        public OrganizationMap()
        {
            Table("Organization");
            Id(p => p.Id);
            Map(p => p.Name);
            Map(p => p.Code);
            Map(p => p.ParentId);
            Map(p => p.ManageCode);
            Map(p => p.Address);
            Map(p => p.Region);
        }
    }
}
