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
    public class FullTableScanRule : IOptimizationRule
    {
        public string Rule => "FULL_TABLE_SCAN";

        public IEnumerable<QueryOptimizationFinding> Analyze(NormalizedExecutionPlan plan, string originalSql)
        {
            foreach (var node in plan.Nodes)
            {
                if (node.NormalizedNodeType != "FullTableScan")
                    continue;

                var isExpensive =
                    (node.ActualRows ?? 0) > 1000 ||
                    (node.LogicalReads ?? 0) > 1000 ||
                    (node.PhysicalReads ?? 0) > 0 ||
                    (node.EstimatedCost ?? 0) > 50;

                if (!isExpensive)
                    continue;

                var column = OptimizationRuleHelper.TryExtractFirstPredicateColumn(node.Predicate);

                yield return new QueryOptimizationFinding
                {
                    RuleCode = Rule,
                    Title = "Full Table Scan Detected",
                    Description = $"The execution plan contains a Full Table Scan on object '{node.ObjectName}' which is likely causing performance issues.",
                    Recommendation = column != null
                        ? $"Consider adding an index on column '{column}' of table '{node.ObjectName}' to improve query performance."
                        : $"Consider adding appropriate indexes to the table '{node.ObjectName}' to avoid full table scans.",
                    SuggestedIndexSql = column != null
                        ? OptimizationRuleHelper.BuildCreateIndexSql(plan.Provider, node.ObjectName, column)
                        : string.Empty,
                    Severity = FindingSeverity.High,
                    Confidence = column == null ? 0.75 : 0.85
                };
            }
        }
    }
}
