using QueryOptimizer.Shared.Common.Enums;
using QueryOptimizer.Shared.Common.Models.ExecutionPlan;
using QueryOptimizer.Shared.Common.Models.Optimization;
using QueryOptimizer.Shared.Infrastructure.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace QueryOptimizer.Optimization.Rules
{
    public class SelectStarRule : IOptimizationRule
    {
        public string Rule => "SELECT_STAR";

        public IEnumerable<QueryOptimizationFinding> Analyze(NormalizedExecutionPlan plan, string originalSql)
        {
            if (!Regex.IsMatch(originalSql, @"\bselect\s+\*", RegexOptions.IgnoreCase))
                yield break;

            yield return new QueryOptimizationFinding()
            { 
                RuleCode = Rule,
                Title = "Usage of SELECT *",
                Description = "Using SELECT * can lead to inefficient queries as it retrieves all columns, including those that may not be needed. This can increase I/O and memory usage.",
                Recommendation = "Specify only the columns that are necessary for your query instead of using SELECT *.",
                Severity = FindingSeverity.Medium,
                Confidence = 0.95
            };
        }
    }
}
