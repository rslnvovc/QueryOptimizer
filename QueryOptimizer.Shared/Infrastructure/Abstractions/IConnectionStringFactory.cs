using QueryOptimizer.Shared.Common.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace QueryOptimizer.Shared.Infrastructure.Abstractions
{
    public interface IConnectionStringFactory
    {
        string Create(string? serverName,
                      string? userName,
                      string? password,
                      string? databaseName,
                      string? host,
                      int? port,
                      string? serviceName);
    }
}
