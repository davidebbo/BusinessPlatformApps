using System.ComponentModel.Composition;
using System.Dynamic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Deployment.Common.ActionModel;
using Microsoft.Deployment.Common.Actions;
using Microsoft.Deployment.Common.ErrorCode;
using Microsoft.Deployment.Common.Helpers;

namespace Microsoft.Deployment.Actions.SQL
{
    [Export(typeof(IAction))]
    public class ValidateAzureSqlExists : BaseAction
    {
        public override async Task<ActionResponse> ExecuteActionAsync(ActionRequest request)
        {
            var azureToken = request.DataStore.GetJson("AzureToken")["access_token"].ToString();
            var subscription = request.DataStore.GetJson("SelectedSubscription")["SubscriptionId"].ToString();
            
            string server = request.DataStore.GetJson("SqlCredentials")["Server"].ToString();

            AzureHttpClient httpClient = new AzureHttpClient(azureToken, subscription);
            dynamic payload = new ExpandoObject();
            payload.name = server.Replace(".database.windows.net", "");
            payload.type = "Microsoft.Sql/servers";


            HttpResponseMessage response = await httpClient.ExecuteWithSubscriptionAsync(HttpMethod.Post, $"/providers/Microsoft.Sql/checkNameAvailability", "2014-04-01-preview", 
                JsonUtility.GetJsonStringFromObject(payload));
            string content = await response.Content.ReadAsStringAsync();
            var json =  JsonUtility.GetJObjectFromJsonString(content);
            bool isAvailable = json["available"].ToString().EqualsIgnoreCase("True");
            string reason = json["reason"].ToString();
            string message = json["message"].ToString();

            if (isAvailable)
            {
                return new ActionResponse(ActionStatus.Success, "");
            }
            else
            {
                // Handle basic errorcases here
                return new ActionResponse(ActionStatus.FailureExpected, json, null, SqlErrorCodes.DatabaseAlreadyExists);
            }
        }
    }
}
