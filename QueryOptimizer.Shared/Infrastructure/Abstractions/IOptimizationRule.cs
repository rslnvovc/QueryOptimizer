using QueryOptimizer.Shared.Common.Models.ExecutionPlan;
using QueryOptimizer.Shared.Common.Models.Optimization;
using System;
using System.Collections.Generic;
using System.Text;

namespace QueryOptimizer.Shared.Infrastructure.Abstractions
{
    public interface IOptimizationRule
    {
        public string Rule { get; }

        IEnumerable<QueryOptimizationFinding> Analyze(
            NormalizedExecutionPlan plan,
            string originalSql
            );
    }
}
