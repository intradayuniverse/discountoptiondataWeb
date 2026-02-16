using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DiscountOptionDataWeb.Models
{
    public class Coupon
    {
        [Key]
        public string CouponSymbol { get; set; }
        public int  PercentDiscount { get; set; }
        public bool Active { get; set; }
    }
}