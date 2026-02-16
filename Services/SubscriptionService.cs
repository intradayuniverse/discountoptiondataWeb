using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Stripe;
using DiscountOptionDataWeb;
using DiscountOptionDataWeb.Data;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;


namespace DiscountOptionDataWeb.Services
{
    public class SubscriptionService : DiscountOptionDataWeb.Services.ISubscriptionService
    {
        private ApplicationUserManager userManager;
        public ApplicationUserManager UserManager
        {
            get
            {
                return userManager ?? HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                userManager = value;
            }
        }

        private StripeCustomerService customerService;
        public StripeCustomerService StripeCustomerService
        {
            get
            {
                return customerService ?? new StripeCustomerService();
            }
            private set
            {
                customerService = value;
            }
        }

        private StripeSubscriptionService subscriptionService;
        public StripeSubscriptionService StripeSubscriptionService
        {
            get
            {
                return subscriptionService ?? new StripeSubscriptionService();
            }
            private set
            {
                subscriptionService = value;
            }
        }

        public SubscriptionService()
        {

        }

        public SubscriptionService(ApplicationUserManager userManager, StripeCustomerService customerService, StripeSubscriptionService subscriptionService)
        {
            this.userManager = userManager;
            this.customerService = customerService;
            this.subscriptionService = subscriptionService;
        }

        public void Create(string userName, Plan plan, string stripeToken)
        {
            var user = UserManager.FindByName(userName);
            
            if (String.IsNullOrEmpty(user.StripeCustomerId))  //first time customer
            {
                //create customer which will create subscription if plan is set and cc info via token is provided
                //var customer = new StripeCustomerCreateOptions()
                //{
                //    Email = user.Email,
                //    Source = new StripeSourceOptions() { TokenId = stripeToken },
                //    PlanId = plan.ExternalId //externalid is stripe plan.id
                //};

                var customer = new StripeCustomerCreateOptions()
                {
                    Email = user.Email,
                     SourceToken = stripeToken ,
                    PlanId = plan.ExternalId //externalid is stripe plan.id
                    
                };

                StripeCustomer stripeCustomer = StripeCustomerService.Create(customer);
                user.StripeCustomerId = stripeCustomer.Id;
                user.ActiveUntil = DateTime.Now.AddDays((double)plan.TrialPeriodDays);
                UserManager.Update(user);

            }
            else //existing customer
            {
                var stripeSubscription = StripeSubscriptionService.Create(user.StripeCustomerId, plan.ExternalId);
                user.ActiveUntil = DateTime.Now.AddDays((double)plan.TrialPeriodDays);
                UserManager.Update(user);
            }

        }

        /// <summary>
        ///lnh: for now cancel all subscriptions of the user (email)
        /// later we might have different plans/services then need to handle those cases
        /// </summary>
        /// <param name="userName"></param>
        public void Cancel(string userName)
        { 
            var user = UserManager.FindByName(userName);
            string stripeCustomerID = user.StripeCustomerId;

            IEnumerable<StripeSubscription> stripeSSList = 
                StripeSubscriptionService.List().Where(item => item.CustomerId == stripeCustomerID);

            //cancel all subscriptions for user immediately on Stripe
            foreach (StripeSubscription ss in stripeSSList)
            {
                StripeSubscriptionService.Cancel(ss.Id, false, null);
            }

            //update local db's StripeCustomerId to null
            user.StripeCustomerId = null;
            UserManager.Update(user);

        }

        //for the future if we have other memberships (so with other plans)
        public void Cancel(string userName, int planID)
        {
            throw new NotImplementedException();
        }

    }
}