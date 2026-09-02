using System.Collections.Generic;
using Shared.Model;

namespace Shared
{
    public interface IIndexDatabase
    {
        //Get all words with key as the value, and the value as the id 
        Dictionary<string, int> GetAllWords();

        // Get the most frequent words in the database, ordered by frequency
        List<(string Name, int Frequency)> GetMostFrequent(int rows);

        // Return the number of documents indexed in the database
        int DocumentCounts { get; }

        void InsertDocument(BEDocument doc);
        
        // Insert a word in the database with id = i and value = v foreach entry
        // (i, v) in [words]
        void InsertAllWords(Dictionary<string, int> words);

        void InsertAllOcc(int docId, ISet<int> wordIds);
    }
}