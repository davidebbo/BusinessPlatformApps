using System;
using System.ComponentModel.Composition;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Deployment.Common.ActionModel;
using Microsoft.Deployment.Common.Actions;
using Microsoft.Deployment.Common.Enums;
using Microsoft.Deployment.Common.Helpers;

namespace Microsoft.Deployment.Actions.Custom.SCCM
{
    [Export(typeof(IAction))]
    public class CheckSCCMVersion : BaseAction
    {
        private readonly int[] MinVersion = {5, 0, 8239, 0};

        public override async Task<ActionResponse> ExecuteActionAsync(ActionRequest request)
        {
            string connectionString = request.DataStore.GetValue("SqlConnectionString");

            // Check if the Sites table exists. If not this isn't a SCCM database
            DataTable result = SqlUtility.RunCommand(connectionString, "SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME='Sites' AND TABLE_TYPE='BASE TABLE' AND TABLE_SCHEMA='dbo'", SqlCommandType.ExecuteWithData);
            if (result == null || result.Rows.Count == 0)
                return new ActionResponse(ActionStatus.Failure, JsonUtility.GetEmptyJObject(), "NotSccmDB");

            // Retrieve SCCM version
            result = SqlUtility.RunCommand(connectionString, " SELECT [Version] FROM dbo.Sites WHERE ReportToSite='' ", SqlCommandType.ExecuteWithData);
            
            // If the version is not there, report and exit
            if (result == null || result.Rows.Count == 0 || result.Rows[0]["Version"] == DBNull.Value)
                return new ActionResponse(ActionStatus.Failure, JsonUtility.GetEmptyJObject(), "SccmVersionNotFound");

            // Parse and decide
            string detectedVersion = Convert.ToString(result.Rows[0]["Version"]);
            string[] detectedVersionSplit = detectedVersion.Split('.');

            if (detectedVersionSplit.Length != 4)
                return new ActionResponse(ActionStatus.Failure, JsonUtility.GetEmptyJObject(), "SccmVersionFormatUnexpected");

            bool isHigher = true;
            for (int i = 0; i < MinVersion.Length; i++)
            {
                int versionPart = int.Parse(detectedVersionSplit[i]);

                if (versionPart > MinVersion[i])
                    break;
                if (versionPart < MinVersion[i])
                {
                    isHigher = false;
                    break;
                }
            }

            if (isHigher)
                return new ActionResponse(ActionStatus.Success, JsonUtility.GetEmptyJObject());
            else
                return new ActionResponse(ActionStatus.Failure, JsonUtility.GetEmptyJObject(), "SccmVersionTooLow");
        }
    }
}