using Newtonsoft.Json;
using System.Text;

namespace StableDiffusion.Services.Clients
{
    public abstract class BaseClient
    {
        protected readonly HttpClient _client;

        public BaseClient(HttpClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public async Task<HttpResponseMessage> PostAsync(string path, object payload)
        {
            var body = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

            return await _client.PostAsync($"{_client.BaseAddress}{path}", body);
        }

        public async Task<T> GetAsync<T>(string path) where T : class
        {
            return await GetAsync<T>(path, new Dictionary<string, string>());
        }

        public async Task<T> GetAsync<T>(string path, Dictionary<string, string> parameters) where T : class
        {
            var uriBuilder = new UriBuilder($"{_client.BaseAddress}{path}");

            var query = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);

            foreach (var key in parameters.Keys)
            {
                query[key] = parameters[key];
            }

            uriBuilder.Query = query.ToString();

            var response = await _client.GetAsync(uriBuilder.ToString());
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(content);
        }

        public async Task<byte[]> GetByteArrayAsync(string path, bool withoutBaseUrl = true)
        {
            UriBuilder uriBuilder;
            if (withoutBaseUrl)
            {
                uriBuilder = new UriBuilder($"{path}");
            }
            else
            {
                uriBuilder = new UriBuilder($"{_client.BaseAddress}{path}");
            }

            var response = await _client.GetAsync(uriBuilder.ToString());
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsByteArrayAsync();
        }
    }
}
