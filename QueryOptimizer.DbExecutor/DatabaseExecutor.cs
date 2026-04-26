using Microsoft.Data.SqlClient;
using QueryOptimizer.DatabaseExecutor.Abstractions;
using QueryOptimizer.DatabaseExecutor.Helpers;
using QueryOptimizer.Providers.Oracle.Analyzing;
using QueryOptimizer.Providers.PostgreSQL.Analyzing;
using QueryOptimizer.Providers.SQLServer.Analyzing;
using QueryOptimizer.Shared.Common.Enums;
using QueryOptimizer.Shared.Common.Exceptions.Database;
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

        public int CommandTimeout { get; set; }

        public DatabaseExecutor(IDatabaseSource dbSource)
        {
            _dbSource = dbSource;

            switch (dbSource.DbType)
            {
                case DatabaseTypes.SqlServer:
                    _queryPerformanceOptimizer = new SqlServerQueryPerformanceAnalyzer();
                    break;
                case DatabaseTypes.PostgreSql:
                    _queryPerformanceOptimizer = new PostgreSqlQueryPerformanceAnalyzer();
                    break;
                case DatabaseTypes.Oracle:
                    _queryPerformanceOptimizer = new OracleQueryPerformanceAnalyzer();
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


        #region Public Methods

        #endregion

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
