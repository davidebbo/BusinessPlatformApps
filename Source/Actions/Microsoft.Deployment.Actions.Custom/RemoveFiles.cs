using System.ComponentModel.Composition;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Deployment.Common.ActionModel;
using Microsoft.Deployment.Common.Actions;
using Microsoft.Deployment.Common.Helpers;

namespace Microsoft.Deployment.Actions.Custom
{
    [Export(typeof(IAction))]
    public class RemoveFiles : BaseAction
    {
        public override async Task<ActionResponse> ExecuteActionAsync(ActionRequest request)
        {
            string path = FileUtility.GetLocalTemplatePath(request.Info.AppName);

            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
                return new ActionResponse(ActionStatus.Success, JsonUtility.GetEmptyJObject());
            }

            return new ActionResponse(ActionStatus.Failure, "Target directory not found.");
        }
    }
}
