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
                Title = "Виявлено неявне з'єднання таблиць",
                Description = "У запиті використано старий стиль JOIN: таблиці перелічені через кому у FROM, а умови з'єднання винесені у WHERE.",
                Recommendation = "Перепишіть запит з використанням явного синтаксису JOIN для кращої читабельності та підтримуваності.",
                Severity = FindingSeverity.Low,
                Confidence = 0.9,
                AdaptiveConfidence = 0.9
            };
        }
    }
}
