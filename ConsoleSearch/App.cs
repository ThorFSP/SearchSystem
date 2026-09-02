using System;
using Shared;
using Shared.Model;

namespace ConsoleSearch
{
    public class App
    {

        public void Run()
        {
            ISearchDatabase db = GetDatabase();
            SearchLogic mSearchLogic = new SearchLogic(db);
            Configs configs = new Configs();
            Console.WriteLine("Console Search");
            
            while (true)
            {
                Console.WriteLine("enter search terms - q for quit");
                Console.WriteLine("type \"help\" for configs");
                string input = Console.ReadLine();
                if (input.Equals("q")) break;
                if (input.Equals("help")) {
                    GetConfigs(configs);
                    continue;
                }
                if (input.Equals("case")) {
                    // Toggle 
                    configs.IsCaseSensitive = !configs.IsCaseSensitive;
                    Console.WriteLine("IsCaseSensitive = " + configs.IsCaseSensitive);
                    continue;
                }

                var query = input.Split(" ", StringSplitOptions.RemoveEmptyEntries);
               

                var result = mSearchLogic.Search(query, 10, configs.IsCaseSensitive);

                if (result.Ignored.Count > 0) {
                    Console.WriteLine($"Ignored: {string.Join(',', result.Ignored)}");
                }
                
                int idx = 1;
                foreach (var doc in result.DocumentHits) {
                    Console.WriteLine($"{idx} : {doc.Document.mUrl} -- contains {doc.NoOfHits} search terms");
                    Console.WriteLine("Index time: " + doc.Document.mIdxTime);
                    Console.WriteLine($"Missing: {ArrayAsString(doc.Missing.ToArray())}");
                    idx++;
                }
                Console.WriteLine("Documents: " + result.Hits + ". Time: " + result.TimeUsed.TotalMilliseconds);
            }
        }
        
        private ISearchDatabase GetDatabase()
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

        public void GetConfigs(Configs configs)
        {
            Console.WriteLine("Configs:");
            Console.WriteLine("IsCaseSensitive = " + configs.IsCaseSensitive + " (type \"case\" to toggle)");
        }

        string ArrayAsString(string[] s) => s.Length == 0?"[]":$"[{String.Join(',', s)}]";
    }
}
