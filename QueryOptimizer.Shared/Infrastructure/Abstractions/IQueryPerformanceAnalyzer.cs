using QueryOptimizer.Shared.Common.Enums;
using QueryOptimizer.Shared.Common.Models.Metrics;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace QueryOptimizer.Shared.Infrastructure.Abstractions
{
    public interface IQueryPerformanceAnalyzer
    {
        Task<QueryPerformanceMetrics> AnalyzeAsync(
            DbCommand command,
            CancellationToken cancellationToken = default);

        Task<string> GetEstimatedExecutionPlanAsync(
            DbCommand command,
            CancellationToken cancellationToken = default);
    }
}
