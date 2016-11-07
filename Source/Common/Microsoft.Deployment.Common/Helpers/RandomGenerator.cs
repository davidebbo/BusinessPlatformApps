using System;
using System.IO;

namespace Microsoft.Deployment.Common.Helpers
{
    public static class RandomGenerator
    {
        public static string GetRandomCharacters()
        {
            var path = Path.GetRandomFileName() + Path.GetRandomFileName();
            return path.Replace(".", "A").Substring(0,15);
        }

        public static string GetRandomLowerCaseCharacters(int length)
        {
            Random random = new Random();
            string rasndomString = string.Empty;
            for (int i=0; i< length; i++)
            {
                int num = random.Next(0, 26); // Zero to 25
                char let = (char)('a' + num);
                rasndomString += let;
            }
            return rasndomString;
        }
    }
}
