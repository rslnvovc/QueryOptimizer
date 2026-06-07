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
                    Title = "Виявлено значну різницю між оціненою та фактичною кількістю рядків",
                    Description = $"Оптимізатор очікував {node.EstimatedRows} рядків, але фактично було оброблено {node.ActualRows} рядків.\nРізниця становить {ratio.Value:F1} разів",
                    Recommendation = "Рекомендується оновити статистику для відповідних таблиць або перевірити можливі проблеми parameter sniffing.",
                    AffectedObject = node.ObjectName,
                    AffectedNodeType = node.NodeType,
                    Severity = ratio.Value >= 100 ? FindingSeverity.High : FindingSeverity.Medium,
                    Confidence = 0.85
                };
            }
        }
    }
}
