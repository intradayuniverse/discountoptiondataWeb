using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DiscountOptionDataWeb.Abstract;
using DiscountOptionDataWeb.Entities;
using DiscountOptionDataWeb.Concrete;
using DiscountOptionDataWeb.DAL;
using DiscountOptionDataWeb.Classes;
using log4net;

namespace DiscountOptionDataWeb.Controllers
{
    public class FreeDataController : Controller
    {
        private IOptionDataRepository repot = null;
        List<OptionData> lstOD = null;
        private OptionDataCenterContext db = new OptionDataCenterContext();
       
        private static readonly ILog logger =
     log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        string _ipAddress = Utility.GetIPAddress();


        public FreeDataController()

        {
           
            repot = new SQLServerOptionDataRepository();
            //repot = new GoogleBigQueryOptionDataRepository();
        }

     

        public PartialViewResult GetDataDetail(string symbol, string dataDate, string expirationDate)
        {
            lstOD = repot.GetOptionDataList(symbol, dataDate, expirationDate).ToList<OptionData>();
            return PartialView(lstOD);

        }

        public JsonResult GetExpirationDates(string symbol, string dataDate)
        {
            IEnumerable<string> data = repot.GetExpirationList(symbol, dataDate);
            return Json(data, JsonRequestBehavior.AllowGet);

        }

        public JsonResult GetOptionDataJson(string symbol, string dataDate, string expirationDate)
        {
            logger.Info("viewing on FreeDataController/GetOptionDataJson: " + _ipAddress);
            Utility.InsertUserIPAddress(_ipAddress, "FreeDataController/GetOptionDataJson", "", "starting");

            IEnumerable<OptionData> data = repot.GetOptionDataList(symbol, dataDate, expirationDate);
            JsonResult jr = Json(data, JsonRequestBehavior.AllowGet);
            return jr;
        }

        public ActionResult Index()
        {
            logger.Info("viewing on FreeDataController/Index: " + _ipAddress);
            Utility.InsertUserIPAddress(_ipAddress, "FreeDataController/Index", "", "starting");
            //create a view bag here
            //Creating generic list

            SelectListItem sel = new SelectListItem();

            List<SelectListItem> ObjList = new List<SelectListItem>()
            {
                new SelectListItem { Text = "spx", Value = "spx",Selected = true  },
                new SelectListItem { Text = "spy", Value = "spy"},
                new SelectListItem { Text = "dia", Value = "dia" },
                new SelectListItem { Text = "rut", Value = "rut" }


            };


            //Assigning generic list to ViewBag
            ViewBag.Symbols = ObjList;

            //
            //lstOD = repot.GetOptionDataList("spx", "2017-06-01", "2017-06-16").ToList<OptionData>();
            //return View(lstOD);
            return View();
        }
    }
}