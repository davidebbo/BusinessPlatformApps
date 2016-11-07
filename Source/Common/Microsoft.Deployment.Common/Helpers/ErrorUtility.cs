using Microsoft.Deployment.Common.ErrorResources;

namespace Microsoft.Deployment.Common.Helpers
{
    public static class ErrorUtility
    {
        public static string GetErrorCode(string code)
        {
            if (EnglishErrorCodes.ResourceManager.GetString(code) != null)
            {
                return EnglishErrorCodes.ResourceManager.GetString(code);
            }

                return EnglishErrorCodes.DefaultErrorCode;
        }
    }
}
