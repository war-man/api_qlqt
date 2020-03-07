﻿using System;

namespace Api.ManagerGift.DTO
{
    public class CustomerDTO
    {
        public string Acctno { get; set; }
        public string PhanHe { get; set; }
        public string TENLOAIHINH { get; set; }
        public string CusName { get; set; }
        public string CusId { get; set; }
        public decimal TERM { get; set; }
        public string TERMCD { get; set; }
        public decimal BALANCE { get; set; }
        public DateTime FRDATE { get; set; }
        public DateTime TODATE { get; set; }
        public string CHEQUENO { get; set; }
        public decimal INTRATE { get; set; }
        public string RATECD { get; set; }
        public string LICENSE { get; set; }
        public string SUBBRID { get; set; }
        public string SUBBRNAME { get; set; }
        public string BRANCHID { get; set; }
        public string BRNAME { get; set; }
        public DateTime? CREATEDDATE { get; set; }
        public string CREATEDBy { get; set; }
        public string ACTYPE { get; set; }
        public string CCYCD { get; set; }
    }
}
