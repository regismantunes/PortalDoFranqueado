﻿using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PortalDoFranqueadoGUI.API
{
    public class SimpleHttpClientRequest
    {
        public string? BearerToken { get; set; }
        public string? RequestUri { get; set; }

        public async Task Delete()
            => await Delete(RequestUri, BearerToken);

        public async Task Get()
            => await Get(RequestUri, BearerToken);

        public async Task Patch<TValue>(TValue value)
            => await Patch(RequestUri, value, BearerToken);

        public async Task Post<TValue>(TValue value)
            => await Post(RequestUri, value, BearerToken);

        public async Task Put<TValue>(TValue value)
            => await Put(RequestUri, value, BearerToken);

        public static async Task Delete(string? requestUri, string? bearerToken = null)
        {
            using var client = CreateHttpClient(bearerToken);
            var response = await client.DeleteAsync(requestUri);
            await GetResult(response);
        }

        public static async Task Get(string? requestUri, string? bearerToken = null)
        {
            using var client = CreateHttpClient(bearerToken);
            var response = await client.GetAsync(requestUri);
            await GetResult(response);
        }

        public static async Task Patch<TValue>(string? requestUri, TValue value, string? bearerToken = null)
        {
            using var client = CreateHttpClient(bearerToken);
            var response = await client.PatchAsync(requestUri, new StringContent(
                        JsonSerializer.Serialize(value), Encoding.UTF8, "application/json"));
            await GetResult(response);
        }

        public static async Task Post<TValue>(string? requestUri, TValue value, string? bearerToken = null)
        {
            using var client = CreateHttpClient(bearerToken);
            var response = await client.PostAsync(requestUri, new StringContent(
                    JsonSerializer.Serialize(value), Encoding.UTF8, "application/json"));
            await GetResult(response);
        }

        public static async Task Put<TValue>(string? requestUri, TValue value, string? bearerToken = null)
        {
            using var client = CreateHttpClient(bearerToken);
            var response = await client.PutAsync(requestUri, new StringContent(
                        JsonSerializer.Serialize(value), Encoding.UTF8, "application/json"));
            await GetResult(response);
        }

        private static HttpClient CreateHttpClient(string? bearerToken)
        {
            var client = new HttpClient();

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (!string.IsNullOrEmpty(bearerToken))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

            return client;
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
                    throw new Exception(msgResult is not null ? msgResult.Message : result);
                }

                throw new Exception(result.Length > 1 ? result[1..^1] : result);
            }
        }
    }
}