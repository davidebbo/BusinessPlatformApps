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
    public class GetTaskStatus : BaseAction
    {
        public override async Task<ActionResponse> ExecuteActionAsync(ActionRequest request)
        {
            string taskName = request.DataStore.GetValue("TaskName");

            TaskCollection tasks = null;

            using (TaskService ts = new TaskService())
            {
                tasks = ts.RootFolder.GetTasks(new Regex(taskName));

                // We expect only one task to match
                foreach (Task task in tasks)
                {
                    switch (task.LastTaskResult)
                    {
                        case 0: return new ActionResponse(ActionStatus.Success, JsonUtility.GetEmptyJObject());
                        case 411:
                            return new ActionResponse(ActionStatus.Failure, JsonUtility.GetEmptyJObject(),
                                                      new Exception("PowerShell version too low - please upgrade to latest version https://msdn.microsoft.com/en-us/powershell/wmf/5.0/requirements"),
                                                      "TaskSchedulerRunFailed");
                    }

                    if (task.State == TaskState.Running)
                        return new ActionResponse(ActionStatus.BatchNoState, JsonUtility.GetEmptyJObject());

                    if (NTHelper.IsCredentialGuardEnabled())
                        return new ActionResponse(ActionStatus.Failure, JsonUtility.GetEmptyJObject(), "CredentialGuardEnabled");

                    ActionResponse response = new ActionResponse(ActionStatus.Failure, JsonUtility.GetEmptyJObject(),
                        new Exception($"Scheduled task exited with code {task.LastTaskResult}"), "TaskSchedulerRunFailed");
                    response.ExceptionDetail.LogLocation = FileUtility.GetLocalTemplatePath(request.Info.AppName);

                    return response;
                }
            }

            // We should never return this
            return new ActionResponse(ActionStatus.Failure, JsonUtility.GetEmptyJObject(), "SccmTaskNotFound");
        }
    }
}