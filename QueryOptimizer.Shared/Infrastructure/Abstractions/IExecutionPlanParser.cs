using QueryOptimizer.Shared.Common.Models.ExecutionPlan;
using QueryOptimizer.Shared.Common.Models.Metrics;
using System;
using System.Collections.Generic;
using System.Text;

namespace QueryOptimizer.Shared.Infrastructure.Abstractions
{
    public interface IExecutionPlanParser
    {
        NormalizedExecutionPlan Parse(QueryPerformanceMetrics executionPlan);
    }
}
