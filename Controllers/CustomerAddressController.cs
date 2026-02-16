using log4net;
using DiscountOptionDataWeb.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DiscountOptionDataWeb.Controllers
{
    /// <summary>
    /// not being used, same CustomerAddress() method is used in CheckoutController class
    /// </summary>
    [Authorize]
    public class CustomerAddressController : Controller
    {

        private OptionDataCenterContext db = new OptionDataCenterContext();


        private static readonly ILog logger =
     log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        // GET: CustomerAddress
        [ChildActionOnly]
        public ActionResult CustomerAddress()
        {
            //assume shipping address is billing address as filled out on PaymentAddress page
            var model = db.CustomerOrders.Where(o =>
                    o.CustomerUserName == User.Identity.Name
                ).Take(1);
            return View(model);
        }
    }
}