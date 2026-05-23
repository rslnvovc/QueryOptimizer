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
                Title = $"Function '{detectedFunction}' used in WHERE clause",
                Description = $"The function '{detectedFunction}' is used in the WHERE clause, which can prevent the use of indexes and lead to performance issues.",
                Recommendation = $"Consider rewriting the query to avoid using the '{detectedFunction}' function in the WHERE clause. For example, if you're using '{detectedFunction}' on a column, try to compute the function's value in a derived column or use a different approach to filter the data.",
                Severity = FindingSeverity.High
            };
        }
    }
}
