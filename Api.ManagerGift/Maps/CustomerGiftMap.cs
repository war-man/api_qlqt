using Api.ManagerGift.Entities;
using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.ManagerGift.Maps
{
    public class CustomerGiftMap : ClassMap<CustomerGift>
    {
        public CustomerGiftMap()
        {
            Table("CustomerGift");
            Id(p => p.Id);
            Map(p => p.Acctno);
            Map(p => p.TENLOAIHINH);
            Map(p => p.CusName);
            Map(p => p.CusId);
            Map(p => p.TERM);
            Map(p => p.TERMCD);
            Map(p => p.BALANCE);
            Map(p => p.FRDATE);
            Map(p => p.TODATE);
            Map(p => p.CHEQUENO);
            Map(p => p.INTRATE);
            Map(p => p.RATECD);
            Map(p => p.LICENSE);
            Map(p => p.SUBBRID);
            Map(p => p.SUBBRNAME);
            Map(p => p.BRANCHID);
            Map(p => p.BRNAME);
            Map(p => p.CREATEDDATE);
            Map(p => p.CREATEDBy);
            Map(p => p.USERID);
            Map(p => p.ACTYPE);
            Map(p => p.Status);
            Map(p => p.PhanHe);
            Map(p => p.NguoiDuyet);
            Map(p => p.NgayDuyet);
            Map(p => p.NumGift);
            Map(p => p.CCYCD);
            References(p => p.Gift).Not.LazyLoad().Column("GiftId");
            References(p => p.Promotion).Not.LazyLoad().Column("PromotionId");
        }
    }
}
