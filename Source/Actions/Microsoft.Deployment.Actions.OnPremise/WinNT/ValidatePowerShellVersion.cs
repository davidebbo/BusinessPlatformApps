using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Deployment.Common.ActionModel;
using Microsoft.Deployment.Common.Actions;
using Microsoft.Deployment.Common.Helpers;

namespace Microsoft.Deployment.Actions.OnPremise.WinNT
{
    [Export(typeof(IAction))]
    public class ValidatePowerShellVersion : BaseAction
    {
        public override async Task<ActionResponse> ExecuteActionAsync(ActionRequest request)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = "-command \"exit $PSVersionTable.PSVersion.Major\"",
                WindowStyle = ProcessWindowStyle.Hidden
            };

            Process p = new Process { StartInfo = startInfo };

            if (!p.Start())
            {
                return new ActionResponse(ActionStatus.Failure, JsonUtility.GetEmptyJObject(), "CannotStartPowerShell");
            }

            p.WaitForExit();

            return p.ExitCode >= 5 ? new ActionResponse(ActionStatus.Success, JsonUtility.GetEmptyJObject()) :
                                     new ActionResponse(ActionStatus.Failure, JsonUtility.GetEmptyJObject(), "LowPowerShellVersion");
        }
    }
}