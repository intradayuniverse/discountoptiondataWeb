using DiscountOptionDataWeb.DAL;
using DiscountOptionDataWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DiscountOptionDataWeb.Classes;
using Google.Apis.Drive.v3;
using Google.Apis.Auth.OAuth2;
using System.IO;
using System.Threading;
using Google.Apis.Util.Store;
using Google.Apis.Services;
using log4net;
using System.Configuration;

namespace DiscountOptionDataWeb.Controllers
{
    [Authorize]
    public class GoogleDriveController : Controller
    {
        private OptionDataCenterContext db = new OptionDataCenterContext();

        private static readonly ILog logger =
     log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        string _ipAddress = Utility.GetIPAddress();

        // If modifying these scopes, delete your previously saved credentials
        // at ~/.credentials/drive-dotnet-quickstart.json
        static string[] Scopes = { DriveService.Scope.DriveReadonly, DriveService.Scope.DriveFile, DriveService.Scope.Drive };
        static string ApplicationName = "rerererew Drive API .NET Quickstart";


        public ActionResult Index()
        {
            //test stuff

            //ListGDrives();

            return View();
        }
        /// <summary>
        /// used for testing only
        /// </summary>
        private void ListGDrives()
        {
             UserCredential credential;

           // string clientSecretFile = Server.MapPath("../Content/") + "client_secret_5277live.json";

           
            string clientSecretFile = @"C:\TradeKingData\client_secret_5277live.json"; // "client_secret_4209.json"; // "client_secret_2727Synott.json"; //client_secret.json
            logger.Info("clientSecretFile: " + clientSecretFile + "for " + _ipAddress);
            using (var stream =
                new FileStream(clientSecretFile, FileMode.Open, FileAccess.Read))
            {
                string credPath = System.Environment.GetFolderPath(
                    System.Environment.SpecialFolder.Personal);
                credPath = Path.Combine(credPath, ".credentials/drive-dotnet-quickstart.json");

                //string credPath = clientSecretPath + "\\drive-dotnet-quickstart.json";

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
                logger.Info("Credential file saved to: " + credPath);
            }

            // Create Drive API service.
            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            IList<Google.Apis.Drive.v3.Data.File> files =  DaimtoGoogleDriveHelper.GetFiles2(service);

            string path = "C:\\LuanHuynh\\Projects\\DiscountOptionDataV_11192017\\DiscountOptionDataWeb\\Content\\";

            //ShareFiles("luanhuynh2727@gmail.com", path, "luanhuynh2727@gmail.com");

            string fileID = DaimtoGoogleDriveHelper.GetFileIdOfFileFolder(service, "liveNoGreeks2018");
            DaimtoGoogleDriveHelper.ShareFileID(service, fileID, "luanhuynh2727@gmail.com");
            Console.ReadLine();
        }

       /// <summary>
       /// working verson
       /// </summary>
       /// <param name="clientSecretPath"></param>
       /// <param name="gmail"></param>
        public void ShareFiles(string currentUserEmail, string clientSecretPath, string gmail)
        {
            UserCredential credential;

            //gmail = Request.Form["txtEmail"].ToString();

            string clientSecretFile = clientSecretPath + "client_secret_5277live.json"; // "client_secret_4209.json"; // "client_secret_2727Synott.json"; //client_secret.json
            //string clientSecretFile = @"C:\TradeKingData\client_secret_5277live.json"; // 

            logger.Info("clientSecretFile: " + clientSecretFile + "for " + _ipAddress);
            using (var stream =
                new FileStream(clientSecretFile, FileMode.Open, FileAccess.Read))
            {
                //use this instead of next line or other wise it would not work, some reason I used the next line and gave me trouble 
                //when trying to relaunch the site 06/29/2019
                //string credPath = System.Environment.GetFolderPath(
                //    System.Environment.SpecialFolder.Personal);
                //credPath = Path.Combine(credPath, ".credentials/drive-dotnet-quickstart.json");

                string credPath = clientSecretPath + "drive-dotnet-quickstart.json";

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
                logger.Info("Credential file saved to: " + credPath);
            }

            // Create Drive API service.
            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            // Define parameters of request.
            //this part could be useless
            FilesResource.ListRequest listRequest = service.Files.List();
            listRequest.PageSize = 1000;
            listRequest.Fields = "nextPageToken, files(id, name)";

            string userEmail = currentUserEmail; // HttpContext.User.Identity.Name; // "luanhuynh73@yahoo.com"; //HttpContext.User.Identity.Name
            //check ordererd product for current user
            //assume we take the latest order only
            IEnumerable<CustomerOrder> coList = db.CustomerOrders.Where(o => o.CustomerUserName ==
            userEmail).OrderByDescending(o => o.DateCreated);

            logger.Info("Share Files...1 for " + _ipAddress);
            //let's say we set up a rule to get only latest orders in past number of days
            //but maybe we just let them download without expirations
            //use selectMany? 
            //double numDays = 10D;

            double numDays = double.Parse(ConfigurationManager.AppSettings["OrderDaysNum"].ToString());

            int customerOrderID = 0;
            List<int> listCustomerOrderIDs = new List<int>();
            foreach (CustomerOrder co in coList)
            {
                if ((DateTime.Now - co.DateCreated).TotalDays < numDays)
                {
                    customerOrderID = co.Id;
                    listCustomerOrderIDs.Add(customerOrderID);
                }
            }

            //dataSource.StateList.Where(s => countryCodes.Contains(s.CountryCode))

            //get all ordered products for customer
            IEnumerable<OrderedProduct> opList = db.Orderedproducts.Where(o => listCustomerOrderIDs.Contains(o.CustomerOrderId));

            logger.Info("Share Files...2 for " + _ipAddress + " for " + _ipAddress);

            OptionFile[] listOpt = { };

            OptionFile[] listOptCombined = { };

            List<int> listProdIDs = new List<int>();


            //get files for all years or years of selected orders
            //lnh: setting up convention
            //if (productID == 3) //buy all: 3 is from db for now
            //{
            //     SharedAllGoogleDrives(service,gmail);
            //}
            //else //selected orders
            //{
            //get list of productIDs
            foreach (OrderedProduct op in opList)
            {
                listProdIDs.Add(op.ProductId);

            }
            logger.Info("Share Files...3 for " + _ipAddress + " for " + _ipAddress);
            SharedGoogleDrives(service, gmail, listProdIDs.Distinct().ToList<int>());
            //}
        }

        // GET: GoogleDrive
        [HttpPost]
        public ActionResult ShareFiles(string gmail)
        {
            UserCredential credential;

            //gmail = Request.Form["txtEmail"].ToString();
           // string clientSecretFile = @"C:\TradeKingData\client_secret_5277live.json";
            string clientSecretFile = Server.MapPath("../Content/client_secret_5277live.json"); // "client_secret_2727Synott.json"; //client_secret.json
            logger.Info("clientSecretFile: " + clientSecretFile);
            using (var stream =
                new FileStream(clientSecretFile, FileMode.Open, FileAccess.Read))
            {
                //string credPath = System.Environment.GetFolderPath(
                //    System.Environment.SpecialFolder.Personal);
                //credPath = Path.Combine(credPath, ".credentials/drive-dotnet-quickstart.json");

                string credPath = Server.MapPath("../Content/drive-dotnet-quickstart.json");
                logger.Info("credPath: " + credPath);
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
            }

            // Create Drive API service.
            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            // Define parameters of request.
            FilesResource.ListRequest listRequest = service.Files.List();
            listRequest.PageSize = 1000;
            listRequest.Fields = "nextPageToken, files(id, name)";

            string userEmail = HttpContext.User.Identity.Name; // "luanhuynh73@yahoo.com"; //HttpContext.User.Identity.Name
            //check ordererd product for current user
            //assume we take the latest order only
            IEnumerable<CustomerOrder> coList = db.CustomerOrders.Where(o => o.CustomerUserName ==
            userEmail).OrderByDescending(o => o.DateCreated);

            logger.Info("Google ShareFiles in progress..." + _ipAddress);
            //let's say we set up a rule to get only latest orders in past number of days
            //but maybe we just let them download without expirations
            //use selectMany? 
            double numDays = 10D;
            int customerOrderID = 0;
            List<int> listCustomerOrderIDs = new List<int>();
            foreach (CustomerOrder co in coList)
            {
                if ((DateTime.Now - co.DateCreated).TotalDays < numDays)
                {
                    customerOrderID = co.Id;
                    listCustomerOrderIDs.Add(customerOrderID);
                }
            }

            //dataSource.StateList.Where(s => countryCodes.Contains(s.CountryCode))

            //get all ordered products for customer
            IEnumerable<OrderedProduct> opList = db.Orderedproducts.Where(o => listCustomerOrderIDs.Contains(o.CustomerOrderId));



            OptionFile[] listOpt = { };

            OptionFile[] listOptCombined = { };

            List<int> listProdIDs = new List<int>();


            //get files for all years or years of selected orders
            //lnh: setting up convention
            //if (productID == 3) //buy all: 3 is from db for now
            //{
            //     SharedAllGoogleDrives(service,gmail);
            //}
            //else //selected orders
            //{
                //get list of productIDs
                foreach (OrderedProduct op in opList)
                {
                    listProdIDs.Add(op.ProductId);
                   
                }
                SharedGoogleDrives(service, gmail, listProdIDs.Distinct().ToList<int>());
            //}

            //return View();
            //return RedirectToAction("Complete", "Checkout");
            //return null;
            return Content("Please log in to your GoogleDrive account to view/download your shared files at <a href='https://drive.google.com/drive/shared-with-me'>https://drive.google.com/drive/shared-with-me</a>");
        }

        
        /// <summary>
        /// ///all the products means all the years
        /// //hard code the years, how about next year?
        /// </summary>
        /// <param name="driveService"></param>
        /// <param name="sharedWithGmailAdress"></param>
        public static void SharedAllGoogleDrives(DriveService driveService, string sharedWithGmailAdress)
        {
           
            List<int> listProductIDs = new List<int>();
            //***lnh:sample only: need to add more later: the hard code way???
            //damn, I've been sharing 2003 and 2004 too!
            //extract to web.config to make it configurable

            // listProductIDs.AddRange(new int[]{35,4,5,6,7,15,17,18,19,20,21,22,23,25, 29,30,39, 41});
            //only years 2010 to 2020 for now
            string activeProducts = System.Configuration.ConfigurationManager.AppSettings["ActiveProducts"].ToString();
            string[] strArr = activeProducts.Split(',');
            int[] intArr = strArr.Select(int.Parse).ToArray();

            //listProductIDs.AddRange(new int[] { 35, 4, 5, 6, 7, 15, 17, 18, 19,  39, 41 });
            listProductIDs.AddRange(intArr);

            for (int i = 0; i < listProductIDs.Count; i++)
            {
                var driveName = GetDriveNameByProductID(listProductIDs[i], "live");
                //shared folder and all its content
                DaimtoGoogleDriveHelper.ShareFile(driveService, driveName,sharedWithGmailAdress);
               
            }

        }

        /// <summary>
        /// 5/6/2018
        /// </summary>
        /// <param name="clientSecretPath"></param>
        /// <param name="sharedWithGmailAdress"></param>
        public void SharedGoogleSubscriptionFolder(string clientSecretPath, string sharedWithGmailAdress)
        {
            //string driveName = "livesubscription";
            //lnh:04/10/2020
            string driveName = "dtnsubscription";
            string ipAddress = Utility.GetIPAddress();


            UserCredential credential;

            //gmail = Request.Form["txtEmail"].ToString();

            string clientSecretFile = clientSecretPath + "client_secret_5277live.json"; // "client_secret_4209.json"; // "client_secret_2727Synott.json"; //client_secret.json
            logger.Info("clientSecretFile: " + clientSecretFile + "for " + ipAddress);
            using (var stream =
                new FileStream(clientSecretFile, FileMode.Open, FileAccess.Read))
            {
                //string credPath = System.Environment.GetFolderPath(
                //    System.Environment.SpecialFolder.Personal);
                //credPath = Path.Combine(credPath, ".credentials/drive-dotnet-quickstart.json");

                string credPath = clientSecretPath + "drive-dotnet-quickstart.json";

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
                logger.Info("Credential file saved to: " + credPath);
            }

            // Create Drive API service.
            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            logger.Info("before sharing livesubscription for " + ipAddress);
            DaimtoGoogleDriveHelper.ShareFile(service, driveName, sharedWithGmailAdress);
            logger.Info("after sharing livesubscription for " + ipAddress);

        }

        /// <summary>
        /// share selected products or years, customer order a few or so, not all
        /// </summary>
        /// <param name="driveService"></param>
        /// <param name="sharedWithGmailAdress"></param>
        /// <param name="listProductIDs"></param>
        public static void SharedGoogleDrives(DriveService driveService, string sharedWithGmailAdress, List<int> listProductIDs)
        {
            string ipAddress = Utility.GetIPAddress();

            //if productID = 3, means share every folder and exit
            if (listProductIDs.Contains(3))
            {
                logger.Info("Share All Folders for ProductID 3. for " + ipAddress);
                SharedAllGoogleDrives(driveService, sharedWithGmailAdress);
                return;
             }
            //1/11/2020
            if (listProductIDs.Contains(40)) //shared bundled 2018-2019
            {
                //each of the year is obtained individually, hard coded 
                var driveName = GetDriveNameByProductID(39, "live");
                logger.Info("Share Bundled 2018-2019...39 for " + ipAddress);
                //shared folder and all its content
                DaimtoGoogleDriveHelper.ShareFile(driveService, driveName, sharedWithGmailAdress);
                driveName = GetDriveNameByProductID(35, "live");
                logger.Info("Share Bundled 2017-2018...35 for " + ipAddress);
                DaimtoGoogleDriveHelper.ShareFile(driveService, driveName, sharedWithGmailAdress);


            }
            if (listProductIDs.Contains(34)) //shared bundled 2017-2018
            {
                //each of the year is obtained individually, hard coded 
                var driveName = GetDriveNameByProductID(4, "live");
                logger.Info("Share Bundled 2017-2018...4 for " + ipAddress);
                //shared folder and all its content
                DaimtoGoogleDriveHelper.ShareFile(driveService, driveName, sharedWithGmailAdress);
                driveName = GetDriveNameByProductID(35, "live");
                logger.Info("Share Bundled 2017-2018...35 for " + ipAddress);
                DaimtoGoogleDriveHelper.ShareFile(driveService, driveName, sharedWithGmailAdress);


            }
            if (listProductIDs.Contains(31)) //shared bundled 2016-2018
            {
                //each of the year is obtained individually, hard coded 
                var driveName = GetDriveNameByProductID(4, "live");
                logger.Info("Share Bundled 2016-2018...4 for " + ipAddress);
                //shared folder and all its content
                DaimtoGoogleDriveHelper.ShareFile(driveService, driveName, sharedWithGmailAdress);
                driveName = GetDriveNameByProductID(7, "live");
                logger.Info("Share Bundled 2016-2018...7 for " + ipAddress);
                //shared folder and all its content
                DaimtoGoogleDriveHelper.ShareFile(driveService, driveName, sharedWithGmailAdress);
                driveName = GetDriveNameByProductID(35, "live");
                logger.Info("Share Bundled 2016-2018...35 for " + ipAddress);
                //shared folder and all its content
                DaimtoGoogleDriveHelper.ShareFile(driveService, driveName, sharedWithGmailAdress);

            }
            if (listProductIDs.Contains(32)) //shared bundled 2015-2018
            {
                //each of the year is obtained individually, hard coded 
                var driveName = GetDriveNameByProductID(4, "live");
                logger.Info("Share Bundled 2015-2018...4 for " + ipAddress);
                //shared folder and all its content
                DaimtoGoogleDriveHelper.ShareFile(driveService, driveName, sharedWithGmailAdress);
                driveName = GetDriveNameByProductID(5, "live");
                logger.Info("Share Bundled 2015-2018...5 for " + ipAddress);
                //shared folder and all its content
                DaimtoGoogleDriveHelper.ShareFile(driveService, driveName, sharedWithGmailAdress);
                driveName = GetDriveNameByProductID(7, "live");
                logger.Info("Share Bundled 2015-2018...7 for " + ipAddress);
                //shared folder and all its content
                DaimtoGoogleDriveHelper.ShareFile(driveService, driveName, sharedWithGmailAdress);
                driveName = GetDriveNameByProductID(35, "live");
                logger.Info("Share Bundled 2015-2018...35 for " + ipAddress);
                //shared folder and all its content
                DaimtoGoogleDriveHelper.ShareFile(driveService, driveName, sharedWithGmailAdress);

            }
            if (listProductIDs.Contains(33)) //shared bundled 2014-2018
            {
                //each of the year is obtained individually, hard coded 
                var driveName = GetDriveNameByProductID(4, "live");
                logger.Info("Share Bundled 2014-2018...4 for " + ipAddress);
                //shared folder and all its content
                DaimtoGoogleDriveHelper.ShareFile(driveService, driveName, sharedWithGmailAdress);
                driveName = GetDriveNameByProductID(5, "live");
                logger.Info("Share Bundled 2014-2018...5 for " + ipAddress);
                //shared folder and all its content
                DaimtoGoogleDriveHelper.ShareFile(driveService, driveName, sharedWithGmailAdress);
                driveName = GetDriveNameByProductID(7, "live");
                logger.Info("Share Bundled 2014-2018...7 for " + ipAddress);
                //shared folder and all its content
                DaimtoGoogleDriveHelper.ShareFile(driveService, driveName, sharedWithGmailAdress);
                driveName = GetDriveNameByProductID(6, "live");
                logger.Info("Share Bundled 2014-2018...6 for " + ipAddress);
                //shared folder and all its content
                DaimtoGoogleDriveHelper.ShareFile(driveService, driveName, sharedWithGmailAdress);
                driveName = GetDriveNameByProductID(35, "live");
                logger.Info("Share Bundled 2014-2018...35 for " + ipAddress);
                //shared folder and all its content
                DaimtoGoogleDriveHelper.ShareFile(driveService, driveName, sharedWithGmailAdress);

            }

            ///share unbundled years, exclude 3, 31, 32, 33, 34
            List<int> bundledList = new List<int> { 3, 31, 32, 33, 34 };
           
            for (int i = 0; i < listProductIDs.Count; i++)
            {
                if (!bundledList.Contains(listProductIDs[i]))
                {
                    logger.Info("Share Files...");
                    var driveName = GetDriveNameByProductID(listProductIDs[i], "live");
                    logger.Info("Share Files...");
                    //shared folder and all its content
                    DaimtoGoogleDriveHelper.ShareFile(driveService, driveName, sharedWithGmailAdress);
                }
            }

        }

        /// <summary>
        /// hard coded by convention, tightly coupled, OK because each bucketname is by year, which is a long time
        /// google cloud needs to have these drivename predefined
        /// //some data only for now, will need to add more later
        /// *** do do : do for years 2018 to 2038 or something
        /// </summary>
        /// <param name="productID"></param>
        /// <returns></returns>
        private static string GetDriveNameByProductID(int productID, string prefix ="")
        {
            string driveName = "";
            switch (productID)
            {
                case 3:
                    driveName = "alldata"; // special case ????
                    break;
                case 36:
                    driveName = "discountallspx";
                    break;
                case 37:
                    driveName = "discountallspy";
                    break;
                case 38:
                    driveName = "discountallqqq";
                    break;
                case 41:
                    driveName = "NoGreeks2020";
                    break;
                case 39: //lnh:06/10/2019
                    driveName = "NoGreeks2019";
                    break;
                case 35:
                    //driveName = "discount2018_optiondatabucketzip";
                    driveName = "NoGreeks2018";
                    break;
                case 4:
                    //driveName = "discount2017_optiondatabucketzip";
                    driveName = "NoGreeks2017";
                    break;
                case 5:
                    //driveName = "discount2015_optiondatabucketzip";
                    driveName = "NoGreeks2015";
                    break;
                case 6:
                    //driveName = "discount2014_optiondatabucketzip";
                    driveName = "NoGreeks2014";
                    break;
                case 7:
                    // driveName = "discount2016_optiondatabucketzip";
                    driveName = "NoGreeks2016";
                    break;
                case 15:
                    //driveName = "discount2013_optiondatabucketzip";
                    driveName = "NoGreeks2013";
                    break;
                case 17:
                    //driveName = "discount2012_optiondatabucketzip";
                    driveName = "NoGreeks2012";
                    break;
                case 18:
                    //driveName = "discount2011_optiondatabucketzip";
                    driveName = "NoGreeks2011";
                    break;
                case 19:
                    //driveName = "discount2010_optiondatabucketzip";
                    driveName = "NoGreeks2010";
                    break;
                case 20:
                    //driveName = "discount2009_optiondatabucketzip";
                    driveName = "NoGreeks2009";
                    break;
                case 21:
                    //driveName = "discount2008_optiondatabucketzip";
                    driveName = "NoGreeks2008";
                    break;
                case 22:
                    //driveName = "discount2007_optiondatabucketzip";
                    driveName = "NoGreeks2007";
                    break;
                case 23:
                   // driveName = "discount2006_optiondatabucketzip";
                    driveName = "NoGreeks2006";
                    break;
                case 25:
                    //driveName = "discount2005_optiondatabucketzip";
                    driveName = "NoGreeks2005";
                    break;
                case 29:
                    //driveName = "discount2004_optiondatabucketzip";
                    driveName = "NoGreeks2004";
                    break;
                //case 30: //don't do 2003 anymore
                //    driveName = "discount2003_optiondatabucketzip";
                //    break;

            }

            return  prefix + driveName;

        }

        /// <summary>
        ///**** NOT BEING USED
        /// by convention only
        /// //need to add more values later
        ///  /// hard coded by convention, tightly coupled, OK because each bucketname is by year, which is a long time
        /// google cloud needs to have these drivename predefined
        /// //some data only for now, will need to add more later
        /// *** do do : do for years 2018 to 2038 or something
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private static string GetDriveNameByFileName(string fileName)
        {
            string driveName = "";
            if (fileName.Contains("2018"))
            {
                driveName = "discount2018_optiondatabucketzip";
            }
            if (fileName.Contains("2017"))
            {
                driveName = "discount2017_optiondatabucketzip";
            }
            else if (fileName.Contains("2016"))
            {
                driveName = "discount2016_optiondatabucketzip";
            }
            else if (fileName.Contains("2015"))
            {
                driveName = "discount2015_optiondatabucketzip";
            }
            else if (fileName.Contains("2014"))
            {
                driveName = "discount2014_optiondatabucketzip";
            }
            else if (fileName.Contains("2013"))
            {
                driveName = "discount2013_optiondatabucketzip";
            }
            else if (fileName.Contains("2012"))
            {
                driveName = "discount2012_optiondatabucketzip";
            }
            else if (fileName.Contains("2011"))
            {
                driveName = "discount2011_optiondatabucketzip";
            }
            else if (fileName.Contains("2010"))
            {
                driveName = "discount2010_optiondatabucketzip";
            }
           
            else if (fileName.Contains("2009"))
            {
                driveName = "discount2009_optiondatabucketzip";
            }
            else if (fileName.Contains("2008"))
            {
                driveName = "discount2008_optiondatabucketzip";
            }
            else if (fileName.Contains("2007"))
            {
                driveName = "discount2007_optiondatabucketzip";
            }
            else if (fileName.Contains("2006"))
            {
                driveName = "discount2006_optiondatabucketzip";
            }
            else if (fileName.Contains("2005"))
            {
                driveName = "discount2005_optiondatabucketzip";
            }
            else if (fileName.Contains("2004"))
            {
                driveName = "discount2004_optiondatabucketzip";
            }
            else if (fileName.Contains("2003"))
            {
                driveName = "discount2003_optiondatabucketzip";
            }

            return driveName;

        }
    }
}