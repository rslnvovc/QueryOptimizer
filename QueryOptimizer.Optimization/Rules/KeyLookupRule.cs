using QueryOptimizer.Shared.Common.Enums;
using QueryOptimizer.Shared.Common.Models.ExecutionPlan;
using QueryOptimizer.Shared.Common.Models.Optimization;
using QueryOptimizer.Shared.Infrastructure.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace QueryOptimizer.Optimization.Rules
{
    public class KeyLookupRule : IOptimizationRule
    {
        public string Rule => "KEY_LOOKUP";

        public IEnumerable<QueryOptimizationFinding> Analyze(NormalizedExecutionPlan plan, string originalSql)
        {
            if (plan.Provider != DatabaseTypes.SqlServer)
                yield break;

            foreach (var node in plan.Nodes)
            {
                if (node.NormalizedNodeType != "KeyLookup")
                    continue;

                yield return new QueryOptimizationFinding()
                { 
                    RuleCode = Rule,
                    Title = "Виявлено операцію Key Lookup",
                    Description = $"У плані виконання виявлено оператор Key Lookup для об'єкта '{node.ObjectName}' з використанням індексу '{node.IndexName}'.\nЦей оператор може спричиняти проблеми продуктивності, якщо повертається велика кількість рядків.",
                    Recommendation = "Розгляньте створення covering index, який включає колонки, необхідні запиту, щоб уникнути операції Key Lookup.",
                    AffectedObject = node.ObjectName,
                    AffectedNodeType = node.NodeType,
                    Severity = FindingSeverity.High,
                    Confidence = 0.9
                };
            }
        }
    }
}
