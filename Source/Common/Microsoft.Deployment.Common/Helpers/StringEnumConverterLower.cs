using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.Deployment.Common.Helpers
{
    public class StringEnumConverterLower : StringEnumConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            value= value.ToString().ToLower();
            writer.WriteValue(value);
        }
    }
}