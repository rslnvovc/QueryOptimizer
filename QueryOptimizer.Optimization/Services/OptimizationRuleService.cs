using QueryOptimizer.Shared.Common.Models.ExecutionPlan;
using QueryOptimizer.Shared.Common.Models.Optimization;
using QueryOptimizer.Shared.Infrastructure.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace QueryOptimizer.Optimization.Services
{
    public class OptimizationRuleService
    {
        private readonly IReadOnlyCollection<IOptimizationRule> _optimizationRules;

        public OptimizationRuleService(IEnumerable<IOptimizationRule> optimizationRules)
        {
            _optimizationRules = optimizationRules.ToList();
        }

        public List<QueryOptimizationFinding> Analyze(
            NormalizedExecutionPlan plan,
            string sql
            )
        {
            return _optimizationRules
                .SelectMany(rule => rule.Analyze(plan, sql))
                .OrderByDescending(x => x.Severity)
                .ToList();
        }
    }
}
