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
                    Title = "Key Lookup Detected",
                    Description = $"A Key Lookup operator was detected on object '{node.ObjectName}' using index '{node.IndexName}'. This operator can lead to performance issues if it retrieves a large number of rows.",
                    Recommendation = "Consider creating a covering index that includes the columns needed by the query to avoid the Key Lookup operation.",
                    AffectedObject = node.ObjectName,
                    AffectedNodeType = node.NodeType,
                    Severity = FindingSeverity.High,
                    Confidence = 0.9
                };
            }
        }
    }
}
