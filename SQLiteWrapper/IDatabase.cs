using System;
using System.Data;
using System.Collections.Generic;
using System.Runtime.Serialization;

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
        /// <exception cref="DatabaseConnectionException">
        /// Could not connect to the database file.
        /// </exception>
        DataTable GetDataTable(string sql, List<Tuple<string, object>> parameters = null);

        /// <summary>
        /// Execute a sql command with no expected return value.
        /// </summary>
        /// <exception cref="DatabaseConnectionException">
        /// Could not connect to the database file.
        /// </exception>
        int ExecuteNonQuery(string sql, List<Tuple<string, object>> parameters = null);

        /// <summary>
        /// Retrieve a single value from the database.
        /// </summary>
        /// <exception cref="DatabaseConnectionException">
        /// Could not connect to the database file.
        /// </exception>
        object ExecuteScalar(string sql, List<Tuple<string, object>> parameters = null);

        #endregion

        #region Generic Table Management Methods

        /// <summary>
        /// Delete all data from the DB.
        /// </summary>
        void ClearDB();

        /// <summary>
        ///  Clear all data from a specific table.
        /// </summary>
        /// <param name="table">The name of the table to clear.</param>
        void ClearTable(String table);

        /// <summary>
        /// Get the name of every table in the database.
        /// </summary>
        /// <returns>The all table names.</returns>
        List<string> GetAllTableNames();

        /// <summary>
        /// Determines if the specified value exists anywhere in the column.
        /// </summary>
        /// <exception cref="ArgumentNullException">One of the provided args was null.</exception>
        /// <exception cref="DatabaseConnectionException">Could not connect to the database file. </exception>
        bool ValueExistsInColumn(string tableName, string column, string value);

        #endregion
    }

    [Serializable]
    public class DatabaseConnectionException : Exception
    {
        public DatabaseConnectionException(string message)
            : base(message) { }
        public DatabaseConnectionException(string message, Exception innerException)
            : base(message, innerException) { }
        protected DatabaseConnectionException(SerializationInfo info, StreamingContext ctxt)
            : base(info, ctxt) { }
    }

    [Serializable]
    public class DatabaseContentException : Exception
    {
        public DatabaseContentException(string message)
            : base(message) { }
        public DatabaseContentException(string message, Exception innerException)
            : base(message, innerException) { }
        protected DatabaseContentException(SerializationInfo info, StreamingContext ctxt)
            : base(info, ctxt) { }
    }
}

