﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Data.Sqlite;

namespace SQLiteWrapper
{
    /// <summary>
    /// A more abstract wrapper layer for SQLite operations -- provides a
    /// more useful interface on top of the lower-level SQLiteConection stuff
    /// and manages the connection settings. 
    /// </summary>
    public class SQLiteDatabase : IDatabase
    {
        private const string MasterSystemTable = "SQLITE_MASTER";
        private readonly DatabaseLocationInfo ConnectionInfo;
        private string CachedDatabaseConnectionString;


        public SQLiteDatabase(DatabaseLocationInfo connectionInfo)
        {
            ConnectionInfo = connectionInfo;
        }

        #region (Optional) Logger

        /// <summary>
        /// If you want to attach a logger for error messages, go for it.
        /// </summary>
        /// <remarks>
        /// I've opted not to wrap this method and do any sort of pooling/threading/batching/etc.  That's 
        /// on you, or otherwise just make sure that your log function doesn't take a really long time.
        /// </remarks>
        public Action<string> LogError { 
            get { return _LogError ?? new Action<string>((x) => { return; }); }
            set { _LogError = value; }
        }

        private Action<string> _LogError = null;

        #endregion

        #region DatabaseConnectionInfo

        /// <summary>
        /// Defines the location of the SQLite database file.  This struct will be required as part of
        ///  the constructor for the SQLiteDatabase class.  
        /// </summary>
        /// <remarks>
        /// There are two ways that the connection string can be defined.  The primary way is using an 
        /// enviornment variable.  If that enviornment variable does not exist on the system, then this 
        /// library will use a backup string specified as part of this struct.  So it is up to the caller 
        /// of this class to specify an enviornment variable name and a hardcoded backup path, and then 
        /// they decide which they want to use by either defining the enviornment variable or not.
        /// </remarks>
        public struct DatabaseLocationInfo
        {
            public readonly string DBLocationEnviornmentVariableName;
            public readonly string DBLocationBackupPath;

            /// <summary>
            /// A struct that defines the connection info, specifically where the .sqlite file is located, 
            /// for the sqlite database
            /// </summary>
            /// <param name="dBLocationEnviornmentVariableName">The name of an enviornment variable which 
            /// can define the path to the database.  If this environment variable exists, it takes precedence 
            /// over the secondary backup path specified in dBLocationBackupPath.</param>
            /// <param name="dBLocationBackupPath">If the enviornment variable does not exist, the 
            /// SQLiteDatabase class will use this to determine the location of the DB file.</param>
            public DatabaseLocationInfo(string dBLocationEnviornmentVariableName, string dBLocationBackupPath)
            {
                DBLocationEnviornmentVariableName = dBLocationEnviornmentVariableName;
                DBLocationBackupPath = dBLocationBackupPath;
            }
        }

        #endregion

        #region Get Connection

        private SqliteConnection GetConnection()
        {
            // Due to SQLite's internal pooling implementation, there is no reason
            // to manage a single connection - we can create a new one each time and
            // pooling is handled for us.
            return new SqliteConnection(GetConnectionString());
        }

        /// <summary>
        /// Serve/Create the database connection settings.
        /// </summary>
        /// <remarks>
        /// Ultimately we're simply specifying the database filepath here; however, there's quite a lot that's 
        /// going on internally by default.  The Microsoft.Data.SQLite library (being somewhat juvenile?)
        /// does not support several of the settings that we would typically like to express explicitly
        /// as part of the connection string (ex. FailIfMissing, Version, Pooling, Pool Size.)
        /// 
        /// My understanding of the non-customizable behavior is that:
        /// - If the database if it does not exist, the SQLiteConnection constructor will create it.
        /// - The library will by default use connection pooling.  Fortunately this is all handled under
        /// the covers - In this abstraction, closing the SqliteConnection object does not actually
        /// close the connection, but returns it to the internal pool.
        /// - Default max pool size is 100.
        /// - Will use version 3.X of SQLite native libs
        /// 
        /// I cache the conection string here for the lifetime of the application in order to avoid doing
        /// the enviornment variable lookup each time, however I could see some value in being able to 
        /// dynamically switch the database location while the application continues runnning.  For now, we
        /// only do that as part of debug builds to facilitate integration tests.
        /// </remarks>
        private string GetConnectionString()
        {
            // For integration testing, we want to disable caching of the source string so we can 
            // dynamically switch the database to use a mocked(?) one.
            #if DEBUG
            CachedDatabaseConnectionString = null;
            #endif

            if(CachedDatabaseConnectionString == null)
            {
                string databaseFilepath = Environment.GetEnvironmentVariable(ConnectionInfo.DBLocationEnviornmentVariableName) ??
                              ConnectionInfo.DBLocationBackupPath;
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(databaseFilepath));

                CachedDatabaseConnectionString = string.Format("Data Source={0}", databaseFilepath);
            }

            return CachedDatabaseConnectionString;
        }

        #endregion

        #region Commands to execute raw SQL

        /// <summary>
        /// Retrieve an entire data table from the database.
        /// </summary>
        /// <exception cref="DatabaseConnectionException">
        /// Could not connect to the database file.
        /// </exception>
        public List<Dictionary<string, object>> GetTable(string sql, List<Tuple<string, object>> parameters = null)
        {
            return ExecuteSQLiteCommand<List<Dictionary<string, object>>>(sql, parameters, x => ExtractRowData(x.ExecuteReader()).ToList());
        }

        private static IEnumerable<Dictionary<string, object>> ExtractRowData(SqliteDataReader data)
        {
            while (data.Read())
            {
                yield return Enumerable.Range(0, data.FieldCount).Aggregate(new Dictionary<string, object>(), 
                    (dict, index) => { dict.Add(data.GetName(index), data.GetValue(index)); return dict; });
            }
        }

        /// <summary>
        /// Execute a sql command with no expected return value.
        /// </summary>
        /// <exception cref="DatabaseConnectionException">
        /// Could not connect to the database file.
        /// </exception>
        public int ExecuteNonQuery(string sql, List<Tuple<string, object>> parameters = null)
        {
            return ExecuteSQLiteCommand<int>(sql, parameters, x => x.ExecuteNonQuery());
        }

        /// <summary>
        /// Retrieve a single value from the database.
        /// </summary>
        /// <exception cref="DatabaseConnectionException">
        /// Could not connect to the database file.
        /// </exception>
        public object ExecuteScalar(string sql, List<Tuple<string, object>> parameters = null)
        {
            return ExecuteSQLiteCommand<object>(sql, parameters, x => x.ExecuteScalar());
        }

        private T ExecuteSQLiteCommand<T>(string sql, List<Tuple<string, object>> parameters, Func<SqliteCommand, T> func)
        {
            SqliteConnection con = GetConnection();
            try
            {
                con.Open();

                SqliteCommand command = new SqliteCommand(sql, con);
                if(parameters != null)
                    command.Parameters.AddRange(parameters.Select(x => new SqliteParameter(x.Item1, x.Item2)).ToArray());

                try
                {
                    return func(command);
                } 
                catch(Exception ex)
                {
                    throw new DatabaseContentException("Error operating on database data with command [" + sql + "].", ex);
                }
            } 
            catch(Exception ex)
            {
                if(ex is DatabaseContentException)
                    throw;

                string errorMessage = string.Format("Error registered in connecting to the SQLITE database at [{0}] for query [{1}].",
                              CachedDatabaseConnectionString, sql);

                LogError(string.Format("{0}.  Error exception:{1}{2}", errorMessage, System.Environment.NewLine, ex.ToString()));
                throw new DatabaseConnectionException(errorMessage, ex);
            }
            finally
            {
                con.Close();
            }
        }

        #endregion

        #region Generic Table Management Methods

        /// <summary>
        /// Delete all data from the DB.
        /// </summary>
        public void ClearDB()
        {
            var tables = GetTable("select NAME from " + MasterSystemTable + " where type='table' order by NAME;");

            foreach(var row in tables)
            {
                var tableName = row["name"].ToString();
                ExecuteNonQuery(string.Format("DROP TABLE IF EXISTS '{0}'", tableName));
            }
        }

        /// <summary>
        ///  Clear all data from a specific table.
        /// </summary>
        /// <param name="table">The name of the table to clear.</param>
        public void ClearTable(string table)
        {
            if(TableExists(table))
                ExecuteNonQuery(string.Format("delete from {0};", table));
        }

        /// <summary>
        /// Get the name of every table in the database.
        /// </summary>
        /// <returns>The all table names.</returns>
        public List<string> GetAllTableNames()
        {
            var tables = GetTable("select NAME from " + MasterSystemTable + " where type='table' order by NAME;");
            return tables.Select(x => x["name"] as string).ToList();
        }

        private bool TableExists(string tableName)
        {
            return GetAllTableNames().Contains(tableName);
        }

        /// <summary>
        /// Determines if the specified value exists anywhere in the column.
        /// </summary>
        /// <exception cref="ArgumentNullException">One of the provided args was null.</exception>
        /// <exception cref="DatabaseConnectionException">Could not connect to the database file. </exception>
        public bool ValueExistsInColumn(string tableName, string column, string value)
        {
            if(tableName == null)
                throw new ArgumentNullException("Table string provided was null.");
            if(column == null)
                throw new ArgumentNullException("column string provided was null.");
            if(value == null)
                throw new ArgumentNullException("value string provided was null.");

            string sql = String.Format("SELECT COUNT(1) FROM '{0}' WHERE {1} = '{2}';", tableName, column, value);
            return (Convert.ToInt32(ExecuteScalar(sql)) != 0);
        }

        #endregion
    }

}

