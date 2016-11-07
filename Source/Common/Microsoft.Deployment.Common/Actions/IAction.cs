using System.Threading.Tasks;
using Microsoft.Deployment.Common.ActionModel;

namespace Microsoft.Deployment.Common.Actions
{
    public interface IAction
    {
        string OperationUniqueName { get; }

        Task<ActionResponse> ExecuteActionAsync(ActionRequest request);
    }
}
