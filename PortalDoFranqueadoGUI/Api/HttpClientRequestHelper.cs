using System;
using System.Net.Http.Headers;
using System.Net.Http;

namespace PortalDoFranqueado.Api
{
    internal static class HttpClientRequestHelper
    {
        public static HttpClient CreateHttpClient(string? bearerToken)
        {
            var client = HttpClientFactory.Create();
            client.Timeout = TimeSpan.FromMinutes(10);

            if (!string.IsNullOrEmpty(bearerToken))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

            return client;
        }

        public static HttpClient CreateJsonHttpClient(string? bearerToken)
        {
            var client = CreateHttpClient(bearerToken);

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return client;
        }
    }
}
