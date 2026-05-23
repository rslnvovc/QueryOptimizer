using QueryOptimizer.Optimization.Helpers;
using QueryOptimizer.Shared.Common.Enums;
using QueryOptimizer.Shared.Common.Models.ExecutionPlan;
using QueryOptimizer.Shared.Common.Models.Optimization;
using QueryOptimizer.Shared.Infrastructure.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace QueryOptimizer.Optimization.Rules
{
    public class BadCardinalityEstimatedRule : IOptimizationRule
    {
        public string Rule => "BAD_CARDINALITY_ESTIMATED";

        public IEnumerable<QueryOptimizationFinding> Analyze(NormalizedExecutionPlan plan, string originalSql)
        {
            foreach (var node in plan.Nodes)
            {
                var ratio = OptimizationRuleHelper.GetEstimatedActualRatio(
                    node.EstimatedRows,
                    node.ActualRows
                    );

                if (!ratio.HasValue || ratio.Value < 10)
                    continue;

                yield return new QueryOptimizationFinding()
                { 
                    RuleCode = Rule,
                    Title = "A huge difference between Estimated Rows and Actual Rows",
                    Description = $"Optimizer expected {node.EstimatedRows} rows, but actual rows were {node.ActualRows}. Difference in {ratio.Value:F1} times",
                    Recommendation = "Consider updating statistics on the involved tables, or check if there are any parameters sniffing issues.",
                    AffectedObject = node.ObjectName,
                    AffectedNodeType = node.NodeType,
                    Severity = ratio.Value >= 100 ? FindingSeverity.High : FindingSeverity.Medium,
                };
            }
        }
    }
}
