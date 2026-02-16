using DiscountOptionDataWeb.DAL;
using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace DiscountOptionDataWeb.Classes
{
    public class Utility
    {
        private static readonly ILog logger =
    log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public static string GetIPAddress()
        {
            System.Web.HttpContext context = System.Web.HttpContext.Current;
            string ipAddress = context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (!string.IsNullOrEmpty(ipAddress))
            {
                string[] addresses = ipAddress.Split(',');
                if (addresses.Length != 0)
                {
                    return addresses[0];
                }
            }

            return context.Request.ServerVariables["REMOTE_ADDR"];
        }

        public static void InsertUserIPAddress(string userIPAddress, string page  , string buttonLink, string description)
        {
            using (OptionDataCenterContext db = new OptionDataCenterContext())
            {
                var pUserIPAddress = new SqlParameter
                {
                    ParameterName = "@IPAddress",
                    SqlDbType = SqlDbType.NVarChar,
                    Direction = ParameterDirection.Input,
                    Value = userIPAddress
                };
                var pPage = new SqlParameter
                {
                    ParameterName = "@Page",
                    SqlDbType = SqlDbType.VarChar,
                    Direction = ParameterDirection.Input,
                    Value = page
                };
                var pButtonLink = new SqlParameter
                {
                    ParameterName = "@ButtonLink",
                    SqlDbType = SqlDbType.VarChar,
                    Direction = ParameterDirection.Input,
                    Value = buttonLink
                };
                var pDescription = new SqlParameter
                {
                    ParameterName = "@Description",
                    SqlDbType = SqlDbType.VarChar,
                    Direction = ParameterDirection.Input,
                    Value = description
                };

                var sqlString = "EXEC [dbo].[sp_InsertUserIPAddress] @IPAddress, @Page, @ButtonLink, @Description";

                try
                {
                    db.Database.ExecuteSqlCommand(sqlString,
                        pUserIPAddress, pPage, pButtonLink, pDescription);
                }
                catch (Exception ex)
                {
                    logger.Fatal(ex.Message);
                }


            }

    }
}
}