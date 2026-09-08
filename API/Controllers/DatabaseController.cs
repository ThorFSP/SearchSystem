// DatabaseApi/Controllers/DatabaseController.cs
using Microsoft.AspNetCore.Mvc;
using Shared;
using Shared.Model;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/database")]
    public class DatabaseController : ControllerBase
    {
        private readonly ISearchDatabase _db;

        public DatabaseController(ISearchDatabase db)
        {
            _db = db; // DI hands a DatabaseSqlite/DatabasePostgres
        }

        [HttpPost("wordids")]
        public ActionResult<WordIdsResponse> GetWordIds(WordIdsRequest req)
        {
            var ids = _db.GetWordIds(req.Query, out var ignored, req.CaseSensitive);
            return new WordIdsResponse { WordIds = ids, Ignored = ignored };
        }

        [HttpGet("docdetails/{docId}")]
        public ActionResult<BEDocument> GetDocDetails(int docId)
            => _db.GetDocDetails(docId);

        [HttpPost("documents")]
        public ActionResult<List<DocPair>> GetDocuments(List<int> wordIds)
            => _db.GetDocuments(wordIds).Select(kv => new DocPair { DocId = kv.Key, Count = kv.Value }).ToList();

        [HttpPost("missing/{docId}")]
        public ActionResult<List<int>> GetMissing(int docId, List<int> wordIds)
            => _db.getMissing(docId, wordIds);

        [HttpPost("wordsfromids")]
        public ActionResult<List<string>> WordsFromIds([FromQuery] bool caseSensitive, List<int> wordIds)
            => _db.WordsFromIds(wordIds, caseSensitive);
    }
}