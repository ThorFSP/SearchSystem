using Shared;
using Api;

public static class SearchLogicFactory
{
    public static ISearchLogic Create(IReadOnlyList<ISearchDatabase> databases)
    {
        return new SearchLogic(databases);
    }
}