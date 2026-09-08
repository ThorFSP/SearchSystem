public static class SearchClientFactory
{
    public static ISearchClient Create(string baseUrl = "http://localhost:5081")
    {
        return new SearchClient(baseUrl);
    }
}