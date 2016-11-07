using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Microsoft.Deployment.Common.ActionModel;
using Microsoft.Deployment.Common.Actions;
using Microsoft.Deployment.Common.Helpers;

namespace Microsoft.Deployment.Actions.OnPremise.WinNT
{
    [Export(typeof(IAction))]
    public class AddLogonAsBatchPermission : BaseAction
    {
        public override async Task<ActionResponse> ExecuteActionAsync(ActionRequest request)
        {
            string domain = request.DataStore.GetValue("ImpersonationDomain") == null || string.IsNullOrEmpty(request.DataStore.GetValue("ImpersonationDomain"))
                ? Environment.GetEnvironmentVariable("USERDOMAIN")
                : request.DataStore.GetValue("ImpersonationDomain");
            string user = request.DataStore.GetValue("ImpersonationUsername") == null || string.IsNullOrEmpty(request.DataStore.GetValue("ImpersonationUsername"))
                ? Environment.GetEnvironmentVariable("USERNAME")
                : request.DataStore.GetValue("ImpersonationUsername");

            string domainAccount = $"{NTHelper.CleanDomain(domain)}\\{NTHelper.CleanUsername(user)}";

            // This will throw an error if the permission cannot be granted
            NTPermissionUtility.SetRight(Environment.MachineName, domainAccount, NTPermissionUtility.LOGON_AS_BATCH_PERM, false);

            return new ActionResponse(ActionStatus.Success, JsonUtility.GetEmptyJObject());
        }
    }
}