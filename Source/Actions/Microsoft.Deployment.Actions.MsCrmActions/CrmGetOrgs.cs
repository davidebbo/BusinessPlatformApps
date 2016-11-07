

namespace Microsoft.Deployment.Common.Actions.MsCrm
{

    using Microsoft.Deployment.Common.ActionModel;
    using Microsoft.Deployment.Common.Actions;
    using Microsoft.Deployment.Common.Helpers;
    using Model;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System.ComponentModel.Composition;
    using System.Dynamic;
    using System.Net;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;

    public class CrmGetOrgs : BaseAction
    {
        [Export(typeof(IAction))]
        public override async Task<ActionResponse> ExecuteActionAsync(ActionRequest request)
        {
            string token = request.DataStore.GetAllValues("Token")[0];
            AuthenticationHeaderValue bearer = new AuthenticationHeaderValue("Bearer", token);

            RestClient rc = new RestClient(MsCrmEndpoints.ENDPOINT, bearer);
            string response = rc.Get(MsCrmEndpoints.URL_ORGANIZATIONS);
            MsCrmOrganization[] orgs = JsonConvert.DeserializeObject<MsCrmOrganization[]>(response);

            for (int i=0; i<orgs.Length; i++)
            {
                response = rc.Get(MsCrmEndpoints.URL_ORGANIZATION_METADATA, $"organizationUrl={WebUtility.UrlEncode(orgs[i].OrganizationUrl)}");
                orgs[i] = JsonConvert.DeserializeObject<MsCrmOrganization>(response);
            }


            // This is a bit of a dance to accomodate ActionResponse and its need for a JObject
            response = JsonConvert.SerializeObject(orgs);

            dynamic d = new ExpandoObject();
            return string.IsNullOrWhiteSpace(response)
                ? new ActionResponse(ActionStatus.Failure, new JObject(), "MsCrm_NoOrgs")
                : new ActionResponse(ActionStatus.Success, JsonUtility.GetJObjectFromStringValue(response));
        }
    }
}