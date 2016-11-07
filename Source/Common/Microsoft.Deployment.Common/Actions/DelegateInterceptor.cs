using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Deployment.Common.ActionModel;
using Microsoft.Deployment.Common.Helpers;

namespace Microsoft.Deployment.Common.Actions
{
    [Export(typeof(IActionRequestInterceptor))]
    public class DelegateInterceptor : IActionRequestInterceptor
    {
#pragma warning disable 1998
        // Defined by the interface
        public async Task<InterceptorStatus> CanInterceptAsync(IAction actionToExecute, ActionRequest request)
#pragma warning restore 1998
        {
            bool impersonationFound = request.DataStore.KeyExists("ImpersonateAction") && request.DataStore.GetValue("ImpersonateAction") == "true";

            if (impersonationFound)
            {
                return InterceptorStatus.IntercepAndHandleAction;
            }

            return InterceptorStatus.Skipped;
        }

        public async Task<ActionResponse> InterceptAsync(IAction actionToExecute, ActionRequest request)
        {
            return await ImpersonateUtility.ExecuteAsync(actionToExecute.ExecuteActionAsync, request);
        }
    }
}
