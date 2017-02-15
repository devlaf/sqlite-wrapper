using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using SQLiteWrapper;

namespace SQLiteWrapperIntegrationTests
{
    /// <summary>
    /// Some mediocre integration tests (sanity checks?) for the SQLiteDatabase implementation.
    /// </summary>
    public class SQLiteDatabaseIntegrationTests
    {
        /// <summary>
        /// Creates a new temporary sqlite database in the /tmp folder and returns the associated IDatabase.
        /// </summary>
        private static IDatabase GetNewEmptyDatabase()
        {
            string uniqueFilePath = "/tmp/SQLite_unittest_" + Guid.NewGuid() + ".db3";
            return new SQLiteDatabase(new SQLiteDatabase.DatabaseLocationInfo("ignoring_this", uniqueFilePath));
        }

        /// <summary>
        /// Creates a populates a temporary sqlite database with example data.  Returned DB contents will look like:
        /// 
        /// TableName: testing
        /// ----------------------
        /// | BLAH | BLEH | BLEE | 
        /// ----------------------
        /// |   a  |   b  |   c  |
        /// ----------------------
        /// |   d  |   e  |   f  |
        /// ----------------------
        /// |   h  |   i  |   j  |
        /// ----------------------
        /// </summary>
        private static IDatabase GetNewPopulatedDatabase()
        {
            const string CREATE_EXAMPLE_TABLE_SQL = "CREATE TABLE IF NOT EXISTS 'testing' (BLAH TEXT, " +
                                                        "BLEH TEXT, BLEE TEXT, PRIMARY KEY (BLAH));";

            const string INSERT_EXAMPLE_ROW_SQL = "INSERT INTO testing(BLAH, BLEH, BLEE) " +
                                                      "VALUES(@param0, @param1, @param2);";

            Func<string, string, string, List<Tuple<string, object>>> GenerateInsertParams =
                (x, y, z) => new List<Tuple<string, object>>
                {
                    new Tuple<string, object>("@param0", x),
                    new Tuple<string, object>("@param1", y),
                    new Tuple<string, object>("@param2", z)
                };

            var db = GetNewEmptyDatabase();

            db.ExecuteNonQuery(CREATE_EXAMPLE_TABLE_SQL);
            db.ExecuteNonQuery(INSERT_EXAMPLE_ROW_SQL, GenerateInsertParams("a", "b", "c"));
            db.ExecuteNonQuery(INSERT_EXAMPLE_ROW_SQL, GenerateInsertParams("d", "e", "f"));
            db.ExecuteNonQuery(INSERT_EXAMPLE_ROW_SQL, GenerateInsertParams("h", "i", "j"));

            return db;
        }

        [Fact]
        public void GetAllTableNames_Should_Succeed()
        {
            var db = GetNewPopulatedDatabase();

            var names = db.GetAllTableNames();
            Assert.True(names.Count == 1);
            Assert.True(names.Contains("testing"));
        }

        [Fact]
        public void GetTable_Should_Succeed()
        {
            var db = GetNewPopulatedDatabase();

            var table = db.GetTable("SELECT * from testing;");
            
            Assert.True(table.Count == 3);
            Assert.True(table.First().Count == 3);

            var firstRow = table.Where(x => x.ContainsValue("a")).First();
            Assert.True(firstRow["BLAH"] as string == "a");
            Assert.True(firstRow["BLEH"] as string == "b");
            Assert.True(firstRow["BLEE"] as string == "c");

            var secondRow = table.Where(x => x.ContainsValue("d")).First();
            Assert.True(secondRow["BLAH"] as string == "d");
            Assert.True(secondRow["BLEH"] as string == "e");
            Assert.True(secondRow["BLEE"] as string == "f");

            var thirdRow = table.Where(x => x.ContainsValue("h")).First();
            Assert.True(thirdRow["BLAH"] as string == "h");
            Assert.True(thirdRow["BLEH"] as string == "i");
            Assert.True(thirdRow["BLEE"] as string == "j");
        }

        [Fact]
        public void ClearTable_Should_Succeed()
        {
            var db = GetNewPopulatedDatabase();

            db.ClearTable("testing");

            var table = db.GetTable("SELECT * from testing;");

            Assert.True(table.Count == 0);
        }

        [Fact]
        public void ClearDB_Should_Succeed()
        {
            var db = GetNewPopulatedDatabase();

            db.ClearDB();

            var tableNames = db.GetAllTableNames();

            Assert.False(tableNames.Contains("testing"));
        }

        [Fact]
        public void ValueExistsInColum_Should_Succeed_ForValidValue()
        {
            var db = GetNewPopulatedDatabase();
            Assert.True(db.ValueExistsInColumn("testing", "BLAH", "d"));
        }

        [Fact]
        public void ValueExistsInColum_Should_Fail_ForInValidValue()
        {
            var db = GetNewPopulatedDatabase();
            Assert.False(db.ValueExistsInColumn("testing", "BLAH", "q"));
        }

        [Fact]
        public void ValueExistsInColum_Should_Throw_ForInValidTable()
        {
            var db = GetNewPopulatedDatabase();
            Assert.Throws<DatabaseContentException>(() => db.ValueExistsInColumn("woops", "BLAH", "a"));
        }
    }
}
