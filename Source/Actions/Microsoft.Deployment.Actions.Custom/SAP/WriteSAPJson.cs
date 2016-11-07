using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Deployment.Common.ActionModel;
using Microsoft.Deployment.Common.Actions;
using Microsoft.Deployment.Common.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Deployment.Actions.Custom.SAP
{
    [Export(typeof(IAction))]
    public class WriteSAPJson : BaseAction
    {
        public override async Task<ActionResponse> ExecuteActionAsync(ActionRequest request)
        {
            var rowBatchSize = request.DataStore.GetValue("RowBatchSize");
            var sapClient = request.DataStore.GetValue("SapClient");
            var sapHost = request.DataStore.GetValue("SapHost");
            var sapLanguage = request.DataStore.GetValue("SapLanguage");
            var sapRouterString = request.DataStore.GetValue("SapRouterString");
            var sapSystemId = request.DataStore.GetValue("SapSystemId");
            var sapSystemNumber = request.DataStore.GetValue("SapSystemNumber");
            var sqlConnectionString = request.DataStore.GetValue("SqlConnectionString");

            string jsonDestination = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), JSON_PATH);
            (new FileInfo(jsonDestination)).Directory.Create();

            JObject config = new JObject(
                new JProperty("RowBatchSize", rowBatchSize),
                new JProperty("SapClient", sapClient),
                new JProperty("SapHost", sapHost),
                new JProperty("SapLanguage", sapLanguage),
                new JProperty("SapRouterString", sapRouterString),
                new JProperty("SapSystemId", sapSystemId),
                new JProperty("SapSystemNumber", sapSystemNumber),
                new JProperty("SqlConnectionString", sqlConnectionString)
            );

            using (StreamWriter file = File.CreateText(jsonDestination))
            { 
                using (JsonTextWriter writer = new JsonTextWriter(file))
                {
                    config.WriteTo(writer);
                }
            }

            return new ActionResponse(ActionStatus.Success, JsonUtility.GetEmptyJObject());
        }

        private const string JSON_PATH = @"Simplement, Inc\Solution Template AR\config.json";
    }
}