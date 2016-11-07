import { ViewModelBase } from '../services/viewmodelbase';
import { DataStoreType } from "../services/datastore";
import { ActionResponse } from "../services/actionresponse";

export class Salesforce extends ViewModelBase {
    salesforceUsername: string = '';
    salesforcePassword: string = '';
    salesforceToken: string = '';
    salesforceUrl: string = '';
    salesforceObjects: string = '';

    constructor() {
        super();
        this.isValidated = false;
        this.showValidation = false;
        this.salesforceUrl = "https://login.salesforce.com/";
    }

    async OnLoaded() {
        this.isValidated = false;
        this.showValidation = false;
    }

    async OnValidate(): Promise<boolean> {
        super.OnValidate();

        this.MS.DataStore.addToDataStore('SalesfoceUser', this.salesforceUsername, DataStoreType.Public);
        this.MS.DataStore.addToDataStore('SalesfocePassword', this.salesforcePassword, DataStoreType.Private);
        this.MS.DataStore.addToDataStore('SalesfoceToken', this.salesforceToken, DataStoreType.Private);
        this.MS.DataStore.addToDataStore('SalesforceUrl', this.salesforceUrl, DataStoreType.Public);
        this.MS.DataStore.addToDataStore('ObjectTables', this.salesforceObjects, DataStoreType.Public);

        let salesforceLoginResponse = await this.MS.HttpService.executeAsync("Microsoft-ValidateSalesforceCredentials");

        if (salesforceLoginResponse.IsSuccess) {
            this.isValidated = true;
            this.showValidation = true;
            this.MS.DataStore.addToDataStore('SalesforceBaseUrl', salesforceLoginResponse.Body.serverUrl, DataStoreType.Public);
        }
        else {
            this.MS.ErrorService.message = salesforceLoginResponse.Body.Message;
        }

        return super.OnValidate();
    }
}