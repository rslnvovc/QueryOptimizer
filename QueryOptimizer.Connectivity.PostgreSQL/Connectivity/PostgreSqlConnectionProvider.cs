using Npgsql;
using QueryOptimizer.Shared.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace QueryOptimizer.Providers.PostgreSQL.Connectivity
{
    public class PostgreSqlConnectionProvider : DatabaseConnectionProvider<NpgsqlConnection, NpgsqlCommand>
    {
        public override async Task<NpgsqlConnection> CreateConnectionAsync(string connectionString, CancellationToken cancellationToken = default)
        {
            var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);
            return connection;
        }

        public override NpgsqlCommand CreateCommand(string commandText, CommandType commandType)
        {
            NpgsqlCommand command = new NpgsqlCommand
            {
#pragma warning disable S3649 // User-provided values should be sanitized before use in SQL statements
                CommandText = commandText,
#pragma warning restore S3649 // User-provided values should be sanitized before use in SQL statements
                CommandType = commandType
            };
            return command;
        }
    }
}
