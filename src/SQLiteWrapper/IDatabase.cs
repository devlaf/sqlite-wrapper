using System;
using System.Data;
using System.Collections.Generic;

namespace SQLiteWrapper
{
    /// <summary>
    /// A more abstract wrapper layer for SQLite operations -- hides 
    /// most of the sqlite implementation details and manages the connection
    /// settings.
    /// </summary>
    public interface IDatabase
    {
        #region Commands to execute raw SQL

        /// <summary>
        /// Retrieve an entire data table from the database.
        /// </summary>
        /// <exception cref="DatabaseConnectionException">Could not connect to the database file.</exception>
        List<Dictionary<string, object>> GetTable(string sql, List<Tuple<string, object>> parameters = null);

        /// <summary>
        /// Execute a sql command with no expected return value.
        /// </summary>
        /// <exception cref="DatabaseConnectionException">Could not connect to the database file.</exception>
        int ExecuteNonQuery(string sql, List<Tuple<string, object>> parameters = null);

        /// <summary>
        /// Retrieve a single value from the database.
        /// </summary>
        /// <exception cref="DatabaseConnectionException">Could not connect to the database file.</exception>
        object ExecuteScalar(string sql, List<Tuple<string, object>> parameters = null);

        #endregion

        #region Generic Table Management Methods

        /// <summary>
        /// Delete all data from the DB.
        /// </summary>
        /// <exception cref="DatabaseConnectionException">Could not connect to the database file. </exception>
        void ClearDB();

        /// <summary>
        /// Clear all data from a specific table, but leave the table.
        /// </summary>
        /// <param name="table">The name of the table to clear.</param>
        /// <exception cref="DatabaseConnectionException">Could not connect to the database file. </exception>
        /// <exception cref="DatabaseContentException">tableName does not exist in db. </exception>
        void ClearTable(String table);

        /// <summary>
        /// Get the name of every table in the database.
        /// </summary>
        /// <returns>The all table names.</returns>
        /// <exception cref="DatabaseConnectionException">Could not connect to the database file. </exception>
        List<string> GetAllTableNames();

        /// <summary>
        /// Determines if the specified value exists anywhere in the column.
        /// </summary>
        /// <exception cref="ArgumentNullException">One of the provided args was null.</exception>
        /// <exception cref="DatabaseConnectionException">Could not connect to the database file. </exception>
        /// <exception cref="DatabaseContentException">tableName or column do not exist in db. </exception>
        bool ValueExistsInColumn(string tableName, string column, string value);

        #endregion
    }

    public class DatabaseConnectionException : Exception
    {
        public DatabaseConnectionException(string message)
            : base(message) { }
        public DatabaseConnectionException(string message, Exception innerException)
            : base(message, innerException) { }
    }

    public class DatabaseContentException : Exception
    {
        public DatabaseContentException(string message)
            : base(message) { }
        public DatabaseContentException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}

