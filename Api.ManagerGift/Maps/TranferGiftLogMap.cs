using Api.ManagerGift.Entities;
using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.ManagerGift.Maps
{
    public class TranferGiftLogMap : ClassMap<TransferGiftLog>
    {
        public TranferGiftLogMap()
        {
            Table("TransferGiftLog");
            Id(p => p.Id);
            References(p => p.TransferGift).Not.LazyLoad().Column("TransferGiftId");
            Map(p => p.AssignUserId);
            Map(p => p.AssignDeaprtmentId);
            Map(p => p.Comment);
            Map(p => p.Data).CustomSqlType("CLOB");
            Map(p => p.Status);
            Map(p => p.UpdateDate);
            References(p => p.Stage).Not.LazyLoad().Column("StageId");
            Map(p => p.Dealine);
            Map(p => p.FlagDieuChuyen);
        }
    }
}
