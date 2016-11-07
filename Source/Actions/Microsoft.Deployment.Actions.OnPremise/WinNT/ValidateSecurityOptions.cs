using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Microsoft.Deployment.Common.ActionModel;
using Microsoft.Deployment.Common.Actions;
using Microsoft.Deployment.Common.Helpers;
using Microsoft.Win32;

namespace Microsoft.Deployment.Actions.OnPremise.WinNT
{
    // Should not run impersonated
    [Export(typeof(IAction))]
    public class ValidateSecurityOptions : BaseAction
    {
        public override async Task<ActionResponse> ExecuteActionAsync(ActionRequest request)
        {
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\Lsa"))
            {
                if (key == null)
                    return new ActionResponse(ActionStatus.Success, JsonUtility.GetEmptyJObject());

                object v = key.GetValue("disabledomaincreds");

                if (v != null && (int)v == 1)
                    return new ActionResponse(ActionStatus.Failure, JsonUtility.GetEmptyJObject(), "DisabledDomainCredsEnabled");
            }

            return new ActionResponse(ActionStatus.Success, JsonUtility.GetEmptyJObject());
        }
    }
}