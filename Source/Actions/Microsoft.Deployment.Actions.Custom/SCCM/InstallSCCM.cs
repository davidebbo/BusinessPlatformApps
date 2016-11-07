using System.ComponentModel.Composition;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Deployment.Common.ActionModel;
using Microsoft.Deployment.Common.Actions;
using Microsoft.Deployment.Common.Helpers;

namespace Microsoft.Deployment.Actions.Custom.SCCM
{
    [Export(typeof(IAction))]
    public class InstallSCCM : BaseAction
    {
        public const string RESOURCE_PATH = @"Service\Resources\Scripts\";

        public override async Task<ActionResponse> ExecuteActionAsync(ActionRequest request)
        {
            string sccmPath = FileUtility.GetLocalTemplatePath(request.Info.AppName);

            if (!Directory.Exists(sccmPath))
            {
                Directory.CreateDirectory(sccmPath);
            }

            string[] files = Directory.GetFiles(Path.Combine(request.Info.App.AppFilePath, RESOURCE_PATH));

            // Copy the files and overwrite destination files if they already exist.
            foreach (string s in files)
            {
                // Use static Path methods to extract only the file name from the path.
                string fileName = Path.GetFileName(s);
                string destFile = Path.Combine(sccmPath, fileName);
                File.Copy(s, destFile, true);
            }

            return new ActionResponse(ActionStatus.Success, JsonUtility.GetEmptyJObject());
        }
    }
}