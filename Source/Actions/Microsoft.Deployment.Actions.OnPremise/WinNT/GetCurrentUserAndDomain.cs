using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Deployment.Common.ActionModel;
using Microsoft.Deployment.Common.Actions;
using Microsoft.Deployment.Common.Helpers;

namespace Microsoft.Deployment.Actions.OnPremise.WinNT
{
    [Export(typeof(IAction))]
    public class GetCurrentUserAndDomain : BaseAction
    {
        public override async Task<ActionResponse> ExecuteActionAsync(ActionRequest request)
        {
            string domain = Environment.GetEnvironmentVariable("USERDOMAIN");
            string user = Environment.GetEnvironmentVariable("USERNAME");
            string domainAccount = $"{NTHelper.CleanDomain(domain)}\\{NTHelper.CleanUsername(user)}";

            return new ActionResponse(ActionStatus.Success, domainAccount);
        }
    }
}
