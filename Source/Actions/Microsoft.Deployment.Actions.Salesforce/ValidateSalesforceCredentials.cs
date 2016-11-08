using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Microsoft.Deployment.Actions.Salesforce.Helpers;
using Microsoft.Deployment.Actions.Salesforce.SalesforceSOAP;
using Microsoft.Deployment.Common.ActionModel;
using Microsoft.Deployment.Common.Actions;
using Microsoft.Deployment.Common.Helpers;

namespace Microsoft.Deployment.Actions.Salesforce
{
    [Export(typeof(IAction))]
    public class ValidateSalesforceCredentials : BaseAction
    {
        public string sandboxUrl = "https://test.salesforce.com/";

        public override async Task<ActionResponse> ExecuteActionAsync(ActionRequest request)
        {
            string sfUsername = request.DataStore.GetValue("SalesforceUser");
            string sfPassword = request.DataStore.GetValue("SalesforcePassword");
            string sfToken = request.DataStore.GetValue("SalesforceToken");
            string sfTestUrl = request.DataStore.GetValue("SalesforceUrl");

            SoapClient binding = new SoapClient("Soap");
            LoginResult lr;

            SecurityHelper.SetTls12();

            binding.ClientCredentials.UserName.UserName = sfUsername;
            binding.ClientCredentials.UserName.Password = sfPassword;

            if (!string.IsNullOrEmpty(sfTestUrl) && sfTestUrl == this.sandboxUrl)
            {
                binding.Endpoint.Address = new System.ServiceModel.EndpointAddress(binding.Endpoint.Address.ToString().Replace("login", "test"));
            }

            try
            {
                lr =
                   binding.login(null, null, sfUsername,
                   string.Concat(sfPassword, sfToken));
            }
            catch (Exception e)
            {
                return new ActionResponse(ActionStatus.Failure, JsonUtility.GetEmptyJObject(), e, "SalesforceLoginInvalid");
            }

            return new ActionResponse(ActionStatus.Success, lr);
        }
    }
}
