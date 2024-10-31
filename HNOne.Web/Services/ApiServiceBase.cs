using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text;

namespace HNOne.Web.Services
{
    public class ApiServiceBase
    {
        private readonly IHttpClientFactory _factory;
        public readonly ILogger<ApiServiceBase> _logger;
        public readonly HttpClient _httpClient;

        public ApiServiceBase(IHttpClientFactory factory, ILogger<ApiServiceBase> logger)
        {
            _logger = logger;
            _factory = factory;
            _httpClient = factory.CreateClient("api");
            //_httpClient.DefaultRequestHeaders.Add("TokenKey", "");
        }

        /// <summary>
        /// REST CLIENT Methode Get
        /// </summary>
        /// <param name="pEnpoint"></param>
        /// <param name="pParams"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>HttpResponseMessage</returns>
        public async Task<HttpResponseMessage> GetAsync(string pEnpoint, Dictionary<string, object?>? pParams = null, CancellationToken? cancellationToken = null)
        {
            string queryString = "";
            if (pParams != null && pParams.Any()) queryString = "?" + string.Join("&", pParams.Select(m => $"{m.Key}={m.Value}"));
            HttpResponseMessage response = await _httpClient.GetAsync($"api/{pEnpoint}{queryString}");
            Debug.Print(_httpClient.BaseAddress + $"api/{pEnpoint}{queryString}");
            return response;
        }

        public async Task<HttpResponseMessage> PostAsync(string pEnpoint, object? pParams = null, CancellationToken? cancellationToken = null)
        {
            string jsonBody = string.Empty;
            if (pParams != null) jsonBody = JsonConvert.SerializeObject(pParams);
            HttpResponseMessage response = await _httpClient.PostAsync($"api/{pEnpoint}", new StringContent(jsonBody, UnicodeEncoding.UTF8, "application/json"));
            Debug.Print(_httpClient.BaseAddress + $"api/{pEnpoint}");
            Debug.Print(jsonBody);
            return response;
        }

        public bool ValidateJsonContent(HttpContent content)
        {
            var mediaType = content?.Headers.ContentType?.MediaType;
            return mediaType != null && mediaType.Equals("application/json", StringComparison.OrdinalIgnoreCase);
        }
    }
}
