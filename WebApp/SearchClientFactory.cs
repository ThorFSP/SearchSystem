public static class SearchClientFactory
{
    private const string ApiBaseUrl = "http://localhost:5081";

    public static ISearchClient Create()
    {
        return new SearchClient(ApiBaseUrl);
    }
}