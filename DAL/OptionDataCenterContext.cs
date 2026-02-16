using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using DiscountOptionDataWeb.Models;

namespace DiscountOptionDataWeb.DAL
{
    public class OptionDataCenterContext:DbContext
    {
        public OptionDataCenterContext():base("OptionDataCenterConnnection")
        {

        }

        public DbSet<Category> Categories { get; set; }

        public DbSet<Customer> Customers { get; set; }

        public DbSet<Product> Products { get; set; }

        public DbSet<CustomerOrder> CustomerOrders { get; set; }

        public DbSet<OrderedProduct> Orderedproducts { get; set; }

        public DbSet<Cart> Carts { get; set; }

        public DbSet<Coupon> Coupons { get; set; }
    }
}