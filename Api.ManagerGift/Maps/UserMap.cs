using Api.ManagerGift.Entities;
using FluentNHibernate.Mapping;

namespace Api.ManagerGift.Maps
{
    public class UserMap : ClassMap<User>
    {
        public UserMap()
        {
            Table("[User]");
            Id(p => p.Id);
            Map(p => p.UserName);
            Map(p => p.Password);
            References(p => p.Position).Not.LazyLoad().Column("PositionId");
            References(p => p.Organization).Not.LazyLoad().Column("OrganizationId");
            Map(p => p.Email);
            Map(p => p.FullName);
            Map(p => p.Status);
            Map(p => p.MonthId);
            Map(p => p.PermisionId);
            Map(p => p.IsUser);
        }
    }
}
