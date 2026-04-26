using Microsoft.Data.SqlClient;
using QueryOptimizer.Shared.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;

namespace QueryOptimizer.Providers.SQLServer.Connectivity
{
    public class SqlServerConnectionProvider : DatabaseConnectionProvider<SqlConnection, SqlCommand>
    {
        public override async Task<SqlConnection> CreateConnectionAsync(string connectionString, CancellationToken cancellationToken = default)
        {
            var connection = new SqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);
            return connection;
        }

        public override SqlCommand CreateCommand(string commandText, CommandType commandType)
        {
            SqlCommand command = new SqlCommand
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
