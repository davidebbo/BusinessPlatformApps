
using Newtonsoft.Json.Linq;

namespace Microsoft.Deployment.Common.Actions
{
    public class DeploymentAction
    {
        public DeploymentAction(string displayName, IAction action, JToken additionalParameters)
        {
            DisplayName = displayName;
            Action = action;
            this.OperationName = this.Action.OperationUniqueName;
            AdditionalParameters = additionalParameters;
        }

        public string DisplayName { get; private set; }
        public IAction Action { get; private set; }
        public JToken AdditionalParameters { get; private set; }
        public string OperationName { get; set; }
    }
}
