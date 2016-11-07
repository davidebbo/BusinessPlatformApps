namespace Microsoft.Deployment.Common.Actions.MsCrm
{
    using Microsoft.Deployment.Common.ActionModel;
    using Microsoft.Deployment.Common.Actions;
    using Microsoft.Deployment.Common.Helpers;
    using Model;
    using Newtonsoft.Json;
    using System.ComponentModel.Composition;
    using System.Net;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;

    public class CrmDeleteProfile : BaseAction
    {
        private RestClient _rc;
        private string _token;
        private string _orgId;

        [Export(typeof(IAction))]
        public override async Task<ActionResponse> ExecuteActionAsync(ActionRequest request)
        {
            _token = request.DataStore.GetAllValues("Token")[0];
            _orgId = request.DataStore.GetAllValues("organizationId")[0];
            string name = request.DataStore.GetAllValues("ProfileName")[0];

            AuthenticationHeaderValue bearer = new AuthenticationHeaderValue("Bearer", _token);
            _rc = new RestClient(request.DataStore.GetAllValues("ConnectorUrl")[0], bearer);

            string response = _rc.Get(MsCrmEndpoints.URL_PROFILES, $"organizationId={WebUtility.UrlEncode(_orgId)}");
            MsCrmProfile[] profiles = JsonConvert.DeserializeObject<MsCrmProfile[]>(response);

            foreach (MsCrmProfile p in profiles)
            {
                if (p.Name.EqualsIgnoreCase(name) && !p.State.EqualsIgnoreCase("3"))
                    _rc.Delete(MsCrmEndpoints.URL_PROFILES + "/" + p.Id);
            }

            return new ActionResponse(ActionStatus.Success, JsonUtility.GetEmptyJObject());
        }
    }
}
