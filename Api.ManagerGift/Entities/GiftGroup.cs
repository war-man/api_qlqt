using Api.ManagerGift.Abstracts;
using System;

namespace Api.ManagerGift.Entities
{
    public class GiftGroup : GeneralGiftAbstract
    {
        public virtual OptionGift OptionGift { get; set; }
    }
}
