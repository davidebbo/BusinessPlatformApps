﻿//-----------------------------------------------------------------------
// <copyright file="GetCMDEntities.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Threading.Tasks;

namespace Microsoft.Deployment.Actions.AzureCustom.CDM
{
    using System.ComponentModel.Composition;
    using System.Threading;
    using Microsoft.Azure;
    using Microsoft.Azure.Management.Resources;
    using Microsoft.Azure.Management.Resources.Models;
    using Microsoft.Deployment.Common.ActionModel;
    using Microsoft.Deployment.Common.Actions;
    using Microsoft.Deployment.Common.ErrorCode;
    using Microsoft.Deployment.Common.Helpers;
    using System.Net.Http;

    [Export(typeof(IAction))]
    public class CreateCDMEntity : BaseAction
    {
        public override async Task<ActionResponse> ExecuteActionAsync(ActionRequest request)
        {
            var azureToken = request.DataStore.GetJson("AzureToken")["access_token"].ToString();
            var objectIds = request.DataStore.GetValue("objectIds").ToString();
            var userEntity = request.DataStore.GetValue("userEntity").ToString();

            var cdmEntity = JsonUtility.GetJObjectFromJsonString(System.IO.File.ReadAllText(System.IO.Path.Combine(request.ControllerModel.SiteCommonFilePath, "Service/Arm/sampleCDMEntity.json")));


            AzureHttpClient client = new AzureHttpClient(azureToken);

            var response = await client.ExecuteGenericRequestWithHeaderAsync(HttpMethod.Put, $"https://management.azure.com/providers/Microsoft.PowerApps/objectIds/{objectIds}/userSettings/environment?api-version=2016-11-01", "{}");
            var responseString = await response.Content.ReadAsStringAsync();
            var responseParsed = JsonUtility.GetJsonObjectFromJsonString(responseString);
            var environId = responseParsed["properties"]["settings"]["portalCurrentEnvironmentName"].ToString();

            request.DataStore.AddToDataStore("environId", environId, DataStoreType.Public);

            return new ActionResponse(ActionStatus.Success);
        }
    }
}
