using Shared.Model;

public interface ISearchLogic
{
    SearchResult Search(string[] query, int maxAmount, bool caseSensitive);
}
