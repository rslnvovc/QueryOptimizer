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
    public class ImplicitJoinRewriteRule : ISqlRewriteRule
    {
        public string RuleCode => "IMPLICIT_JOIN";

        public bool CanRewrite(QueryOptimizationFinding finding)
        {
            return finding.RuleCode == RuleCode;
        }

        public SqlRewriteCandidate? TryRewrite(string originalSql, QueryOptimizationFinding finding, DatabaseTypes provider)
        {
            if (!originalSql.Contains(","))
                return null;

            var parsed = TryParseSelectFromWhere(originalSql);

            if (parsed == null)
                return null;

            var tables = SplitByComma(parsed.FromPart)
                .Select(ParseTableReference)
                .Where(x => x != null)
                .Select(x => x!)
                .ToList();

            if (tables.Count < 2)
                return null;

            var conditions = SplitByAnd(parsed.WherePart);

            var joinConditions = conditions
                .Where(IsJoinCondition)
                .ToList();

            if (joinConditions.Count == 0)
                return null;

            var remainingConditions = conditions
                .Where(x => !joinConditions.Contains(x))
                .ToList();

            var usedAliases = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                tables[0].Alias
            };

            var notJoinedTables = tables
                .Skip(1)
                .ToList();

            var fromBuilder = new List<string>
            {
                tables[0].OriginalText.Trim()
            };

            while (notJoinedTables.Count > 0)
            {
                var joinedSomething = false;

                foreach (var table in notJoinedTables.ToList())
                {
                    var condition = joinConditions.FirstOrDefault(x =>
                        ConditionConnectsWithUsedAlias(x, table.Alias, usedAliases));

                    if (condition == null)
                        continue;

                    fromBuilder.Add($"INNER JOIN {table.OriginalText.Trim()} ON {condition.Trim()}");

                    usedAliases.Add(table.Alias);
                    notJoinedTables.Remove(table);
                    joinConditions.Remove(condition);

                    joinedSomething = true;
                }

                if (!joinedSomething)
                    return null;
            }

            var candidateSql = parsed.SelectPart.TrimEnd() +
                               Environment.NewLine +
                               "FROM " +
                               string.Join(Environment.NewLine, fromBuilder);

            if (remainingConditions.Count > 0)
            {
                candidateSql += Environment.NewLine +
                                "WHERE " +
                                string.Join(" AND ", remainingConditions);
            }

            if (!string.IsNullOrWhiteSpace(parsed.TailPart))
            {
                candidateSql += Environment.NewLine + parsed.TailPart.Trim();
            }

            return new SqlRewriteCandidate
            {
                RuleCode = RuleCode,
                OriginalSql = originalSql,
                CandidateSql = candidateSql,
                Description = "Converted old comma-separated joins to explicit INNER JOIN syntax.",
                IsSafeToBenchmark = true
            };
        }

        private static ParsedSql? TryParseSelectFromWhere(string sql)
        {
            var pattern = @"(?is)^(?<select>select[\s\S]+?)\bfrom\b\s+(?<from>[\s\S]+?)\bwhere\b\s+(?<where>[\s\S]*?)(?<tail>\s+(group\s+by|order\s+by|having|limit|offset|fetch)\b[\s\S]*)?$";

            var match = Regex.Match(sql, pattern);

            if (!match.Success)
                return null;

            return new ParsedSql
            {
                SelectPart = match.Groups["select"].Value.Trim() + Environment.NewLine,
                FromPart = match.Groups["from"].Value.Trim(),
                WherePart = match.Groups["where"].Value.Trim(),
                TailPart = match.Groups["tail"].Value.Trim()
            };
        }

        private static List<string> SplitByComma(string value)
        {
            return value
                .Split(',')
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();
        }

        private static List<string> SplitByAnd(string value)
        {
            return Regex.Split(
                    value,
                    @"\s+AND\s+",
                    RegexOptions.IgnoreCase)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();
        }

        private static TableReference? ParseTableReference(string tableText)
        {
            var cleaned = tableText.Trim();

            var parts = cleaned
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .ToList();

            if (parts.Count == 0)
                return null;

            if (parts.Count == 1)
            {
                var tableName = parts[0];

                return new TableReference
                {
                    OriginalText = cleaned,
                    Alias = CleanIdentifier(tableName)
                };
            }

            var alias = parts.Last();

            if (string.Equals(alias, "AS", StringComparison.OrdinalIgnoreCase))
                return null;

            if (parts.Count >= 3 &&
                string.Equals(parts[^2], "AS", StringComparison.OrdinalIgnoreCase))
            {
                alias = parts[^1];
            }

            return new TableReference
            {
                OriginalText = cleaned,
                Alias = CleanIdentifier(alias)
            };
        }

        private static bool IsJoinCondition(string condition)
        {
            return Regex.IsMatch(
                condition,
                @"\b(?<a1>[a-zA-Z_][a-zA-Z0-9_]*)\.[a-zA-Z0-9_\[\]""]+\s*=\s*(?<a2>[a-zA-Z_][a-zA-Z0-9_]*)\.[a-zA-Z0-9_\[\]""]+\b",
                RegexOptions.IgnoreCase);
        }

        private static bool ConditionConnectsWithUsedAlias(
            string condition,
            string tableAlias,
            HashSet<string> usedAliases)
        {
            var aliases = ExtractAliasesFromCondition(condition);

            if (!aliases.Contains(tableAlias, StringComparer.OrdinalIgnoreCase))
                return false;

            return aliases.Any(x => usedAliases.Contains(x));
        }

        private static List<string> ExtractAliasesFromCondition(string condition)
        {
            return Regex.Matches(
                    condition,
                    @"\b(?<alias>[a-zA-Z_][a-zA-Z0-9_]*)\.",
                    RegexOptions.IgnoreCase)
                .Select(x => x.Groups["alias"].Value)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static string CleanIdentifier(string value)
        {
            return value
                .Replace("[", string.Empty)
                .Replace("]", string.Empty)
                .Replace("\"", string.Empty)
                .Trim();
        }

        private sealed class ParsedSql
        {
            public string SelectPart { get; set; } = default!;

            public string FromPart { get; set; } = default!;

            public string WherePart { get; set; } = default!;

            public string TailPart { get; set; } = default!; 
        }

        private sealed class TableReference
        {
            public string OriginalText { get; set; } = default!;

            public string Alias { get; set; } = default!;
        }
    }
}
