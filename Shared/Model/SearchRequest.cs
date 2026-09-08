namespace Shared.Model
{
    public class SearchRequest
    {
        public string[] Query { get; set; }
        public int MaxAmount { get; set; } = 10;
        public bool CaseSensitive { get; set; } = false;
    }
}

