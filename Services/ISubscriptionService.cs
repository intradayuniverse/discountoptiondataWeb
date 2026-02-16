using System;
namespace DiscountOptionDataWeb.Services
{
    public interface ISubscriptionService
    {
        void Create(string userName, DiscountOptionDataWeb.Data.Plan plan, string stripeToken);
        Stripe.StripeCustomerService StripeCustomerService { get; }
        Stripe.StripeSubscriptionService StripeSubscriptionService { get; }
        DiscountOptionDataWeb.ApplicationUserManager UserManager { get; }

        void Cancel(string userName);
        void Cancel(string userName, int planID);
    }
}
