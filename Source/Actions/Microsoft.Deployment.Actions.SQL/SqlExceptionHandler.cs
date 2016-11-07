using System;
using System.ComponentModel.Composition;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.Deployment.Common.ActionModel;
using Microsoft.Deployment.Common.Actions;
using Microsoft.Deployment.Common.ErrorCode;
using Microsoft.Deployment.Common.Helpers;

namespace Microsoft.Deployment.Actions.SQL
{
    [Export(typeof(IActionExceptionHandler))]
    public class SqlExceptionHandler : IActionExceptionHandler
    {
        public Type ExceptionExpected { get; } = typeof(SqlException);

        public async Task<ActionResponse> HandleExceptionAsync(ActionRequest request,  Exception exception)
        {
            SqlException sqlException = exception as SqlException;
            if (sqlException != null)
                switch (sqlException.Number)
                {
                    case 18456:
                        return new ActionResponse(ActionStatus.FailureExpected, JsonUtility.GetEmptyJObject(), exception, SqlErrorCodes.LoginFailed);
                }

            return new ActionResponse(ActionStatus.UnhandledException);
        }
    }
}
