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
                    Title = "Виявлено дороговартісну операцію сортування",
                    Description = $"План виконання містить дорогу операцію Sort для великої кількості записів.",
                    Recommendation = $"Якщо запит часто використовує ORDER BY, варто розглянути створення індексу для відповідних колонок або зменшити кількість записів, які потрібно сортувати.",
                    SuggestedIndexSql = string.Empty,
                    AffectedObject = node.ObjectName,
                    AffectedNodeType = node.NodeType,
                    Confidence = 0.8
                };
            }
        }
    }
}
