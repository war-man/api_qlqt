using Api.ManagerGift.Entities;
using FluentNHibernate.Mapping;

namespace Api.ManagerGift.Maps
{
    public class StoreMap : ClassMap<Store>
    {
        public StoreMap()
        {
            Table("Store");
            Id(p => p.Id);
            Map(p => p.DepartmentId);
            Map(p => p.ManagerType);
            Map(p => p.PromotionId);
            Map(p => p.GiftId);
            Map(p => p.Amount);
            Map(p => p.UpdatedDate);
            Map(p => p.LogTransfer);
            //Map(p => p.TransferId);
        }
    }
}
