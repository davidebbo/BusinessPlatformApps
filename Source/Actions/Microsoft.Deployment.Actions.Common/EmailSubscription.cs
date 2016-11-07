using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Deployment.Common.ActionModel;
using Microsoft.Deployment.Common.Actions;
using Microsoft.Deployment.Common.ErrorCode;
using Microsoft.Deployment.Common.Helpers;

namespace Microsoft.Deployment.Actions.Common
{
    [Export(typeof(IAction))]
    public class EmailSubscription : BaseAction
    {
        public override async Task<ActionResponse> ExecuteActionAsync(ActionRequest request)
        {
            string emailAddress = request.DataStore.GetValue("EmailAddress") == null
                 ? string.Empty
                 : request.DataStore.GetValue("EmailAddress");
            if (!string.IsNullOrEmpty(emailAddress))
            {
                request.Logger.LogCustomProperty("Email", emailAddress);
            }

            return new ActionResponse(ActionStatus.Success);
        }
    }
}
