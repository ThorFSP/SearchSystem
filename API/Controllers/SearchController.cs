// DatabaseApi/Controllers/SearchController.cs
using Microsoft.AspNetCore.Mvc;
using Shared;
using Shared.Model;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/search")]
    public class SearchController : ControllerBase
    {
        private readonly ISearchLogic mSearchLogic;

        public SearchController(ISearchLogic searchLogic)
        {
            mSearchLogic = searchLogic;
        }

        [HttpPost]
        public ActionResult<SearchResult> Search(SearchRequest req)
        {
            var result = mSearchLogic.Search(req.Query, req.MaxAmount, req.CaseSensitive);
            return result;
        }
    }
}