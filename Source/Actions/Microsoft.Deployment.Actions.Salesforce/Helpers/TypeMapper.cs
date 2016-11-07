using System;
using System.Collections.Generic;

namespace Microsoft.Deployment.Actions.Salesforce.Helpers
{
    public static class TypeMapper
    {
        public static Dictionary<string, string> SalesforceToDotNet = new Dictionary<string, string>
        {
            {"string", "string" },
            {"picklist", "string"},
            {"multipicklist", "string"},
            {"combobox", "string"},
            {"reference", "string"},
            {"base64", "string"},
            {"boolean", "boolean"},
            {"currency", "double"},
            {"textarea", "string"},
            {"int", "int"},
            {"double", "double"},
            {"percent", "double"},
            {"phone", "string"},
            {"id", "string"},
            {"date", "date"},
            {"datetime", "datetime"},
            {"time", "string"},
            {"url", "string"},
            {"email", "string"},
            {"encryptedstring", "string"},
            {"datacategorygroupreference", "string"},
            {"location", "string"},
            {"address", "string"},
            {"anyType", "string"},
            {"complexvalue", "string"}
         };

        public static Dictionary<string, string> DotNetToSql = new Dictionary<string, string>
        {
            {"string", "nvarchar" },
            {"Int16", "smallint" },
            {"Int32", "int" },
            {"Int64", "bigint" },
            {"boolean", "tinyint" },
            {"float", "float" },
            {"double", "float" },
            {"date", "date" },
            {"datetime", "datetime" }
         };
    }
}
