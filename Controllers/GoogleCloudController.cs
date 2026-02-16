using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;



namespace DiscountOptionDataWeb.Controllers
{
    public class GoogleCloudController : Controller
    {
        // GET: GoogleCloud
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Download()
        {

            //// //download a physical file
            string sFileName = "https://storage.googleapis.com/luanbucket2/my-file2.txt"; //doesn't work bc not a virtual file
            //string sFileName = "/Content2/" + "test.zip";
            return File(sFileName, "application/octet-stream", sFileName);

            
        }
    }
}