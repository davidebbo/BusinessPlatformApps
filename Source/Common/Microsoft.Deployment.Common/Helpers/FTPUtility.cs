using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;

namespace Microsoft.Deployment.Common.Helpers
{
    public class FtpUtility
    {
        public static FtpWebRequest GetRequest(string uri)
        {
            var request = (FtpWebRequest)WebRequest.Create(uri);
            //request.KeepAlive = true;
            //request.ConnectionGroupName = "UploadFunction";
            //request.ServicePoint.ConnectionLimit = 8;
            request.Timeout = -1;
            return request;
        }

        public static void ListDirectory(string ftpserver, string user, string password)
        {
            var request = GetRequest(ftpserver);
            request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;

            request.Credentials = new NetworkCredential(user, password);

            FtpWebResponse response = (FtpWebResponse)request.GetResponse();

            Stream responseStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(responseStream);
            var read = reader.ReadToEnd();
            reader.Close();
            response.Close();
        }

        public static void UploadFile(string ftpserver, string user, string password, string path, string file)
        {
            byte[] data = File.ReadAllBytes(path + "/" + file);
            UploadFile(ftpserver, user, password, file, data);
        }


        public static bool UploadAllViaZip(string ftpserver, string user, string password, string zip, int index, int batch)
        {

            var fileStream = File.OpenRead(zip);
            bool isFinished = false;
            ZipArchive archive = new ZipArchive(fileStream);

            if (archive.Entries.Count - index <= batch)
            {
                batch = archive.Entries.Count;
                isFinished = true;
            }
            else
            {
                batch += index;
            }

            Dictionary<string, bool> directoryCreated = new Dictionary<string, bool>();

            for (int i = index; i < batch; i++)
            {
                Debug.WriteLine(i + " of " + archive.Entries.Count);
                var entry = archive.Entries[i];
                var directory = entry.FullName.Remove(entry.FullName.LastIndexOf('/'));
                if (!directoryCreated.ContainsKey(directory))
                {
                    directoryCreated.Add(directory, true);
                    UploadFolder(ftpserver, user, password, directory);
                }

                byte[] data = new byte[entry.Length];
                using (var stream = entry.Open())
                {
                    stream.Read(data, 0, (int) entry.Length);
                }

                if (!(string.IsNullOrEmpty(entry.Name)))
                {
                    RetryUtility.Retry(()=>
                    UploadFile(ftpserver, user, password, entry.FullName, data));
                }
            }

            return isFinished;
        }


        public static void UploadFile(string ftpserver, string user, string password, string relPath, byte[] data)
        {
            var request = GetRequest(ftpserver + "/" + relPath);

            request.Method = WebRequestMethods.Ftp.UploadFile;
            request.Credentials = new NetworkCredential(user, password);
            request.UseBinary = true;

            request.ContentLength = data.Length;
            Stream requestStream = request.GetRequestStream();
            requestStream.Write(data, 0, data.Length);
            requestStream.Close();
            requestStream.Flush();
            FtpWebResponse response = (FtpWebResponse)request.GetResponse();

            if (response.StatusCode != FtpStatusCode.ClosingData)
            {
                throw new Exception("");
            }

            requestStream.Dispose();
            response.Close();
            response.Dispose();
        }

        public static void UploadFolder(string ftpserver, string user, string password, string relativefoldePath)
        {
            try
            {
                string currentDir = ftpserver;

                currentDir = currentDir + "/" + relativefoldePath;
                var request = GetRequest(currentDir);
                request.Method = WebRequestMethods.Ftp.MakeDirectory;
                request.UseBinary = true;
                request.Credentials = new NetworkCredential(user, password);
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                var ftpStream = response.GetResponseStream();
                ftpStream.Close();
                response.Close();
            }
            catch (Exception)
            {
                //if(relativefoldePath.Contains("/"))
                //{
                //    throw;
                //}
            }
        }

    }
}
