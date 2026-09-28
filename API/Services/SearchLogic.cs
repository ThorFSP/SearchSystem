// DatabaseApi/SearchLogic.cs
using System;
using Shared;
using Shared.Model;

namespace Api
{
    public class SearchLogic : ISearchLogic
    {
        private readonly IReadOnlyList<ISearchDatabase> mDatabases;

        public SearchLogic(IReadOnlyList<ISearchDatabase> databases)
        {
            if (databases.Count == 0)
                throw new ArgumentException("At least one search database is required.", nameof(databases));

            mDatabases = databases;
        }

        /* Perform search of documents containing words from query. The result will
         * contain details about amost maxAmount of documents.
         */
        public SearchResult Search(string[] query, int maxAmount, bool caseSensitive)
        {
            List<string> ignored;

            DateTime start = DateTime.Now;

            var hitsByDocument = new Dictionary<int, (ISearchDatabase Database, int Score, List<int> WordIds)>();
            var ignoredByShard = new List<List<string>>();

            foreach (var database in mDatabases)
            {
                var wordIds = database.GetWordIds(query, out var shardIgnored, caseSensitive);
                ignoredByShard.Add(shardIgnored);

                if (wordIds.Count == 0)
                    continue;

                foreach (var hit in database.GetDocuments(wordIds))
                {
                    if (!hitsByDocument.TryAdd(hit.Key, (database, hit.Value, wordIds)))
                        throw new InvalidOperationException($"Document {hit.Key} is indexed in more than one search database.");
                }
            }

            ignored = query
                .Where(word => ignoredByShard.All(shardIgnored => shardIgnored.Contains(word)))
                .Distinct()
                .ToList();

            var top = hitsByDocument
                .OrderByDescending(hit => hit.Value.Score)
                .ThenBy(hit => hit.Key)
                .Take(Math.Max(0, maxAmount));

            var docresult = new List<DocumentHit>();
            foreach (var hit in top)
            {
                var database = hit.Value.Database;
                var doc = database.GetDocDetails(hit.Key);
                var missing = database.WordsFromIds(database.getMissing(hit.Key, hit.Value.WordIds));
                missing.AddRange(ignored);
                docresult.Add(new DocumentHit(doc, hit.Value.Score, missing));
            }

            return new SearchResult(query, hitsByDocument.Count, docresult, ignored, DateTime.Now - start);
        }
    }
}