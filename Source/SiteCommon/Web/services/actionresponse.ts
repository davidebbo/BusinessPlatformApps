import {DataStore} from "./datastore";

export class ActionResponse {
    Body:any;
    Status: ActionStatus;
    DataStore:any;
    DoesResponseContainsCredentials: boolean;
    ExceptionDetail: ActionResponseExceptionDetail;
    IsSuccess: boolean;
}

export enum ActionStatus {
    Failure,
    FailureExpected,
    BatchNoState,
    BatchWithState,
    UserInteractionRequired,
    Success,
}

export class ActionResponseExceptionDetail {
    LogLocation: string;
    FriendlyMessageCode: string;
    FriendlyErrorMessage: string;
    AdditionalDetailsErrorMessage: string;
    ExceptionCaught: any;
}
