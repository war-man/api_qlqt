using Api.ManagerGift.Entities;
using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.ManagerGift.Maps
{
    public class ProductWorkflowMap : ClassMap<ProductWorkflow>
    {
        public ProductWorkflowMap()
        {
            Table("ProductWorkflow");
            Id(p => p.Id);
            Map(p => p.ContentData).CustomSqlType("CLOB");
            Map(p => p.DesignData).CustomSqlType("CLOB");
        }
    }
}
