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
    public class ImplicitJoinRule : IOptimizationRule
    {
        public string Rule => "IMPLICIT_JOIN";

        public IEnumerable<QueryOptimizationFinding> Analyze(NormalizedExecutionPlan plan, string originalSql)
        {
            var hasCommaJoin = Regex.IsMatch(
                originalSql,
                @"(?is)\bfrom\b[\s\S]+,[\s\S]+\bwhere\b");

            var hasJoinPredicate = Regex.IsMatch(
                originalSql,
                @"\b[a-zA-Z_][a-zA-Z0-9_]*\.[a-zA-Z0-9_\[\]""]+\s*=\s*[a-zA-Z_][a-zA-Z0-9_]*\.[a-zA-Z0-9_\[\]""]+\b");

            if (!hasCommaJoin || !hasJoinPredicate)
                yield break;

            yield return new QueryOptimizationFinding()
            {
                RuleCode = Rule,
                Title = "Implicit Join Detected",
                Description = "Query use old style of JOIN through comma in FROM and conditions in WHERE",
                Recommendation = "Rewrite query to use explicit JOIN syntax for better readability and maintainability.",
                Severity = FindingSeverity.Low,
                Confidence = 0.9,
                AdaptiveConfidence = 0.9
            };
        }
    }
}
