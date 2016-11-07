using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Deployment.Common.AppLoad;

namespace Microsoft.Deployment.Common.Controller
{
    public class UserInfo
    {
        public string UserId { get; set; }
        public string UserGenId { get; set; }
        public string SessionId { get; set; }
        public string OperationId { get; set; }
        public string UniqueLink { get; set; }
        public string AppName { get; set; }
        public string ActionName { get; set; }

        public App App { get; set; }

        public string SerivceRootUrl { get; set; }
        public string WebsiteRootUrl { get; set; }
    }
}
