using System;

namespace Api.ManagerGift.DTO
{
    public class PhanBoQuaTang
    {
        public string id { get; set; }
        public string [] chinhanh_pgd { get; set; }
        public string CodeGift { get; set; }
        public string NameGift { get; set; }
        public string GiftId { get; set; }
        public int Amount { get; set; }
        public string UnitName { get; set; }
        public string Price { get; set; }
        public string TotalPrice { get; set; }

    }
}
