using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ExternalApiProxy
{
    public class ExternalApiClient : IExternalApiClient
    {
        private readonly HttpClient _httpClient;

        public ExternalApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GetSuccessfulResponse()
        {
            var response = await _httpClient.GetStringAsync(new Uri("api/external/success", UriKind.Relative));

            return response;
        }
    }
}
