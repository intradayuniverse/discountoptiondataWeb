using DiscountOptionDataWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.IO;
using Google.Apis.Storage.v1;
using Google.Apis.Services;
using Google.Cloud.Storage.V1;
using Google.Apis.Download;
using System.Threading.Tasks;
using DiscountOptionDataWeb.DAL;
using log4net;
using System.Configuration;
using DiscountOptionDataWeb.Classes;

namespace DiscountOptionDataWeb.Controllers
{
    [Authorize]
    public class FileDeliveryController : Controller
    {
        private static IDictionary<string, long> tasks = new Dictionary<string, long>();
        private OptionDataCenterContext db = new OptionDataCenterContext();
        private static readonly ILog logger =
     log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        string _ipAddress = Utility.GetIPAddress();

        public FileDeliveryController()
        {
        }

        // [START create_storage_client]
        public StorageService CreateStorageClient()
        {
            var credentials = Google.Apis.Auth.OAuth2.GoogleCredential.GetApplicationDefaultAsync().Result;

            if (credentials.IsCreateScopedRequired)
            {
                credentials = credentials.CreateScoped(new[] { StorageService.Scope.DevstorageFullControl });
            }

            var serviceInitializer = new BaseClientService.Initializer()
            {
                ApplicationName = "Storage Sample",
                HttpClientInitializer = credentials
            };

            return new StorageService(serviceInitializer);
        }
        // GET: 
        //productID in Products table of category 1
        //each productID is a year: data will contain 12 zip files, one for each month
        //for now productID = 4 is for 2016
        /// <summary>
        /// just randomly default productID to 5, not sure why I did this?
        /// </summary>
        /// <param name="productID"></param>
        /// <returns></returns>
        public ActionResult Index(int productID = 5)
        {
            logger.Info("viewing on FileDeliveryInfo/Index: " + _ipAddress);
            Utility.InsertUserIPAddress(_ipAddress, "FileDeliveryInfo/Index", "", "starting");

            //check orders for current user
            IEnumerable<CustomerOrder> coList = db.CustomerOrders.Where(o => o.CustomerUserName == 
            HttpContext.User.Identity.Name).Where(p => p.PaymentCharged == true).OrderByDescending(o => o.DateCreated);

            logger.Info("viewing on FileDeliveryInfo/Index: " + _ipAddress);
            Utility.InsertUserIPAddress(_ipAddress, "FileDeliveryInfo/Index", "", "starting 2");

            //let's say we set up a rule to get only latest orders in past number of days
            //but maybe we just let them download without expirations
            //use selectMany? 
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

            logger.Info("viewing on FileDeliveryInfo/Index: " + _ipAddress);
            Utility.InsertUserIPAddress(_ipAddress, "FileDeliveryInfo/Index", "", "starting 3");

            //get all ordered products for customer
            IEnumerable<OrderedProduct> opList = db.Orderedproducts.Where(o => listCustomerOrderIDs.Contains(o.CustomerOrderId));
            OptionFile[] listOpt = { };

            OptionFile[] listOptTemp;
            OptionFile[] listOptCombined = { };

            List<OptionFile> listOptFile = new List<OptionFile>();


            //get files for all years or years of selected orders
            //lnh: setting up convention
            //if (productID == 3) //buy all: 3 is from db for now
            //{
            //    listOpt = ListAllObjectsGoogleStorage();
            //}
            //else
            //{
                foreach (OrderedProduct op in opList)
                {
                    //productID = op.ProductId;
                    if (op.ProductId == 3) //all data
                    {
                        listOpt = ListAllObjectsGoogleStorage();
                       
                    }
                    else if (op.ProductId == 31) //bundle 3 years: 2015-2017
                    {
                        listOpt = ListObjectArrayGoogleStorageBundled(31);
                    }
                    else if (op.ProductId == 32) 
                    {
                        listOpt = ListObjectArrayGoogleStorageBundled(32);
                    }
                    else if (op.ProductId == 33) 
                    {
                        listOpt = ListObjectArrayGoogleStorageBundled(33);
                    }
                    else if (op.ProductId == 34) //2016-2017
                    {
                        listOpt = ListObjectArrayGoogleStorageBundled(34);
                    }
                    else //by individual year
                    {
                         listOpt = ListObjectArrayGoogleStorage(op.ProductId);
                    }
                    for (int i = 0; i < listOpt.Length; i++)
                    {
                        OptionFile optFile = listOpt[i];
                         listOptFile.Add(optFile);
                    }
                }

                //get distinct file names
                listOpt = listOptFile.GroupBy(x => x.FileName).Select(g => g.First()).ToArray<OptionFile>();

            logger.Info("viewing on FileDeliveryInfo/Index: " + _ipAddress);
            Utility.InsertUserIPAddress(_ipAddress, "FileDeliveryInfo/Index", "", "starting 4");

            //}
            return View(listOpt);
        }

       

        /// <summary>
        /// NOT BEING USED
        /// from local folder
        /// list of files could be from a drive folder or from google storate etc
        /// //see see same method on WebGoogleStorage project, homecontroller
        /// //could be refactored later to use interface, factory method to be fancy, etc
        /// path: C:\LuanHuynh\Projects\DiscountOptionDataWeb\DiscountOptionDataWeb\DeliveryFiles
        /// </summary>
        /// <param name="productID"></param>
        /// <returns></returns>
        public OptionFile[] ListObjectsArray(int productID)
        {
            StringBuilder sb = new System.Text.StringBuilder();
            //List<OptionFile> myList = new List<OptionFile>();
            string path = Server.MapPath("DeliveryFiles");
            OptionFile[] optionFileArr = null;
            List<string> lst = new List<string>();
           
            switch (productID)
            {
                case 4: //2017
                    optionFileArr = GetFilesByYearFolder(Server.MapPath("..//..//DeliveryFiles//2017"));
                    break;
                case 5: //2015
                    optionFileArr = GetFilesByYearFolder(Server.MapPath("..//..//DeliveryFiles//2015"));
                    break;
                case 6: //2014
                    optionFileArr = GetFilesByYearFolder(Server.MapPath("..//..//DeliveryFiles//2014"));
                    break;

            }

            return optionFileArr;
        }

        /// <summary>
        /// not being used, copied from WebGoogleStorage project
        /// </summary>
        /// <param name="bucketName"></param>
        /// <returns></returns>
        public OptionFile[] ListObjectsArray(string bucketName)
        {
            StringBuilder sb = new StringBuilder();
            List<OptionFile> myList = new List<OptionFile>();
            StorageService storage = CreateStorageClient();

            var objects = storage.Objects.List(bucketName).Execute();

            if (objects.Items != null)
            {
                foreach (var obj in objects.Items)
                {
                    //Console.WriteLine($"Object: {obj.Name}");
                    //sb.Append(obj.Name + ", ");
                    OptionFile op = new OptionFile();
                    op.FileName = obj.Name;
                    myList.Add(op);
                }
            }

            return myList.ToArray<OptionFile>();
        }

        /// <summary>
        /// productID: year
        /// list all the zip files for year by the bucket name: eg:productID =4 has bucket name of "discount2017_optiondatabucketzip";
        /// </summary>
        /// <param name="productID"></param>
        /// <returns></returns>
        public OptionFile[] ListObjectArrayGoogleStorage(int productID)
        {
            //var client = CreateStorageClient(); //not using this????

            var client = StorageClient.Create();

            ///* bucketname could be conventionalized later by year ?
            var bucketName = this.GetBucketNameByProductID(productID); // "luan2016_optiondatabucket";// Guid.NewGuid().ToString(); // must be globally unique
            List<OptionFile> lstOptionFile = new List<OptionFile>();
            // List objects
            OptionFile optFile;
            foreach (var obj 
                in client.ListObjects(bucketName, ""))
            {
                optFile = new OptionFile();
                optFile.FileName = obj.Name;
                //file.substring(0, file.lastIndexOf('.'));
                optFile.ShortFileName = obj.Name.Substring(0, obj.Name.LastIndexOf('.'));
                //divide to get GB unit
                optFile.FileSize = obj.Size/1000000;
          
                lstOptionFile.Add(optFile);
            }
            return lstOptionFile.ToArray<OptionFile>();
        }


        /// <summary>
        /// lnh: 12/27/2017
        /// for bundled products: bundle is 31 is 3 years, 2015 to 2017
        /// //taking a hard code approach for bundled years
        /// </summary>
        /// <param name="productID"></param>
        /// <returns></returns>
        public OptionFile[] ListObjectArrayGoogleStorageBundled(int productID)
        {

            List<OptionFile> lstOptionFile = new List<OptionFile>();
           switch (productID)
            {
                case 31:
                    lstOptionFile = ListObjectArrayGoogleStorageBundled31();
                    break;
                case 32:
                    lstOptionFile = ListObjectArrayGoogleStorageBundled32();
                    break;
                case 33:
                    lstOptionFile = ListObjectArrayGoogleStorageBundled33();
                    break;
                case 34:
                    lstOptionFile = ListObjectArrayGoogleStorageBundled34();
                    break;


            }
            
            return lstOptionFile.ToArray<OptionFile>();
        }

        //productID 31: bundled 2016-2017: 2 years
        public List<OptionFile> ListObjectArrayGoogleStorageBundled34()
        {
            var client = StorageClient.Create();
            List<OptionFile> lstOptionFile = new List<OptionFile>();

            ///* bucketname could be conventionalized later by year ?
            var bucketName2017 = this.GetBucketNameByProductID(4); // "luan2016_optiondatabucket";// Guid.NewGuid().ToString(); // must be globally unique

            // List objects
            OptionFile optFile;
            foreach (var obj
                in client.ListObjects(bucketName2017, ""))
            {
                optFile = new OptionFile();
                optFile.FileName = obj.Name;
                //file.substring(0, file.lastIndexOf('.'));
                optFile.ShortFileName = obj.Name.Substring(0, obj.Name.LastIndexOf('.'));
                //divide to get GB unit
                optFile.FileSize = obj.Size / 1000000;

                lstOptionFile.Add(optFile);
            }
            var bucketName2016 = this.GetBucketNameByProductID(7); // "luan2016_optiondatabucket";// Guid.NewGuid().ToString(); // must be globally unique
            foreach (var obj
                in client.ListObjects(bucketName2016, ""))
            {
                optFile = new OptionFile();
                optFile.FileName = obj.Name;
                //file.substring(0, file.lastIndexOf('.'));
                optFile.ShortFileName = obj.Name.Substring(0, obj.Name.LastIndexOf('.'));
                //divide to get GB unit
                optFile.FileSize = obj.Size / 1000000;

                lstOptionFile.Add(optFile);
            }
           

            return lstOptionFile;
        }

        //productID 31: bundled 2015-2017: 3 years
        public List<OptionFile> ListObjectArrayGoogleStorageBundled31()
        {
                 var client = StorageClient.Create();
                 List<OptionFile> lstOptionFile = new List<OptionFile>();
          
                ///* bucketname could be conventionalized later by year ?
                var bucketName2017 = this.GetBucketNameByProductID(4); // "luan2016_optiondatabucket";// Guid.NewGuid().ToString(); // must be globally unique

                // List objects
                OptionFile optFile;
                foreach (var obj
                    in client.ListObjects(bucketName2017, ""))
                {
                    optFile = new OptionFile();
                    optFile.FileName = obj.Name;
                    //file.substring(0, file.lastIndexOf('.'));
                    optFile.ShortFileName = obj.Name.Substring(0, obj.Name.LastIndexOf('.'));
                    //divide to get GB unit
                    optFile.FileSize = obj.Size / 1000000;

                    lstOptionFile.Add(optFile);
                }
                var bucketName2016 = this.GetBucketNameByProductID(7); // "luan2016_optiondatabucket";// Guid.NewGuid().ToString(); // must be globally unique
                foreach (var obj
                    in client.ListObjects(bucketName2016, ""))
                {
                    optFile = new OptionFile();
                    optFile.FileName = obj.Name;
                    //file.substring(0, file.lastIndexOf('.'));
                    optFile.ShortFileName = obj.Name.Substring(0, obj.Name.LastIndexOf('.'));
                    //divide to get GB unit
                    optFile.FileSize = obj.Size / 1000000;

                    lstOptionFile.Add(optFile);
                }
                var bucketName2015 = this.GetBucketNameByProductID(5); // "luan2016_optiondatabucket";// Guid.NewGuid().ToString(); // must be globally unique
                foreach (var obj
                    in client.ListObjects(bucketName2015, ""))
                {
                    optFile = new OptionFile();
                    optFile.FileName = obj.Name;
                    //file.substring(0, file.lastIndexOf('.'));
                    optFile.ShortFileName = obj.Name.Substring(0, obj.Name.LastIndexOf('.'));
                    //divide to get GB unit
                    optFile.FileSize = obj.Size / 1000000;

                    lstOptionFile.Add(optFile);
                }

            return lstOptionFile;
        }

        //productID 32: bundled 2014-2017: 4 years
        public List<OptionFile> ListObjectArrayGoogleStorageBundled32()
        {
            var client = StorageClient.Create();
            List<OptionFile> lstOptionFile = new List<OptionFile>();

            ///* bucketname could be conventionalized later by year ?
            var bucketName2017 = this.GetBucketNameByProductID(4); // "luan2016_optiondatabucket";// Guid.NewGuid().ToString(); // must be globally unique

            // List objects
            OptionFile optFile;
            foreach (var obj
                in client.ListObjects(bucketName2017, ""))
            {
                optFile = new OptionFile();
                optFile.FileName = obj.Name;
                //file.substring(0, file.lastIndexOf('.'));
                optFile.ShortFileName = obj.Name.Substring(0, obj.Name.LastIndexOf('.'));
                //divide to get GB unit
                optFile.FileSize = obj.Size / 1000000;

                lstOptionFile.Add(optFile);
            }
            var bucketName2016 = this.GetBucketNameByProductID(7); // "luan2016_optiondatabucket";// Guid.NewGuid().ToString(); // must be globally unique
            foreach (var obj
                in client.ListObjects(bucketName2016, ""))
            {
                optFile = new OptionFile();
                optFile.FileName = obj.Name;
                //file.substring(0, file.lastIndexOf('.'));
                optFile.ShortFileName = obj.Name.Substring(0, obj.Name.LastIndexOf('.'));
                //divide to get GB unit
                optFile.FileSize = obj.Size / 1000000;

                lstOptionFile.Add(optFile);
            }
            var bucketName2015 = this.GetBucketNameByProductID(5); // "luan2016_optiondatabucket";// Guid.NewGuid().ToString(); // must be globally unique
            foreach (var obj
                in client.ListObjects(bucketName2015, ""))
            {
                optFile = new OptionFile();
                optFile.FileName = obj.Name;
                //file.substring(0, file.lastIndexOf('.'));
                optFile.ShortFileName = obj.Name.Substring(0, obj.Name.LastIndexOf('.'));
                //divide to get GB unit
                optFile.FileSize = obj.Size / 1000000;

                lstOptionFile.Add(optFile);
            }
            var bucketName2014 = this.GetBucketNameByProductID(6); // "luan2016_optiondatabucket";// Guid.NewGuid().ToString(); // must be globally unique
            foreach (var obj
                in client.ListObjects(bucketName2014, ""))
            {
                optFile = new OptionFile();
                optFile.FileName = obj.Name;
                //file.substring(0, file.lastIndexOf('.'));
                optFile.ShortFileName = obj.Name.Substring(0, obj.Name.LastIndexOf('.'));
                //divide to get GB unit
                optFile.FileSize = obj.Size / 1000000;

                lstOptionFile.Add(optFile);
            }

            return lstOptionFile;
        }

        //productID 32: bundled 2013-2017: 5 years
        public List<OptionFile> ListObjectArrayGoogleStorageBundled33()
        {
            var client = StorageClient.Create();
            List<OptionFile> lstOptionFile = new List<OptionFile>();

            ///* bucketname could be conventionalized later by year ?
            var bucketName2017 = this.GetBucketNameByProductID(4); // "luan2016_optiondatabucket";// Guid.NewGuid().ToString(); // must be globally unique

            // List objects
            OptionFile optFile;
            foreach (var obj
                in client.ListObjects(bucketName2017, ""))
            {
                optFile = new OptionFile();
                optFile.FileName = obj.Name;
                //file.substring(0, file.lastIndexOf('.'));
                optFile.ShortFileName = obj.Name.Substring(0, obj.Name.LastIndexOf('.'));
                //divide to get GB unit
                optFile.FileSize = obj.Size / 1000000;

                lstOptionFile.Add(optFile);
            }
            var bucketName2016 = this.GetBucketNameByProductID(7); // "luan2016_optiondatabucket";// Guid.NewGuid().ToString(); // must be globally unique
            foreach (var obj
                in client.ListObjects(bucketName2016, ""))
            {
                optFile = new OptionFile();
                optFile.FileName = obj.Name;
                //file.substring(0, file.lastIndexOf('.'));
                optFile.ShortFileName = obj.Name.Substring(0, obj.Name.LastIndexOf('.'));
                //divide to get GB unit
                optFile.FileSize = obj.Size / 1000000;

                lstOptionFile.Add(optFile);
            }
            var bucketName2015 = this.GetBucketNameByProductID(5); // "luan2016_optiondatabucket";// Guid.NewGuid().ToString(); // must be globally unique
            foreach (var obj
                in client.ListObjects(bucketName2015, ""))
            {
                optFile = new OptionFile();
                optFile.FileName = obj.Name;
                //file.substring(0, file.lastIndexOf('.'));
                optFile.ShortFileName = obj.Name.Substring(0, obj.Name.LastIndexOf('.'));
                //divide to get GB unit
                optFile.FileSize = obj.Size / 1000000;

                lstOptionFile.Add(optFile);
            }
            var bucketName2014 = this.GetBucketNameByProductID(6); // "luan2016_optiondatabucket";// Guid.NewGuid().ToString(); // must be globally unique
            foreach (var obj
                in client.ListObjects(bucketName2014, ""))
            {
                optFile = new OptionFile();
                optFile.FileName = obj.Name;
                //file.substring(0, file.lastIndexOf('.'));
                optFile.ShortFileName = obj.Name.Substring(0, obj.Name.LastIndexOf('.'));
                //divide to get GB unit
                optFile.FileSize = obj.Size / 1000000;

                lstOptionFile.Add(optFile);
            }
            var bucketName2013 = this.GetBucketNameByProductID(15); // "luan2016_optiondatabucket";// Guid.NewGuid().ToString(); // must be globally unique
            foreach (var obj
                in client.ListObjects(bucketName2013, ""))
            {
                optFile = new OptionFile();
                optFile.FileName = obj.Name;
                //file.substring(0, file.lastIndexOf('.'));
                optFile.ShortFileName = obj.Name.Substring(0, obj.Name.LastIndexOf('.'));
                //divide to get GB unit
                optFile.FileSize = obj.Size / 1000000;

                lstOptionFile.Add(optFile);
            }

            return lstOptionFile;
        }



        /// <summary>
        /// get list of all file names of all buckets
        /// </summary>
        /// <returns></returns>
        public OptionFile[] ListAllObjectsGoogleStorage()
        {
            List<OptionFile> lstOptionFile = new List<OptionFile>();


            List<int> listProductIDs = new List<int>();
            //lnh:sample only: need to add more later
            //listProductIDs.Add(4);
            //listProductIDs.Add(5);
            //don't get productID =  3 which is everything thing
            
            var Products = db.Products.Where(p=> p.Id !=3).ToList<Product>();

            ///could have used some linq with select tranformation for fanciness
            foreach (var p in Products)
            {
                listProductIDs.Add(p.Id);
            }

            var client = StorageClient.Create();
            ///* bucketname could be conventionalized later by year ?


            for (int i = 0; i < listProductIDs.Count; i++)
            {
                var bucketName = this.GetBucketNameByProductID(listProductIDs[i]);
                // List objects
                OptionFile optFile;
                foreach (var obj in client.ListObjects(bucketName, ""))
                {
                    optFile = new OptionFile();
                    optFile.FileName = obj.Name;
                    //file.substring(0, file.lastIndexOf('.'));
                    optFile.ShortFileName = obj.Name.Substring(0, obj.Name.LastIndexOf('.'));
                    //divide to get GB unit
                    optFile.FileSize = obj.Size / 1000000;

                    lstOptionFile.Add(optFile);
                }
            }


            return lstOptionFile.ToArray<OptionFile>();
        }
        /// <summary>
        /// NOT BEING USED?
        /// get list of all file names of all buckets
        /// </summary>
        /// <returns></returns>
        public OptionFile[] ListObjectArrayGoogleStorageVersion2(int productID)
        {

            StringBuilder sb = new StringBuilder();
            List<OptionFile> myList = new List<OptionFile>();
            StorageService storage = CreateStorageClient();

            var bucketName = this.GetBucketNameByProductID(productID);

            var objects = storage.Objects.List(bucketName).Execute();
            // List objects
            OptionFile optFile;
            List<OptionFile> lstOptionFile = new List<OptionFile>();
            if (objects.Items != null)
            {
                foreach (var obj in objects.Items)
                {
                    optFile = new OptionFile();
                    optFile.FileName = obj.Name;
                    //file.substring(0, file.lastIndexOf('.'));
                    optFile.ShortFileName = obj.Name.Substring(0, obj.Name.LastIndexOf('.'));
                    optFile.FileSize = obj.Size / 1000000;

                    lstOptionFile.Add(optFile);
                }
            }

            return myList.ToArray<OptionFile>();
        }

        /// <summary>
        /// hard coded by convention, tightly coupled, OK because each bucketname is by year, which is a long time
        /// google cloud needs to have these bucketname predefined
        /// //some data only for now, will need to add more later
        ///  /// hard coded by convention, tightly coupled, OK because each bucketname is by year, which is a long time
        /// google cloud needs to have these drivename predefined
        /// //some data only for now, will need to add more later
        /// *** to do : do for years 2018 to 2038 or something
        /// </summary>
        /// <param name="productID"></param>
        /// <returns></returns>
        private string GetBucketNameByProductID(int productID)
        {
            string bucketName = "";
            switch (productID)
            {
                case 3:
                    bucketName = "alldata"; // special case ????
                    break;
                case 36:
                    bucketName = "discountallspx"; // special case ????
                    break;
                case 37:
                    bucketName = "discountallspy"; // special case ????
                    break;
                case 38:
                    bucketName = "discountallqqq"; // special case ????
                    break;
                case 35:
                    bucketName = "discount2018_optiondatabucketzip";
                    break;
                case 4:
                    bucketName = "discount2017_optiondatabucketzip";
                    break;
                case 5:
                    bucketName = "discount2015_optiondatabucketzip";
                    break;
                case 6:
                    bucketName = "discount2014_optiondatabucketzip";
                    break;
                case 7:
                    bucketName = "discount2016_optiondatabucketzip";
                    break;
                case 15:
                    bucketName = "discount2013_optiondatabucketzip";
                    break;
                case 17:
                    bucketName = "discount2012_optiondatabucketzip";
                    break;
                case 18:
                    bucketName = "discount2011_optiondatabucketzip";
                    break;
                case 19:
                    bucketName = "discount2010_optiondatabucketzip";
                    break;
                case 20:
                    bucketName = "discount2009_optiondatabucketzip";
                    break;
                case 21:
                    bucketName = "discount2008_optiondatabucketzip";
                    break;
                case 22:
                    bucketName = "discount2007_optiondatabucketzip";
                    break;
                case 23:
                    bucketName = "discount2006_optiondatabucketzip";
                    break;
                case 25:
                    bucketName = "discount2005_optiondatabucketzip";
                    break;
                case 29:
                    bucketName = "discount2004_optiondatabucketzip";
                    break;
                case 30:
                    bucketName = "discount2003_optiondatabucketzip";
                    break;

            }

            return "live" + bucketName;

        }

        /// <summary>
        /// by convention only
        /// //need to add more values later
        ///  /// hard coded by convention, tightly coupled, OK because each bucketname is by year, which is a long time
        /// google cloud needs to have these drivename predefined
        /// //some data only for now, will need to add more later
        /// *** do do : do for years 2018 to 2038 or something
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private string GetBucketNameByFileName(string fileName)
        {
            string bucketName = "";
            if (fileName.Contains("spx"))
            {
                bucketName = "discountallspx";
            }
            else if (fileName.Contains("spy"))
            {
                bucketName = "discountallspy";
            }
            else if (fileName.Contains("qqq"))
            {
                bucketName = "discountallqqq";
            }
            if (fileName.Contains("2018"))
            {
                bucketName = "discount2018_optiondatabucketzip";
            }
            if (fileName.Contains("2017"))
            {
                bucketName = "discount2017_optiondatabucketzip";
            }
            else if (fileName.Contains("2016"))
            {
                bucketName = "discount2016_optiondatabucketzip";
            }
            else if (fileName.Contains("2015"))
            {
                bucketName = "discount2015_optiondatabucketzip";
            }
            else if (fileName.Contains("2014"))
            {
                bucketName = "discount2014_optiondatabucketzip";
            }
            else if (fileName.Contains("2013"))
            {
                bucketName = "discount2013_optiondatabucketzip";
            }
            else if (fileName.Contains("2012"))
            {
                bucketName = "discount2012_optiondatabucketzip";
            }
            else if (fileName.Contains("2011"))
            {
                bucketName = "discount2011_optiondatabucketzip";
            }
            else if (fileName.Contains("2010"))
            {
                bucketName = "discount2010_optiondatabucketzip";
            }
            else if (fileName.Contains("2009"))
            {
                bucketName = "discount2009_optiondatabucketzip";
            }
            else if (fileName.Contains("2008"))
            {
                bucketName = "discount2008_optiondatabucketzip";
            }
            else if (fileName.Contains("2007"))
            {
                bucketName = "discount2007_optiondatabucketzip";
            }
            else if (fileName.Contains("2006"))
            {
                bucketName = "discount2006_optiondatabucketzip";
            }
            else if (fileName.Contains("2005"))
            {
                bucketName = "discount2005_optiondatabucketzip";
            }
            else if (fileName.Contains("2004"))
            {
                bucketName = "discount2005_optiondatabucketzip";
            }
            else if (fileName.Contains("2003"))
            {
                bucketName = "discount2003_optiondatabucketzip";
            }

            return "live" + bucketName;

        }

        //not being used
        private OptionFile[] GetFilesByYearFolder(string yearFolderPath)
        {
            List<OptionFile> myList = new List<OptionFile>();
            string[] files = Directory.GetFiles(yearFolderPath);

            OptionFile opFile;

            for (int i = 0; i < files.Length; i++)
            {
                opFile = new OptionFile();
                opFile.FileName = files[i];// files[i].Substring(files[i].LastIndexOf("\\") + 1);
                myList.Add(opFile);
            }

            return myList.ToArray<OptionFile>();
        }


        /// <summary>
        /// the DownloadGoogleStorage exits anyway, so is this still useful?
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Start(string fileName)
        {
           
            tasks.Add(fileName, 0);

            Task.Factory.StartNew(() =>
            {
                DownloadGoogleStorage(fileName);

                tasks.Remove(fileName);
        });

            return Json(fileName);
        }

        //[HttpGet]
         
        public ActionResult Test(int productID)
        {
            string fileName = "";
            if (productID == 4)
            {
                fileName = "Jan2017.zip";
            }
            else if (productID == 5)
            {
                fileName = "Feb2017.zip";
            }

            if (tasks.Keys.Contains(fileName) == false)
                tasks.Add(fileName, 0);

            //don't use task here since it's async and we don't want it here on the server
            //Task.Factory.StartNew(() =>
            //{
                DownloadGoogleStorage(fileName);

                tasks.Remove(fileName);
            //});

            return Content(fileName);
        }

        /// <summary>
        /// not being used?
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public ActionResult Download(string fileName)
        {
            string bucketName = this.GetBucketNameByFileName(fileName);
            DownloadStreamGoogleStorage(bucketName, fileName);

            //DownloadStreamLocalDrive(fileName);
            return null;
        }

        /// <summary>
        /// so this call is pretty much useless??? since we download from web site anyway
        /// </summary>
        /// <param name="fileName"></param>
        //using Google.Cloud.Storage.V1
        public void DownloadGoogleStorage(string fileName = "abc")
        {
            //fileName = "Dec2016.zip";
            var client = StorageClient.Create();
           
            // Create a bucket
           // var bucketName = "luan2016_optiondatabucket";// Guid.NewGuid().ToString(); // must be globally unique
                                                         //var bucket = client.CreateBucket(projectId, bucketName);
            string bucketName = this.GetBucketNameByFileName(fileName);

            string path = Server.MapPath("..//DeliveryFiles//");
            //string path = Server.MapPath("//DeliveryFiles//");

            //lnh: need to put it back later
            //if file exists in path don't download
            ///since we have files in DeliveryFiles, this always exit
            if (System.IO.File.Exists(path + fileName))
                return;

            //download another try
            var storage = StorageClient.Create();
            string localFileName = Path.GetFileName(fileName);
            //Path.GetTempFileName
            // yes, hard code here, so we can specify the path to download
            FileStream outputFile = null;
            using (outputFile = System.IO.File.OpenWrite(path + localFileName))
            {
                // IDownloadProgress defined in Google.Apis.Download namespace
                var progress = new Progress<IDownloadProgress>(
                  p => UpdateProgress(fileName, p.BytesDownloaded)
              );
                //storage.DownloadObject(bucketName, fileName, outputFile);
                storage.DownloadObject(bucketName, fileName, outputFile, null, progress);

            }

            //return Json(fileName);


        }


        // [START download_stream]
        //not being used
        public void DownloadStreamGoogleStorage(string bucketName, string fileName)
        {
            StorageService storage = CreateStorageClient();

            using (var stream = new MemoryStream())
            {
                storage.Objects.Get(bucketName, fileName).Download(stream);

               
                Response.Cache.SetCacheability(HttpCacheability.Private);
                Response.Buffer = false;
                Response.BufferOutput = false;

                Response.ClearContent();
                Response.ClearHeaders();
                //Response.Buffer = true;
                //string attachment = "attachment; filename=MyCsvLol.csv";
                Response.Clear();
                
                Response.AddHeader("Content-Disposition", "attachment; filename=" + fileName);
                //Response.AddHeader("Content-Disposition", "attachment; filename=" + "options_20020208.csv");
                Response.AddHeader("Content-Length", stream.Length.ToString());
                Response.BinaryWrite(stream.ToArray());

                Response.Flush();
                Response.End();
;
            }
        }

        public ActionResult Progress(string fileName)
        {
             
            return Json(tasks.Keys.Contains(fileName) ? tasks[fileName] : 100);

            //return Json(tasks[id]);
            //return Json(randomNumber);
        }

        [HttpPost]
        private void UpdateProgress(string fileName, long progress)
        {
            tasks[fileName] = progress / 1000;
        }


     
     
        /// <summary>
        /// large file .5 G has out of memory problem
        /// </summary>
        /// <param name="fileName"></param>
        public void DownloadStreamLocalDrive(string fileName)
        {
            //file too large, out of memory
            ////MemoryStream ms = new MemoryStream();
            ////using (FileStream fs = System.IO.File.OpenRead(fileName))
            ////{
            ////    fs.CopyTo(ms);
            ////}

            //using (var ms = new MemoryStream(System.IO.File.ReadAllBytes(fileName)))
            //{

            //}
            //out of memory for very large file
            //MemoryStream ms = new MemoryStream(System.IO.File.ReadAllBytes(fileName));


            const int BUF_SIZE = 102;
            System.IO.FileStream fs = new System.IO.FileStream(fileName, System.IO.FileMode.Open);
            byte[] buf = new byte[BUF_SIZE];
            int bytesRead;

            // Read the file one kilobyte at a time.
            MemoryStream ms = new MemoryStream();
            do
            {
                bytesRead = fs.Read(buf, 0, BUF_SIZE);
                fs.CopyTo(ms);
                // 'buf' contains the last 1024 bytes read of the file.
            } while (bytesRead == BUF_SIZE);
           
            
            fs.Close();


            Response.ClearContent();
            Response.ClearHeaders();
            Response.Buffer = true;
            //string attachment = "attachment; filename=MyCsvLol.csv";
            Response.Clear();
            //Response.ContentType = "text/csv";
            Response.AddHeader("Content-Disposition", "attachment; filename=" + fileName);
            //Response.AddHeader("Content-Disposition", "attachment; filename=" + "options_20020208.csv");
            Response.AddHeader("Content-Length", ms.Length.ToString());
            Response.BinaryWrite(ms.ToArray());

            Response.Flush();
            Response.End();

        }


        public ActionResult TransmitToClient(string fileName)
        {
            //test download/upload speed
            // http://speedtest.xfinity.com/
            try
            {
                fileName = Server.MapPath("..//DeliveryFiles//") + fileName + ".zip";
                //doing this way to avoid really long name being created on the client's machine
                FileInfo fileInfo = new FileInfo(fileName);

                Response.ContentType = "application/zip";
                Response.AddHeader("Content-Disposition", String.Format("attachment;filename=\"{0}\"", fileInfo.Name));
                Response.AddHeader("Content-Length", fileInfo.Length.ToString());
                Response.TransmitFile(fileInfo.FullName);
                Response.End();
            }
            catch (Exception ex)
            {

            }

            return RedirectToAction("Index");
        }
    }
}