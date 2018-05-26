using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Nanny.Common.Utils
{
    public static class RestExtensions
    {
        public async static Task<string> RestGet(this HttpClient client, string url)
        {
            return await client.RestCall(HttpMethod.Get, url);
        }

        private async static Task<string> RestCall(this HttpClient client, HttpMethod method, string url, object body = null)
        {
            var request = new HttpRequestMessage(method, url)
            {
                Content = (body != null) ? new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json") : null
            };
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
    }
}
