using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DiscountOptionDataWeb.DAL;
using DiscountOptionDataWeb.Models;

using System.Threading.Tasks;
using Stripe;
using System.Text;
using log4net;
using System.Configuration;
using DiscountOptionDataWeb.Classes;

namespace DiscountOptionDataWeb.Controllers
{

    [Authorize]
    public class CheckoutController : Controller
    {

        private OptionDataCenterContext db = new OptionDataCenterContext();


        private static readonly ILog logger =
     log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        string _ipAddress = Utility.GetIPAddress();

        bool isAdminEmailOn = bool.Parse(ConfigurationManager.AppSettings["AdminEmailOn"].ToString());
        string adminEmailAddress = ConfigurationManager.AppSettings["AdminEmailAddress"].ToString();

        public ActionResult AddressAndPayment()
        {
            logger.Info("viewing on Checkout/AddressAndPayment: " + _ipAddress);
            Utility.InsertUserIPAddress(_ipAddress, "Checkout/AddressAndPayment", "", "starting");
            return View();
        }

        /// <summary>
        /// lnh
        /// </summary>
        /// <returns></returns>
        public ActionResult AddressAndPaymentAN()
        {
            return View();
        }


        public ActionResult AddressAndPaymentTest()
        {
            return View();
        }

        [HttpPost]
        //public ActionResult AddressAndPayment(FormCollection values)
        public async Task<ActionResult> AddressAndPayment(CustomerOrder customerOrder)
        {
            if (!ModelState.IsValid)
            {
                return View(customerOrder);
            }

            var order = new CustomerOrder();
            ////what happens when uncomments, what's the below for?
            //TryUpdateModel(order);

            try
            {
                //***test stuff
                //int a = 1;
                //int b = 0;
                //int c = a / b;

                //the sequence is add order to db, charge to stripe, share to google drive if available
                //what what happens if something fails before charge? transactional issue?

                //lnh:10/05/2018
                //test kicking out people
                string blockedEmails = System.Configuration.ConfigurationManager.AppSettings["BlockedEmails"].ToString();
                string[] strArr = blockedEmails.Split(',');

                for (int i = 0; i < strArr.Length; i++)
                {
                    if (User.Identity.Name.ToLower().Contains(strArr[i]))
                    {
                        return View(customerOrder);
                    }
                }

                order.CustomerUserName = User.Identity.Name;
                    order.DateCreated = DateTime.Now;
                
                    order.Address = customerOrder.Address;
                    order.Amount = customerOrder.Amount;
                    order.City = customerOrder.City;
                    order.Country = customerOrder.Country;
                    order.Email = User.Identity.Name; // customerOrder.Email;
                    order.FirstName = customerOrder.FirstName;
                    order.LastName = customerOrder.LastName;
                    order.PostalCode = customerOrder.PostalCode;

                    order.State = customerOrder.State; //lnh***: didn't have this before and it worked?
                    order.Token = customerOrder.Token;//lnh: didn't have this before and it worked?

                //phone is optional
                //order.Phone = "203222555"; // customerOrder.Phone;
                //lnh
                //hacking here, check if email contains Gmail then default
                //gmail address to it
                if (User.Identity.Name.Contains("gmail"))
                    {
                        order.GmailAddress = User.Identity.Name;
                    }
                    else
                    {
                        order.GmailAddress = customerOrder.GmailAddress;
                    }
                    
                    order.ShipToHome = customerOrder.ShipToHome;
                    
                    

                    logger.Info("before adding order 123: " + _ipAddress);
                    db.CustomerOrders.Add(order);
                logger.Info("after adding saving order 123: " + _ipAddress);

                db.SaveChanges();
                    logger.Info("after saving order 123");
                    //order's amount is blank before the cart transfer update here
                    var cart = ShoppingCart.GetCart(this.HttpContext);

                
                logger.Info("before creating order for the cart, Amount is still blank " + _ipAddress);
                cart.CreateOrder(order);
                logger.Info("after creating order for the cart!!! " + _ipAddress);

                //lnh:11/1/2020
                order.FinalAmount = order.Amount;

                string activeCoupon = new CouponController().GetActiveCoupon();

                //lets assume coupons of 5%, 10%, 15%,20%
                string coupon = customerOrder.Coupon;

                if (activeCoupon != String.Empty)
                {
                    switch (coupon)
                    {
                        case "5PER":
                            customerOrder.DiscountPercent = 5;
                            break;
                        case "10PER":
                            customerOrder.DiscountPercent = 10;
                            break;
                        case "15PER":
                            customerOrder.DiscountPercent = 15;
                            break;
                        case "20PER":
                            customerOrder.DiscountPercent = 20;
                            break;
                        default:
                             customerOrder.DiscountPercent = 0;
                            break;

                    }
                }
               
                if (customerOrder.DiscountPercent != null)
                {
                    order.FinalAmount = order.Amount - (((decimal)customerOrder.DiscountPercent/100)*order.Amount);
                }
                else
                {
                    order.FinalAmount = order.Amount;
                }
               

                //lnh: if they want ship to home, charge 20 bucks!
                if (order.ShipToHome)
                {
                    decimal shippingCost = Decimal.Parse(ConfigurationManager.AppSettings["ShippingCost"].ToString());
                    order.FinalAmount = order.FinalAmount + shippingCost;
                }

                db.SaveChanges();//we have received the total amount lets update it
                logger.Info("we have received the total amount lets update it. amount order is set now. " + _ipAddress);

                //amount order is set now

            }
            //**** if error encountered above,should exit the code and alert customer????
            catch (System.Data.Entity.Validation.DbEntityValidationException exx)
            {
               foreach(var a in exx.EntityValidationErrors)
                {
                    foreach(var b in a.ValidationErrors)
                    {
                        logger.Error("System.Data.Entity.Validation.DbEntityValidationException: " + b.ErrorMessage  + " " + _ipAddress);
                        logger.Error("System.Data.Entity.Validation.DbEntityValidationException.Property Name: " + " " + b.PropertyName + _ipAddress);

                                                //Console.WriteLine(b.PropertyName);
                                                //Console.WriteLine(b.ErrorMessage);
                    }
                }
            }
            catch(Exception ex)
            {
                logger.Error("Regular: " + ex.Message + " , There is an issue taking your payment.Please check your credit card information and try again. " + _ipAddress);
                ViewBag.PaymentIssue = "There is an issue taking your payment. Please check your credit card information and try again.";
                //return View();
                return View(order);
            }



            //try test:
            //Stripe charge
            //await Charge(customerOrder);
            if (order.GmailAddress == null)
                order.GmailAddress = "";
            logger.Info("before charging to Stripe");
            try
            {
              //functionality to share files are in Charge function
              //
               await Charge(order);
               // await Charge(null);
            }
            catch (Exception ex)
            {
                logger.Error("Regular: " + ex.Message + ".  There is an issue charging to your credit card. Please try again. " + _ipAddress);
                ViewBag.PaymentIssue = ex.Message +  " There is an issue charging to your credit card. Please try again.";
               // return View();
                return View(order);
            }
            logger.Info("after charging to Stripe " + _ipAddress);

            //* lnh: need to check logic to see if everything is fine before sending email
            //user who failed payment might have got confirmation email too!
            //send confirmation email
            //*** that's OK because if the charge fails the catch return the View() without reaching the SendOrderConformationEmail()
            //12/31/2017: one big order all data was placed, google drive failed to share all years
            //did get to this section, fix implemented
            logger.Info("before SendOrderConfirmationEmail " + _ipAddress);
            SendOrderConfirmationEmail(order.Id);
            logger.Info("after SendOrderConfirmationEmail" + _ipAddress);

            //lnh: empty shopping cart move here?
            var cart2 = ShoppingCart.GetCart(this.HttpContext);
            cart2.EmptyCart();

            logger.Info("done paying on Checkout/AddressAndPayment: " + _ipAddress);
            Utility.InsertUserIPAddress(_ipAddress, "Checkout/AddressAndPayment", "Pay Final", "done paying");

            return RedirectToAction("Complete", new { id = order.Id });
        }

        /// <summary>
        /// not being used?
        /// </summary>
        /// <returns></returns>
        public ActionResult Charge()
        {
            ViewBag.Message = "Learn how to process payments with Stripe";

            return View(new DiscountOptionDataWeb.Models.StripeChargeModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Charge(CustomerOrder model)
        {
           
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            //is it sometimes Payment is OK but chargeId is empty string?
            //live: order 55 come through (with fraud alert) but PaymentCharged field is still 0

            string chargeId = await ProcessPayment(model);
            logger.Info("chargeId: " + chargeId.ToString() + " for orderId: " + model.Id.ToString());

            if (chargeId.ToString().Length > 0)
            {
                try
                {
                    //charged is sucessful, update database table customer order
                    model.PaymentCharged = true;
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    logger.Fatal("Can't update payment charged: " +ex.Message + " " + _ipAddress);
                }
            }


            //lnh:call I call action on GoogleDriveController ?
            //12/24/2017: need  to check Payment Charged too, that Bolivia got shared all years!
            //need to do try catch. 
            //on late night 12/31/2017: an all years order happened, failed on sharing on some files
            //the email confirmation email section is not reached
            if ( model.GmailAddress.Trim().Length > 0 && model.PaymentCharged == true)
            {
                logger.Info("before GoogleDriveController().ShareFiles " + _ipAddress);
                Utility.InsertUserIPAddress(_ipAddress, "before GoogleDriveController().ShareFiles", "Checkout", "Charge function");
                try
                {
                    new GoogleDriveController().ShareFiles(HttpContext.User.Identity.Name, Server.MapPath("../Content/"), model.GmailAddress);
                }
                catch (Exception ex)
                {
                    Utility.InsertUserIPAddress(_ipAddress, "after GoogleDriveController().ShareFiles", "Checkout", ex.Message);
                    logger.Fatal("GoogleDriveController().ShareFiles " + ex.Message +  _ipAddress);
                }
                logger.Info("after GoogleDriveController().ShareFiles " + _ipAddress);
                Utility.InsertUserIPAddress(_ipAddress, "after GoogleDriveController().ShareFiles", "Checkout", "Charge function");
            }

            return View(model); //try this: 12/25/2017: this is OK
            //return View("Index");
        }

        

        private async Task<string> ProcessPayment(CustomerOrder model)
        {
            return await Task.Run(() =>
            {
                var myCharge = new StripeChargeCreateOptions
                {
                    // convert the amount of £12.50 to pennies i.e. 1250
                    Amount = (int)(model.FinalAmount * 100),
                    Currency = "usd",
                    Description = "Charging " + (model.FinalAmount).ToString() + " for " + model.Email.ToString(),
                    SourceTokenOrExistingSourceId =  model.Token
                };
                string stripeSecretKey = ConfigurationManager.AppSettings["StripeSecretKey"].ToString();
                // var chargeService = new StripeChargeService("sk_test_fOwZnZUgJG2MfKsNTjD10bEm");
                //discountoptiondata account
                //var chargeService = new StripeChargeService("sk_test_AN5z8Qd9kHctGi2ZoWjuI1zM");
                //live: prod
                // var chargeService = new StripeChargeService("sk_live_d1wbxjhEvJF4892TSpDmQufq");

                var chargeService = new StripeChargeService(stripeSecretKey);

                var stripeCharge = chargeService.Create(myCharge);
                return stripeCharge.Id;

                //StripeCharge stripeCharge = null;
                //try
                //{
                //    stripeCharge = chargeService.Create(myCharge);
                //}
                //catch (Exception ex)
                //{
                //    throw ex;
                //}

                //////send email 
                ////SendOrderConfirmationEmail(customerOrder);

                //if (stripeCharge != null)
                //    return stripeCharge.Id;
                //else
                //    return "";
            });
        }

        /// <summary>
        /// lnh
        /// </summary>
        /// <param name="model"></param>
        private  void SendOrderConfirmationEmail(int orderID)
        {
            
         
            var model = db.CustomerOrders.SingleOrDefault(oo => oo.Id == orderID &&
         oo.CustomerUserName == User.Identity.Name);

            var mOP = db.Orderedproducts.Where(op => op.CustomerOrderId == orderID).ToList<OrderedProduct>();


            string fromEmail =  "sales@discountoptiondata.com"; //"discountoptiondata@gmail.com";//
            List<string> toList1 = new List<string>();
            List<string> toList2 = new List<string>();
            toList1.Add(model.Email);
            //send same email to admin if checked
            if (isAdminEmailOn)
                toList2.Add(adminEmailAddress);

            //act like we have a lot of orders, so start at 1300 for example
            int orderIDInit = Int32.Parse(System.Configuration.ConfigurationManager.AppSettings["OrderIDInit"].ToString());
            //string subject = "Order Confirmation for Order ID: " + (orderIDInit + orderID).ToString();
            string subject = "Order Confirmation for Order ID: " + (orderID).ToString() + " for " + fromEmail;


            decimal totalAmount = 0;
            StringBuilder sb = new StringBuilder();
            sb.Append("Thank you for your order from <a href='www.discountoptiondata.com'>DiscountOptiondata</a><br/>");

            sb.Append("<h3>Below is your order summary</h3>");

            sb.Append("<table style='width:50%'><tr><th style='text-align: left;'>Product Name</th><th style='text-align: left;'>Amount</th></tr>");

           

            foreach (OrderedProduct od in mOP)
            {
                totalAmount = totalAmount + (decimal)od.CustomerOrder.FinalAmount;
                sb.Append("<tr><td style='text-align: left;'>" + od.Product.Name + "</td>");
                sb.Append("<td style='text-align: left;'>$" + od.Product.Price.ToString() + "</td></tr>");
            }
            sb.Append("</table>");


            sb.Append("<p></p>");
            sb.Append("<table><tr><th>Total Amount</th></tr>");
            sb.Append("<tr><td>$" + model.FinalAmount + "</td></tr>");
            sb.Append("</table>");


            //download instruction: always available 
            sb.Append("<p></p>");
            //sb.Append("</br>To download for your files from <a href='www.discountoptiondata.com'>discountoptiondata.com</a>. Please click the following link <a href='www.discountoptiondata.com/FileDelivery/Index'>www.discountoptiondata.com/FileDelivery/Index</a>");
            //google drive
            if (model.GmailAddress != null && model.GmailAddress.Length > 0)
            {
                sb.Append("<p></p>");
                sb.Append("</br>To get your files from Google Drive, please go to <a href='https://www.google.com/drive/' target='_blank''>https://www.google.com/drive/</a>. Please allow up to 24 hours for the files to be available.");
            }

            //ship to home
            if (model.ShipToHome)
            {
                sb.Append("<p></p>");
                sb.Append("Your order (Flash Drive) will be shipped to:</br>");
                sb.Append("<table><tr><td>" + model.FirstName + " " + model.LastName + "</td></tr>");
                sb.Append("<tr><td>" + model.Address + "</td></tr>");
                sb.Append("<tr><td>" + model.City + ", " + model.State + " " + model.PostalCode + "</td></tr>");

                sb.Append("</table>");
            }

            string messageBody = sb.ToString();
            logger.Info("before OptionsDataService.EmailService.SendSimpleMessage");
            //mailgun has issues sending to hotmail, etc...use gmail for now
            //11/18/2017
            //OptionsDataService.EmailService.SendSimpleMessage(fromEmail, toList, subject, messageBody);
            //OptionsDataService.EmailService.SendGmailMessage(fromEmail, toList, subject, messageBody);
            //06/12/2022: gmail doesnot work anymore so using SendGrid
            OptionsDataService.EmailService.SendGridEmailMessage(fromEmail, toList1, subject, messageBody).Wait(500);
            OptionsDataService.EmailService.SendGridEmailMessage(fromEmail, toList2, subject, messageBody).Wait(500);
            logger.Info("after OptionsDataService.EmailService.SendSimpleMessage");

        }

        /// <summary>
        /// lnh
        /// </summary>
        /// <param name="model"></param>
        private void SendOrderConfirmationEmail(CustomerOrder model)
        {
            string fromEmail = "sales@discountoptiondata.com"; //"discountoptiondata@gmail.com";//
            List<string> toList = new List<string>();
            toList.Add(model.Email);
            //send same email to admin if checked
            if (isAdminEmailOn)
                toList.Add(adminEmailAddress);

            //act like we have a lot of orders, so start at 1301
            //DBCC checkident ('CustomerOrders', reseed, 1300)
            string subject = "Order Confirmation for Order ID: " + (model.Id).ToString();

     
            decimal totalAmount = 0;
            StringBuilder sb = new StringBuilder();
            sb.Append("Thank you for your order from <a href='www.discountoptiondata.com'>DiscountOptiondata</a><br/>");
           
            sb.Append("<h3>Below is your order summary</h3>");
           
            sb.Append("<table style='width:50%'><tr><th style='text-align: left;'>Product Name</th><th style='text-align: left;'>Amount</th></tr>");

            foreach (OrderedProduct od in model.OrderProducts)
            {
                totalAmount = totalAmount + (decimal)od.CustomerOrder.FinalAmount;
                sb.Append("<tr><td style='text-align: left;'>" + od.Product.Name + "</td>");
                sb.Append("<td style='text-align: left;'>$" + od.Product.Price.ToString() + "</td></tr>");
            }
            sb.Append("</table>");
          
          
            sb.Append("<p></p>");
            sb.Append("<table><tr><th>Total Amount</th></tr>");
            sb.Append("<tr><td>$" + model.FinalAmount + "</td></tr>");
            sb.Append("</table>");


            //download instruction
            sb.Append("<p></p>");
           // sb.Append("</br>To download for your files from <a href='www.discountoptiondata.com'>discountoptiondata.com</a>. Please click the following link<a href='www.discountoptiondata.com/FileDelivery/Index'>www.discountoptiondata.com/FileDelivery/Index</a>");
            //google drive
            if (model.GmailAddress != null && model.GmailAddress.Length > 0)
            {
                sb.Append("<p></p>");
                sb.Append("</br>To get your files from Google Drive, please go to <a href='https://www.google.com/drive/' target='_blank''>https://www.google.com/drive/</a>");
            }
            string messageBody = sb.ToString();

            //11/18/2017
            //OptionsDataService.EmailService.SendSimpleMessage(fromEmail, toList, subject, messageBody);
            //OptionsDataService.EmailService.SendGmailMessage(fromEmail, toList, subject, messageBody);
            //06/12/2022: gmail doesnot work anymore so using SendGrid
            OptionsDataService.EmailService.SendGridEmailMessage(fromEmail, toList, subject, messageBody).Wait();


        }

        public ActionResult Complete(int id)
        {
            bool isValid = db.CustomerOrders.Any(
                o => o.Id == id &&
                     o.CustomerUserName == User.Identity.Name
                );

            //lnh:
            var customerOrder = db.CustomerOrders.SingleOrDefault(oo => oo.Id == id &&
            oo.CustomerUserName == User.Identity.Name

           );

            logger.Info("on Checkout/Complete");

            if (isValid)
            {

                ////send email 
                //SendOrderConfirmationEmail(customerOrder);
                //return View(id);
                return View(customerOrder);
            }
            else
            {
                return View("Error");
            }
        }

        // GET: CustomerAddress
        [ChildActionOnly]
        public ActionResult CustomerAddress()
        {
            //assume shipping address is billing address as filled out on PaymentAddress page
            var model = db.CustomerOrders.Where(o =>
                    o.CustomerUserName == User.Identity.Name
                ).OrderByDescending(i =>i.DateCreated).Take(1); //use one address only
            //either of the below would  work
           // return View(model);
            return PartialView(model);
        }


    }
}