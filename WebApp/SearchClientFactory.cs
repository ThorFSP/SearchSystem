public static class SearchClientFactory
{
    private const string ApiBaseUrl = "http://localhost:5000";

    public static ISearchClient Create()
    {
        return new SearchClient(ApiBaseUrl);
    }
}