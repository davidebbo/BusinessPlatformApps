using System;
using System.ComponentModel.Composition;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Deployment.Common.ActionModel;
using Microsoft.Deployment.Common.Actions;
using Microsoft.Deployment.Common.Helpers;
using Microsoft.Win32.TaskScheduler;
using Task = Microsoft.Win32.TaskScheduler.Task;

namespace Microsoft.Deployment.Actions.OnPremise.TaskScheduler
{
    [Export(typeof(IAction))]
    public class RunTask : BaseAction
    {
        public override async Task<ActionResponse> ExecuteActionAsync(ActionRequest request)
        {
            var taskName = request.DataStore.GetValue("TaskName");

            TaskCollection tasks = null;

            using (TaskService ts = new TaskService())
            {
                tasks = ts.RootFolder.GetTasks(new Regex(taskName));
                foreach (Task task in tasks)
                {
                    try
                    {
                        RunningTask runningTask = task.Run();
                        return new ActionResponse(ActionStatus.Success, JsonUtility.GetEmptyJObject());
                    }
                    catch (Exception e)
                    {
                        if (NTHelper.IsCredentialGuardEnabled())
                            return new ActionResponse(ActionStatus.Failure, JsonUtility.GetEmptyJObject(), "CredentialGuardEnabled");

                        return new ActionResponse(ActionStatus.Failure, JsonUtility.GetEmptyJObject(), e, "RunningTaskFailed");
                    }
                }
            }

            // We should never return this
            return new ActionResponse(ActionStatus.Failure, JsonUtility.GetEmptyJObject(), "SccmTaskNotFound");
        }
    }
}