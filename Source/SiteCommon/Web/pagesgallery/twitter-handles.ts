
import {ViewModelBase} from "../services/viewmodelbase";
import {DataStoreType} from "../services/datastore";

export class TwitterHandles extends ViewModelBase {
    accounts: string = '';
    twitterHandleName: string = '';
    twitterHandleId: string = '';

    constructor() {
        super();
        this.isValidated = true;
    }

    async OnValidate(): Promise<boolean> {
        if (!super.OnValidate()) {
            return false;
        }

        let body: any = {};
        body.Accounts = this.accounts;
        let response = await this.MS.HttpService.executeAsync('Microsoft-ValidateTwitterAccount', body);
        if (response.IsSuccess) {
            this.isValidated = true;
            this.showValidation = true;
            this.twitterHandleName = response.Body.twitterHandle;
            this.twitterHandleId = response.Body.twitterHandleId;
        }

        this.MS.DataStore.addToDataStore('TwitterHandles', this.accounts, DataStoreType.Public);
    }

    async Invalidate() {
        super.Invalidate();
        if (!this.accounts) {
            this.isValidated = true;
        }
    }

    async NavigatingNext(): Promise<boolean> {
        this.MS.DataStore.addToDataStore( 'SqlSubGroup', 'Twitter', DataStoreType.Public);
        this.MS.DataStore.addToDataStore('SqlGroup', 'SolutionTemplate', DataStoreType.Public);
        this.MS.DataStore.addToDataStore('SqlEntryName', 'twitterHandle', DataStoreType.Public);
        this.MS.DataStore.addToDataStore('SqlEntryValue', this.twitterHandleName, DataStoreType.Public);

        this.MS.DataStore.addToDataStore('SqlGroup', 'SolutionTemplate', DataStoreType.Public);
        this.MS.DataStore.addToDataStore('SqlSubGroup', 'Twitter', DataStoreType.Public);
        this.MS.DataStore.addToDataStore('SqlEntryName', 'twitterHandleId', DataStoreType.Public);
        this.MS.DataStore.addToDataStore('SqlEntryValue', this.twitterHandleId, DataStoreType.Public);
        return super.NavigatingNext();
    }
}