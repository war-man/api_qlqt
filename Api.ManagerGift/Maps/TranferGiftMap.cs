using Api.ManagerGift.Entities;
using FluentNHibernate.Mapping;

namespace Api.ManagerGift.Maps
{
    public class TranferGiftMap : ClassMap<TransferGift>
    {
        public TranferGiftMap()
        {
            Table("TransferGift");
            Id(p => p.Id);
            Map(p => p.Code);
            References(p => p.Product).Not.LazyLoad().Column("ProductId");
            Map(p => p.IsFinished);
            Map(p => p.IsComplete);
            Map(p => p.DepartmentId);
            Map(p => p.PromotionId);
            Map(p => p.Status);
            Map(p => p.CreatedBy);
            Map(p => p.CreatedDate);
            Map(p => p.Deadline);
            Map(p => p.StageCurrent);
            Map(p => p.FlagDieuChuyen);
            Map(p => p.NguoiDuyet);
            Map(p => p.NgayDuyet);
        }
    }
}
