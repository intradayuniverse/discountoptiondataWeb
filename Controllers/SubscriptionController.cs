using DiscountOptionDataWeb.Models;
using DiscountOptionDataWeb.Services;
using Stripe;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using DiscountOptionDataWeb.Data;
using DiscountOptionDataWeb.DAL;
using DiscountOptionDataWeb.Classes;
using System.Text;

namespace DiscountOptionDataWeb.Controllers
{
    public class SubscriptionController : Controller
    {

     
        private static readonly log4net.ILog logger =
     log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        string _ipAddress = Utility.GetIPAddress();

        bool isAdminEmailOn = bool.Parse(ConfigurationManager.AppSettings["AdminEmailOn"].ToString());
        string adminEmailAddress = ConfigurationManager.AppSettings["AdminEmailAddress"].ToString();

        private IPlanService planService;

        private ISubscriptionService subscriptionService;
        public ISubscriptionService SubscriptionService
        {
            get
            {
                return subscriptionService ?? new SubscriptionService();
            }
            private set
            {
                subscriptionService = value;
            }
        }

        public SubscriptionController(IPlanService planService, ISubscriptionService subscriptionService)
        {
            this.planService = planService;
            this.subscriptionService = subscriptionService;
        }


        public SubscriptionController()
        {
           
        }

        public IPlanService PlanService
        {
            get
            {
                return planService ?? new PlanService();
            }
            private set
            {
                planService = value;
            }
        }


        // GET: Subscription
        public async Task<ActionResult> Index()
        {
            //lnh:04/10/2020
            //if not a stripe, subscription user then go to Plan page to subscribe
            //if a user, redirect to subscription page
            var userM = SubscriptionService.UserManager;
            ApplicationUser user = await userM.FindByNameAsync(User.Identity.Name);
            //if there is StripeCustomerID 
            if (user != null && !String.IsNullOrEmpty(user.StripeCustomerId))
            {
                ViewBag.HaveSubscription = "havesubscription"; // "To get your files from Google Drive, please go to <a href='https://www.google.com/drive/' target='_blank''>https://www.google.com/drive/</a>";

                return View("SubscriptionPage");
            }
          

            var viewModel = new SubIndexViewModel() { Plans = PlanService.List() };
            return View(viewModel);
        }

        [Authorize]
        public async Task<ActionResult> Billing(int productId)
        {
            //if user already subscribed then redirect to SubscriptionFiles
            var userM = SubscriptionService.UserManager;
            ApplicationUser user = await userM.FindByNameAsync(User.Identity.Name);

           
            if (!String.IsNullOrEmpty(user.StripeCustomerId))
            {
                //return RedirectToAction("SubscriptionFiles", "Subscription");
                return RedirectToAction("SubscriptionPage", "Subscription");
            }

            //else display credit card for billing
            else
            {
                string stripePublishableKey = ConfigurationManager.AppSettings["StripePublicKey"]; //"pk_test_FV6Th1IbZs5mYovNyOrxOigs";//
                var viewModel = new BillingViewModel() { Plan = PlanService.Find(productId), StripePublishableKey = stripePublishableKey };
                return View(viewModel);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<ActionResult> Billing(BillingViewModel billingViewModel)
        {
            billingViewModel.Plan = PlanService.Find(billingViewModel.Plan.Id);
            try
            {
                SubscriptionService.Create(User.Identity.Name, billingViewModel.Plan, billingViewModel.StripeToken);

                //get user and send subscription email 
                var userM = SubscriptionService.UserManager;
                ApplicationUser user = await userM.FindByNameAsync(User.Identity.Name);
                SendSubscriptionConfirmationEmail(user.Email, billingViewModel.Plan);

                //share google drive if user has gmail
                if (user.Email.Trim().Length > 0 && user.Email.Contains("gmail"))
                {
                    logger.Info("before GoogleDriveController().ShareFiles " + _ipAddress);
                    Utility.InsertUserIPAddress(_ipAddress, "before GoogleDriveController().SharedGoogleSubscriptionFolder", "Billing", "Billing function");
                    try
                    {
                        new GoogleDriveController().SharedGoogleSubscriptionFolder(Server.MapPath("../../Content/"), user.Email);
                    }
                    catch (Exception ex)
                    {
                        Utility.InsertUserIPAddress(_ipAddress, "after GoogleDriveController().SharedGoogleSubscriptionFolder", "Billing", ex.Message);
                        logger.Fatal("GoogleDriveController().SharedGoogleSubscriptionFolder " + ex.Message + _ipAddress);
                    }
                    logger.Info("after GoogleDriveController().SharedGoogleSubscriptionFolder " + _ipAddress);
                    Utility.InsertUserIPAddress(_ipAddress, "after GoogleDriveController().SharedGoogleSubscriptionFolder", "Billing", "Billing function");

                }
            }
            catch (StripeException stripeException)
            {
                ModelState.AddModelError(string.Empty, stripeException.Message);
                return View(billingViewModel);
            }
            //return RedirectToAction("SubscriptionFiles", "Subscription");
            return RedirectToAction("SubscriptionPage", "Subscription");
        }


        /// <summary>
        /// lnh
        /// </summary>
        /// <param name="model"></param>
        private void SendSubscriptionConfirmationEmail(string userEmail, Plan plan)
        {

          

            string fromEmail =  "sales@discountoptiondata.com"; //"discountoptiondata@gmail.com";//
            List<string> toList = new List<string>();
            toList.Add(userEmail);
            //send same email to admin if checked
            if (isAdminEmailOn)
                toList.Add(adminEmailAddress);

            string subject = "Daily Subscription Order Confirmation for " + userEmail;


            string planPrice = plan.AmountInDollars.ToString() + "/" + plan.IntervalCount.ToString() + " " +  plan.Interval.ToString();
            StringBuilder sb = new StringBuilder();
            sb.Append("Thank you for your order from <a href='www.discountoptiondata.com'>DiscountOptiondata.com</a><br/>");

            sb.Append("<h3>Below is your order summary</h3>");

            sb.Append("<table style='width:50%'><tr><th style='text-align: left;'>Plan Name</th><th style='text-align: left;'>Amount</th></tr>");

            
            sb.Append("<tr><td style='text-align: left;'>" + plan.Name + "</td>");
            sb.Append("<td style='text-align: left;'>$" + planPrice  + "</td></tr>");
            
            sb.Append("</table>");


            sb.Append("<p></p>");
            sb.Append("<table><tr><th>Your Trial Period</th></tr>");
            sb.Append("<tr><td>$" + plan.TrialPeriodDays.ToString() + " days" + "</td></tr>");
            sb.Append("</table>");


            ////download instruction: always available 
            //sb.Append("<p></p>");
            //sb.Append("</br>To download for your files from <a href='www.discountoptiondata.com'>discountoptiondata.com</a>. Please click the following link <a href='https://www.discountoptiondata.com/Subscription/SubscriptionFiles'>www.discountoptiondata.com/Subscription/SubscriptionFiles</a>");
            //google drive
          
                sb.Append("<p></p>");
                sb.Append("</br>To get your files from Google Drive, please go to <a href='https://www.google.com/drive/' target='_blank''>https://www.google.com/drive/</a>. Please allow up to 24 hours for the first delivery.");
            

          
            string messageBody = sb.ToString();
            logger.Info("before OptionsDataService.EmailService.SendSimpleMessage");
            //mailgun has issues sending to hotmail, etc...use gmail for now
            //11/18/2017
            //OptionsDataService.EmailService.SendSimpleMessage(fromEmail, toList, subject, messageBody);
            //OptionsDataService.EmailService.SendGmailMessage(fromEmail, toList, subject, messageBody);
            //06/12/2022: gmail doesnot work anymore so using SendGrid
            OptionsDataService.EmailService.SendGridEmailMessage(fromEmail, toList, subject, messageBody).Wait();
            logger.Info("after OptionsDataService.EmailService.SendSimpleMessage");



        }

        /// <summary>
        /// obsolete
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public  async Task<ViewResult> SubscriptionFiles()
        {
            var userM = SubscriptionService.UserManager;
           ApplicationUser user = await  userM.FindByNameAsync(User.Identity.Name);
            //if there is no StripeCustomerID then user is just a regular customer
            if (String.IsNullOrEmpty(user.StripeCustomerId))
            {
                ViewBag.HaveSubscription = "You do not have subscription";
                return View("NoSubscription");
            }
            else //if there is a stripeCustomerId, then you're a subscriber
            {
                ViewBag.HaveSubscription = "Please download below";
                List<OptionFile> listOptFile = new List<OptionFile>();
                //hard code for testing but should get all files from folder SubscriptionFiles
                //listOptFile.Add(new OptionFile { FileName = "20170604_OData.csv", FileSize = 100, ShortFileName = "20170604_OData.csv" });
                //listOptFile.Add(new OptionFile { FileName = "20170610_OData.csv", FileSize = 100, ShortFileName = "20170610_OData.csv" });

                string subLocation = ConfigurationManager.AppSettings["SubscriptionLocation"].ToString();
               var optionFileArr = GetFilesByPath(Server.MapPath(subLocation));

                return View(optionFileArr);
            }
         
        }

        /// <summary>
        /// lnh:04/04/2020
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public async Task<ViewResult> SubscriptionPage()
        {

            var userM = SubscriptionService.UserManager;
            ApplicationUser user = await userM.FindByNameAsync(User.Identity.Name);
            //if there is no StripeCustomerID then user is just a regular customer
            if (String.IsNullOrEmpty(user.StripeCustomerId))
            {
                ViewBag.HaveSubscription = "nosubscription";
                return View("SubscriptionPage");
            }
            else //if there is a stripeCustomerId, then you're a subscriber
            {
                ViewBag.HaveSubscription = "havesubscription"; // "To get your files from Google Drive, please go to <a href='https://www.google.com/drive/' target='_blank''>https://www.google.com/drive/</a>";
              
                return View("SubscriptionPage");
            }
        }

        //lnh
        public  ViewResult Cancel()
        {
          
            SubscriptionService.Cancel(User.Identity.Name);

            return View();
        }

        public ActionResult TransmitToClient(string fileName)
        {
            //test download/upload speed
            // http://speedtest.xfinity.com/
            string subLocation = ConfigurationManager.AppSettings["SubscriptionLocation"].ToString();
            fileName = Server.MapPath(subLocation) + fileName + ".zip";
            //doing this way to avoid really long name being created on the client's machine
            FileInfo fileInfo = new FileInfo(fileName);

            //Response.ContentType = "text/plain";
            Response.ContentType = "application/zip";
            Response.AddHeader("Content-Disposition", String.Format("attachment;filename=\"{0}\"", fileInfo.Name));
            Response.AddHeader("Content-Length", fileInfo.Length.ToString());
            Response.TransmitFile(fileInfo.FullName);
            Response.End();

            return RedirectToAction("SubscriptionFiles");
        }

        /// <summary>
        /// get all files in a directory
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private OptionFile[] GetFilesByPath(string path)
        {
            List<OptionFile> myList = new List<OptionFile>();
            string[] files = Directory.GetFiles(path);

            OptionFile opFile;

            for (int i = 0; i < files.Length; i++)
            {
                opFile = new OptionFile();
                //damn it, make the file name and short file name the same!
                opFile.FileName = files[i].Substring(files[i].LastIndexOf("\\") + 1);// files[i];// files[i].Substring(files[i].LastIndexOf("\\") + 1);
                opFile.ShortFileName = files[i].Substring(files[i].LastIndexOf("\\") + 1);
                opFile.FileSize = ulong.Parse((new FileInfo(files[i]).Length/1000).ToString());
                
                myList.Add(opFile);
            }

            return myList.ToArray<OptionFile>();
        }
    }
}