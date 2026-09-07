using System;
using System.Collections.Generic;
using System.IO;
using Shared;

namespace Indexer
{
	public class App
	{
		public void Run()
		{
			var folder = new DirectoryInfo(Config.FOLDER);

			if (!folder.Exists)
			{
				Console.WriteLine($"Folder does not exist: {folder.FullName}");
				return;
			}

			Console.WriteLine($"Indexing .txt files in {folder.FullName}");

			IIndexDatabase database = new DatabaseSqlite();
			var crawler = new Crawler(database);
			crawler.IndexFilesIn(folder, new List<string> { ".txt" });

			Console.WriteLine($"Indexed {database.DocumentCounts} documents.");
		}
	}
}
