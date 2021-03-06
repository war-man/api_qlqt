﻿using System;

namespace Api.ManagerGift.Entities
{
    public class Product
    {
        public virtual Guid Id { get; set; }
        public virtual string Name { get; set; }
        public virtual string Code { get; set; }
        public virtual bool Publish { get; set; }
    }
}
