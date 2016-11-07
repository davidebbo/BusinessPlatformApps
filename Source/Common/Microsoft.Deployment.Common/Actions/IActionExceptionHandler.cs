using System;
using System.Threading.Tasks;
using Microsoft.Deployment.Common.ActionModel;

namespace Microsoft.Deployment.Common.Actions
{
    public interface IActionExceptionHandler
    {
        Type ExceptionExpected { get; }

        Task<ActionResponse> HandleExceptionAsync(ActionRequest request, Exception exception);
    }
}
