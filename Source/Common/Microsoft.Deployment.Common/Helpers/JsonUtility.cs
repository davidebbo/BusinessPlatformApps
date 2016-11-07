using System;
using System.Data;
using System.Dynamic;
using System.Globalization;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Microsoft.Deployment.Common.Helpers
{
    public static class JsonUtility
    {
        public static JObject GetJsonObjectFromJsonString(string json)
        {
            var obj = JObject.Parse(json);
            return obj;
        }

        public static JObject GetEmptyJObject()
        {
            return GetJsonObjectFromJsonString("{}");
        }

        public static JObject GetJObjectFromObject(object json)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            var obj = JObject.FromObject(json);
            return obj;
        }

        public static dynamic GetDynamicFromJObject(JObject json)
        {
            var jsonString = json.Root.ToString();
            var converter = new ExpandoObjectConverter();
            dynamic obj = JsonConvert.DeserializeObject<ExpandoObject>(jsonString, converter);
            return obj;
        }

        public static string GetJsonStringFromObject(object json)
        {
            if(json == null)
            {
                return JsonUtility.GetEmptyJObject().ToString();
            }

            JsonSerializerSettings settings = new JsonSerializerSettings();
            var obj = JObject.FromObject(json);
            return obj.Root.ToString();
        }

        public static JObject GetJObjectFromStringValue(string value)
        {
            return GetJsonObjectFromJsonString("{\"value\":" + JsonConvert.ToString(value) + "}");
        }

        public static JObject GetJObjectFromJsonString(string json)
        {
            JObject templatefileContent = new JObject();

            if (string.IsNullOrEmpty(json) || json.Equals("null", StringComparison.OrdinalIgnoreCase))
            {
                json = GetEmptyJObject().ToString();
            }

            templatefileContent = (JObject) JToken.Parse(json);
            return templatefileContent;
        }

        public static JObject CreateJObjectWithValueFromObject(object response)
        {
            dynamic obj = new ExpandoObject();
            obj.value = response;
            return JObject.FromObject(obj);
        }

        public static bool IsNullOrEmpty(this JToken token)
        {
            return token == null 
                ||token.Type == JTokenType.Array && !token.HasValues
                ||token.Type == JTokenType.Object && !token.HasValues 
                ||token.Type == JTokenType.String && token.ToString() == String.Empty 
                || token.Type == JTokenType.Null;
        }


        public static string Serialize(DataTable table)
        {
            string result;

            JsonSerializer json = new JsonSerializer
            {
                NullValueHandling = NullValueHandling.Ignore,
                ObjectCreationHandling = ObjectCreationHandling.Replace,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            };
            json.Converters.Add(new DataTableConverter());

            StringWriter sw = null;
            try
            {
                sw = new StringWriter(CultureInfo.InvariantCulture);
                using (JsonTextWriter writer = new JsonTextWriter(sw) { Formatting = Formatting.Indented, QuoteChar = '"' })
                {
                    json.Serialize(writer, table);
                    result = sw.ToString();
                    sw = null;
                }
            }
            finally
            {
                if (sw != null)
                    sw.Dispose();
            }

            return result;
        }

    }
}
