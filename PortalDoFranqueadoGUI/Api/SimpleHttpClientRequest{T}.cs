using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PortalDoFranqueado.Api
{
    public class SimpleHttpClientRequest<T>
    {
        public string? BearerToken { get; set; }
        public required string? RequestUri { get; set; }

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
            using var client = HttpClientRequestHelper.CreateJsonHttpClient(bearerToken);
            using var response = await client.DeleteAsync(requestUri);
            return await GetResult(response);
        }

        public static async Task<T> Get(string? requestUri, string? bearerToken = null)
        {
            using var client = HttpClientRequestHelper.CreateJsonHttpClient(bearerToken);
            using var response = await client.GetAsync(requestUri);
            return await GetResult(response);
        }

        public static async Task<T> Patch<TValue>(string? requestUri, TValue value, string? bearerToken = null)
        {
            using var client = HttpClientRequestHelper.CreateJsonHttpClient(bearerToken);
            using var response = await client.PatchAsync(requestUri, new StringContent(
                        JsonSerializer.Serialize(value), Encoding.UTF8, "application/json"));
            return await GetResult(response);
        }

        public static async Task<T> Post<TValue>(string? requestUri, TValue value, string? bearerToken = null)
        {
            using var client = HttpClientRequestHelper.CreateJsonHttpClient(bearerToken);
            using var response = await client.PostAsync(requestUri, new StringContent(
                    JsonSerializer.Serialize(value), Encoding.UTF8, "application/json"));
            return await GetResult(response);
        }

        public static async Task<T> Put<TValue>(string? requestUri, TValue value, string? bearerToken = null)
        {
            using var client = HttpClientRequestHelper.CreateJsonHttpClient(bearerToken);
            using var response = await client.PutAsync(requestUri, new StringContent(
                        JsonSerializer.Serialize(value), Encoding.UTF8, "application/json"));
            return await GetResult(response);
        }

        public static async Task<T> PostFile(string? requestUri, byte[] bytes, string contentType, string? bearerToken = null)
        {
            using var client = HttpClientRequestHelper.CreateHttpClient(bearerToken);
            var requestContent = new MultipartFormDataContent();
            var content = new ByteArrayContent(bytes);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);

            requestContent.Add(content);

            var response = await client.PostAsync(requestUri, content);
            return await GetResult(response);
        }

        private static readonly JsonSerializerOptions jsonSerializerOptions = new(JsonSerializerDefaults.Web);

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
                    var msgResult = JsonSerializer.Deserialize<MessageResult>(result, jsonSerializerOptions);
                    throw new Exception(msgResult != null ? msgResult.Message : result);
                }

                throw new Exception(string.IsNullOrEmpty(result) ? $"Falha - {response.StatusCode}" : result);
            }

            var deserialized = JsonSerializer.Deserialize<T>(result, jsonSerializerOptions);
            
            return deserialized ?? throw new Exception("Não foi possível deserializar o retorno.");
        }
    }
}
