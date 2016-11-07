using System.ComponentModel.Composition;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.Deployment.Common.ActionModel;
using Microsoft.Deployment.Common.Actions;
using Microsoft.Deployment.Common.Helpers;

namespace Microsoft.Deployment.Actions.OnPremise.WinNT
{
    // Should not run impersonated
    [Export(typeof(IAction))]
    public class ValidateAdminPrivileges : BaseAction
    {
        public override async Task<ActionResponse> ExecuteActionAsync(ActionRequest request)
        {
            WindowsPrincipal current = new WindowsPrincipal(WindowsIdentity.GetCurrent());

            return current.IsInRole(WindowsBuiltInRole.Administrator)
                ? new ActionResponse(ActionStatus.Success, JsonUtility.GetEmptyJObject())
                : new ActionResponse(ActionStatus.Failure, JsonUtility.GetEmptyJObject(), "NotAdmin");
        }
    }
}