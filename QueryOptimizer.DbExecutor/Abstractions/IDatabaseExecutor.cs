using QueryOptimizer.Shared.Common.Models.Metrics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;

namespace QueryOptimizer.DatabaseExecutor.Abstractions
{
    public interface IDatabaseExecutor : IDisposable
    {
        int CommandTimeout { get; set; }

        Task<DbCommand> GetCommandAsync(string commandText);
        Task<DbCommand> GetCommandAsync(string commandText, Dictionary<string, object> parameters);
        Task<DbCommand> GetCommandAsync(string commandText, Dictionary<string, object> parameters, CommandType commandType);

        Task<QueryPerformanceMetrics> AnalyzeAsync(string sql, Dictionary<string, object> parameters, CancellationToken cancellationToken = default);
        Task<string> GetExecutionPlanAsync(string sql, Dictionary<string, object> parameters, CancellationToken cancellationToken = default);
    }
}
