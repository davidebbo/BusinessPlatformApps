using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace Microsoft.Deployment.Site.Service
{
    public class OptionsHttpMessageHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Method == HttpMethod.Options)
            {
                var apiExplorer = GlobalConfiguration.Configuration.Services.GetApiExplorer();

                var controllerRequested = request.GetRouteData().Values["controller"] as string;
                var supportedMethods = apiExplorer.ApiDescriptions
                    .Where(d =>
                    {
                        var controller = d.ActionDescriptor.ControllerDescriptor.ControllerName;
                        return string.Equals(controller, controllerRequested, StringComparison.OrdinalIgnoreCase);
                    })
                    .Select(d => d.HttpMethod.Method)
                    .Distinct();

                if (!supportedMethods.Any())
                {
                    return Task.Factory.StartNew(() => request.CreateResponse(HttpStatusCode.NotFound));
                }

                List<string> supportedHeaders = new List<string>() { "OperationId", "UserGeneratedId", "TemplateName", "UserId", "SessionId", "UniqueId",
                    "operationid", "usergeneratedid", "templatename", "userid", "sessionid", "uniqueid" };
                return Task.Factory.StartNew(() =>
                {
                    var response = new HttpResponseMessage(HttpStatusCode.OK);
                    //response.Headers.Add("Access-Control-Allow-Methods", string.Join(",", supportedMethods));
                    //response.Headers.Add("Access-Control-Allow-Headers", string.Join(",", supportedHeaders));
                    return response;
                });
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}