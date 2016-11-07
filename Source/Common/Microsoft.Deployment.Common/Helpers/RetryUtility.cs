using System;

namespace Microsoft.Deployment.Common.Helpers
{
    public class RetryUtility
    {
        private const int retries = 3;

        public static void Retry(int retry, Action func)
        {
            for (int i = 0; i < retries; i++)
            {
                try
                {
                    func.Invoke();
                    return;
                }
                catch (Exception)
                {
                    if (i + 1 >= retries)
                    {
                        throw;
                    }
                }
            }
        }

        public static void Retry(Action func)
        {
            RetryUtility.Retry(retries, func);
        }
    }
}
