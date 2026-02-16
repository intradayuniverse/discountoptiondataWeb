using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DiscountOptionDataWeb.Data;

namespace DiscountOptionDataWeb.Models
{
    public class SubIndexViewModel
    {
        public IList<Plan> Plans { get; set; }
    }

    public class BillingViewModel
    {
        public Plan Plan { get; set; }
        public string StripePublishableKey { get; set; }

        public string StripeToken { get; set; }
    }
}