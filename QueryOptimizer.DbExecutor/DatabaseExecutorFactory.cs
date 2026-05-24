using QueryOptimizer.DatabaseExecutor.Abstractions;
using QueryOptimizer.Shared.Common.Enums;
using QueryOptimizer.Shared.Common.Models.Metrics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace QueryOptimizer.DatabaseExecutor
{
    public static class DatabaseExecutorFactory
    {
        #region Fields
        private static Func<string> _getConnectionStringFunc;
        private static Func<DatabaseTypes> _getDatabaseTypeFunc;
        private static string _connectionString;
        private static DatabaseTypes _databaseType;
        #endregion Fields

        private static string ConnectionString => _getConnectionStringFunc?.Invoke() ?? _connectionString;
        private static DatabaseTypes DatabaseType => _getDatabaseTypeFunc?.Invoke() ?? _databaseType;

        public static void InitDatabase(string connectionString)
        {
            _connectionString = !string.IsNullOrEmpty(connectionString)
                ? connectionString
                : throw new InvalidDataException(nameof(connectionString));
        }

        public static void InitDatabase(DatabaseTypes dbType, string connectionString)
        {
            _databaseType = dbType;

            _connectionString = !string.IsNullOrEmpty(connectionString)
                ? connectionString
                : throw new InvalidDataException(nameof(connectionString));
        }

        public static void AssignGetConnectionStringFunc(Func<string> getConnectionStringFunc)
        {
            _getConnectionStringFunc = getConnectionStringFunc;
        }

        public static void DisposeGetConnectionStringFunc()
        {
            _getConnectionStringFunc = null;
        }

        public static IDatabaseExecutor CreateDbExecutor()
        {
            return new DatabaseExecutor(new DatabaseSource(ConnectionString, DatabaseType));
        }

        public static IDatabaseExecutor CreateDbExecutor(
            DatabaseTypes dbType,
            string connectionString)
        {
            var databaseSource = new DatabaseSource(connectionString, dbType);

            return new DatabaseExecutor(databaseSource);
        }

        public static async Task<QueryPerformanceMetrics> AnalyzeAsync(
            string sql, 
            Dictionary<string, object> parameters, 
            CancellationToken cancellationToken = default)
        {
            using var executor = CreateDbExecutor();
            return await executor.AnalyzeAsync(sql, parameters, cancellationToken);
        }

        public static async Task<string> GetExecutionPlanAsync(
            string sql,
            Dictionary<string, object> parameters,
            CancellationToken cancellationToken = default)
        {
            using var executor = CreateDbExecutor();
            return await executor.GetExecutionPlanAsync(sql, parameters, cancellationToken);
        }
    }
}
