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
    public class LeadingWildcardLikeRule : IOptimizationRule
    {
        public string Rule => "LEADING_WILDCARD_LIKE";

        public IEnumerable<QueryOptimizationFinding> Analyze(NormalizedExecutionPlan plan, string originalSql)
        {
            var hasLeadingWildcardLike = Regex.IsMatch(
                originalSql,
                @"\blike\s+N?'%[^']*'",
                RegexOptions.IgnoreCase);

            if (!hasLeadingWildcardLike)
                yield break;

            yield return new QueryOptimizationFinding()
            {
                RuleCode = Rule,
                Title = "Виявлено початковий wildcard у LIKE",
                Description = "Умова LIKE із символом % на початку зазвичай не дозволяє ефективно використовувати індекси.",
                Recommendation = "Розгляньте можливість прибрати початковий символ % з умови LIKE або використати повнотекстовий пошук, якщо це доречно.",
                Severity = FindingSeverity.Medium,
                Confidence = 0.9
            };
        }
    }
}
