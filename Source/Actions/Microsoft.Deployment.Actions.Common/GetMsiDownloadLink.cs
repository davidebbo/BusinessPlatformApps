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
    public class GetMsiDownloadLink : BaseAction
    {
        public override async Task<ActionResponse> ExecuteActionAsync(ActionRequest request)
        {
            var msi = System.IO.Directory.GetFiles(request.Info.App.AppFilePath, "*.exe");
            if (msi.Length == 1)
            {
                var file = new FileInfo(msi.First());
                string serverPath = request.Info.SerivceRootUrl + "/" + request.Info.App.AppRelativeFilePath + $"/{file.Name}";
                serverPath.Replace("\\", "/");
                return new ActionResponse(ActionStatus.Success, JsonUtility.GetJObjectFromStringValue(serverPath));
            }

            //try
            //{
            //    string evaluationEmail = request.Message["EvaluationEmail"] == null
            //        ? string.Empty
            //        : request.Message["EvaluationEmail"][0].ToString();
            //    if (!string.IsNullOrEmpty(evaluationEmail))
            //    {
            //        request.Logger.LogCustomProperty("Email", evaluationEmail);
            //    }
            //}
            //catch
            //{
            //    // Logging email failed
            //}

            return new ActionResponse(ActionStatus.Failure, JsonUtility.GetEmptyJObject(), null, DefaultErrorCodes.DefaultErrorCode, "Msi count:" + msi.Length);
        }
    }
}