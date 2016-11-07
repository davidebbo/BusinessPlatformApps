using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Deployment.Common.Actions;
using Microsoft.Deployment.Common.AppLoad;
using Microsoft.Deployment.Common.Helpers;
using Microsoft.Deployment.Common.Model;
using Newtonsoft.Json.Linq;

namespace Microsoft.Deployment.Common.Tags
{
    [Export(typeof(ITagHandler))]
    class InstallTagHandler : ITagHandler
    {
        public string Tag { get; } = "Install";

        public bool Recurse { get; } = true;

        public object ProcessTag(JToken innerJson, JToken entireJson, Dictionary<string, UIPage> allPages, Dictionary<string, IAction> allActions, App app, List<TagReturn> childObjects)
        {
            var pages = childObjects.Where(c => c.Tag == "Pages");
            var actions = childObjects.Where(c => c.Tag == "Actions");

            foreach(var page in pages)
            {
                app.Pages.Add(page.Output as UIPage);
            }

            foreach (var action in actions)
            {
                app.Actions.Add(action.Output as DeploymentAction);
            }

            return null;
        }
    }
}
