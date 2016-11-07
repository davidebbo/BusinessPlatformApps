using System;
using Microsoft.Win32;

namespace Microsoft.Deployment.Common.Helpers
{
    public class NTHelper
    {
        public static string CleanDomain(string domain)
        {
            string cleanedDomain = domain;

            if (string.IsNullOrEmpty(domain))
            {
                cleanedDomain = Environment.GetEnvironmentVariable("USERDOMAIN");
            }
            else if (domain.EqualsIgnoreCase("."))
            {
                cleanedDomain = Environment.MachineName;
            }

            return cleanedDomain;
        }

        public static string CleanUsername(string username)
        {
            string cleanedUsername = username;

            if (string.IsNullOrEmpty(username))
            {
                cleanedUsername = Environment.GetEnvironmentVariable("USERNAME");
            }

            return cleanedUsername;
        }

        public static bool IsCredentialGuardEnabled()
        {
            bool isCredentialGuardEnabled = false;

            try
            {
                using (RegistryKey rk = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\Lsa"))
                {
                    object o = rk.GetValue("LsaCfgFlags");
                    int rv = (int)o;
                    isCredentialGuardEnabled = rv == 1 || rv == 2;
                }
            }
            catch
            {
                // Checking credential guard failed
            }

            return isCredentialGuardEnabled;
        }
    }
}