using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DiscountOptionDataWeb.Data
{
    public  class Plan
    {
        public Plan()
        {
            this.Features = new HashSet<Feature>();
        }

        public string Name { get; set; }
        public int AmountInCents { get; set; }
        public string Currency { get; set; }
        public string Interval { get; set; }
        public int IntervalCount { get; set; } //new 03/29/18
        public int? TrialPeriodDays { get; set; }
        public int AmountInDollars
        {
            get
            {
                return (int)Math.Floor((decimal)this.AmountInCents / 100);
            }
        }
        [Key]
        public int Id { get; set; }
        public string ExternalId { get; set; }
        public string Description { get; set; }
        public string CSSThemeClasses { get; set; }
        public string CSSThemeButtonClasses { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        public System.DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public System.DateTime ModifiedAt { get; set; }
        public string ModifiedBy { get; set; }

        public virtual ICollection<Feature> Features { get; set; }
    }
}