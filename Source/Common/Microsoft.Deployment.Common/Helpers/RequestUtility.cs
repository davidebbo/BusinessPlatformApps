using System.Linq;
using System.Threading.Tasks;
using Microsoft.Deployment.Common.ActionModel;

namespace Microsoft.Deployment.Common.Helpers
{
    public class RequestUtility
    {
        public static async Task<ActionResponse> CallAction(ActionRequest request, string actionName)
        {
            var action = request.ControllerModel.AppFactory.Actions[actionName];
            return await action.ExecuteActionAsync(request);
        }
    }
}