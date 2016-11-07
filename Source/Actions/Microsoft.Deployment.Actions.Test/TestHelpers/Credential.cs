using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Deployment.Actions.Test.TestHelpers
{
    public class Credential
    {
        public static Credential Instance { get; private set; }

        public AAD AAD { get; set; }

        public static void Load()
        {
            Credential cred = new Credential();

            if (File.Exists("../../../../../../Private/Credentials/credentials.json"))
            {
                string text = File.ReadAllText("../../../../../../Private/Credentials/credentials.json");
                cred = JsonConvert.DeserializeObject<Credential>(text);
            }

            Instance = cred;
        }
    }
}
