namespace Microsoft.Deployment.Common.Actions.MsCrm
{
    using Microsoft.Deployment.Common.ActionModel;
    using Microsoft.Deployment.Common.Actions;
    using Microsoft.Deployment.Common.Helpers;
    using Model;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;

    [Export(typeof(IAction))]
    public class CrmCreateProfile : BaseAction
    {
        private RestClient _rc;
        private string _orgUrl;
        private string _token;
        private string _orgId;

        private List<string> RetrieveInvalidEntities(string[] entities)
        {
            List<string> result = new List<string>();
            string response = _rc.Get(MsCrmEndpoints.URL_ENTITIES, "organizationurl=" + _orgUrl);
            MsCrmEntity[] provisionedEntities = JsonConvert.DeserializeObject<MsCrmEntity[]>(response);

            for (int i = 0; i < entities.Length; i++)
            {
                // TODO: This is a hack to avoid bug with systemusermanagermap
                bool found = entities[i].EqualsIgnoreCase("systemusermanagermap");
                // The loop won't execute for the systemusermanagermap
                for (int j = 0; !found && j < provisionedEntities.Length; j++)
                    found = entities[i].EqualsIgnoreCase(provisionedEntities[j].LogicalName);

                if (!found)
                    result.Add(entities[i]);
            }

            return result;
        }

        public override async Task<ActionResponse> ExecuteActionAsync(ActionRequest request)
        {
            _token = request.DataStore.GetAllValues("Token")[0];
            AuthenticationHeaderValue bearer = new AuthenticationHeaderValue("Bearer", _token);
            _rc = new RestClient(request.DataStore.GetAllValues("ConnectorUrl")[0], bearer);

            _orgUrl = request.DataStore.GetAllValues("OrganizationUrl")[0];
            _orgId = request.DataStore.GetAllValues("OrganizationId")[0];
            string name = request.DataStore.GetAllValues("ProfileName")[0];
            string kV = request.DataStore.GetAllValues("kV")[0];
            string[] entities = request.DataStore.GetAllValues("Entities")[0].Split(new[] {',', ' ', '\t'}, StringSplitOptions.RemoveEmptyEntries);

            MsCrmProfile profile = new MsCrmProfile
            {
                Entities = new MsCrmEntity[entities.Length],
                Name = name,
                OrganizationId = _orgId,
                DestinationKeyVaultUri = kV,
                DestinationPrefix = string.Empty,
                DestinationSchemaName = "dbo"
            };

            for (int i = 0; i < profile.Entities.Length; i++)
            {
                MsCrmEntity e = new MsCrmEntity {Type = entities[i]};
                profile.Entities[i] = e;
            }

            List<string> invalidEntities = RetrieveInvalidEntities(entities);

            if (invalidEntities.Count > 0)
                return new ActionResponse(ActionStatus.Failure, JsonUtility.GetEmptyJObject(),
                    new ArgumentException("The following entities are not provisioned for replication: " + string.Join(", ", invalidEntities)),
                    "MsCrm_ErrorCreateProfile");

            try
            {
                string response = _rc.Post(MsCrmEndpoints.URL_PROFILES, JsonConvert.SerializeObject(profile));
                MsCrmProfile createdProfile = JsonConvert.DeserializeObject<MsCrmProfile>(response);

                return new ActionResponse(ActionStatus.Success, new JObject(createdProfile));
            }
            catch (Exception e)
            {
                return new ActionResponse(ActionStatus.Failure, JsonUtility.GetEmptyJObject(), e, "MsCrm_ErrorCreateProfile");
            }
        }
    }
}