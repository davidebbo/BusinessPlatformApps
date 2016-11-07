import { ViewModelBase } from '../../common/Web/services/viewmodelbase';

export class Customize extends ViewModelBase {
    actuals: string = '';
    emails: string = '';
    emailRegex: RegExp;
    fiscalMonth: string = '';
    pipelineFrequency: string = '';
    pipelineInterval: number;
    recurrent: string = '';

    constructor() {
        super();
        this.isValidated = false;
        this.showValidation = false;
        this.fiscalMonth = "January";
        this.recurrent = "None";
        this.actuals = "Closed opportunities";
        this.emailRegex = /^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
    }

    async OnLoaded() {
        this.isValidated = false;
        this.showValidation = false;
    }

    async OnValidate() {
        super.OnValidate();

        this.isValidated = true;
        this.showValidation = true;

        if (this.emails != null && this.emails != '') {
            let mails = this.emails.split(',');
            for (let mail in mails) {
                if (!this.emailRegex.test(mails[mail])) {
                    this.isValidated = false;
                    this.showValidation = false;
                    this.MS.ErrorService.message = "Validation failed. The email address " + mails[mail] + " is not valid.";
                }
                else {
                    this.isValidated = true;
                    this.showValidation = true;
                }
            }
        }

        switch (this.recurrent) {
            case "Every 15 minutes":
                this.pipelineFrequency = "Minute";
                this.pipelineInterval = 15;
                break;
            case "Every 30 minutes":
                this.pipelineFrequency = "Minute";
                this.pipelineInterval = 30;
                break;
            case "Hourly":
                this.pipelineFrequency = "Hour";
                this.pipelineInterval = 1;
                break;
            case "Daily":
                this.pipelineFrequency = "Day";
                this.pipelineInterval = 1;
                break;
            case "None":
                this.pipelineFrequency = "Week";
                this.pipelineInterval = 1;
                break;
            default:
                break;
        }

        this.MS.DataService.AddToDataStore('Customize', "fiscalMonth", this.fiscalMonth);
        this.MS.DataService.AddToDataStore('Customize', "actuals", this.actuals);

        this.MS.DataService.AddToDataStore('ADF', 'EmailAddresses', this.emails);
        this.MS.DataService.AddToDataStore('ADF', 'pipelineStart', null);
        this.MS.DataService.AddToDataStore('ADF', 'pipelineEnd', null);
        this.MS.DataService.AddToDataStore('ADF', 'pipelineType', null);
        this.MS.DataService.AddToDataStore('ADF', 'postDeploymentPipelineFrequency', this.pipelineFrequency);
        this.MS.DataService.AddToDataStore('ADF', 'postDeploymentPipelineInterval', this.pipelineInterval.toString());

        if (this.recurrent == "None") {
            this.MS.DataService.AddToDataStore("ADF", "historicalOnly", "true");
        }
        else {
            this.MS.DataService.AddToDataStore("ADF", "historicalOnly", "false");
        }

        this.MS.DataService.AddToDataStore('ADF', 'pipelineFrequency', 'Month');
        this.MS.DataService.AddToDataStore('ADF', 'pipelineInterval', 1);
        this.MS.DataService.AddToDataStore('ADF', 'pipelineStart', '');
        this.MS.DataService.AddToDataStore('ADF', 'pipelineEnd', '');
        this.MS.DataService.AddToDataStore('ADF', 'pipelineType', "PreDeployment");
    }
}