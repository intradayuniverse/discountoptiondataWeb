using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using System.Web.Mvc;
using DiscountOptionDataWeb.DAL;
using DiscountOptionDataWeb.Models;
using DiscountOptionDataWeb.ViewModels;
using log4net;
using DiscountOptionDataWeb.Classes;

namespace DiscountOptionDataWeb.Controllers
{
    public class ShoppingCartController : Controller
    {
        private OptionDataCenterContext db = new OptionDataCenterContext();
        string _ipAddress = Utility.GetIPAddress();

        private static readonly ILog logger =
     log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public ActionResult Index()
        {
            logger.Info("viewing on ShoppingCart/Index: " + _ipAddress);
            Utility.InsertUserIPAddress(_ipAddress, "ShoppingCart/Index", "", "starting");

            var cart = ShoppingCart.GetCart(this.HttpContext);

            var viewModel = new ShoppingCartViewModel
            {
                CartItems = cart.GetCartItems(),
                CartTotal = cart.GetTotal()
            };

            //ViewBag.ProductID = "OK";

            return View(viewModel);
        }

        public ActionResult IndexTest()
        {
            var cart = ShoppingCart.GetCart(this.HttpContext);

            var viewModel = new ShoppingCartViewModel
            {
                CartItems = cart.GetCartItems(),
                CartTotal = cart.GetTotal()
            };

            return View(viewModel);
        }

        public ActionResult AddToCart(int id)
        {
            var addedProduct = db.Products.Single(product => product.Id == id);

            var cart = ShoppingCart.GetCart(this.HttpContext);

            //doesn't make sense to have 2 items of the same year in the basket
            //this is not like regular products like fruit with more then 1 item of the same thing
            //lnh: should we have a message on the client side not letting them adding 2 items of the same thing?
            List<Cart> lstCart = cart.GetCartItems();
            if (lstCart.Where(c => c.ProductId == id).Count() == 0)
            {
                cart.AddToCart(addedProduct);
            }

            logger.Info("AddToCart on ShoppingCart/AddToCart: " + _ipAddress);
            Utility.InsertUserIPAddress(_ipAddress, "ShoppingCart/AddToCart", "AddToChart", "starting");


            return RedirectToAction("Index");
        }

        //[HttpPost]
        public ActionResult RemoveFromCart(int id)
        {
            var cart = ShoppingCart.GetCart(this.HttpContext);

            string productName = db.Carts.FirstOrDefault(item => item.ProductId == id).Product.Name;

            int itemCount = cart.RemoveFromCart(id);

            logger.Info("AddToCart on ShoppingCart/RemoveFromCart: " + _ipAddress);
            Utility.InsertUserIPAddress(_ipAddress, "ShoppingCart/RemoveFromCart", "RemoveFromCart", "starting");

            return RedirectToAction("Index");

            //what's this stuff about ???
            //var results = new ShoppingCartRemoveViewModel
            //{
            //    Message = Server.HtmlEncode(productName) + " has been removed from your shopping cart",
            //    CartTotal = cart.GetTotal(),
            //    CartCount = cart.GetCount(),
            //    ItemCount = itemCount,
            //    DeleteId = id
            //};

            //return Json(results);
        }

        [ChildActionOnly]
        public ActionResult CartSummary()
        {
            var cart = ShoppingCart.GetCart(this.HttpContext);

            ViewData["CartCount"] = cart.GetCount();
            //return View("CartSummary");
            return PartialView("CartSummary");
        }

    }
}