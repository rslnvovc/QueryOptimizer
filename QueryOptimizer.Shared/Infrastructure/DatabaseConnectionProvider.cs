using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace QueryOptimizer.Shared.Infrastructure
{
    public abstract class DatabaseConnectionProvider<TConnection, TCommand>
        where TConnection : IDbConnection
        where TCommand : IDbCommand
    {
        public abstract Task<TConnection> CreateConnectionAsync(string connectionString, CancellationToken cancellationToken = default);
        public abstract TCommand CreateCommand(string commandText, CommandType commandType);
    }
}
