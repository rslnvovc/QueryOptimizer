using QueryOptimizer.Models.Application;
using QueryOptimizer.Optimization.Services.Abstractions;
using QueryOptimizer.Shared.Common.Enums;
using QueryOptimizer.Shared.Common.Models.Optimization;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace QueryOptimizer.Optimization.Rules.Rewrite
{
    public class YearFunctionRewriteRule : ISqlRewriteRule
    {
        public string RuleCode => "YEAR_FUNCTION_IN_WHERE";

        public bool CanRewrite(QueryOptimizationFinding finding)
        {
            return finding.RuleCode == RuleCode ||
                   finding.RuleCode == "FUNCTION_IN_WHERE";
        }

        public SqlRewriteCandidate? TryRewrite(string originalSql, QueryOptimizationFinding finding, DatabaseTypes provider)
        {
            var pattern =
                @"YEAR\s*\(\s*(?<column>[a-zA-Z_][a-zA-Z0-9_\.\[\]""]*)\s*\)\s*=\s*(?<year>\d{4})";

            var match = Regex.Match(
                originalSql,
                pattern,
                RegexOptions.IgnoreCase);

            if (!match.Success)
                return null;

            var column = match.Groups["column"].Value;
            var year = int.Parse(match.Groups["year"].Value);

            var dateLiteralStart = BuildDateLiteral(provider, $"{year}-01-01");
            var dateLiteralEnd = BuildDateLiteral(provider, $"{year + 1}-01-01");

            var replacement =
                $"{column} >= {dateLiteralStart} AND {column} < {dateLiteralEnd}";

            var candidateSql = Regex.Replace(
                originalSql,
                pattern,
                replacement,
                RegexOptions.IgnoreCase);

            return new SqlRewriteCandidate
            {
                RuleCode = RuleCode,
                OriginalSql = originalSql,
                CandidateSql = candidateSql,
                Description = "Replaced YEAR(column) filter with a date range condition.",
                IsSafeToBenchmark = true
            };
        }

        private static string BuildDateLiteral(DatabaseTypes provider, string date)
        {
            return provider switch
            {
                DatabaseTypes.Oracle => $"DATE '{date}'",
                _ => $"'{date}'"
            };
        }
    }
}
