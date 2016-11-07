using System.Linq;
using System.Threading;
using Microsoft.Azure.Management.Resources;
using Microsoft.Azure.Management.Resources.Models;
using Microsoft.Deployment.Common.ActionModel;

namespace Microsoft.Deployment.Actions.Salesforce.Helpers
{
    class DeploymentHelper
    {
        public ActionResponse WaitForDeployment(string resourceGroup, string deploymentName, ResourceManagementClient client)
        {
            while (true)
            {
                Thread.Sleep(5000);
                var status = client.Deployments.GetAsync(resourceGroup, deploymentName, new CancellationToken()).Result;
                var operations = client.DeploymentOperations.ListAsync(resourceGroup, deploymentName, new DeploymentOperationsListParameters(), new CancellationToken()).Result;

                var provisioningState = status.Deployment.Properties.ProvisioningState;
                if (provisioningState == "Accepted" || provisioningState == "Running")
                {
                    continue;
                }
                if (provisioningState == "Succeeded")
                {
                    return new ActionResponse(ActionStatus.Success, operations);
                }

                var operation = operations.Operations.First(p => p.Properties.ProvisioningState == ProvisioningState.Failed);
                var operationFailed = client.DeploymentOperations.GetAsync(resourceGroup, deploymentName, operation.OperationId, new CancellationToken()).Result;

                return new ActionResponse(ActionStatus.Failure, operationFailed);
            }
        }
    }
}
