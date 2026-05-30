using QueryOptimizer.Shared.Common.Enums;
using QueryOptimizer.Shared.Common.Exceptions.Database;
using QueryOptimizer.Shared.Infrastructure.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace QueryOptimizer.Providers.SQLServer.Connectivity
{
    public class SqlServerConnectionStringFactory : IConnectionStringFactory
    {
        public string Create(string? serverName, 
                             string? userName, 
                             string? password,
                             string? databaseName,
                             string? host, 
                             int? port, 
                             string? serviceName)
        {
            if (!ValidateConnectionStringParameters(serverName, databaseName))
                throw new NotValidConnectionStringException();

            string connectionString =
                $"Data Source={serverName};Initial Catalog={databaseName};Integrated Security=True;Persist Security Info=False;Pooling=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;"; ;

            return connectionString;
        }

        private bool ValidateConnectionStringParameters(
                             string? serverName,
                             string? databaseName)
        {
            if (string.IsNullOrEmpty(serverName) ||
                string.IsNullOrEmpty(databaseName)) return false;

            return true;
        }
    }
}
