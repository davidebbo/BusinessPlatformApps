using System.Threading.Tasks;
using Microsoft.Deployment.Common.ActionModel;

namespace Microsoft.Deployment.Common.Actions
{
    public abstract class BaseAction : IAction
    {
        // Partners should append their own name infront of the actions for telemtry
        public virtual string OperationUniqueName =>  $"Microsoft-{this.GetType().Name}";

        public abstract Task<ActionResponse> ExecuteActionAsync(ActionRequest request);
    }
}
