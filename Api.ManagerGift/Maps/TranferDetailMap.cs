using Api.ManagerGift.Entities;
using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.ManagerGift.Maps
{
    public class TranferDetailMap : ClassMap<TransferDetail>
    {
        public TranferDetailMap()
        {
            Table("TransferDetail");
            Id(p => p.Id);
            References(p => p.TransferGift).Not.LazyLoad().Column("TransferId");
            Map(p => p.ReceivingPromotion);
            Map(p => p.ReceivingDepartment);
            Map(p => p.GiftId);
            Map(p => p.Amount);
            Map(p => p.FlagDieuChuyen);
        }
    }
}
