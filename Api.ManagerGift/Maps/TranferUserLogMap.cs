using Api.ManagerGift.Entities;
using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.ManagerGift.Maps
{
    public class TranferUserLogMap : ClassMap<TransferUserLog>
    {
        public TranferUserLogMap()
        {
            Table("TransferUserLog");
            Id(p => p.Id);
            Map(p => p.UserName);
            Map(p => p.OldDepartmentCode);
            Map(p => p.OldPositionCode);
            Map(p => p.NewDepartmentCode);
            Map(p => p.NewPositionCode);
            Map(p => p.Note);
            Map(p => p.TransferDate);
            Map(p => p.UserTransfer);
            Map(p => p.IsTransfer);
        }
    }
}
