using System;
using System.Collections.Generic;

namespace Api.ManagerGift.DTO
{
    public class NewOrganizationDTO
    {
        public NewOrganizationDTO()
        {
            items = new List<NewOrganizationDTO>();
        }
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string text { get; set; }
        public Guid? ParentId { get; set; }
        public string ManageCode { get; set; }
        public List<NewOrganizationDTO> items { get; set; }
    }
}
