using System;
using DatabaseTest;
using LiteDB;
using SQLite;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Data.SqlTypes;

namespace DatabaseTest
{
    public class TestClass
    {
        public static TestClass MockedInstance(int index)
        {
            return new TestClass
            {
                Id = $"{nameof(Id)}.{index}",
                StringProperty = $"{nameof(StringProperty)}.{index}",
                IntegerProperty = index,
            };
        }

        [BsonId]
        public string Id { get; set; } = "id";

        [BsonField]
        public string StringProperty { get; set; } = "stringValue";

        [BsonField]
        public int IntegerProperty { get; set; } = 42;

        public override string ToString()
        {
            return $"<{this.GetType().Name} {nameof(StringProperty)}='{StringProperty}' {nameof(IntegerProperty)}='{IntegerProperty}'>";
        }
    }

    public static class PerformanceTest
    {
        public static string FilterId = "[DBTEST]";
        public static void TestDatabase()
        {
            Console.WriteLine($"{FilterId} Testing DB performance");

            // demo encryption key
            string key = "nothing sucks seeds like parrot with no teeth";

            // clear and create folder for DB files
            string localDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string dbFolder = Path.Combine(localDataFolder, "Databases");
            if (Directory.Exists(dbFolder))
            {
                Directory.Delete(dbFolder, true);
            }
            Directory.CreateDirectory(dbFolder);

            //// Test collection - 25k items
            //List<DataObject> items = new List<DataObject>();
            //for (int i = 0; i < 25000; ++i)
            //{
            //    items.Add(DataObject.CreateMocked(i));
            //}

            // Test collection - 25k items
            List<TestClass> items = new List<TestClass>();
            for (int i = 0; i < 25000; ++i)
            {
                items.Add(TestClass.MockedInstance(i));
            }


            // Try LiteDB --------------------------------------
            Console.WriteLine($"{FilterId} Testing LiteDB ------------------------");
            string liteDbPath = Path.Combine(dbFolder, "litedb.db");
            TryLiteDB(new LiteDatabase($"Filename={liteDbPath};Journal=false"), items);

            Console.WriteLine($"{FilterId} Testing encrypted LiteDB ------------------------");
            string encryptedLiteDbPath = Path.Combine(dbFolder, "litedb.encrypted.db");
            TryLiteDB(new LiteDatabase($"Filename={encryptedLiteDbPath}; Password={key};Journal=false"), items);

            // test SQLite --------------------------------------
            Console.WriteLine($"{FilterId} Testing SQLite ------------------------");
            string sqliteDbPath = Path.Combine(dbFolder, "sqlite.db");
            TrySqlite(new SQLite.SQLiteConnectionString(sqliteDbPath, true), items);

            Console.WriteLine($"{FilterId} Testing encrypted SQLite ------------------------");
            string encryptedSqliteDbPath = Path.Combine(dbFolder, "sqlite.encrypted.db");
            TrySqlite(new SQLite.SQLiteConnectionString(encryptedSqliteDbPath, true, key), items);
        }

        private static void TryLiteDB<T>(LiteDatabase liteDatabase, IEnumerable<T> items)
        {
            try
            {
                Stopwatch watch = new Stopwatch();

                liteDatabase.Checkpoint();

                var typedCollection = liteDatabase.GetCollection<T>();
                //typedCollection.EnsureIndex(x => x.ParentCode);
                //typedCollection.EnsureIndex(x => x.TeamLibraryCode);
                //typedCollection.EnsureIndex(x => x.ExternalId);

                watch.Restart();
                //typedCollection.InsertBulk(items);
                foreach (var item in items)
                {
                    typedCollection.Insert(item);
                }
                watch.Stop();
                Console.WriteLine($"{FilterId} LiteDB inserted {items.Count()} items in {watch.Elapsed.TotalSeconds}");

                watch.Restart();
                List<T> selected = typedCollection.FindAll().ToList();
                watch.Stop();
                Console.WriteLine($"{FilterId} LiteDB selected {selected.Count} items as {typeof(T).Name} in {watch.Elapsed.TotalSeconds}");

                var bsonCollection = liteDatabase.GetCollection(typeof(T).Name);
                watch.Restart();
                List<BsonDocument> selectedBsons = bsonCollection.FindAll().ToList();
                watch.Stop();
                Console.WriteLine($"{FilterId} LiteDB selected {selectedBsons.Count} items as {nameof(BsonDocument)} in {watch.Elapsed.TotalSeconds}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"{FilterId} Exception testing LiteDB: {e}");
            }
        }

        private static void TrySqlite<T>(SQLiteConnectionString connectionString, IEnumerable<T> items) where T : new()
        {
            try
            {
                Stopwatch watch = new Stopwatch();

                SQLiteConnection connection = new SQLiteConnection(connectionString);
                connection.CreateTable<T>();
                //connection.CreateIndex<T>(o => o.ParentCode);
                //connection.CreateIndex<T>(o => o.TeamLibraryCode);
                //connection.CreateIndex<T>(o => o.ExternalId);

                watch.Restart();
                connection.InsertAll(items);
                watch.Stop();
                Console.WriteLine($"{FilterId} SQLite inserted {items.Count()} items in {watch.Elapsed.TotalSeconds}");

                watch.Restart();
                List<T> selected = connection.Table<T>().ToList();
                watch.Stop();
                Console.WriteLine($"{FilterId} SQLite selected {selected.Count} items in {watch.Elapsed.TotalSeconds}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"{FilterId} Exception testing SQLite: {e}");
            }
        }
    }
}

