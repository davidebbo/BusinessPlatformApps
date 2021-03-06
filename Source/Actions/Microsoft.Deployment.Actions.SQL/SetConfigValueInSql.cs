﻿using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Deployment.Common.ActionModel;
using Microsoft.Deployment.Common.Actions;
using Microsoft.Deployment.Common.ErrorCode;
using Microsoft.Deployment.Common.Helpers;
using Newtonsoft.Json.Linq;

namespace Microsoft.Deployment.Actions.SQL
{
    [Export(typeof(IAction))]
    public class SetConfigValueInSql : BaseAction
    {
        public override async Task<ActionResponse> ExecuteActionAsync(ActionRequest request)
        {
            // Provided by the json 
            var sqlIndex = int.Parse(request.DataStore.GetValue("SqlServerIndex"));
            string configTable = request.DataStore.GetValue("SqlConfigTable");


            // Provided by thge user including the messages below
            string connectionString = request.DataStore.GetAllValues("SqlConnectionString")[sqlIndex].ToString();
                // Must specify Initial Catalog

            // Get list of settings to deploy;
            JToken listGroup = request.DataStore.GetJson("SqlGroup");
            JToken listSubgroup = request.DataStore.GetJson("SqlSubGroup");
            JToken listConfigEntryName = request.DataStore.GetJson("SqlEntryName");
            JToken listConfigEntryValue = request.DataStore.GetJson("SqlEntryValue");

            if (listGroup == null || listSubgroup == null || listConfigEntryName == null || listConfigEntryValue == null)
            {
                return new ActionResponse(ActionStatus.Success, JsonUtility.GetEmptyJObject(),null, DefaultErrorCodes.DefaultErrorCode, 
                    "Configuration value properties not found");
            }

            if (listGroup.Type != JTokenType.Array || listSubgroup.Type != JTokenType.Array ||
                listConfigEntryName.Type != JTokenType.Array || listConfigEntryValue.Type != JTokenType.Array)
            {
                return new ActionResponse(ActionStatus.Success, JsonUtility.GetEmptyJObject(), null, DefaultErrorCodes.DefaultErrorCode, "Configuration is invalid");
            }


            for (int i = 0; i < listGroup.Count(); i++)
            {
                string group = request.DataStore.GetJson("SqlGroup")[i].ToString();
                string subgroup = request.DataStore.GetJson("SqlSubGroup")[i].ToString();
                string configEntryName = request.DataStore.GetJson("SqlEntryName")[i].ToString();
                string configEntryValue = request.DataStore.GetJson("SqlEntryValue")[i].ToString();

                string query = string.Format(queryTemplate, configTable, group, subgroup, configEntryName,
                    configEntryValue);

                SqlUtility.InvokeSqlCommand(connectionString, query, null);
            }

            return new ActionResponse(ActionStatus.Success, JsonUtility.GetEmptyJObject());
        }

        private const string queryTemplate = @"MERGE {0} AS t  
                                           USING ( VALUES('{1}', '{2}', '{3}', '{4}') ) AS s(configuration_group, configuration_subgroup, [name], [value])
                                           ON t.configuration_group=s.configuration_group AND t.configuration_subgroup=s.configuration_subgroup AND t.[name]=s.[name]
                                           WHEN matched THEN
                                               UPDATE SET [value]=s.[value]
                                           WHEN NOT matched THEN
                                               INSERT (configuration_group, configuration_subgroup, [name], [value]) VALUES (s.configuration_group, s.configuration_subgroup, s.[name], s.[value]);";

    }
}