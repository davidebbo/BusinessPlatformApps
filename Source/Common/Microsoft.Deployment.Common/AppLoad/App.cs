using System;
using System.Collections.Generic;
using Microsoft.Deployment.Common.Actions;
using Newtonsoft.Json;

namespace Microsoft.Deployment.Common.AppLoad
{
    public class App
    {
        public App()
        {
            this.Pages = new List<UIPage>();
            this.Actions = new List<DeploymentAction>();
            this.UninstallPages = new List<UIPage>();
            this.UninstallActions = new List<DeploymentAction>();
        }
        public string Name { get; set; }

        public List<UIPage> Pages { get; set; }
        public List<DeploymentAction> Actions { get; set; }

        public List<UIPage> UninstallPages { get; set; }
        public List<DeploymentAction> UninstallActions { get; set; }

        public Guid MsiGuid { get; set; } = Guid.Empty;



        [JsonIgnore]
        public string AppFilePath { get; set; }

        [JsonIgnore]
        public string AppRelativeFilePath { get; set; }
    }
}