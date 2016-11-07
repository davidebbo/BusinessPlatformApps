using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Deployment.Common.Helpers
{
    public class HttpClientUtility
    {
        public Task<HttpResponseMessage> ExecuteGenericAsync(HttpMethod method, string url, string body, string contentType = "application/json", Dictionary<string, string> customHeader = null )
        {
            HttpClient client = new HttpClient();
            string requestUri = url;
            HttpRequestMessage message = new HttpRequestMessage(method, requestUri);

            if (method == HttpMethod.Post || method == HttpMethod.Put)
            {
                if (string.IsNullOrEmpty(contentType))
                {
                    message.Content = new StringContent(body);
                }
                else
                {
                    message.Content = new StringContent(body, Encoding.UTF8, contentType);

                }
            }

            if (customHeader != null)
            {
                foreach (var key in customHeader.Keys)
                {
                    client.DefaultRequestHeaders.Add(key, customHeader[key]);
                }
            }

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/javascript"));

            return client.SendAsync(message);
        }
    }
}
