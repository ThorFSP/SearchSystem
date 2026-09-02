using System.Collections.Generic;

namespace Shared.Model
{
    public class WordIdsRequest
    {
        public string[] Query { get; set; }
        public bool CaseSensitive { get; set; }
    }

    public class WordIdsResponse
    {
        public List<int> WordIds { get; set; }
        public List<string> Ignored { get; set; }
    }

    public class DocPair
    {
        public int DocId { get; set; }
        public int Count { get; set; }
    }
}