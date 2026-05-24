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
                    Title = "High Logical Reads / Buffer gets",
                    Description = $"This query has a high number of logical reads ({node.LogicalReads.Value}) which can indicate inefficient query design or missing indexes.",
                    Recommendation = "Review the execution plan to identify the source of high logical reads. Consider optimizing the query, adding appropriate indexes, or updating statistics.",
                    AffectedObject = node.ObjectName,
                    AffectedNodeType = node.NodeType,
                    Severity = FindingSeverity.High,
                    Confidence = 0.85
                };
            }
        }
    }
}
