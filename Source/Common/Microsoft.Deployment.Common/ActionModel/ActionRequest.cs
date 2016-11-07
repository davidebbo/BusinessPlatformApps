using System.Collections.Generic;
using Microsoft.Deployment.Common.Actions;
using Microsoft.Deployment.Common.Controller;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Deployment.Common.ActionModel
{
    public class ActionRequest
    {
        [JsonIgnore]
        public CommonControllerModel ControllerModel { get; set; }

        [JsonIgnore]
        public UserInfo Info { get; set; }

        [JsonIgnore]
        public Logger Logger { get; set; }


        public DataStore DataStore { get; set; } = new DataStore();


        public ActionRequest()
        {
        }
    }
}