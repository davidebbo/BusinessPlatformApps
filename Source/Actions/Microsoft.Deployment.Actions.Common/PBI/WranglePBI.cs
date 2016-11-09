using System.ComponentModel.Composition;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Deployment.Common.ActionModel;
using Microsoft.Deployment.Common.Actions;
using Microsoft.Deployment.Common.Helpers;

namespace Microsoft.Deployment.Actions.Common.PBI
{
    [Export(typeof(IAction))]
    public class WranglePBI : BaseAction
    {
        public override async Task<ActionResponse> ExecuteActionAsync(ActionRequest request)
        {
            int sqlIndex = 0;
            if (request.DataStore.KeyExists("SqlServerIndex"))
            {
                sqlIndex = int.Parse(request.DataStore.GetValue("SqlServerIndex"));
            }

            var filename = request.DataStore.GetValue("FileName");
            string connectionString = request.DataStore.GetAllValues("SqlConnectionString")[sqlIndex];

            var templateFullPath = request.Info.App.AppFilePath.Replace("\\", "/") + $"/service/PowerBI/{filename}";
            var tempfileName = Path.GetRandomFileName();
            var templateTempFullPath = request.ControllerModel.ServiceRootFilePath.Replace("\\","/") + $"/Temp/{tempfileName}/{filename}";
            Directory.CreateDirectory(request.ControllerModel.ServiceRootFilePath + $"/Temp/{tempfileName}");

            var creds = SqlUtility.GetSqlCredentialsFromConnectionString(connectionString);

            using (PBIXUtils wrangler = new PBIXUtils(templateFullPath, templateTempFullPath))
            {
                wrangler.ReplaceKnownVariableinMashup("STSqlServer", creds.Server);
                wrangler.ReplaceKnownVariableinMashup("STSqlDatabase", creds.Database);
            }

            string serverPath = request.ControllerModel.ServiceRootFilePath + $"/Temp/{tempfileName}/{filename}";
            return new ActionResponse(ActionStatus.Success, JsonUtility.GetJObjectFromStringValue(serverPath));
        }
    }
}