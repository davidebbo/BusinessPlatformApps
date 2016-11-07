using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Deployment.Actions.Salesforce.Helpers
{
    public static class SqlConfigQueries
    {
        public const string configQuery = @"INSERT INTO [smgt].[configuration]
           ([configuration_group]
           ,[configuration_subgroup]
           ,[name]
           ,[value]
           ,[visible])
     VALUES
           ('arg1'
           ,'arg2'
           ,'arg3'
           ,'arg4'
           ,'arg5')";

        public const string deleteConfigQuery = @"DELETE FROM [smgt].[configuration]
                  WHERE configuration_group = 'arg1' AND
                  configuration_subgroup = 'arg2' AND
                  name = 'arg3'";
    }
}
