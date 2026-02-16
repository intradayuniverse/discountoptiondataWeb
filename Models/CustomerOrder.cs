using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DiscountOptionDataWeb.Models
{
    [Bind(Exclude = "Id")]
    public class CustomerOrder
    {
        //for Stripe
        [Required]
        //[ScaffoldColumn(false)]
        public string Token { get; set; }


        [ScaffoldColumn(false)]
        [DisplayName("Order ID")]
        public int Id { get; set; }

        //[Required(ErrorMessage = "First Name is required")]
        [DisplayName("First Name")]
        [StringLength(160)]
        public string FirstName { get; set; }
        //[Required(ErrorMessage = "Last Name is required")]
        [DisplayName("Last Name")]
        [StringLength(160)]
        public string LastName { get; set; }
        //[Required(ErrorMessage = "Address is required")]
        [StringLength(70)]
        public string Address { get; set; }
        //[Required(ErrorMessage = "City is required")]
        [StringLength(40)]
        public string City { get; set; }

        //[Required(ErrorMessage = "State is required")]
        [StringLength(40)]
        public string State { get; set; }

        //[Required(ErrorMessage = "Postal Code is required")]
        [DisplayName("Postal Code")]
        [StringLength(10)]
        public string PostalCode { get; set; }

        //[Required(ErrorMessage = "Country is required")]
        [StringLength(40)]
        public string Country { get; set; }
        //[Required(ErrorMessage = "Phone is required")]
        //[StringLength(24)]
        public string Phone { get; set; }
        //[Required(ErrorMessage = "Email Address is required")]
        //[DisplayName("Email Address")]

        //[RegularExpression(@"[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,4}",
        //    ErrorMessage = "Email is is not valid.")]
        //[DataType(DataType.EmailAddress)]
        public string GmailAddress { get; set; }
        public bool ShipToHome { get; set; }
        public bool PaymentCharged { get; set; }
        public string Email { get; set; }

        [ScaffoldColumn(false)]
        [Column(TypeName = "datetime2")]
        [DisplayName("Order Date")]
        public DateTime DateCreated { get; set; }

        //also used by stripe
        [ScaffoldColumn(false)]
        public Decimal Amount { get; set; }

        //11/1/2020
        public  int? DiscountPercent { get; set; }
        public string Coupon { get; set; }
        //also used by stripe
        [ScaffoldColumn(false)]
        public Decimal? FinalAmount { get; set; }

        [ScaffoldColumn(false)]
        public string CustomerUserName { get; set; }

        //lnh
        public virtual ICollection<OrderedProduct> OrderProducts { get; set; }

    }
}