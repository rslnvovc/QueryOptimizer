using QueryOptimizer.Shared.Common.Enums;
using QueryOptimizer.Shared.Common.Models.ExecutionPlan;
using QueryOptimizer.Shared.Common.Models.Optimization;
using QueryOptimizer.Shared.Infrastructure.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace QueryOptimizer.Optimization.Rules
{
    public class HighLogicalReadsRule : IOptimizationRule
    {
        public string Rule => "HIGH_LOGICAL_READS";

        public IEnumerable<QueryOptimizationFinding> Analyze(NormalizedExecutionPlan plan, string originalSql)
        {
            foreach (var node in plan.Nodes)
            {
                if (!node.LogicalReads.HasValue)
                    continue;

                if (node.LogicalReads.Value < 5000)
                    continue;

                yield return new QueryOptimizationFinding()
                { 
                    RuleCode = Rule,
                    Title = "Висока кількість логічних читань / Buffer gets",
                    Description = $"Запит має високу кількість логічних читань ({node.LogicalReads.Value}), що може вказувати на неефективну структуру запиту або відсутність потрібних індексів.",
                    Recommendation = "Перегляньте план виконання, щоб визначити джерело великої кількості логічних читань.\nРозгляньте оптимізацію запиту, додавання відповідних індексів або оновлення статистики.",
                    AffectedObject = node.ObjectName,
                    AffectedNodeType = node.NodeType,
                    Severity = FindingSeverity.High,
                    Confidence = 0.85
                };
            }
        }
    }
}
