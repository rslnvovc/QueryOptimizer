using QueryOptimizer.Shared.Common.Enums;
using QueryOptimizer.Shared.Common.Models.ExecutionPlan;
using QueryOptimizer.Shared.Common.Models.Metrics;
using QueryOptimizer.Shared.Infrastructure.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace QueryOptimizer.Providers.Oracle.Parsing
{
    public class OracleExecutionPlanParser : IExecutionPlanParser
    {
        public NormalizedExecutionPlan Parse(QueryPerformanceMetrics executionPlan)
        {
            var result = new NormalizedExecutionPlan()
            {
                Provider = DatabaseTypes.Oracle,
                RawPlan = executionPlan.ExecutionPlan ?? string.Empty,
                TotalExecutionTimeMs = executionPlan.DatabaseElapsedMs,
                TotalLogicalReads = executionPlan.LogicalReads,
                TotalPhysicalReads = executionPlan.PhysicalReads,
            };

            if (string.IsNullOrEmpty(result.RawPlan))
                return result;

            return result;
        }
    }
}
