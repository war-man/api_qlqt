using Api.ManagerGift.Abstracts;

namespace Api.ManagerGift.Entities
{
    public class CustomerGiftLog : GeneralLogAbstract
    {
        public virtual CustomerGift CustomerGift { get; set; }
        //public virtual int Status { get; set; }
    }
}
