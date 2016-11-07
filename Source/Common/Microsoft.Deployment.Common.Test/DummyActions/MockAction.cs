using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Deployment.Common.ActionModel;
using Microsoft.Deployment.Common.Actions;
using System.ComponentModel.Composition;

namespace Microsoft.Deployment.Common.Test.DummyActions
{
    [Export(typeof(IAction))]
    public class MockAction : BaseAction
    {
        public override async Task<ActionResponse> ExecuteActionAsync(ActionRequest request)
        {
            return new ActionResponse(ActionStatus.Success, "Hello"); 
        }
    }
}
