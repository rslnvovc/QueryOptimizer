using QueryOptimizer.DatabaseExecutor.Abstractions;
using QueryOptimizer.Models.Application;
using QueryOptimizer.Shared.Common.Models.Metrics;
using System;
using System.Collections.Generic;
using System.Text;

namespace QueryOptimizer.Bll.Services.Abstractions
{
    public interface IQueryOptimizationWorkflowService
    {
        Task<QueryOptimizationResult> AnalyzeAsync(
            QueryOptimizationRequest request,
            CancellationToken cancellationToken = default);
    }
}
