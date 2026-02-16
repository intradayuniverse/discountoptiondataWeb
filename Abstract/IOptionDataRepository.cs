using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DiscountOptionDataWeb.Entities;

namespace DiscountOptionDataWeb.Abstract
{
    public interface IOptionDataRepository
    {
        IEnumerable<OptionData> GetOptionDataList(string symbol = "", string dataDate = "", string expirationDate = "");
        IEnumerable<string> GetExpirationList(string symbol, string dataDate);
    }
}