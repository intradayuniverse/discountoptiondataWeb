using System;
namespace DiscountOptionDataWeb.Services
{
    public interface IPlanService
    {
        DiscountOptionDataWeb.Data.Plan Find(int id);
        System.Collections.Generic.IList<DiscountOptionDataWeb.Data.Plan> List();
    }
}
