using QueryOptimizer.Shared.Common.Models.ExecutionPlan;
using QueryOptimizer.Shared.Common.Models.Metrics;
using QueryOptimizer.Shared.Infrastructure.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace QueryOptimizer.Providers.SQLServer.Parsing
{
    public class SqlServerExecutionPlanParser : IExecutionPlanParser
    {
        public NormalizedExecutionPlan Parse(QueryPerformanceMetrics executionPlan)
        {
            throw new NotImplementedException();
        }
    }
}
