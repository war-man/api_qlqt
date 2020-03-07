using Api.ManagerGift.Entities;
using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.ManagerGift.Maps
{
    public class ReceivingDepartmentMap : ClassMap<ReceivingDepartment>
    {
        public ReceivingDepartmentMap()
        {
            Table("ReceivingDepartments");
            Id(p => p.Id);
            Map(p => p.DepartmentId);
            References(p => p.TransferGift).Not.LazyLoad().Column("TransferId");
        }
    }
}
