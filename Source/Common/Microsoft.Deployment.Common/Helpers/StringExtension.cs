using System;

namespace Microsoft.Deployment.Common.Helpers
{
    public static class StringExtension
    {
        public static bool EqualsIgnoreCase(this string value, string compare)
        {
            if (value == null || compare == null)
            {
                return false;
            }

            return value.Equals(compare, StringComparison.OrdinalIgnoreCase);
        }
    }
}
