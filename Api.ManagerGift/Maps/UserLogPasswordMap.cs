using Api.ManagerGift.Entities;
using FluentNHibernate.Mapping;

namespace Api.ManagerGift.Maps
{
    public class UserLogPasswordMap : ClassMap<UserLogPassword>
    {
        public UserLogPasswordMap()
        {
            Table("[UserLogPassword]");
            Id(p => p.Id);
            Map(p => p.UserId);
            Map(p => p.Password);
            Map(p => p.Time);
            Map(p => p.CreateDate);
        }
    }
}
