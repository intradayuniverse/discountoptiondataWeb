using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DiscountOptionDataWeb.Models
{
    public class OptionFile
    {
        public string FileName { get; set; }
        public ulong? FileSize { get; set; }
        public string ShortFileName { get; set; }
        
    }
}