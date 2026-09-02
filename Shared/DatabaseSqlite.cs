using System;
using System.Collections.Generic;
using Shared.Model;
using Microsoft.Data.Sqlite;

namespace Shared
{
    public class DatabaseSqlite : ISearchDatabase, IIndexDatabase
    {
        private SqliteConnection _connection;

        private Dictionary<string, int> mWords = null;
        private Dictionary<string, List<int>> mWordsIgnoreCase = null;

        public DatabaseSqlite()
        {
            var connectionStringBuilder = new SqliteConnectionStringBuilder();

            connectionStringBuilder.Mode = SqliteOpenMode.ReadWriteCreate;
            connectionStringBuilder.DataSource = Paths.SQLITE_DATABASE;

            _connection = new SqliteConnection(connectionStringBuilder.ConnectionString);

            _connection.Open();

            Execute("DROP TABLE IF EXISTS Occ");

            Execute("DROP TABLE IF EXISTS document");
            Execute("CREATE TABLE document(id INTEGER PRIMARY KEY, url TEXT, idxTime TEXT, creationTime TEXT)");

            Execute("DROP TABLE IF EXISTS word");
            Execute("CREATE TABLE word(id INTEGER PRIMARY KEY, name VARCHAR(50))");

            Execute("CREATE TABLE Occ(wordId INTEGER, docId INTEGER, "
                    + "FOREIGN KEY (wordId) REFERENCES word(id), "
                    + "FOREIGN KEY (docId) REFERENCES document(id))");
            Execute("CREATE INDEX word_index ON Occ (wordId)");
        }

        private void Execute(string sql)
        {
            var cmd = _connection.CreateCommand();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();
        }

        private string AsString(List<int> x) => $"({string.Join(',', x)})";

        // ---------------- IIndexDatabase ----------------

        public List<(string Name, int Frequency)> GetMostFrequent(int rows)
        {
            var result = new List<(string, int)>();

            var cmd = _connection.CreateCommand();
            cmd.CommandText = @"
                                    SELECT word.name, COUNT(Occ.wordId) AS frequency
                                    FROM word
                                    JOIN Occ ON word.id = Occ.wordId
                                    GROUP BY word.id
                                    ORDER BY frequency DESC
                                    LIMIT @rows";

            cmd.Parameters.AddWithValue("@rows", rows);

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                result.Add((
                    reader.GetString(0),
                    reader.GetInt32(1)
                ));
            }

            return result;
        }

        public void InsertAllWords(Dictionary<string, int> res)
        {
            using (var transaction = _connection.BeginTransaction())
            {
                var command = _connection.CreateCommand();
                command.CommandText =
                    @"INSERT INTO word(id, name) VALUES(@id,@name)";

                var paramName = command.CreateParameter();
                paramName.ParameterName = "name";
                command.Parameters.Add(paramName);

                var paramId = command.CreateParameter();
                paramId.ParameterName = "id";
                command.Parameters.Add(paramId);

                foreach (var p in res)
                {
                    paramName.Value = p.Key;
                    paramId.Value = p.Value;
                    command.ExecuteNonQuery();
                }

                transaction.Commit();
            }
        }

        public void InsertAllOcc(int docId, ISet<int> wordIds)
        {
            using (var transaction = _connection.BeginTransaction())
            {
                var command = _connection.CreateCommand();
                command.CommandText =
                    @"INSERT INTO occ(wordId, docId) VALUES(@wordId,@docId)";

                var paramwordId = command.CreateParameter();
                paramwordId.ParameterName = "wordId";
                command.Parameters.Add(paramwordId);

                var paramDocId = command.CreateParameter();
                paramDocId.ParameterName = "docId";
                paramDocId.Value = docId;
                command.Parameters.Add(paramDocId);

                foreach (var p in wordIds)
                {
                    paramwordId.Value = p;
                    command.ExecuteNonQuery();
                }

                transaction.Commit();
            }
        }

        public void InsertDocument(BEDocument doc)
        {
            var insertCmd =
                new SqliteCommand(
                    "INSERT INTO document(id, url, idxTime, creationTime) VALUES(@id,@url, @idxTime, @creationTime)");
            insertCmd.Connection = _connection;

            var pId = new SqliteParameter("id", doc.mId);
            insertCmd.Parameters.Add(pId);

            var pUrl = new SqliteParameter("url", doc.mUrl);
            insertCmd.Parameters.Add(pUrl);

            var pIdxTime = new SqliteParameter("idxTime", doc.mIdxTime);
            insertCmd.Parameters.Add(pIdxTime);

            var pCreationTime = new SqliteParameter("creationTime", doc.mCreationTime);
            insertCmd.Parameters.Add(pCreationTime);

            insertCmd.ExecuteNonQuery();
        }

        public Dictionary<string, int> GetAllWords()
        {
            Dictionary<string, int> res = new Dictionary<string, int>();

            var selectCmd = _connection.CreateCommand();
            selectCmd.CommandText = "SELECT * FROM word";

            using (var reader = selectCmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var id = reader.GetInt32(0);
                    var w = reader.GetString(1);

                    res.Add(w, id);
                }
            }

            return res;
        }

        public int DocumentCounts
        {
            get
            {
                var selectCmd = _connection.CreateCommand();
                selectCmd.CommandText = "SELECT count(*) FROM document";

                using (var reader = selectCmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var count = reader.GetInt32(0);
                        return count;
                    }
                }

                return -1;
            }
        }

        // ---------------- ISearchDatabase ----------------

        // key is the id of the document, the value is number of search words in the document
        public List<KeyValuePair<int, int>> GetDocuments(List<int> wordIds)
        {
            var res = new List<KeyValuePair<int, int>>();

            var sql = "SELECT docId, COUNT(wordId) as count FROM Occ where ";
            sql += "wordId in " + AsString(wordIds) + " GROUP BY docId ";
            sql += "ORDER BY count DESC;";

            var selectCmd = _connection.CreateCommand();
            selectCmd.CommandText = sql;

            using (var reader = selectCmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var docId = reader.GetInt32(0);
                    var count = reader.GetInt32(1);

                    res.Add(new KeyValuePair<int, int>(docId, count));
                }
            }

            return res;
        }

        public BEDocument GetDocDetails(int docId)
        {
            var selectCmd = _connection.CreateCommand();
            selectCmd.CommandText = $"SELECT * FROM document where id = {docId}";

            using (var reader = selectCmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    var id = reader.GetInt32(0);
                    var url = reader.GetString(1);
                    var idxTime = reader.GetString(2);
                    var creationTime = reader.GetString(3);

                    return new BEDocument { mId = id, mUrl = url, mIdxTime = idxTime, mCreationTime = creationTime };
                }
            }
            return null;
        }

        /* Return a list of id's for words; all them among wordIds, but not present in the document
         */
        public List<int> getMissing(int docId, List<int> wordIds)
        {
            var sql = "SELECT wordId FROM Occ where ";
            sql += "wordId in " + AsString(wordIds) + " AND docId = " + docId;
            sql += " ORDER BY wordId;";

            var selectCmd = _connection.CreateCommand();
            selectCmd.CommandText = sql;

            List<int> present = new List<int>();

            using (var reader = selectCmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var wordId = reader.GetInt32(0);
                    present.Add(wordId);
                }
            }
            var result = new List<int>(wordIds);
            foreach (var w in present)
                result.Remove(w);

            return result;
        }

        public List<string> WordsFromIds(List<int> wordIds, bool caseSensitive = false)
        {
            var sql = "SELECT name FROM Word where ";
            sql += "id in " + AsString(wordIds);
            if (!caseSensitive)
                sql += " COLLATE NOCASE";

            var selectCmd = _connection.CreateCommand();
            selectCmd.CommandText = sql;

            List<string> result = new List<string>();

            using (var reader = selectCmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var wordId = reader.GetString(0);
                    result.Add(wordId);
                }
            }
            return result;
        }

        public List<int> GetWordIds(string[] query, out List<string> outIgnored, bool caseSensitive = false)
        {
            if (mWords == null)
            {
                mWords = GetAllWords();
                mWordsIgnoreCase = new Dictionary<string, List<int>>(StringComparer.OrdinalIgnoreCase);
                foreach (var kvp in mWords)
                {
                    if (!mWordsIgnoreCase.TryGetValue(kvp.Key, out var ids))
                    {
                        ids = new List<int>();
                        mWordsIgnoreCase[kvp.Key] = ids;
                    }
                    ids.Add(kvp.Value);
                }
            }

            var res = new List<int>();
            var ignored = new List<string>();

            foreach (var aWord in query)
            {
                if (caseSensitive)
                {
                    if (mWords.TryGetValue(aWord, out int id))
                        res.Add(id);
                    else
                        ignored.Add(aWord);
                }
                else
                {
                    if (mWordsIgnoreCase.TryGetValue(aWord, out var ids))
                        res.AddRange(ids);
                    else
                        ignored.Add(aWord);
                }
            }

            outIgnored = ignored;
            return res;
        }
    }
}