using QueryOptimizer.Models.Application;
using QueryOptimizer.Shared.Common.Enums;
using QueryOptimizer.Shared.Common.Models.Metrics;
using QueryOptimizer.Shared.Common.Models.Optimization;
using System;
using System.Collections.Generic;
using System.Text;

namespace QueryOptimizer.Optimization.Services.Abstractions
{
    public interface IAdaptiveLearningService
    {
        Task ApplyHistoryToFindingsAsync(
            DatabaseTypes provider,
            IList<QueryOptimizationFinding> findings,
            CancellationToken cancellationToken = default);

        Task LearnFromCandidateResultsAsync(
            DatabaseTypes provider,
            string normalizedSqlHash,
            QueryPerformanceMetrics originalMetrics,
            IList<OptimizationCandidateResult> candidateResults,
            CancellationToken cancellationToken = default);
    }
}
