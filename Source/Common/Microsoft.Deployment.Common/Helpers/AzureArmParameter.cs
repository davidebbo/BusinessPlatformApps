using System;
using System.Collections.Generic;
using System.Dynamic;

namespace Microsoft.Deployment.Common.Helpers
{
    public class AzureArmParameterGenerator
    {
        private readonly List<Tuple<string,string,string>> parameter = new List<Tuple<string, string, string>>();
        public void AddParameter(string name, string type, string value)
        {
            this.parameter.Add(new Tuple<string, string, string>(name, type, value));
        }

        public void AddStringParam(string name, string value)
        {
            this.parameter.Add(new Tuple<string, string, string>(name, "String", value));
        }

        public ExpandoObject GetDynamicObject()
        {
            dynamic obj = new ExpandoObject();
            obj.parameters = new ExpandoObject();
            foreach (var item in parameter)
            {
                IDictionary<string, dynamic> x = (IDictionary<string, dynamic>) obj.parameters;
                x.Add(item.Item1, new ExpandoObject());
                x[item.Item1].type = item.Item2;
                x[item.Item1].defaultValue = item.Item3;
            }

            return obj;
        }
    }
}