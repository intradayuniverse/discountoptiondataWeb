using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DiscountOptionDataWeb.Entities;
using DiscountOptionDataWeb.Abstract;
using System.Configuration;
using System.Data.SqlClient;
using Google.Cloud.BigQuery.V2;

namespace DiscountOptionDataWeb.Concrete
{
    public class GoogleBigQueryOptionDataRepository : IOptionDataRepository
    {
        public IEnumerable<string> GetExpirationList(string symbol, string dataDate)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<OptionData> GetOptionDataList(string symbol = "", string dataDate = "", string expirationDate = "")
        {


            List<OptionData> lst = new List<OptionData>();
            //string projectID = "dodlive-176117";
            //string dataSetName = "OptionDataDS";
            //string tableName = "OptionDataDTTest";

           

            //var client = BigQueryClient.Create(projectID);
            //var table = client.GetTable(projectID, dataSetName, tableName);

            //string query = $@"select *  FROM `{table.FullyQualifiedId}` WHERE symbol='" + symbol +
            //    "' AND ExpirationDate = '" + expirationDate + "' AND dataDate='" + dataDate + "'";



            ////        var table = client.GetTable("bigquery-public-data", "samples", "shakespeare");

            ////        string query = $@"SELECT corpus AS title, COUNT(*) AS unique_words FROM `{table.FullyQualifiedId}` 
            ////GROUP BY title ORDER BY unique_words DESC LIMIT 42";
            ////        var result = client.ExecuteQuery(query);

            //ExecuteQueryOptions eo = new ExecuteQueryOptions();
            //eo.UseQueryCache = true;
            //var result = client.ExecuteQuery(query, eo);

            ////Console.Write("\nQuery Results:\n------------\n");
            ////foreach (var row in result.GetRows())
            ////{
            ////    Console.WriteLine($"{row["title"]}: {row["unique_words"]}");
            ////}

            //foreach (var row in result.GetRows())
            //{

            //    OptionData od = new OptionData();
            //    od.AskPrice = $"{row["AskPrice"]}";
            //    od.AskSize = $"{row["AskSize"]}";
            //    od.BidPrice = $"{row["BidPrice"]}";
            //    od.BidPrice = $"{row["BidPrice"]}";
            //    od.LastPrice = $"{row["LastPrice"]}";
            //    od.PutCall = $"{row["PutCall"]}";
            //    //od.Exchange = $"{row["Exchange"]}";
            //    od.StrikePrice = $"{row["StrikePrice"]}";
            //    od.Symbol = $"{row["Symbol"]}";
            //    od.Volume = $"{row["Volume"]}";
            //    od.ExpirationDate = $"{row["ExpirationDate"]}";
            //    od.ImpliedVolatility = $"{row["ImpliedVolatility"]}";
            //    od.Delta = $"{row["Delta"]}";
            //    od.Gamma = $"{row["Gamma"]}";
            //    od.Vega = $"{row["Vega"]}";
            //    od.Rho = $"{row["Rho"]}";
            //    od.OpenInterest = $"{row["OpenInterest"]}";
            //    od.UnderlyingPrice = $"{row["UnderlyingPrice"]}";
            //    od.DataDate = $"{row["DataDate"]}";
            //    lst.Add(od);
            //}

            return lst;
        }
    }
}