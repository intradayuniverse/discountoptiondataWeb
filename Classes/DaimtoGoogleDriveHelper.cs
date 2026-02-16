using Google.Apis.Drive.v3;
using System;


using System.Collections.Generic;
//using Google.Apis.Drive.v2;
//using Google.Apis.Drive.v2.Data;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Download;
using System.IO;
using Google.Apis.Requests;

namespace DiscountOptionDataWeb.Classes
{
    public class DaimtoGoogleDriveHelper
    {

        private static readonly log4net.ILog logger =
   log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Download a file
        /// Documentation: https://developers.google.com/drive/v2/reference/files/get
        /// </summary>
        /// <param name="_service">a Valid authenticated DriveService</param>
        /// <param name="_fileResource">File resource of the file to download</param>
        /// <param name="_saveTo">location of where to save the file including the file name to save it as.</param>
        /// <returns></returns>
        public static Boolean downloadFile(DriveService _service, Google.Apis.Drive.v3.Data.File _fileResource, string _saveTo)
        {

            if (!String.IsNullOrEmpty(_fileResource.Name))
            {
                try
                {
                    var fileId = _fileResource.Id;
                    var request = _service.Files.Get(fileId);
                    var stream = new System.IO.MemoryStream();

                    // Add a handler which will be notified on progress changes.
                    // It will notify on each chunk download and when the
                    // download is completed or failed.
                    request.MediaDownloader.ProgressChanged +=
                        (IDownloadProgress progress) =>
                        {
                            switch (progress.Status)
                            {
                                case DownloadStatus.Downloading:
                                    {
                                        Console.WriteLine(progress.BytesDownloaded);
                                        break;
                                    }
                                case DownloadStatus.Completed:
                                    {
                                        Console.WriteLine("Download complete.");
                                        break;
                                    }
                                case DownloadStatus.Failed:
                                    {
                                        Console.WriteLine("Download failed.");
                                        break;
                                    }
                            }
                        };
                    request.Download(stream);

                    //byte[] arrBytes = ReadFully(stream);
                    //System.IO.File.WriteAllBytes(_saveTo, arrBytes);

                    SaveStreamToFile(_saveTo, stream);
                    return true;
                }
                catch (Exception e)
                {
                    Console.WriteLine("An error occurred: " + e.Message);
                    return false;
                }
            }
            else
            {
                // The file doesn't have any content stored on Drive.
                return false;
            }
        }

        public static void SaveStreamToFile(string fileFullPath, Stream stream)
        {
            if (stream.Length == 0) return;

            // Create a FileStream object to write a stream to a file
            using (FileStream fileStream = System.IO.File.Create(fileFullPath, (int)stream.Length))
            {
                // Fill the bytes[] array with the stream data
                byte[] bytesInStream = new byte[stream.Length];
                stream.Read(bytesInStream, 0, (int)bytesInStream.Length);

                // Use FileStream object to write to the specified file
                fileStream.Write(bytesInStream, 0, bytesInStream.Length);
            }
        }

        public static byte[] ReadFully(Stream input)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }


        private static string GetMimeType(string fileName)
        {
            string mimeType = "application/unknown";
            string ext = System.IO.Path.GetExtension(fileName).ToLower();
            Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
            if (regKey != null && regKey.GetValue("Content Type") != null)
                mimeType = regKey.GetValue("Content Type").ToString();
            return mimeType;
        }

        ///// <summary>
        ///// Uploads a file
        ///// Documentation: https://developers.google.com/drive/v2/reference/files/insert
        ///// </summary>
        ///// <param name="_service">a Valid authenticated DriveService</param>
        ///// <param name="_uploadFile">path to the file to upload</param>
        ///// <param name="_parent">Collection of parent folders which contain this file. 
        /////                       Setting this field will put the file in all of the provided folders. root folder.</param>
        ///// <returns>If upload succeeded returns the File resource of the uploaded file 
        /////          If the upload fails returns null</returns>
        public static Google.Apis.Drive.v3.Data.File uploadFile(DriveService _service, string _uploadFile, string _parent)
        {

            if (System.IO.File.Exists(_uploadFile))
            {

                try
                {
                    Google.Apis.Drive.v3.Data.File body = new Google.Apis.Drive.v3.Data.File();
                    body.Name = System.IO.Path.GetFileName(_uploadFile);
                    body.Description = "File uploaded ";
                    body.MimeType = GetMimeType(_uploadFile);
                    //body.Parents = null;

                    using (var stream = new System.IO.FileStream(_uploadFile,
                                            System.IO.FileMode.Open))
                    {
                        FilesResource.CreateMediaUpload request = _service.Files.Create(
                            body, stream, "text/csv");
                        request.Fields = "id";
                        request.Upload();



                        var file = request.ResponseBody;
                        Console.WriteLine("File ID: " + file.Id);
                        return file;
                    }



                }
                catch (Exception e)
                {
                    Console.WriteLine("An error occurred: " + e.Message);
                    return null;
                }
            }
            else
            {
                Console.WriteLine("File does not exist: " + _uploadFile);
                return null;
            }

        }

        public static void ShareFile(DriveService driveService, string fileId)
        {
            //fileId = "0B112VdkndJaxM2taal9nbEpNZGc";
            var batch = new BatchRequest(driveService);
            BatchRequest.OnResponse<Permission> callback = delegate (
                Permission permission,
                RequestError error,
                int index,
                System.Net.Http.HttpResponseMessage message)
            {
                if (error != null)
                {
                    // Handle error
                    Console.WriteLine(error.Message);
                }
                else
                {
                    Console.WriteLine("Permission ID: " + permission.Id);
                }
            };
            Permission userPermission = new Permission();
            userPermission.Type = "user";
            userPermission.Role = "reader";
            userPermission.EmailAddress = "luanhuynh73@gmail.com"; //lnh: why hard code here
            var request = driveService.Permissions.Create(userPermission, fileId);
            request.Fields = "id";
            batch.Queue(request, callback);

            //Permission domainPermission = new Permission();
            //domainPermission.Type = "domain";
            //domainPermission.Role = "reader";
            //domainPermission.Domain = "appsrocks.com";
            //request = driveService.Permissions.Create(domainPermission, fileId);
            //request.Fields = "id";
            batch.Queue(request, callback);
            var task = batch.ExecuteAsync();
        }


        public static void ShareFile(DriveService driveService, string fileName, string targetEmailAddress)
        {
            logger.Info("before DaimtoGoogleDriveHelper Share Files..." + fileName);

            //lnh: test only
            //***hard code
            //fileName = "20181106_OData.csv";// "liveNG2004"; // "DTNSubscription";
            string fileId = GetFileIdOfFileFolder(driveService, fileName);
            logger.Info("before DaimtoGoogleDriveHelper Share Files..." + fileId);
            //fileId = "0B112VdkndJaxM2taal9nbEpNZGc";
            var batch = new BatchRequest(driveService);
            BatchRequest.OnResponse<Permission> callback = delegate (
                Permission permission,
                RequestError error,
                int index,
                System.Net.Http.HttpResponseMessage message)
            {
                if (error != null)
                {
                    // Handle error
                    Console.WriteLine(error.Message);
                }
                else
                {
                    Console.WriteLine("Permission ID: " + permission.Id);
                }
            };
            Permission userPermission = new Permission();
            userPermission.Type = "user";
            userPermission.Role = "reader";

      


            userPermission.EmailAddress = targetEmailAddress;
            var request = driveService.Permissions.Create(userPermission, fileId);
            request.Fields = "id";
            batch.Queue(request, callback);

            //Permission domainPermission = new Permission();
            //domainPermission.Type = "domain";
            //domainPermission.Role = "reader";
            //domainPermission.Domain = "gmail.com";
            //request = driveService.Permissions.Create(domainPermission, fileId);
            //request.Fields = "id";
            //batch.Queue(request, callback);
            try
            {
                var task = batch.ExecuteAsync();

               
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        public static void ShareFileID(DriveService driveService, string fileId, string targetEmailAddress)
        {
            logger.Info("before DaimtoGoogleDriveHelper Share Files..." + fileId);

           
            logger.Info("before DaimtoGoogleDriveHelper Share Files..." + fileId);
            //fileId = "0B112VdkndJaxM2taal9nbEpNZGc";
            var batch = new BatchRequest(driveService);
            BatchRequest.OnResponse<Permission> callback = delegate (
                Permission permission,
                RequestError error,
                int index,
                System.Net.Http.HttpResponseMessage message)
            {
                if (error != null)
                {
                    // Handle error
                    Console.WriteLine(error.Message);
                }
                else
                {
                    Console.WriteLine("Permission ID: " + permission.Id);
                }
            };
            Permission userPermission = new Permission();
            userPermission.Type = "user";
            userPermission.Role = "reader";
            userPermission.EmailAddress = targetEmailAddress;
            var request = driveService.Permissions.Create(userPermission, fileId);
            request.Fields = "id";
            batch.Queue(request, callback);

            //Permission domainPermission = new Permission();
            //domainPermission.Type = "domain";
            //domainPermission.Role = "reader";
            //domainPermission.Domain = "appsrocks.com";
            //request = driveService.Permissions.Create(domainPermission, fileId);
            //request.Fields = "id";
            //batch.Queue(request, callback);
            var task = batch.ExecuteAsync();
        }

        public static bool FileOrFolderExists(DriveService service, string fileFolderName)
        {
            bool val = false;
            IList<Google.Apis.Drive.v3.Data.File> files = GetFiles2(service);
            if (files != null && files.Count > 0)
            {
                foreach (var file in files)
                {

                    if (file.Name.ToLower() == fileFolderName.ToLower())
                    {
                        val = true;
                        break;
                    }

                }
            }
            return val;
        }

        public static string GetFileIdOfFileFolder(DriveService service, string fileFolderName)
        {
            string val = "";
            logger.Info("GetFileIdOfFileFolder: " + fileFolderName);
            IList<Google.Apis.Drive.v3.Data.File> files = GetFiles2(service);
            logger.Info("GetFileIdOfFileFolder after: " + fileFolderName);

            List<string> greeksList = new List<string>();
            if (files != null && files.Count > 0)
            {
                foreach (var file in files)
                {
                    if (file.Name.ToLower().Contains("nogreeks"))
                    {
                        Console.WriteLine(file.Name);
                        if (!greeksList.Contains(file.Name))
                        {
                            greeksList.Add(file.Name);
                        }
                    }

                    if (file.Name.ToLower() == fileFolderName.ToLower())
                    {
                        val = file.Id;
                        break;
                    }

                }
            }

            
            return val;
        }

        public static void CreaterFolder(DriveService service, string folderName)
        {
            //don't create if folder already exists
            if (FileOrFolderExists(service, folderName))
                return;

            //create a file
            //creating a folder
            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = folderName,
                MimeType = "application/vnd.google-apps.folder"
            };


            var request = service.Files.Create(fileMetadata);
            request.Fields = "id";
            var file2 = request.Execute();
        }

        public static IList<Google.Apis.Drive.v3.Data.File> GetFiles2(DriveService service)
        {

            //// Define parameters of request.
            //FilesResource.ListRequest listRequest = service.Files.List();
            //listRequest.PageSize = 1000; //yeah!!! lnh, make it so big to get all
            ////https://github.com/googleapis/google-api-dotnet-client/issues/1348
            ////filter for folder name only
            ////listRequest.Q = "mimeType='application/vnd.google-apps.folder'";
            //listRequest.Fields = "nextPageToken, files(id, name)";
            //// List files.
            //IList<Google.Apis.Drive.v3.Data.File> files = listRequest.Execute().Files;

            //return files;

            //////https://stackoverflow.com/questions/41572228/how-to-list-of-more-than-1000-records-from-google-drive-api-v3-in-c-sharp
            List<Google.Apis.Drive.v3.Data.File> allFiles = new List<Google.Apis.Drive.v3.Data.File>();

            Google.Apis.Drive.v3.Data.FileList result = null;
            while (true)
            {
                if (result != null && string.IsNullOrWhiteSpace(result.NextPageToken))
                    break;

                FilesResource.ListRequest listRequest = service.Files.List();
                //listRequest.Q = "mimeType='application/vnd.google-apps.folder'";
                listRequest.PageSize = 1000;
                listRequest.Fields = "nextPageToken, files(id, name)";
                if (result != null)
                    listRequest.PageToken = result.NextPageToken;

                result = listRequest.Execute();
                allFiles.AddRange(result.Files);
            }

            return allFiles;

        }

       

        ///// <summary>
        ///// Updates a file
        ///// Documentation: https://developers.google.com/drive/v2/reference/files/update
        ///// </summary>
        ///// <param name="_service">a Valid authenticated DriveService</param>
        ///// <param name="_uploadFile">path to the file to upload</param>
        ///// <param name="_parent">Collection of parent folders which contain this file. 
        /////                       Setting this field will put the file in all of the provided folders. root folder.</param>
        ///// <param name="_fileId">the resource id for the file we would like to update</param>                      
        ///// <returns>If upload succeeded returns the File resource of the uploaded file 
        /////          If the upload fails returns null</returns>
        //public static File updateFile(DriveService _service, string _uploadFile, string _parent, string _fileId)
        //{

        //    if (System.IO.File.Exists(_uploadFile))
        //    {
        //        File body = new File();
        //        body.Title = System.IO.Path.GetFileName(_uploadFile);
        //        body.Description = "File updated by Diamto Drive Sample";
        //        body.MimeType = GetMimeType(_uploadFile);
        //        body.Parents = new List<ParentReference>() { new ParentReference() { Id = _parent } };

        //        // File's content.
        //        byte[] byteArray = System.IO.File.ReadAllBytes(_uploadFile);
        //        System.IO.MemoryStream stream = new System.IO.MemoryStream(byteArray);
        //        try
        //        {
        //            FilesResource.UpdateMediaUpload request = _service.Files.Update(body, _fileId, stream, GetMimeType(_uploadFile));
        //            request.Upload();
        //            return request.ResponseBody;
        //        }
        //        catch (Exception e)
        //        {
        //            Console.WriteLine("An error occurred: " + e.Message);
        //            return null;
        //        }
        //    }
        //    else
        //    {
        //        Console.WriteLine("File does not exist: " + _uploadFile);
        //        return null;
        //    }

        //}


        ///// <summary>
        ///// Create a new Directory.
        ///// Documentation: https://developers.google.com/drive/v2/reference/files/insert
        ///// </summary>
        ///// <param name="_service">a Valid authenticated DriveService</param>
        ///// <param name="_title">The title of the file. Used to identify file or folder name.</param>
        ///// <param name="_description">A short description of the file.</param>
        ///// <param name="_parent">Collection of parent folders which contain this file. 
        /////                       Setting this field will put the file in all of the provided folders. root folder.</param>
        ///// <returns></returns>
        //public static Google.Apis.Drive.v3.Data.File createDirectory(DriveService _service, string _title, string _description, string _parent)
        //{

        //    Google.Apis.Drive.v3.Data.File NewDirectory = null;

        //    // Create metaData for a new Directory
        //    File body = new Google.Apis.Drive.v3.Data.File();
        //    body.Title = _title;
        //    body.Description = _description;
        //    body.MimeType = "application/vnd.google-apps.folder";
        //    body.Parents = new List<ParentReference>() { new ParentReference() { Id = _parent } };
        //    try
        //    {
        //        FilesResource.InsertRequest request = _service.Files.Insert(body);
        //        NewDirectory = request.Execute();
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine("An error occurred: " + e.Message);
        //    }

        //    return NewDirectory;
        //}


        ///// <summary>
        ///// List all of the files and directories for the current user.  
        ///// 
        ///// Documentation: https://developers.google.com/drive/v2/reference/files/list
        ///// Documentation Search: https://developers.google.com/drive/web/search-parameters
        ///// </summary>
        ///// <param name="service">a Valid authenticated DriveService</param>        
        ///// <param name="search">if Search is null will return all files</param>        
        ///// <returns></returns>
        //    public static IList<Google.Apis.Drive.v3.Data.File> GetFiles(DriveService service, string search)
        //    {

        //        IList<Google.Apis.Drive.v3.Data.File> Files = new List<Google.Apis.Drive.v3.Data.File>();

        //        try
        //        {
        //            //List all of the files and directories for the current user.  
        //            // Documentation: https://developers.google.com/drive/v2/reference/files/list
        //            FilesResource.ListRequest list = service.Files.List();
        //    list.MaxResults = 1000;
        //            if (search != null)
        //            {
        //                list.Q = search;
        //            }
        //FileList filesFeed = list.Execute();

        //            //// Loop through until we arrive at an empty page
        //            while (filesFeed.Items != null)
        //            {
        //                // Adding each item  to the list.
        //                foreach (File item in filesFeed.Items)
        //                {
        //                    Files.Add(item);
        //                }

        //                // We will know we are on the last page when the next page token is
        //                // null.
        //                // If this is the case, break.
        //                if (filesFeed.NextPageToken == null)
        //                {
        //                    break;
        //                }

        //                // Prepare the next page of results
        //                list.PageToken = filesFeed.NextPageToken;

        //                // Execute and process the next page request
        //                filesFeed = list.Execute();
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            // In the event there is an error with the request.
        //            Console.WriteLine(ex.Message);
        //        }
        //        return Files;
        //    }

    }
}