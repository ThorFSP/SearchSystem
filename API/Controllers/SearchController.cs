// DatabaseApi/Controllers/SearchController.cs
using ConsoleSearch;
using Microsoft.AspNetCore.Mvc;
using Shared;
using Shared.Model;

namespace DatabaseApi.Controllers
{
    public class SearchRequest
    {
        public string[] Query { get; set; }
        public int MaxAmount { get; set; } = 10;
        public bool CaseSensitive { get; set; } = false;
    }

    [ApiController]
    [Route("api/search")]
    public class SearchController : ControllerBase
    {
        private readonly SearchLogic mSearchLogic;

        public SearchController(ISearchDatabase database)
        {
            mSearchLogic = new SearchLogic(database);
        }

        [HttpPost]
        public ActionResult<SearchResult> Search(SearchRequest req)
        {
            var result = mSearchLogic.Search(req.Query, req.MaxAmount, req.CaseSensitive);
            return result;
        }
    }
}