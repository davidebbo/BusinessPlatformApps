using System.Threading.Tasks;
using Microsoft.Deployment.Common.ActionModel;

namespace Microsoft.Deployment.Common.Actions
{
    public interface IActionRequestInterceptor
    {
        Task<InterceptorStatus> CanInterceptAsync(IAction actionToExecute, ActionRequest request);

        Task<ActionResponse> InterceptAsync(IAction actionToExecute, ActionRequest request);
    }
}
