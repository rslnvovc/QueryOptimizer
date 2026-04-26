using Microsoft.Data.SqlClient;
using QueryOptimizer.Providers.PostgreSQL.Connectivity;
using QueryOptimizer.Providers.SQLServer.Connectivity;
using QueryOptimizer.Providers.Oracle.Connectivity;
using QueryOptimizer.DatabaseExecutor.Abstractions;
using QueryOptimizer.Shared.Common.Enums;
using QueryOptimizer.Shared.Common.Exceptions.Database;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;

namespace QueryOptimizer.DatabaseExecutor
{
    public class DatabaseSource : IDatabaseSource
    {
        private readonly DatabaseTypes _dbType;
        private DbConnection _connection;
        private readonly SqlServerConnectionProvider _sqlServerConnectionProvider;
        private readonly PostgreSqlConnectionProvider _postgresDbConnectionProvider;
        private readonly OracleConnectionProvider _oracleConnectionProvider;
        private readonly string _connectionString;

        public DatabaseSource(string connectionString, DatabaseTypes dbType)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _dbType = dbType;

            _sqlServerConnectionProvider = new();
            _postgresDbConnectionProvider = new();
            _oracleConnectionProvider = new();
        }

        public DatabaseTypes DbType => _dbType;
        public Task<DbCommand> GetCommandAsync(string commandText)
        {
            return GetCommandAsync(commandText, CommandType.Text);
        }

        public async Task<DbCommand> GetCommandAsync(string commandText, CommandType commandType)
        {
            var command = CreateCommand(commandText, commandType);

            command.Connection = await GetConnectionAsync();

            return command;
        }

        private async Task<DbConnection> GetConnectionAsync()
        {
            if (_connection != null && !string.IsNullOrEmpty(_connection.ConnectionString))
            {
                if (_connection.State == ConnectionState.Broken)
                {
                    // must Close() a broken connection before it can be reopened
                    _connection.Close();
                    await _connection.OpenAsync();
                }
                else if (_connection.State == ConnectionState.Closed)
                {
                    await _connection.OpenAsync();
                }

                return _connection;
            }

            // create new connection.
            switch (_dbType)
            {
                case DatabaseTypes.SqlServer:
                    _connection = await _sqlServerConnectionProvider.CreateConnectionAsync(_connectionString);
                    break;
                case DatabaseTypes.PostgreSql:
                    _connection = await _postgresDbConnectionProvider.CreateConnectionAsync(_connectionString);
                    break;
                case DatabaseTypes.Oracle:
                    _connection = await _oracleConnectionProvider.CreateConnectionAsync(_connectionString);
                    break;
                default: throw new NotSupportedDBTypeException();
            }

            return _connection;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        private DbCommand CreateCommand(string commandText, CommandType commandType)
        {
            DbCommand command;

            switch (_dbType)
            {
                case DatabaseTypes.SqlServer:
                    command = _sqlServerConnectionProvider.CreateCommand(commandText, commandType);
                    break;
                case DatabaseTypes.PostgreSql:
                    command = _postgresDbConnectionProvider.CreateCommand(commandText, commandType);
                    break;
                case DatabaseTypes.Oracle:
                    command = _oracleConnectionProvider.CreateCommand(commandText, commandType);
                    break;
                default: throw new NotSupportedDBTypeException();
            }

            return command;
        }

        #region Disposing
        private bool _disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                try { _connection.Close(); } catch { };
                _connection.Dispose();
                _disposed = true;
            }
        }
        #endregion
    }
}
