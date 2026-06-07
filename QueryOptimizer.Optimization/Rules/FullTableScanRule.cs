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
                    Title = "Виявлено повне сканування таблиці",
                    Description = $"План виконання містить повне сканування об'єкта '{node.ObjectName}', що може спричиняти проблеми продуктивності.",
                    Recommendation = column != null
                        ? $"Розгляньте можливість додавання індексу для колонки '{column}' таблиці '{node.ObjectName}', щоб покращити продуктивність запиту."
                        : $"Розгляньте можливість додавання відповідних індексів до таблиці '{node.ObjectName}', щоб уникнути повного сканування.",
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
