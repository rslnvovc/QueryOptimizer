using QueryOptimizer.Shared.Common.Enums;
using QueryOptimizer.Shared.Common.Models.ExecutionPlan;
using QueryOptimizer.Shared.Common.Models.Metrics;
using System;
using System.Collections.Generic;
using System.Text;

namespace QueryOptimizer.Shared.Infrastructure.Abstractions
{
    public interface IExecutionPlanParser
    {
        DatabaseTypes Provider { get; }
        NormalizedExecutionPlan Parse(QueryPerformanceMetrics executionPlan);
    }
}
