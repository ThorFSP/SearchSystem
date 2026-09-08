using Shared.Model;

public class SearchClient : ISearchClient
{
    private readonly HttpClient _client;

    public SearchClient(string baseUrl)
    {
        _client = new HttpClient { BaseAddress = new Uri(baseUrl) };
    }

    public async Task<SearchResult> Search(string[] query, int maxAmount, bool caseSensitive)
    {
        var response = await _client.PostAsJsonAsync("api/search", new
        {
            Query = query,
            MaxAmount = maxAmount,
            CaseSensitive = caseSensitive
        });

        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<SearchResult>())!;
    }
}