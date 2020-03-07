using Api.ManagerGift.Abstracts;

namespace Api.ManagerGift.Entities
{
    public class Gift : GeneralGiftAbstract
    {
        public virtual GiftGroup GiftGroup { get; set; }
        public virtual Unit Unit { get; set; }
        public virtual decimal Price { get; set; }
    }
}
