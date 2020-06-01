using Api.ManagerGift.Entities;
using System;
using System.Collections.Generic;

namespace Api.ManagerGift.DTO
{
    public class NavDTO
    {
        public Guid id { get; set; }
        public string name { get; set; }
        public string icon { get; set; }
        public string url { get; set; }
        public bool title { get; set; }
        public List<NavDTO> children { get; set; }
    }
}
