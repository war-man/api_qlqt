using Api.ManagerGift.Entities;
using FluentNHibernate.Mapping;

namespace Api.ManagerGift.Maps
{
    public class DraftMap : ClassMap<Draft>
    {
        public DraftMap()
        {
            Table("Draft");
            Id(p => p.Id);
            Map(p => p.Data);
            Map(p => p.PhanHe);
        }
    }
}
