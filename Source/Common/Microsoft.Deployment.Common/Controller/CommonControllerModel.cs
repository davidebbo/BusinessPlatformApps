using System.Collections.Generic;
using Microsoft.Deployment.Common.AppLoad;

namespace Microsoft.Deployment.Common.Controller
{
    public class CommonControllerModel
    {
        // Initialise once and maintain
        public AppFactory AppFactory { get; set; }
        public string Source { get; set; }
        
        public string AppRootFilePath { get; set; }
        public string SiteCommonFilePath { get; set; }
        public string ServiceRootFilePath { get; set; }
    }
}
