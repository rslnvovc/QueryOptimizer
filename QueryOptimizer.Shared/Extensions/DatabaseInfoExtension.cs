using Microsoft.Data.SqlClient;
using Npgsql;
using Oracle.ManagedDataAccess.Client;
using QueryOptimizer.Shared.Common.Exceptions.Database;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace QueryOptimizer.Shared.Extensions
{
    public static class DatabaseInfoExtension
    {
        #region SQLServer
        public static void CheckSqlServerDbPassword(string connectionString)
        {
            var builder = new SqlConnectionStringBuilder(connectionString);

            if (String.IsNullOrEmpty(builder.Password))
                throw new NotValidConnectionStringException();
        }

        public static string GetSqlServerDatabaseName(string connectionString)
        {
            CheckSqlServerDbPassword(connectionString);
            var builder = new SqlConnectionStringBuilder(connectionString);
            return builder.InitialCatalog;
        }
        #endregion

        #region Postgres
        public static void CheckPostgresDbPassword(string connectionString)
        {
            var builder = new NpgsqlConnectionStringBuilder(connectionString);

            if (String.IsNullOrEmpty(builder.Password))
                throw new NotValidConnectionStringException();
        }

        public static string GetPostgresDatabaseName(string connectionString)
        {
            CheckPostgresDbPassword(connectionString);
            var builder = new NpgsqlConnectionStringBuilder(connectionString);
            return builder.Database ?? throw new DatabaseNameIsNotExistsException();
        }
        #endregion

        #region Oracle
        public static void CheckOracleDbPassword(string connectionString)
        {
            var builder = new OracleConnectionStringBuilder(connectionString);

            if (String.IsNullOrEmpty(builder.Password))
                throw new NotValidConnectionStringException();
        }

        public static string GetOracleDatabaseName(string connectionString)
        {
            CheckOracleDbPassword(connectionString);
            var builder = new OracleConnectionStringBuilder(connectionString);
            return builder.DataSource;
        }

        #endregion
    }
}
