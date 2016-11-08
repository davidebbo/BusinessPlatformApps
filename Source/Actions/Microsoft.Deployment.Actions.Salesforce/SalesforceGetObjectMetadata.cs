using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Dynamic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using Microsoft.Deployment.Actions.Salesforce.Helpers;
using Microsoft.Deployment.Actions.Salesforce.SalesforceSOAP;
using Microsoft.Deployment.Common.ActionModel;
using Microsoft.Deployment.Common.Actions;
using Microsoft.Deployment.Common.Helpers;

namespace Microsoft.Deployment.Actions.Salesforce
{
    [Export(typeof(IAction))]
    class SalesforceGetObjectMetadata : BaseAction
    {
        private string sandboxUrl = "https://test.salesforce.com/";

        public override async Task<ActionResponse> ExecuteActionAsync(ActionRequest request)
        {
            string objects = request.DataStore.GetValue("ObjectTables");
            string sfUsername = request.DataStore.GetValue("SalesforceUser");
            string sfPassword = request.DataStore.GetValue("SalesforcePassword");
            string sfToken = request.DataStore.GetValue("SalesforceToken");
            string sfTestUrl = request.DataStore.GetValue("SalesforceUrl");
            List<string> sfObjects = objects.Split(',').ToList();

            SoapClient binding = new SoapClient("Soap");

            if (!string.IsNullOrEmpty(sfTestUrl) && sfTestUrl == this.sandboxUrl)
            {
                binding.Endpoint.Address = new System.ServiceModel.EndpointAddress(binding.Endpoint.Address.ToString().Replace("login", "test"));
            }

            LoginResult lr;

            SecurityHelper.SetTls12();

            binding.ClientCredentials.UserName.UserName = sfUsername;
            binding.ClientCredentials.UserName.Password = sfPassword;

            lr =
               binding.login(null, null,
               sfUsername,
               string.Concat(sfPassword, sfToken));

            dynamic metadata = new ExpandoObject();

            binding = new SoapClient("Soap");
            metadata.Objects = new List<DescribeSObjectResult>();
            SessionHeader sheader = new SessionHeader();
            BasicHttpBinding bind = new BasicHttpBinding();
            bind = (BasicHttpBinding)binding.Endpoint.Binding;
            bind.MaxReceivedMessageSize = 2147483647;
            bind.MaxBufferPoolSize = 2147483647;
            bind.MaxBufferSize = 2147483647;
            bind.CloseTimeout = new TimeSpan(0, 0, 5, 0);
            bind.OpenTimeout = new TimeSpan(0, 0, 5, 0);
            bind.ReaderQuotas.MaxArrayLength = 2147483647;
            bind.ReaderQuotas.MaxDepth = 2147483647;
            bind.ReaderQuotas.MaxNameTableCharCount = 2147483647;
            bind.ReaderQuotas.MaxStringContentLength = 2147483647;
            bind.ReaderQuotas.MaxBytesPerRead = 2147483647;
            bind.ReaderQuotas.MaxNameTableCharCount = 2147483647;

            binding.Endpoint.Binding = bind;
            binding.Endpoint.Address = new EndpointAddress(lr.serverUrl);

            sheader.sessionId = lr.sessionId;

            binding.Endpoint.ListenUri = new Uri(lr.metadataServerUrl);

            foreach (var obj in sfObjects)
            {
                DescribeSObjectResult sobject;
                
                binding.describeSObject(sheader, null, null, null, obj, out sobject);

                metadata.Objects.Add(sobject);
            }

            return new ActionResponse(ActionStatus.Success, JsonUtility.GetJObjectFromObject(metadata));
        }
    }
}