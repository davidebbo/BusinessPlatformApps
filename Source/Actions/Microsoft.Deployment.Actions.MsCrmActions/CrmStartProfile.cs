namespace Microsoft.Deployment.Common.Actions.MsCrm
{
    using Microsoft.Deployment.Common.ActionModel;
    using Microsoft.Deployment.Common.Actions;
    using Microsoft.Deployment.Common.Helpers;
    using Model;
    using Newtonsoft.Json;
    using System;
    using System.ComponentModel.Composition;
    using System.Net;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;


    [Export(typeof(IAction))]
    public class CrmStartProfile : BaseAction
    {
        private RestClient _rc;
        private string _orgUrl;
        private string _token;
        private string _orgId;


        private string GetProfileId(string organizationId, string name)
        {
            string response = _rc.Get(MsCrmEndpoints.URL_PROFILES, $"organizationId={WebUtility.UrlEncode(organizationId)}");

            MsCrmProfile[] profiles = JsonConvert.DeserializeObject<MsCrmProfile[]>(response);

            for (int i = 0; i < profiles.Length; i++)
                if (profiles[i].Name.EqualsIgnoreCase(name))
                    return profiles[i].Id;

            return null;
        }

        public override async Task<ActionResponse> ExecuteActionAsync(ActionRequest request)
        {
            _token = request.DataStore.GetAllValues("Token")[0];
            _orgId = request.DataStore.GetAllValues("OrganizationId")[0];
            AuthenticationHeaderValue bearer = new AuthenticationHeaderValue("Bearer", _token);
            _rc = new RestClient(request.DataStore.GetAllValues("ConnectorUrl")[0], bearer);

            string profileId = GetProfileId(_orgId, request.DataStore.GetAllValues("ProfileName")[0]);
            try
            {
                string response = _rc.Post(string.Format(MsCrmEndpoints.URL_PROFILES_ACTIVATE, profileId), string.Empty);
                MsCrmProfile validatedProfile = JsonConvert.DeserializeObject<MsCrmProfile>(response);

                return new ActionResponse(ActionStatus.Success, JsonUtility.GetEmptyJObject());
            }
            catch (Exception e)
            {
                return new ActionResponse(ActionStatus.Failure, JsonUtility.GetEmptyJObject(), e, "MsCrm_ErrorCreateProfile");
            }
        }
    }
}