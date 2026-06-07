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
                    Title = "Виявлено дороговартісну операцію Nested Loop Join",
                    Description = "З'єднання типу Nested Loop може бути неефективним, якщо зовнішній або внутрішній набір даних містить багато рядків.",
                    Recommendation = "Перевірте наявність індексів на колонках, які використовуються в JOIN.\nТакож варто перевірити актуальність статистики та умови фільтрації.",
                    AffectedObject = node.ObjectName,
                    AffectedNodeType = node.NormalizedNodeType,
                    Severity = FindingSeverity.Medium,
                    Confidence = 0.75
                };
            }
        }
    }
}
