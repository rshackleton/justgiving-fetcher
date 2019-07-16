using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JustGivingFetcher
{
    public static class JustGivingClient
    {
        private static HttpClient _client;

        private static HttpClient Client
        {
            get
            {
                if (_client == null)
                {
                    _client = new HttpClient();
                    _client.DefaultRequestHeaders.Add("Accept", "application/json");
                }

                return _client;
            }
        }

        public static async Task<JObject> GetPageDetails(string appId, string pageShortName)
        {
            var url = $"https://api.justgiving.com/{appId}/v1/fundraising/pages/{pageShortName}";
            var response = await Client.GetAsync(url);
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject(responseContent) as JObject;
        }

        public static async Task<JObject> GetDonations(string appId, string pageShortName)
        {
            var url = $"https://api.justgiving.com/{appId}/v1/fundraising/pages/{pageShortName}/donations";
            var response = await Client.GetAsync(url);
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject(responseContent) as JObject;
        }
    }
}