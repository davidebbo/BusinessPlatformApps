using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Deployment.Common.Model
{
    public class TagReturn
    {
        public string Tag { get; }
        public object Output { get; }

        public TagReturn(string tag, object output)
        {
            this.Tag = tag;
            this.Output = output;
        }
    }
}
