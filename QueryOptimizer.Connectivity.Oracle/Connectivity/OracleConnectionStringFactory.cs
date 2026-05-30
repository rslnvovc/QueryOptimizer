using QueryOptimizer.Shared.Common.Exceptions.Database;
using QueryOptimizer.Shared.Infrastructure.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace QueryOptimizer.Providers.Oracle.Connectivity
{
    public class OracleConnectionStringFactory : IConnectionStringFactory
    {
        public string Create(string? serverName, string? userName, string? password, string? databaseName, string? host, int? port, string? serviceName)
        {
            if (!ValidateConnectionStringParameters(userName, password, host, port, serviceName))
                throw new NotValidConnectionStringException();

            string connectionString =
                $"User Id={userName};Password={password};" +
                    $"Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST={host})(PORT={port}))" +
                    $"(CONNECT_DATA=(SERVICE_NAME={serviceName})));" +
                    "Connection Timeout=60;Pooling=false;";

            return connectionString;
        }

        private bool ValidateConnectionStringParameters(
            string? userName,
            string? password,
            string? host,
            int? port,
            string? serviceName
            )
        {
            if (string.IsNullOrEmpty(userName) ||
                string.IsNullOrEmpty(password) ||
                string.IsNullOrEmpty(host) ||
                port is null ||
                string.IsNullOrEmpty(serviceName)) return false;

            return true;
        }
    }
}
