using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Text;

namespace PactConsumerTests
{
    public class SendRequest
    {
        private readonly Uri _baseAddress;
        public SendRequest(Uri uri)
        {
            _baseAddress = uri;
        }
        public async Task<(TResponse response, string error)> SendRequestAsync<TRequest, TResponse>(TRequest request, string requestUrl) where TResponse : class
        {
            var jsonSerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            var payload = JsonConvert.SerializeObject(request, jsonSerializerSettings);
            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            var _client = new HttpClient();
            _client.BaseAddress = _baseAddress;
            var httpResponse = await _client.PostAsync(requestUrl, content);
            var stream = await httpResponse.Content.ReadAsStringAsync();
            return httpResponse.IsSuccessStatusCode
                       ? (JsonConvert.DeserializeObject<TResponse>(stream), default(string))
                       : (null, httpResponse.ReasonPhrase);
        }
    }
}
