using Shared.Model;

public interface ISearchClient
{
    Task<SearchResult> Search(string[] query, int maxAmount, bool caseSensitive);
}