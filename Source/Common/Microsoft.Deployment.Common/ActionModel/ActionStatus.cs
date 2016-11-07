namespace Microsoft.Deployment.Common.ActionModel
{
    public enum ActionStatus
    {
        Failure,
        FailureExpected,
        BatchNoState,
        BatchWithState,
        UserInteractionRequired,
        Success,

        // Used for when you encounter an Error - these should never be returned
        Retry,
        UnhandledException
    }

    public static class ActionStatusHelper

    {
        public static bool IsSucessfullStatus(this ActionStatus status)
        {
            return status != ActionStatus.Failure;
        }

        public static string ResponseCode(this ActionStatus status)
        {
            switch (status)
            {
                case ActionStatus.Success:
                    return "200";

                case ActionStatus.BatchWithState:
                    return "202";

                case ActionStatus.BatchNoState:
                    return "202";

                case ActionStatus.UserInteractionRequired:
                    return "204";

                case ActionStatus.Failure:
                    return "504";

                case ActionStatus.FailureExpected:
                    return "250";

                case ActionStatus.Retry:
                    return "300";

                case ActionStatus.UnhandledException:
                    return "506";
            }
            return "500";
        }
    }
}