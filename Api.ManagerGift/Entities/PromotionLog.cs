using Api.ManagerGift.Abstracts;
using System;

namespace Api.ManagerGift.Entities
{
    public class PromotionLog : GeneralLogAbstract
    {
        //public virtual Guid Id { get; set; }
        public virtual Promotion Promotion { get; set; }
        public virtual string DataPromotion { get; set; }
        public virtual string GiftPromotion { get; set; }
        public virtual string ConfigPromotion { get; set; }
    }
}
