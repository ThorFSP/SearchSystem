using Shared;
using Api;

public static class SearchLogicFactory
{
    public static ISearchLogic Create(ISearchDatabase database)
    {
        return new SearchLogic(database);
    }
}