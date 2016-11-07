using System;
using System.ComponentModel.Composition;
using System.DirectoryServices.AccountManagement;
using System.Threading.Tasks;
using Microsoft.Deployment.Common.ActionModel;
using Microsoft.Deployment.Common.Actions;
using Microsoft.Deployment.Common.Helpers;

namespace Microsoft.Deployment.Actions.OnPremise.WinNT
{
    [Export(typeof(IAction))]
    public class ValidateNtCredential : BaseAction
    {
        public override async Task<ActionResponse> ExecuteActionAsync(ActionRequest request)
        {
            string domain = NTHelper.CleanDomain(request.DataStore.GetValue("ImpersonationDomain"));
            string user = NTHelper.CleanUsername(request.DataStore.GetValue("ImpersonationUsername"));
            string password = request.DataStore.GetValue("ImpersonationPassword");

            bool isValid;
            ContextType context = Environment.MachineName.EqualsIgnoreCase(domain) ? ContextType.Machine : ContextType.Domain;

            using (PrincipalContext pc = new PrincipalContext(context, domain))
            {
                // validate the credentials
                isValid = pc.ValidateCredentials(user, password, ContextOptions.Negotiate);
            }

            return isValid
                ? new ActionResponse(ActionStatus.Success, JsonUtility.GetEmptyJObject())
                : new ActionResponse(ActionStatus.Failure, JsonUtility.GetEmptyJObject(), "IncorrectNTCredentials");
        }
    }
}