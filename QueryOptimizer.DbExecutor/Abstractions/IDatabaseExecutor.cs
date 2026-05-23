using QueryOptimizer.Shared.Common.Models.ExecutionPlan;
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

        NormalizedExecutionPlan ParseExecutionPlan(QueryPerformanceMetrics metrics);

        Task<int> ExecuteNonQueryAsync(string sql, CancellationToken cancellationToken = default);
        Task<int> ExecuteNonQueryAsync(string sql, Dictionary<string, object> parameters, CancellationToken cancellationToken = default);
        Task<int> ExecuteNonQueryAsync(string sql, Dictionary<string, object> parameter, CommandType commantType, CancellationToken cancellationToken = default);

        Task<T> ExecuteScalarAsync<T>(string sql, CancellationToken cancellationToken = default);
        Task<T> ExecuteScalarAsync<T>(string sql, Dictionary<string, object> parameters, CancellationToken cancellationToken = default);
        Task<T> ExecuteScalarAsync<T>(string sql, Dictionary<string, object> parameters, CommandType commandType, CancellationToken cancellationToken = default);

        Task ExecuteComplexAsync(string sql, Dictionary<string, object> parameters, CommandType commandType, CancellationToken cancellationToken = default, params IList[] res);

        Task<IList<T>> ExecuteQueryAsync<T>(string sql, CancellationToken cancellationToken = default);
        Task<IList<T>> ExecuteQueryAsync<T>(string sql, Dictionary<string, object> parameters, CancellationToken cancellationToken = default);
        Task<IList<T>> ExecuteQueryAsync<T>(string sql, Dictionary<string, object> parameters, CommandType commandType, CancellationToken cancellationToken = default);
    }
}
