using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DiscountOptionDataWeb.DAL;
using DiscountOptionDataWeb.Models;
using log4net;
using System.Web.UI;
using DiscountOptionDataWeb.Classes;

namespace DiscountOptionDataWeb.Controllers
{
   //[RequireHttps]
    public class HomeController : Controller
    {
        private OptionDataCenterContext db = new OptionDataCenterContext();

        private static readonly ILog logger =
     log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        //[OutputCache(Duration = 3600, VaryByParam = "none")]

        string _ipAddress = Utility.GetIPAddress();
        public ActionResult Index()
        {
            //test
            //if (Request.Url.ToString().ToLower().Equals("https://discountoptiondata.com"))
            //{
            //    Response.Redirect("https://www.discountoptiondata.com");
            //}
            

            logger.Info("viewing on Home/Index: " + _ipAddress);
            Utility.InsertUserIPAddress(_ipAddress, "Home/Index", "", "starting");

            //test: hard code for now, category=1, OptionData
            Category category = db.Categories.Find(1);
            if (category == null)
            {
                logger.Warn("There is not category for Category of 1");
                return HttpNotFound();

            }
           
          
            return View(category);
        }

        public ActionResult About()
        {
            logger.Info("viewing on Home/About: " + _ipAddress);
            Utility.InsertUserIPAddress(_ipAddress, "Home/About", "", "starting");
            ViewBag.Message = "About Us";

            return View();
        }

        public ActionResult Contact()
        {
            logger.Info("viewing on Home/Contact: " + _ipAddress);
            Utility.InsertUserIPAddress(_ipAddress, "Home/Contact", "", "starting");
            ViewBag.Message = "Your contact page.";

            return View();
        }
        public ActionResult Product()
        {
            logger.Info("viewing on Home/Product: " + _ipAddress);
            return View();
        }

        public ActionResult FAQ()
        {
            logger.Info("viewing on Home/FAQ: " + _ipAddress);
            Utility.InsertUserIPAddress(_ipAddress, "Home/FAQ", "", "starting");
            return View();
        }

        public ActionResult ProductDetails()
        {
            logger.Info("viewing on Home/ProductDetails: " + _ipAddress);
            Utility.InsertUserIPAddress(_ipAddress, "Home/ProductDetails", "", "starting");
            return View();
        }

        public  ActionResult PrivacyPolicy()
        {
            return View();
        }

        public ActionResult TermsConditions()
        {
            return View();
        }

        public ActionResult Test()
        {
            return View();
        }
    }
}