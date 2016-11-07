using Microsoft.Deployment.Common;
using Microsoft.Deployment.Common.ActionModel;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Deployment.Actions.Test.TestHelpers
{
    public class AAD
    {
        public string ClientID { get; set; }

        public string ClientSecret { get; set; }

        public string TenantId { get; set; }

        public static async Task<DataStore> GetTokenWithDataStore()
        {
            if (DataStoreWithToken == null)
            {
                ClientCredential cred = new ClientCredential(Credential.Instance.AAD.ClientID, Credential.Instance.AAD.ClientSecret);

                AuthenticationContext context = new AuthenticationContext("https://login.windows.net/" + Credential.Instance.AAD.TenantId);
                var token = await context.AcquireTokenAsync(Constants.AzureManagementCoreApi, cred);
                dynamic tokenObj = new ExpandoObject();
                tokenObj.access_token = token.AccessToken;

                DataStore datastore = new DataStore();
                datastore.AddToDataStore("AzureToken", JObject.FromObject(tokenObj), DataStoreType.Private);

                DataStoreWithToken = datastore;

            }

            return DataStoreWithToken;
        }

        private static DataStore DataStoreWithToken { get; set; }

        public static async Task<DataStore> GetUserTokenFromPopup()
        {
            AuthenticationContext context = new AuthenticationContext("https://login.windows.net/" + Credential.Instance.AAD.TenantId);
            var token = await context.AcquireTokenAsync(Constants.AzureManagementCoreApi, Constants.MicrosoftClientId, new Uri("https://unittest/redirect.html"), new PlatformParameters(PromptBehavior.Auto
                ));
            dynamic tokenObj = new ExpandoObject();
            tokenObj.access_token = token.AccessToken;

            DataStore datastore = new DataStore();
            datastore.AddToDataStore("AzureToken", JObject.FromObject(tokenObj), DataStoreType.Private);
            return datastore;
        }
    }
}
