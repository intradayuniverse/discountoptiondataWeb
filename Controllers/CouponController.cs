using DiscountOptionDataWeb.DAL;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DiscountOptionDataWeb.Models;


namespace DiscountOptionDataWeb.Controllers
{
    public class CouponController : Controller
    {
        private OptionDataCenterContext db = new OptionDataCenterContext();

        private static readonly ILog logger =
     log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        //[OutputCache(Duration = 3600, VaryByParam = "none")]
        // GET: Coupon
        public ActionResult Index()
        {
            List<Coupon> lst = new List<Coupon>();
            lst = db.Coupons.ToList<Coupon>();
            return View(lst);
        }

        // To fill data in the form  
        // to enable easy editing 
        public ActionResult Update(string couponSymbol)
        {
            using (var context = new OptionDataCenterContext())
            {
                var data = context.Coupons.Where(x => x.CouponSymbol == couponSymbol).SingleOrDefault();
                return View(data);
            }
        }


        // To specify that this will be  
        // invoked when post method is called 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(string couponSymbol, Coupon model)
        {
            using (var context = new OptionDataCenterContext())
            {

                // Use of lambda expression to access 
                // particular record from a database 
                var data = context.Coupons.FirstOrDefault(x => x.CouponSymbol == couponSymbol);

                // Checking if any such record exist  
                if (data != null)
                {
                    data.Active = model.Active;
                    context.SaveChanges();

                    // It will redirect to  
                    // the Read method 
                    return RedirectToAction("Update");
                }
                else
                    return View();
            }
        }

        public List<Coupon> GetAllCoupons()
        {
            List<Coupon> lst = new List<Coupon>();
            lst = db.Coupons.ToList<Coupon>();

            return lst;
        }
        //assume there is always 1 and only 1 active coupon
        public string GetActiveCoupon()
        {
            string activeCoupon = string.Empty;
            Coupon coupon = null;
            try
            {
                if (db.Coupons.Where(item => item.Active) != null)
                {
                    coupon = db.Coupons.Where(item => item.Active).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            if (coupon != null)
            {
                activeCoupon = coupon.CouponSymbol;
            }
            return activeCoupon;
       
        }
    }
}