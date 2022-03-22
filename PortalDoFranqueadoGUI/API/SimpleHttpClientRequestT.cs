using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PortalDoFranqueadoGUI.API
{
    public class SimpleHttpClientRequest<T>
    {
        public string? BearerToken { get; set; }
        public string? RequestUri { get; set; }

        public async Task<T> Delete()
            => await Delete(RequestUri, BearerToken);

        public async Task<T> Get()
            => await Get(RequestUri, BearerToken);

        public async Task<T> Patch<TValue>(TValue value)
            => await Patch(RequestUri, value, BearerToken);

        public async Task<T> Post<TValue>(TValue value)
            => await Post(RequestUri, value, BearerToken);

        public async Task<T> PostFile(byte[] bytes, string contentType)
            => await PostFile(RequestUri, bytes, contentType, BearerToken);

        public async Task<T> Put<TValue>(TValue value)
            => await Put(RequestUri, value, BearerToken);

        public static async Task<T> Delete(string? requestUri, string? bearerToken = null)
        {
            using var client = CreateJsonHttpClient(bearerToken);
            using var response = await client.DeleteAsync(requestUri);
            return await GetResult(response);
        }

        public static async Task<T> Get(string? requestUri, string? bearerToken = null)
        {
            using var client = CreateJsonHttpClient(bearerToken);
            using var response = await client.GetAsync(requestUri);
            return await GetResult(response);
        }

        public static async Task<T> Patch<TValue>(string? requestUri, TValue value, string? bearerToken = null)
        {
            using var client = CreateJsonHttpClient(bearerToken);
            using var response = await client.PatchAsync(requestUri, new StringContent(
                        JsonSerializer.Serialize(value), Encoding.UTF8, "application/json"));
            return await GetResult(response);
        }

        public static async Task<T> Post<TValue>(string? requestUri, TValue value, string? bearerToken = null)
        {
            using var client = CreateJsonHttpClient(bearerToken);
            using var response = await client.PostAsync(requestUri, new StringContent(
                    JsonSerializer.Serialize(value), Encoding.UTF8, "application/json"));
            return await GetResult(response);
        }

        public static async Task<T> Put<TValue>(string? requestUri, TValue value, string? bearerToken = null)
        {
            using var client = CreateJsonHttpClient(bearerToken);
            using var response = await client.PutAsync(requestUri, new StringContent(
                        JsonSerializer.Serialize(value), Encoding.UTF8, "application/json"));
            return await GetResult(response);
        }

        public static async Task<T> PostFile(string? requestUri, byte[] bytes, string contentType, string? bearerToken = null)
        {
            using var client = CreateHttpClient(bearerToken);
            var requestContent = new MultipartFormDataContent();
            var content = new ByteArrayContent(bytes);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);

            requestContent.Add(content);

            var response = await client.PostAsync(requestUri, content);
            return await GetResult(response);
        }

        private static HttpClient CreateHttpClient(string? bearerToken)
        {
            var client = new HttpClient()
            { 
                Timeout = new TimeSpan(0, 10, 0) 
            };

            if (!string.IsNullOrEmpty(bearerToken))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

            return client;
        }

        private static HttpClient CreateJsonHttpClient(string? bearerToken)
        {
            var client = CreateHttpClient(bearerToken);

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return client;
        }

        private static async Task<T> GetResult(HttpResponseMessage response)
        {
            var result = await response.Content.ReadAsStringAsync();
            
            if (response.StatusCode != HttpStatusCode.OK)
            {
                if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    Configuration.Current.DisconectSession();
                    throw new Exception("A sessão foi desconectada.");
                }

                if (response.StatusCode == HttpStatusCode.NoContent)
                    return default;

                if (result.StartsWith('{'))
                {
                    var msgResult = JsonSerializer.Deserialize<MessageResult>(result, new JsonSerializerOptions(JsonSerializerDefaults.Web));
                    throw new Exception(msgResult != null ? msgResult.Message : result);
                }

                throw new Exception(string.IsNullOrEmpty(result) ? $"Falha - {response.StatusCode}" : result);
            }

            var deserialized = JsonSerializer.Deserialize<T>(result, new JsonSerializerOptions(JsonSerializerDefaults.Web));
            if (deserialized == null)
                throw new Exception("Não foi possível deserializar o retorno.");

            return deserialized;
        }
    }
}
