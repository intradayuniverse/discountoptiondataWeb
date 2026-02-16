using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DiscountOptionDataWeb.DAL;

namespace DiscountOptionDataWeb.Models
{
    public partial class ShoppingCart
    {
        OptionDataCenterContext db = new OptionDataCenterContext();

        public string ShoppingCartId { get; set; }

        public const string CartSessionKey = "cartId";

        public static ShoppingCart GetCart(HttpContextBase context)
        {
           
           
            var cart = new ShoppingCart();

            cart.ShoppingCartId = cart.GetCartId(context);

            return cart;
        }
        // Helper method to simplify shopping cart calls
        public static ShoppingCart GetCart(Controller controller)
        {
            return GetCart(controller.HttpContext);
        }

        /// <summary>
        /// add product to db
        /// </summary>
        /// <param name="product"></param>
        public void AddToCart(Product product)
        {
            // Get the matching cart and album instances
            var cartItem = db.Carts.SingleOrDefault(c=>c.CartId == ShoppingCartId && c.ProductId == product.Id);
           
            // Create a new cart item if no cart item exists
            if (cartItem == null)
            {
                cartItem = new Cart
                {
                    ProductId = product.Id,
                    CartId = ShoppingCartId,
                    Count = 1,
                    DateCreated = DateTime.Now
                };
                // If the item does exist in the cart, then add one to the quantity
                db.Carts.Add(cartItem);
            }
            else
            {
                cartItem.Count++;
            }

            db.SaveChanges();
        }

        public int RemoveFromCart(int id)
        {
            var cartItem = db.Carts.SingleOrDefault(cart => cart.CartId == ShoppingCartId && cart.ProductId == id);

            int itemCount = 0;

            if (cartItem != null)
            {
                if (cartItem.Count > 1)
                {
                    cartItem.Count--;
                    itemCount = cartItem.Count;
                }
                else
                {
                    db.Carts.Remove(cartItem);
                }

                db.SaveChanges();
            }
            return itemCount;
        }

        public void EmptyCart()
        {
            var cartItems = db.Carts.Where(cart => cart.CartId == ShoppingCartId);

            foreach (var cartItem in cartItems)
            {
                db.Carts.Remove(cartItem);
            }
            db.SaveChanges();
        }

        public List<Cart> GetCartItems()
        {
            return db.Carts.Where(cart => cart.CartId == ShoppingCartId).ToList();
        }

        public int GetCount()
        {
            int? count =
                (from cartItems in db.Carts where cartItems.CartId == ShoppingCartId select (int?) cartItems.Count).Sum();

            return count ?? 0;
        }

        public decimal GetTotal()
        {
            decimal? total = (from cartItems in db.Carts
                where cartItems.CartId == ShoppingCartId
                select (int?) cartItems.Count*cartItems.Product.Price).Sum();

            return total ?? decimal.Zero;
        }

        public int CreateOrder(CustomerOrder customerOrder)
        {
            decimal orderTotal = 0;

            var cartItems = GetCartItems();

            foreach (var item in cartItems)
            {
                var orderedProduct = new OrderedProduct
                {
                    ProductId = item.ProductId,
                    CustomerOrderId = customerOrder.Id,
                    Quantity = item.Count
                };

                orderTotal += (item.Count*item.Product.Price);

                db.Orderedproducts.Add(orderedProduct);
            }

            customerOrder.Amount = orderTotal;

            db.SaveChanges();

            //lnh: 12/25/2017: this is creating issues of card payment invalid and try again?
            //EmptyCart();

            return customerOrder.Id;
        }

        /// <summary>
        /// store  the cart in Session if null, else return the cart session
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public string GetCartId(HttpContextBase context)
        {
            if (context.Session[CartSessionKey] == null)
            {
                if (!string.IsNullOrWhiteSpace(context.User.Identity.Name))
                {
                    context.Session[CartSessionKey] = context.User.Identity.Name;
                }

                else
                {
                    // Generate a new random GUID using System.Guid class
                    Guid tempCartId = Guid.NewGuid();
                    context.Session[CartSessionKey] = tempCartId.ToString();
                }
            }

            return context.Session[CartSessionKey].ToString();
        }

        /// <summary>
        ///  // When a user has logged in, migrate their shopping cart to
        // be associated with their username
        /// </summary>
        /// <param name="userName"></param>
        public void MigrateCart(string userName)
        {
            var shoppingCart = db.Carts.Where(c => c.CartId == ShoppingCartId);
            foreach (Cart item in shoppingCart)
            {
                item.CartId = userName;
            }

            db.SaveChanges();
        }

    }
}