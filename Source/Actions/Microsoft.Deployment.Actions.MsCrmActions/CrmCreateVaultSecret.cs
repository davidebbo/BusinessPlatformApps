using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure;
using Microsoft.Azure.KeyVault;

namespace Microsoft.Deployment.Common.Actions.MsCrm
{
    using Microsoft.Deployment.Common.ActionModel;
    using Microsoft.Deployment.Common.Actions;
    using Microsoft.Deployment.Common.Helpers;
    using Model;
    using System.Threading.Tasks;

    using System.Net.Http.Headers;
    using Newtonsoft.Json.Linq;
    using System.ComponentModel.Composition;
    using System.Dynamic;
    using System.Net;
    using Newtonsoft.Json;

    using Microsoft.Azure.Management.KeyVault;

    public class CrmCreateVaultSecret : BaseAction
    {
        private string _azureToken = "NO_TOKEN";

        public async Task<string> GetAccessToken(string authority, string resource, string scope)
        {
            return _azureToken;
        }

        [Export(typeof(IAction))]
        public override async Task<ActionResponse> ExecuteActionAsync(ActionRequest request)
        {
            string _azureToken = request.DataStore.GetAllValues("Token")[0];
            string subscriptionID = request.DataStore.GetAllValues("SubscriptionID")[0];
            string resourceGroup = request.DataStore.GetAllValues("ResourceGroup")[0];
            string vaultName = request.DataStore.GetAllValues("VaultLine")[0];
            string secretName = request.DataStore.GetAllValues("SecretName")[0] ?? "pbicms";
            string connectionString = request.DataStore.GetAllValues("ConnectionString")[0];
            string organizationId = request.DataStore.GetAllValues("OrganizationId")[0];
            string tenantId = request.DataStore.GetAllValues("TenantId")[0];

            SubscriptionCloudCredentials credentials = new TokenCloudCredentials(subscriptionID, _azureToken);
            KeyVaultManagementClient client = new KeyVaultManagementClient(credentials);

            // Check if a vault already exists
            bool found = false;
            Vault vault = null;
            VaultListResponse vaults = client.Vaults.List(resourceGroup, 100);
            foreach (var v in vaults.Vaults)
            {
                found = v.Name.EqualsIgnoreCase(vaultName);
                if (!found) continue;
                vault = (Vault) v;
                break;
            }

            // Create the vault
            if (!found)
            {
                vault = client.Vaults.CreateOrUpdate(resourceGroup, vaultName, new VaultCreateOrUpdateParameters()).Vault;
            }

            // Create the secret
            KeyVaultClient kvClient = new KeyVaultClient(GetAccessToken);
            var secret = kvClient.SetSecretAsync(vault.Properties.VaultUri, secretName, connectionString, new Dictionary<string, string>() {{organizationId, tenantId}},
                                                 null, new SecretAttributes() {Enabled = true}).GetAwaiter().GetResult();
            

            return new ActionResponse(ActionStatus.Success, JsonUtility.GetEmptyJObject());
        }
    }
}