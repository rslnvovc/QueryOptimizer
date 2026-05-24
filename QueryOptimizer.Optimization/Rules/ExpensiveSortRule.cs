using QueryOptimizer.Shared.Common.Enums;
using QueryOptimizer.Shared.Common.Models.ExecutionPlan;
using QueryOptimizer.Shared.Common.Models.Optimization;
using QueryOptimizer.Shared.Infrastructure.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace QueryOptimizer.Optimization.Rules
{
    public class ExpensiveSortRule : IOptimizationRule
    {
        public string Rule => "EXPENSIVE_SORT";

        public IEnumerable<QueryOptimizationFinding> Analyze(NormalizedExecutionPlan plan, string originalSql)
        {
            foreach (var node in plan.Nodes)
            {
                if (node.NormalizedNodeType != "Sort")
                    continue;

                var isExpensive =
                    (node.ActualRows ?? 0) > 1000 ||
                    (node.EstimatedRows ?? 0) > 1000 ||
                    (node.EstimatedCost ?? 0) > 20 ||
                    (node.LogicalReads ?? 0) > 1000;

                if (!isExpensive)
                    continue;

                yield return new QueryOptimizationFinding()
                { 
                    RuleCode = Rule,
                    Title = "Expensive Sort Detected",
                    Description = $"The execution plan contains an expensive Sort operation for big amount of records.",
                    Recommendation = $"If query often use ORDER BY clause, you need to consider index for some columns or decrease quantity of records for sorting.",
                    SuggestedIndexSql = string.Empty,
                    AffectedObject = node.ObjectName,
                    AffectedNodeType = node.NodeType,
                    Confidence = 0.8
                };
            }
        }
    }
}
