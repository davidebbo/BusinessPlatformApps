using System.ComponentModel.Composition;
using System.Dynamic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hyak.Common;
using Microsoft.Azure;
using Microsoft.Azure.Subscriptions;
using Microsoft.Deployment.Common.ActionModel;
using Microsoft.Deployment.Common.Actions;

namespace Microsoft.Deployment.Actions.AzureCustom.Common
{
    [Export(typeof(IAction))]
    public class GetAzureSubscriptions : BaseAction
    {
        public override async Task<ActionResponse> ExecuteActionAsync(ActionRequest request)
        {
            var azureToken = request.DataStore.GetJson("AzureToken")["access_token"].ToString();

            CloudCredentials creds = new TokenCloudCredentials(azureToken);
            Microsoft.Azure.Subscriptions.SubscriptionClient client = new SubscriptionClient(creds);
            var subscriptionList = (await client.Subscriptions.ListAsync(new CancellationToken())).Subscriptions.ToList();
            dynamic subscriptionWrapper = new ExpandoObject();
            subscriptionWrapper.value = subscriptionList;
            return new ActionResponse(ActionStatus.Success, subscriptionWrapper);
        }
    }
}