using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PortalDoFranqueado.Api
{
    public class SimpleHttpClientRequest
    {
        public string? BearerToken { get; set; }
        public string? RequestUri { get; set; }

        public async Task Delete()
            => await Delete(RequestUri, BearerToken);

        public async Task Delete<TValue>(TValue value)
            => await Delete(RequestUri, value, BearerToken);

        public async Task Get()
            => await Get(RequestUri, BearerToken);

        public async Task<string> GetFile()
            => await GetFile(RequestUri, BearerToken);

        public async Task Patch<TValue>(TValue value)
            => await Patch(RequestUri, value, BearerToken);

        public async Task Post<TValue>(TValue value)
            => await Post(RequestUri, value, BearerToken);

        public async Task PostFile(byte[] bytes, string contentType, string name, string fileName)
            => await PostFile(RequestUri, bytes, contentType, name, fileName, BearerToken);

        public async Task Put<TValue>(TValue value)
            => await Put(RequestUri, value, BearerToken);

        public static async Task Delete(string? requestUri, string? bearerToken = null)
        {
            using var client = HttpClientRequestHelper.CreateJsonHttpClient(bearerToken);
            var response = await client.DeleteAsync(requestUri);
            await GetResult(response);
        }

        public static async Task Delete<TValue>(string? requestUri, TValue value, string? bearerToken = null)
        {
            using var client = HttpClientRequestHelper.CreateJsonHttpClient(bearerToken);
            var request = new HttpRequestMessage(HttpMethod.Delete, requestUri)
            {
                Content = new StringContent(JsonSerializer.Serialize(value), Encoding.UTF8, "application/json")
            };
            var response = await client.SendAsync(request);

            await GetResult(response);
        }

        public static async Task Get(string? requestUri, string? bearerToken = null)
        {
            using var client = HttpClientRequestHelper.CreateJsonHttpClient(bearerToken);
            var response = await client.GetAsync(requestUri);
            await GetResult(response);
        }

        public static async Task<string> GetFile(string? requestUri, string? bearerToken = null)
        {
            using var client = HttpClientRequestHelper.CreateHttpClient(bearerToken);
            using var response = await client.GetAsync(requestUri);

            var contentStream = await response.Content.ReadAsStreamAsync();
            var tmpFile = Path.GetTempFileName();

            using var fs = new FileStream(tmpFile, FileMode.Create);
            await contentStream.CopyToAsync(fs);

            return tmpFile;
        }

        public static async Task Patch<TValue>(string? requestUri, TValue value, string? bearerToken = null)
        {
            using var client = HttpClientRequestHelper.CreateJsonHttpClient(bearerToken);
            var response = await client.PatchAsync(requestUri, new StringContent(
                        JsonSerializer.Serialize(value), Encoding.UTF8, "application/json"));
            await GetResult(response);
        }

        public static async Task Post<TValue>(string? requestUri, TValue value, string? bearerToken = null)
        {
            using var client = HttpClientRequestHelper.CreateJsonHttpClient(bearerToken);
            var response = await client.PostAsync(requestUri, new StringContent(
                    JsonSerializer.Serialize(value), Encoding.UTF8, "application/json"));
            await GetResult(response);
        }

        public static async Task Put<TValue>(string? requestUri, TValue value, string? bearerToken = null)
        {
            using var client = HttpClientRequestHelper.CreateJsonHttpClient(bearerToken);
            var response = await client.PutAsync(requestUri, new StringContent(
                        JsonSerializer.Serialize(value), Encoding.UTF8, "application/json"));
            await GetResult(response);
        }

        public static async Task PostFile(string? requestUri, byte[] bytes, string contentType, string name, string fileName, string? bearerToken = null)
        {
            using var client = HttpClientRequestHelper.CreateHttpClient(bearerToken);
            
            var multpartContent = new MultipartFormDataContent();
            var content = new ByteArrayContent(bytes);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);

            multpartContent.Add(content, name, fileName);

            var response = await client.PostAsync(requestUri, multpartContent);
            await GetResult(response);
        }

        public static async Task GetResult(HttpResponseMessage response)
        {
            var result = await response.Content.ReadAsStringAsync();
            if (response.StatusCode != HttpStatusCode.OK)
            {
                if(response.StatusCode == HttpStatusCode.Forbidden)
                {
                    Configuration.Current.DisconectSession();
                    throw new Exception("A sessão foi desconectada.");
                }

                if (result.StartsWith('{'))
                {
                    var msgResult = JsonSerializer.Deserialize<MessageResult>(result, new JsonSerializerOptions(JsonSerializerDefaults.Web));
                    throw new Exception(msgResult != null ? msgResult.Message : result);
                }

                throw new Exception(string.IsNullOrEmpty(result) ? $"Falha - {response.StatusCode}" : result);
            }
        }
    }
}
