using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Deployment.Common.Helpers;

namespace Microsoft.Deployment.Common.ActionModel
{
    public class ActionResponseExceptionDetail
    {
        public Exception ExceptionCaught = null;

        public string LogLocation { get; set; }  = string.Empty;

        public string FriendlyMessageCode { get; set; }

        public string FriendlyErrorMessage
        {
            get
            {
                if (this.FriendlyMessageCode == null)
                {
                    return string.Empty;
                }

                return ErrorUtility.GetErrorCode(this.FriendlyMessageCode);
            }
        }

        public string AdditionalDetailsErrorMessage { get; set; } = string.Empty;
    }
}
