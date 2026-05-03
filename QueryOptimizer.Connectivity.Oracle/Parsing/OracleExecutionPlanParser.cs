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
            throw new NotImplementedException();
        }
    }
}
