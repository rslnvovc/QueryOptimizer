using Oracle.ManagedDataAccess.Client;
using QueryOptimizer.Shared.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace QueryOptimizer.Providers.Oracle.Connectivity
{
    public class OracleConnectionProvider : DatabaseConnectionProvider<OracleConnection, OracleCommand>
    {
        public override async Task<OracleConnection> CreateConnectionAsync(string connectionString, CancellationToken cancellationToken = default)
        {
            var connection = new OracleConnection(connectionString);
            await connection.OpenAsync(cancellationToken);
            return connection;
        }

        public override OracleCommand CreateCommand(string commandText, CommandType commandType)
        {
            OracleCommand command = new OracleCommand() 
            { 
                CommandText = commandText, 
                CommandType = commandType 
            };

            return command;
        }
    }
}
