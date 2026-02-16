using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DiscountOptionDataWeb.Entities;
using DiscountOptionDataWeb.Abstract;
using System.Configuration;
using System.Data.SqlClient;

namespace DiscountOptionDataWeb.Concrete
{
    public class SQLServerOptionDataRepository : IOptionDataRepository
    {
        public IEnumerable<string> GetExpirationList(string symbol, string dataDate)
        {
            //          SELECT distinct ExpirationDate
            //FROM[HDOptionsData].[dbo].[options]
            //      Where Symbol = 'spx' and DataDate = '2017-10-20'

            List<string> lst = new List<string>();

            //db has different format for xdate:20171011, eg
            //  expirationDate = expirationDate.Replace("-", "");

            //
            // You need to access the project's connection string here.
            //
            string connectionString = ConfigurationManager.ConnectionStrings["TradeKingConn"].ToString();
            string sql = "SELECT  xdate FROM tblOptionChainFree WHERE undersymbol='" + symbol + "'" +
                " AND dataDate='" + dataDate + "' ORDER BY CAST(xdate AS Date) ASC";
            //
            // Create new SqlConnection object.
            //
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                //
                // Create new SqlCommand object.
                //
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    //
                    // Invoke ExecuteReader method.
                    //
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {

                        string expirationDate = reader.GetString(reader.GetOrdinal("xdate"));

                        lst.Add(expirationDate);
                    }
                }
            }



            return lst.Distinct();
        }

        /// <summary>
        /// this is to get data from TradeKingOptions data
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="dataDate"></param>
        /// <param name="expirationDate"></param>
        /// <returns></returns>
        //public IQueryable<OptionData> GetOptionDataList(string symbol = "", string dataDate = "", string expirationDate = "")
        //{
        //    List<OptionData> lst = new List<OptionData>();

        //    //db has different format for xdate:20171011, eg
        //    expirationDate = expirationDate.Replace("-", "");

        //    //
        //    // You need to access the project's connection string here.
        //    //
        //    string connectionString = ConfigurationManager.ConnectionStrings["TradeKingConn"].ToString();
        //    string sql = "SELECT * FROM tblOptionChain WHERE undersymbol='" +  symbol + 
        //        "' AND xDate = '" + expirationDate + "'" +
        //        " AND dataDate='" + dataDate + "'";
        //    //
        //    // Create new SqlConnection object.
        //    //
        //    using (SqlConnection connection = new SqlConnection(connectionString))
        //    {
        //        connection.Open();
        //        //
        //        // Create new SqlCommand object.
        //        //
        //        using (SqlCommand command = new SqlCommand(sql, connection))
        //        {
        //            //
        //            // Invoke ExecuteReader method.
        //            //
        //            SqlDataReader reader = command.ExecuteReader();
        //            while (reader.Read())
        //            {
        //                OptionData od = new OptionData();
        //                od.AskPrice = reader.GetString(reader.GetOrdinal("ask"));
        //                od.AskSize = reader.GetString(reader.GetOrdinal("asksz"));
        //                od.BidPrice = reader.GetString(reader.GetOrdinal("bid"));
        //                od.BidSize = reader.GetString(reader.GetOrdinal("bidsz"));
        //                od.LastPrice = reader.GetString(reader.GetOrdinal("last"));
        //                od.PutCall = reader.GetString(reader.GetOrdinal("put_call"));
        //                //od.Exchange = reader.GetString(reader.GetOrdinal("exch_desc"));
        //                od.StrikePrice = reader.GetString(reader.GetOrdinal("strikeprice"));
        //                od.Symbol = reader.GetString(reader.GetOrdinal("undersymbol"));
        //                od.Volume = reader.GetString(reader.GetOrdinal("vl"));
        //                od.ExpirationDate = reader.GetString(reader.GetOrdinal("xdate"));
        //                od.ImpliedVolatility = reader.GetString(reader.GetOrdinal("imp_volatility"));
        //                od.Delta = reader.GetString(reader.GetOrdinal("idelta"));
        //                od.Gamma = reader.GetString(reader.GetOrdinal("igamma"));
        //                od.Vega = reader.GetString(reader.GetOrdinal("ivega"));
        //                od.Rho = reader.GetString(reader.GetOrdinal("irho"));
        //                od.OpenInterest = reader.GetString(reader.GetOrdinal("openinterest"));
        //                od.UnderlyingPrice = reader.GetString(reader.GetOrdinal("UnderlyingPrice"));
        //                od.DataDate = reader.GetDateTime(reader.GetOrdinal("DataDate")).ToString();
        //                lst.Add(od);
        //            }
        //        }
        //    }



        //    return lst.AsQueryable();
        //}

        //this is to get data from deltaneutral (historicaloptiondata.com)
        public IEnumerable<OptionData> GetOptionDataList(string symbol = "", string dataDate = "", string expirationDate = "")
        {
            List<OptionData> lst = new List<OptionData>();

            //db has different format for xdate:20171011, eg
            //  expirationDate = expirationDate.Replace("-", "");

            //
            // You need to access the project's connection string here.
            //
            string connectionString = ConfigurationManager.ConnectionStrings["TradeKingConn"].ToString();
            string sql = "SELECT * FROM tblOptionChainFree WHERE undersymbol='" + symbol +
                "' AND xdate = '" + expirationDate + "'" +
                " AND dataDate='" + dataDate + "' ORDER BY put_call ASC";
            //
            // Create new SqlConnection object.
            //
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                //
                // Create new SqlCommand object.
                //
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    //
                    // Invoke ExecuteReader method.
                    //
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        OptionData od = new OptionData();
                        od.AskPrice = reader.GetString(reader.GetOrdinal("ask"));
                        od.AskSize =  reader.GetString(reader.GetOrdinal("asksz"));
                        od.BidPrice = reader.GetString(reader.GetOrdinal("bid"));
                        od.BidSize =  reader.GetString(reader.GetOrdinal("bidsz"));
                        od.LastPrice = reader.GetString(reader.GetOrdinal("last"));
                        od.PutCall = reader.GetString(reader.GetOrdinal("put_call"));
                        //od.Exchange = reader.GetString(reader.GetOrdinal("exch_desc"));
                        od.StrikePrice = reader.GetString(reader.GetOrdinal("strikeprice"));
                        od.Symbol = reader.GetString(reader.GetOrdinal("undersymbol"));
                        od.Volume = reader.GetString(reader.GetOrdinal("vl"));
                        od.ExpirationDate = reader.GetString(reader.GetOrdinal("xdate"));
                        od.ImpliedVolatility = reader.GetString(reader.GetOrdinal("imp_volatility"));
                        od.Delta = reader.GetString(reader.GetOrdinal("idelta"));
                        od.Gamma = reader.GetString(reader.GetOrdinal("igamma"));
                        od.Vega = reader.GetString(reader.GetOrdinal("ivega"));
                        od.Rho =  reader.GetString(reader.GetOrdinal("irho"));
                        od.OpenInterest = reader.GetString(reader.GetOrdinal("openinterest"));
                        od.UnderlyingPrice = reader.GetString(reader.GetOrdinal("UnderlyingPrice"));
                        od.DataDate = reader.GetOrdinal("DataDate").ToString();
                        lst.Add(od);
                    }
                }
            }



            return lst;
        }
    }
}