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
    public class FunctionInWhereRule : IOptimizationRule
    {
        public string Rule => "FUNCTION_IN_WHERE";

        private static readonly string[] SuspiciousFunctions =
        {
            "YEAR",
            "MONTH",
            "DAY",
            "LOWER",
            "UPPER",
            "LTRIM",
            "RTRIM",
            "TRIM",
            "CAST",
            "CONVERT",
            "TO_CHAR",
            "TO_DATE",
            "DATE_TRUNC"
        };

        public IEnumerable<QueryOptimizationFinding> Analyze(NormalizedExecutionPlan plan, string originalSql)
        {
            var whereMatch = Regex.Match(
                originalSql,
                @"\bwhere\b(?<where>[\s\S]*)",
                RegexOptions.IgnoreCase
                );

            if (!whereMatch.Success)
                yield break;

            var wherePart = whereMatch.Groups["where"].Value;

            var detectedFunction = SuspiciousFunctions.FirstOrDefault(function =>
                Regex.IsMatch(
                    wherePart,
                    $@"\b{Regex.Escape(function)}\s*\(",
                    RegexOptions.IgnoreCase));

            if (detectedFunction is null)
                yield break;

            yield return new QueryOptimizationFinding()
            { 
                RuleCode = Rule,
                Title = $"Функція '{detectedFunction}' використовується в умові WHERE",
                Description = $"Функція '{detectedFunction}' використовується в секції WHERE, що може заважати використанню індексів і призводити до проблем продуктивності.",
                Recommendation = $"Розгляньте можливість переписати запит так, щоб не використовувати функцію '{detectedFunction}' у секції WHERE.\nНаприклад, якщо функція застосовується до колонки, спробуйте використовувати обчислювану колонку або інший підхід до фільтрації даних.",
                Severity = FindingSeverity.High,
                Confidence = 0.85
            };
        }
    }
}
