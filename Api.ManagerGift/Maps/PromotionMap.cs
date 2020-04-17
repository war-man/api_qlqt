using Api.ManagerGift.Entities;
using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.ManagerGift.Maps
{
    public class PromotionMap : ClassMap<Promotion>
    {
        public PromotionMap()
        {
            Table("Promotion");
            Id(p => p.Id);
            Map(p => p.Code);
            Map(p => p.Name);
            Map(p => p.Status);
            Map(p => p.CreatedBy);
            Map(p => p.CreatedDate);
            Map(p => p.NguoiDuyet);
            Map(p => p.NgayDuyet);
            Map(p => p.NumberOdEdit);
            Map(p => p.GiftPromotionId);
            Map(p => p.ConfigPromotion);
            Map(p => p.StartDate);
            Map(p => p.FinishDate);
            Map(p => p.Description);
            Map(p => p.MaxGiftWithCustomer);
            Map(p => p.MaxGiftInDay);
            Map(p => p.IsChange);
            Map(p => p.SoLanHPB);
        }
    }
}
