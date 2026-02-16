using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DiscountOptionDataWeb.DAL;
using DiscountOptionDataWeb.Models;

using System.Threading.Tasks;
using log4net;

namespace DiscountOptionDataWeb.Controllers
{
    [Authorize]
    public class CustomerOrdersController : Controller
    {
        private OptionDataCenterContext db = new OptionDataCenterContext();


        private static readonly ILog logger =
     log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // GET: CustomerOrders
        public ActionResult Index()
        {
            //assume shipping address is billing address as filled out on PaymentAddress page
            var model = db.CustomerOrders.Where(o =>
                    o.CustomerUserName == User.Identity.Name
                ).Where(p => p.PaymentCharged == true).OrderByDescending(ii => ii.DateCreated).ToList<CustomerOrder>();
            return View(model);
        }
    }
}