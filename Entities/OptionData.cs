using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DiscountOptionDataWeb.Entities
{
    public class OptionData
    {
        public string Symbol { get; set; }
        public string ExpirationDate { get; set; }
        public string AskPrice { get; set; }
        public string AskSize { get; set; }
        public string BidPrice { get; set; }
        public string BidSize { get; set; }
        public string LastPrice { get; set; }
        public string PutCall { get; set; }
        public string StrikePrice { get; set; }
        public string Volume { get; set; }

        public string ImpliedVolatility { get; set; }
        //public string Exchange { get; set; }
        public string Delta { get; set; }
        public string Gamma { get; set; }
        public string Vega { get; set; }
        public string Rho { get; set; }
        public string OpenInterest { get; set; }
        public string UnderlyingPrice { get; set; }
        public string DataDate { get; set; }
    }
}