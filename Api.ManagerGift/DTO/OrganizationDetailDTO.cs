using System;
using System.Collections.Generic;

namespace Api.ManagerGift.DTO
{
    public class OrganizationDetailDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public Guid? ParentId { get; set; }
        public string ManageCode { get; set; }
        public string Address { get; set; }
        public string Region { get; set; }
        public string RegionName { get; set; }
        public string ParentName { get; set; }
    }
}
