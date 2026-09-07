// ConsoleSearch/SearchClient.cs
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Shared.Model;

namespace ConsoleSearch
{
    public class SearchClient
    {
        private readonly HttpClient mClient;

        public SearchClient(string baseUrl = "http://localhost:5081")
        {
            mClient = new HttpClient { BaseAddress = new System.Uri(baseUrl) };
        }

        public async Task<SearchResult> Search(string[] query, int maxAmount, bool caseSensitive)
        {
            var response = await mClient.PostAsJsonAsync("api/search", new
            {
                Query = query,
                MaxAmount = maxAmount,
                CaseSensitive = caseSensitive
            });

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<SearchResult>();
        }
    }
}