using System;
using System.Collections.Generic;
using System.IO;
using Shared;
using System.Diagnostics;

namespace Indexer
{
    public class App
    {
        public void Run()
        {

            IIndexDatabase db = GetDatabase();
            Crawler crawler = new Crawler(db);

            var root = new DirectoryInfo(Config.FOLDER);

            DateTime start = DateTime.Now;

            crawler.IndexFilesIn(root, new List<string> { ".txt" });

            TimeSpan used = DateTime.Now - start;
            Console.WriteLine("DONE! used " + used.TotalMilliseconds);

            var all = db.GetAllWords();

            Console.WriteLine($"Indexed {db.DocumentCounts} documents");
            Console.WriteLine($"Number of different words: {all.Count}");
            int count = 10;
            Console.WriteLine($"The first {count} is:");
            foreach (var p in all)
            {
                Console.WriteLine("<" + p.Key + ", " + p.Value + ">");
                count--;
                if (count == 0) break;
            }

            Console.WriteLine("Hvor mange ord vil du se - inddelt efter hyppighed?");
            var numberOfWords = Console.ReadLine();

            MostFrequentWords(int.Parse(numberOfWords), db);

        }

        private IIndexDatabase GetDatabase()
        {
            Console.Write("Use SQLite (1) or Postgres (2) database?");
            string input = Console.ReadLine();
            if (input.Equals("1"))
                return new Shared.DatabaseSqlite();
            else if (input.Equals("2"))
                return new Shared.DatabasePostgres();
            Console.WriteLine("Wrong input - try again...");
            return GetDatabase();
        }

        public void MostFrequentWords(int rows, IIndexDatabase db)
        {
            var words = db.GetMostFrequent(rows);   

            foreach (var word in words)
                Console.WriteLine($"<{word.Name}> - {word.Frequency}");
        }
    }
}
