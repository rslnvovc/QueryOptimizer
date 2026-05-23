using Microsoft.Data.SqlClient;
using QueryOptimizer.DatabaseExecutor.Abstractions;
using QueryOptimizer.DatabaseExecutor.Helpers;
using QueryOptimizer.Providers.Oracle.Analyzing;
using QueryOptimizer.Providers.Oracle.Parsing;
using QueryOptimizer.Providers.PostgreSQL.Analyzing;
using QueryOptimizer.Providers.PostgreSQL.Parsing;
using QueryOptimizer.Providers.SQLServer.Analyzing;
using QueryOptimizer.Providers.SQLServer.Parsing;
using QueryOptimizer.Shared.Common.Enums;
using QueryOptimizer.Shared.Common.Exceptions.Database;
using QueryOptimizer.Shared.Common.Models.ExecutionPlan;
using QueryOptimizer.Shared.Common.Models.Metrics;
using QueryOptimizer.Shared.Infrastructure.Abstractions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Xml.XPath;

namespace QueryOptimizer.DatabaseExecutor
{
    internal class DatabaseExecutor : IDatabaseExecutor
    {
        #region IDbSource related code ...

        private readonly IDatabaseSource _dbSource;
        private readonly IQueryPerformanceAnalyzer _queryPerformanceOptimizer;
        private readonly IExecutionPlanParser _executionPlanParser;

        public int CommandTimeout { get; set; }

        public DatabaseExecutor(IDatabaseSource dbSource)
        {
            _dbSource = dbSource;

            switch (dbSource.DbType)
            {
                case DatabaseTypes.SqlServer:
                    _queryPerformanceOptimizer = new SqlServerQueryPerformanceAnalyzer();
                    _executionPlanParser = new SqlServerExecutionPlanParser();
                    break;
                case DatabaseTypes.PostgreSql:
                    _queryPerformanceOptimizer = new PostgreSqlQueryPerformanceAnalyzer();
                    _executionPlanParser = new PostgresExecutionPlanParser();
                    break;
                case DatabaseTypes.Oracle:
                    _queryPerformanceOptimizer = new OracleQueryPerformanceAnalyzer();
                    _executionPlanParser = new OracleExecutionPlanParser();
                    break;
                default: throw new NotSupportedDBTypeException();
            }

            CommandTimeout = -1;
        }


        public Task<DbCommand> GetCommandAsync(string commandText)
        {
            return GetCommandAsync(commandText, null);
        }

        public Task<DbCommand> GetCommandAsync(string commandText, Dictionary<string, object> parameters)
        {
            return GetCommandAsync(commandText, parameters, CommandType.Text);
        }

        public async Task<DbCommand> GetCommandAsync(string commandText, Dictionary<string, object> parameters, CommandType commandType)
        {
            var command = await _dbSource.GetCommandAsync(commandText, commandType);

            if (CommandTimeout != -1)
                command.CommandTimeout = CommandTimeout;

            PopulateCommandParameters(command, parameters);

            return command;
        }
        #endregion   

        public async Task<QueryPerformanceMetrics> AnalyzeAsync(string sql, Dictionary<string, object> parameters, CancellationToken cancellationToken = default)
        {
            using var command = await GetCommandAsync(sql, parameters);
            var result = await _queryPerformanceOptimizer.AnalyzeAsync(command, cancellationToken);
            return result;
        }

        public async Task<string> GetExecutionPlanAsync(string sql, Dictionary<string, object> parameters, CancellationToken cancellationToken = default)
        {
            using var command = await GetCommandAsync(sql, parameters);
            var result = await _queryPerformanceOptimizer.GetEstimatedExecutionPlanAsync(command, cancellationToken);
            return result;
        }

        public NormalizedExecutionPlan ParseExecutionPlan(QueryPerformanceMetrics metrics)
            => _executionPlanParser.Parse(metrics);

        public async Task<int> ExecuteNonQueryAsync(string sql, CancellationToken cancellationToken = default)
        {
            return await ExecuteNonQueryAsync(sql, null, cancellationToken);
        }

        public async Task<int> ExecuteNonQueryAsync(string sql, Dictionary<string, object> parameters, CancellationToken cancellationToken = default)
        {
            return await ExecuteNonQueryAsync(sql, parameters, CommandType.StoredProcedure, cancellationToken);
        }

        public async Task<int> ExecuteNonQueryAsync(string sql, Dictionary<string, object> parameter, CommandType commantType, CancellationToken cancellationToken = default)
        {
            try
            {
                using var command = await GetCommandAsync(sql, parameter, commantType);
                return await command.ExecuteNonQueryAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<T> ExecuteScalarAsync<T>(string sql, CancellationToken cancellationToken = default)
        {
            return await ExecuteScalarAsync<T>(sql, null, cancellationToken);
        }

        public async Task<T> ExecuteScalarAsync<T>(string sql, Dictionary<string, object> parameters, CancellationToken cancellationToken = default)
        {
            return await ExecuteScalarAsync<T>(sql, parameters, CommandType.StoredProcedure, cancellationToken);
        }

        public async Task<T> ExecuteScalarAsync<T>(string sql, Dictionary<string, object> parameters, CommandType commandType, CancellationToken cancellationToken = default)
        {
            object result = null;

            try
            {
                using var command = await GetCommandAsync(sql, parameters, commandType);
                result = (await command.ExecuteScalarAsync(cancellationToken)).GetTypedValue<T>();
            }
            catch (Exception ex)
            {
                throw;
            }
            return (T)result;
        }

        public async Task ExecuteComplexAsync(string sql, Dictionary<string, object> parameters, CommandType commandType, CancellationToken cancellationToken = default, params IList[] res)
        {
            using var command = await GetCommandAsync(sql, parameters, commandType);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            var resultSetIndex = 0;

            do
            {
                if (resultSetIndex >= res.Length)
                    break;

                var targetList = res[resultSetIndex];

                if (targetList == null)
                {
                    resultSetIndex++;
                    continue;
                }

                var itemType = ResultSetExtensions.GetListItemType(targetList);

                while (await reader.ReadAsync(cancellationToken))
                {
                    var item = ResultSetExtensions.MapReaderRowToObject(reader, itemType);
                    targetList.Add(item);
                }

                resultSetIndex++;
            }
            while (await reader.NextResultAsync(cancellationToken));
        }

        public async Task<IList<T>> ExecuteQueryAsync<T>(string sql, CancellationToken cancellationToken = default)
        {
            return await ExecuteQueryAsync<T>(
                sql,
                null,
                CommandType.Text,
                cancellationToken);
        }

        public async Task<IList<T>> ExecuteQueryAsync<T>(string sql, Dictionary<string, object> parameters, CancellationToken cancellationToken = default)
        {
            return await ExecuteQueryAsync<T>(
                sql,
                parameters,
                CommandType.StoredProcedure,
                cancellationToken);
        }

        public async Task<IList<T>> ExecuteQueryAsync<T>(string sql, Dictionary<string, object> parameters, CommandType commandType, CancellationToken cancellationToken = default)
        {
            var result = new List<T>();

            using var command = await GetCommandAsync(sql, parameters, commandType);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                var item = ResultSetExtensions.MapReaderRowToObject<T>(reader);
                result.Add(item);
            }

            return result;
        }

        private static void PopulateCommandParameters(IDbCommand command, Dictionary<string, object> parameters)
        {
            if (parameters != null && parameters.Count != 0)
            {
                foreach (var pair in parameters)
                {
                    var p = command.CreateParameter();
                    p.ParameterName = pair.Key;
                    p.Value = pair.Value ?? DBNull.Value;

                    var table = pair.Value as DataTable;
                    if (table != null)
                    {
                        var sqlParam = (SqlParameter)p;
                        sqlParam.SqlDbType = SqlDbType.Structured;
                        sqlParam.TypeName = table.TableName;
                    }

                    command.Parameters.Add(p);
                }
            }
        }

        #region IDisposable...

        private bool _disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing) _dbSource?.Dispose();
                _disposed = true;
            }
        }

        ~DatabaseExecutor()
        {
            Dispose(false);
        }

        #endregion  
    }
}
