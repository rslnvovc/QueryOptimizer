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
    public class MissingWhereClauseRule : IOptimizationRule
    {
        public string Rule => "MISSING_WHERE";

        public IEnumerable<QueryOptimizationFinding> Analyze(NormalizedExecutionPlan plan, string originalSql)
        {
            if (!OptimizationRuleHelper.IsSelectQuery(originalSql))
                yield break;

            if (OptimizationRuleHelper.HasWhereClause(originalSql))
                yield break;

            var hasLargeScan = plan.Nodes.Any(x =>
                x.NormalizedNodeType is "FullTableScan" or "IndexScan" &&
                ((x.ActualRows ?? 0) > 1000 || (x.LogicalReads ?? 0) > 1000)
            );

            if (!hasLargeScan)
                yield break;

            yield return new QueryOptimizationFinding()
            {
                RuleCode = Rule,
                Title = "Відсутня умова WHERE у SELECT-запиті",
                Description = "Запит виконує повне сканування без умови WHERE, що може призводити до проблем продуктивності.",
                Recommendation = "Розгляньте можливість додати умову WHERE для фільтрації результатів і зменшення кількості рядків, які потрібно сканувати.",
                Severity = FindingSeverity.Medium,
                Confidence = 0.75
            };
        }
    }
}
