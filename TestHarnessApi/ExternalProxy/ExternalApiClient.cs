using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace TestHarnessApi.ExternalProxy
{
    public class ExternalApiClient : IExternalApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public ExternalApiClient(HttpClient httpClient, IHttpContextAccessor context) // IHttpContextAccessor is only here because we are faking an external api with a controller in this api
        {
            _httpClient = httpClient;
            _baseUrl = $"{context.HttpContext.Request.Scheme}://{context.HttpContext.Request.Host.Value}";
        }

        public async Task<string> GetSuccessfulResponse()
        {
            var response = await _httpClient.GetStringAsync(new Uri($"{_baseUrl}/api/external/success"));

            return response;
        }

        public async Task<string> GetUnreliableResponse()
        {
            var response = await _httpClient.GetStringAsync(new Uri($"{_baseUrl}/api/external/unreliable"));

            return response;
        }

        public async Task<string> GetFailureResponse()
        {
            var response = await _httpClient.GetStringAsync(new Uri($"{_baseUrl}/api/external/broken"));

            return response;
        }
    }
}
