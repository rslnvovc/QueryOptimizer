using QueryOptimizer.Shared.Common.Models.Queries;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace QueryOptimizer.Shared.Common.Helpers
{
    public static class SqlJoinHelper
    {
        private static readonly Regex JoinRegex = new(
            @"(?is)\b(?<joinType>INNER|LEFT\s+OUTER|LEFT|RIGHT\s+OUTER|RIGHT|FULL\s+OUTER|FULL|CROSS)?\s*JOIN\s+" +
            @"(?<table>(?:\[[^\]]+\]|""[^""]+""|[A-Za-z_][\w$]*)(?:\s*\.\s*(?:\[[^\]]+\]|""[^""]+""|[A-Za-z_][\w$]*))*)\s+" +
            @"(?:AS\s+)?" +
            @"(?<alias>\[[^\]]+\]|""[^""]+""|[A-Za-z_][\w$]*)\s+ON\b",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

        private static readonly Regex ClauseEndRegex = new(
        @"(?is)\b(WHERE|GROUP\s+BY|HAVING|ORDER\s+BY|LIMIT|OFFSET|FETCH|UNION)\b",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

        public static IReadOnlyList<SqlJoinInfo> ExtractJoins(string sql)
        {
            var matches = JoinRegex.Matches(sql);
            var result = new List<SqlJoinInfo>();

            for (var i = 0; i < matches.Count; i++)
            {
                var match = matches[i];

                var nextJoinIndex = i + 1 < matches.Count
                    ? matches[i + 1].Index
                    : int.MaxValue;

                var clauseEndMatch = ClauseEndRegex.Match(sql, match.Index + match.Length);

                var clauseEndIndex = clauseEndMatch.Success
                    ? clauseEndMatch.Index
                    : int.MaxValue;

                var endIndex = Math.Min(nextJoinIndex, clauseEndIndex);

                if (endIndex == int.MaxValue)
                    endIndex = sql.Length;

                var joinType = match.Groups["joinType"].Value;

                if (string.IsNullOrWhiteSpace(joinType))
                    joinType = "INNER";

                result.Add(new SqlJoinInfo()
                {
                    JoinType = NormalizeWhitespace(joinType),
                    TableName = match.Groups["table"].Value.Trim(),
                    TableAlias = NormalizeIdentifier(match.Groups["alias"].Value),
                    StartIndex = match.Index,
                    EndIndex = endIndex
                });
            }

            return result;
        }

        public static string RemoveJoin(string sql, SqlJoinInfo join)
        {
            var length = join.EndIndex - join.StartIndex;

            if (join.StartIndex < 0 || length <= 0 || join.StartIndex + length > sql.Length)
                return sql;

            var result = sql.Remove(join.StartIndex, length);

            result = Regex.Replace(result, @"[ \t]{2,}", " ");
            result = Regex.Replace(result, @"\n\s*\n\s*\n", "\n\n");

            return result.Trim();
        }

        public static bool ContainsAliasReference(string sql, string alias)
        {
            var cleanAlias = NormalizeIdentifier(alias);

            var patterns = new[]
            {
            $@"\b{Regex.Escape(cleanAlias)}\s*\.",
            $@"\[{Regex.Escape(cleanAlias)}\]\s*\.",
            $@"""{Regex.Escape(cleanAlias)}""\s*\."
        };

            return patterns.Any(pattern =>
                Regex.IsMatch(sql, pattern, RegexOptions.IgnoreCase));
        }

        private static string NormalizeIdentifier(string identifier)
        {
            return identifier
                .Trim()
                .Trim('[', ']')
                .Trim('"');
        }

        private static string NormalizeWhitespace(string value)
        {
            return Regex.Replace(value.Trim(), @"\s+", " ");
        }
    }
}
