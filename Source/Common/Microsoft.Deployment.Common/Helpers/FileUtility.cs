using System;
using System.IO;

namespace Microsoft.Deployment.Common.Helpers
{
    public class FileUtility
    {
        private const string BPST_PATH = "Business Platform Solution Templates";

        public static string GetLocalPath(string localPath)
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), localPath);
        }

        public static string GetLocalTemplatePath(string templateName)
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), BPST_PATH, templateName);
        }
    }
}