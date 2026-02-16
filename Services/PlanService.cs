using DiscountOptionDataWeb.Data;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DiscountOptionDataWeb.Services
{
    public class PlanService : DiscountOptionDataWeb.Services.IPlanService
    {
        private IPaymentsModel db;
        private StripePlanService stripePlanService;

        public PlanService(IPaymentsModel paymentsModel, StripePlanService stripePlanService)
        {
            this.db = paymentsModel;
            this.stripePlanService = stripePlanService;
        }

        public PlanService()
            : this(new PaymentsModel(), new StripePlanService())
        {
          
        }

        public Plan Find(int id)
        {
            var plan = (from p in db.Plans.Include("Features")
                        where p.Id == id
                        select p).SingleOrDefault();

            var stripePlan = stripePlanService.Get(plan.ExternalId);
            StripePlanToPlan(stripePlan, plan);

            return plan;

        }


        public IList<Plan> List() {
            var plans = (from p in db.Plans.Include("Features").Where(item => item.IsActive == true)
                         orderby p.DisplayOrder
                         select p).ToList();
            
            var stripePlans = (from p in stripePlanService.List() select p).ToList();
            foreach (var plan in plans)
            {
                var stripePlan = stripePlans.Single(p => p.Id == plan.ExternalId);
                StripePlanToPlan(stripePlan, plan);
            }

            return plans;
        }

        private static void StripePlanToPlan(StripePlan stripePlan, Plan plan)
        {
            plan.Name = stripePlan.Name;
            plan.AmountInCents = stripePlan.Amount;
            plan.Currency = stripePlan.Currency;
            if (stripePlan.IntervalCount > 1) //03/29/18
            {
                plan.Interval = stripePlan.Interval + "s";
            }
            else
            {
                plan.Interval = stripePlan.Interval;
            }
           
            plan.IntervalCount = stripePlan.IntervalCount;//lnh: new 03/29/18
            plan.TrialPeriodDays = stripePlan.TrialPeriodDays;
        }


    }
}