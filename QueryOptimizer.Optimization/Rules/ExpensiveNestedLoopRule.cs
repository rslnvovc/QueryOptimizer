using QueryOptimizer.Shared.Common.Enums;
using QueryOptimizer.Shared.Common.Models.ExecutionPlan;
using QueryOptimizer.Shared.Common.Models.Optimization;
using QueryOptimizer.Shared.Infrastructure.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace QueryOptimizer.Optimization.Rules
{
    public class ExpensiveNestedLoopRule : IOptimizationRule
    {
        public string Rule => "EXPENSIVE_NESTED_LOOP";

        public IEnumerable<QueryOptimizationFinding> Analyze(NormalizedExecutionPlan plan, string originalSql)
        {
            foreach (var node in plan.Nodes)
            {
                if (node.NormalizedNodeType != "NestedLoopJoin")
                    continue;

                var isExpensive =
                    (node.ActualRows ?? 0) > 5000 ||
                    (node.LogicalReads ?? 0) > 5000 ||
                    (node.EstimatedCost ?? 0) > 50;

                if (!isExpensive)
                    continue;

                yield return new QueryOptimizationFinding()
                { 
                    RuleCode = Rule,
                    Title = "Expensive Nested Loop Join Detected",
                    Description = "Nested loop joins can be inefficient if the outer or inner dataset contains many rows.",
                    Recommendation = "Check for indexes on JOIN columns. It is also worth checking the relevance of statistics and the selectivity of filtering conditions.",
                    AffectedObject = node.ObjectName,
                    AffectedNodeType = node.NormalizedNodeType,
                    Severity = FindingSeverity.Medium
                };
            }
        }
    }
}
