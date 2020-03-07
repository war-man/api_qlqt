namespace Api.ManagerGift.DTO
{
    public class DataNhapKho
    {
        public string id { get; set; }
        public string GiftId { get; set; }
        public string GiftCode { get; set; }
        public string GiftName { get; set; }
        public string GiftGroupName { get; set; }
        public int Amount { get; set; }
        public string UnitName { get; set; }
        public double Price { get; set; }
        public string TotalPrice { get; set; }
        public string PromotionId { get; set; }
        public string Note { get; set; }

    }
}
