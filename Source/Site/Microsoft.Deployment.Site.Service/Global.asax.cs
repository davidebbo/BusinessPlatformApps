using System.Web.Http;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Deployment.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.Deployment.Site.Service
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            TelemetryConfiguration.Active.InstrumentationKey = Constants.AppInsightsKey;
            GlobalConfiguration.Configure(WebApiConfig.Register);
            HttpConfiguration config = GlobalConfiguration.Configuration;
            config.Formatters.JsonFormatter.SerializerSettings.Converters.Add(new StringEnumConverter { CamelCaseText = true });

            JsonConvert.DefaultSettings = (() =>
            {
                var settings = new JsonSerializerSettings();
                settings.Converters.Add(new StringEnumConverter { CamelCaseText = true });
                return settings;
            });
        }
    }
}