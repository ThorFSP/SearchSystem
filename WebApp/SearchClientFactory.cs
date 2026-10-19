public static class SearchClientFactory
{
    public static ISearchClient Create()
    {
        var baseUrl = Environment.GetEnvironmentVariable("SEARCH_API_BASE_URL")
            ?? "http://localhost:5000";
        return new SearchClient(baseUrl);
    }
}