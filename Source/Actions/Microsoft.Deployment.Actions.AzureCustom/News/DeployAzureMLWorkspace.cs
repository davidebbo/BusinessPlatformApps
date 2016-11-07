//-----------------------------------------------------------------------
// <copyright file="DeployAzureMLWorkspace.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Threading.Tasks;

namespace Microsoft.Bpst.Actions.AzureActions.News
{
    using System.ComponentModel.Composition;
    using System.Threading;
    using System.IO;
    using Microsoft.Azure;
    using Microsoft.Azure.Management.Resources;
    using Microsoft.Azure.Management.Resources.Models;
    using Microsoft.Deployment.Common.ActionModel;
    using Microsoft.Deployment.Common.Actions;
    using Microsoft.Deployment.Common.ErrorCode;
    using Microsoft.Deployment.Common.Helpers;


    [Export(typeof(IAction))]
    public class DeployAzyreMLWorkspace : BaseAction
    {
        public override async Task<ActionResponse> ExecuteActionAsync(ActionRequest request)
        {
            var azureToken = request.DataStore.GetJson("AzureToken")["access_token"].ToString();
            var subscription = request.DataStore.GetJson("SelectedSubscription")["SubscriptionId"].ToString();
            var resourceGroup = request.DataStore.GetValue("SelectedResourceGroup");
            var deploymentName = request.DataStore.GetValue("DeploymentName");

            var workspaceName = request.DataStore.GetValue("WorkspaceName");
            var storageAccountName = request.DataStore.GetValue("StorageAccountName");
            var planName = request.DataStore.GetValue("PlanName");
            var skuName = request.DataStore.GetValue("SkuName");
            var skuTier = request.DataStore.GetValue("SkuTier");
            var skuCapacity = request.DataStore.GetValue("SkuCapacity");

            var param = new AzureArmParameterGenerator();
            param.AddStringParam("name", workspaceName);
            param.AddStringParam("resourcegroup", resourceGroup);
            param.AddStringParam("subscription", subscription);
            param.AddStringParam("newStorageAccountName", storageAccountName);
            param.AddStringParam("planName", planName);
            param.AddStringParam("skuName", skuName);
            param.AddStringParam("skuTier", skuTier);
            param.AddStringParam("skuCapacity", skuCapacity);

            SubscriptionCloudCredentials creds = new TokenCloudCredentials(subscription, azureToken);
            Microsoft.Azure.Management.Resources.ResourceManagementClient client = new ResourceManagementClient(creds);
            var registeration = client.Providers.RegisterAsync("Microsoft.CognitiveServices").Result;

            var armTemplate = JsonUtility.GetJObjectFromJsonString(System.IO.File.ReadAllText(Path.Combine(request.Info.App.AppFilePath, "Service/AzureArm/azureml.json")));
            var armParamTemplate = JsonUtility.GetJObjectFromObject(param.GetDynamicObject());
            armTemplate.Remove("parameters");
            armTemplate.Add("parameters", armParamTemplate["parameters"]);


            var deployment = new Microsoft.Azure.Management.Resources.Models.Deployment()
            {
                Properties = new DeploymentPropertiesExtended()
                {
                    Template = armTemplate.ToString(),
                    Parameters = JsonUtility.GetEmptyJObject().ToString()
                }
            };

            var validate = client.Deployments.ValidateAsync(resourceGroup, deploymentName, deployment, new CancellationToken()).Result;
            if (!validate.IsValid)
            {
                return new ActionResponse(ActionStatus.Failure, JsonUtility.GetJObjectFromObject(validate), null,
                     DefaultErrorCodes.DefaultErrorCode, $"Azure:{validate.Error.Message} Details:{validate.Error.Details}");
            }

            var deploymentItem = client.Deployments.CreateOrUpdateAsync(resourceGroup, deploymentName, deployment, new CancellationToken()).Result;

            return new ActionResponse(ActionStatus.Success);
        }
    }
}

