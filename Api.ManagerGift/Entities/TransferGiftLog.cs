using Api.ManagerGift.Abstracts;
using System;

namespace Api.ManagerGift.Entities
{
    public class TransferGiftLog : GeneralLogAbstract
    {
        public virtual TransferGift TransferGift { get; set; }
        public virtual string Data { get; set; }
        public virtual Stage Stage { get; set; }
        public virtual DateTime? Dealine { get; set; }
        public virtual Guid FlagDieuChuyen { get; set; }
    }
}
