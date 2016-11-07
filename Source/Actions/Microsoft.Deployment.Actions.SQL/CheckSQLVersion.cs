using System;
using System.ComponentModel.Composition;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Deployment.Common.ActionModel;
using Microsoft.Deployment.Common.Actions;
using Microsoft.Deployment.Common.Enums;
using Microsoft.Deployment.Common.Helpers;

namespace Microsoft.Deployment.Actions.SQL
{
    [Export(typeof(IAction))]
    public class CheckSQLVersion : BaseAction
    {
        public override async Task<ActionResponse> ExecuteActionAsync(ActionRequest request)
        {
            // TODO fix hardcoded string as action here
            string connectionString = request.DataStore.GetAllValues("SqlConnectionString")[1];

            DataTable result = SqlUtility.RunCommand(connectionString, "SELECT SERVERPROPERTY('ProductVersion') AS SqlVersion, SERVERPROPERTY('IsLocalDB') AS IsLocalDB, SERVERPROPERTY('Edition') AS SqlEdition", SqlCommandType.ExecuteWithData);
            string serverVersion =(string)result.Rows[0]["SqlVersion"];
            int majorServerVersion = int.Parse(serverVersion.Substring(0, serverVersion.IndexOf('.')));

            if (majorServerVersion < 11)
                return new ActionResponse(ActionStatus.Failure, JsonUtility.GetEmptyJObject(), "SQLVersionTooLow");

            if ( Convert.ToString(result.Rows[0]["SqlEdition"]).IndexOf("Express Edition", StringComparison.OrdinalIgnoreCase) > -1 || Convert.ToInt32(result.Rows[0]["IsLocalDB"]) == 1)
                return new ActionResponse(ActionStatus.Failure, JsonUtility.GetEmptyJObject(), "SQLDenyLocalAndExpress");


            return new ActionResponse(ActionStatus.Success, JsonUtility.GetEmptyJObject());
        }
    }
}