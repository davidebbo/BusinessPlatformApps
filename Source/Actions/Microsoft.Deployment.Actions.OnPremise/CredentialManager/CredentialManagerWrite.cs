using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Deployment.Common.ActionModel;
using Microsoft.Deployment.Common.Actions;
using Newtonsoft.Json.Linq;
using Simple.CredentialManager;

namespace Microsoft.Deployment.Actions.OnPremise.CredentialManager
{
    [Export(typeof(IAction))]
    public class CredentialManagerWrite : BaseAction
    {
        public override async Task<ActionResponse> ExecuteActionAsync(ActionRequest request)
        {
            string targetName = request.DataStore.GetValue("CredentialTarget");
            string userName = request.DataStore.GetValue("CredentialUsername");
            string password = request.DataStore.GetValue("CredentialPassword");

            Credential c = new Credential(userName, password, targetName, CredentialType.Generic);
            c.PersistenceType = PersistenceType.LocalComputer;

            if (c.Save())
                return new ActionResponse(ActionStatus.Success, new JObject());
            else
                return new ActionResponse(ActionStatus.Failure, new JObject(), new Win32Exception(Marshal.GetLastWin32Error()), "CredMgrWriteError");
        }
    }
}