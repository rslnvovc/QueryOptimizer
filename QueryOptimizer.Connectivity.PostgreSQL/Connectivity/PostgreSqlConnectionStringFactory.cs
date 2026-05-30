using QueryOptimizer.Shared.Common.Exceptions.Database;
using QueryOptimizer.Shared.Infrastructure.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace QueryOptimizer.Providers.PostgreSQL.Connectivity
{
    public class PostgreSqlConnectionStringFactory : IConnectionStringFactory
    {
        public string Create(string? serverName, 
                             string? userName, 
                             string? password, 
                             string? databaseName, 
                             string? host, 
                             int? port, 
                             string? serviceName)
        {
            if (!ValidateConnectionStringParameters(serverName, databaseName, userName, password, host, port))
                throw new NotValidConnectionStringException();

            string connectionString =
                $"Host={host};Port={port};Database={databaseName};Username={userName};Password={password}";

            return connectionString;
        }

        private bool ValidateConnectionStringParameters(
                             string? serverName,
                             string? databaseName,
                             string? userName,
                             string? password,
                             string? host,
                             int? port)
        {
            if (string.IsNullOrEmpty(serverName) ||
                string.IsNullOrEmpty(databaseName) ||
                string.IsNullOrEmpty(userName) ||
                string.IsNullOrEmpty(password) ||
                string.IsNullOrEmpty(host) ||
                port is null) return false;

            return true;
        }
    }
}
