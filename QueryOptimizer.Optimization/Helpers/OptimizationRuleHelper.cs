using QueryOptimizer.Shared.Common.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace QueryOptimizer.Optimization.Helpers
{
    internal static class OptimizationRuleHelper
    {
        public static bool IsSelectQuery(string sql)
        {
            return Regex.IsMatch(
                sql.TrimStart(),
                @"^select\s",
                RegexOptions.IgnoreCase);
        }

        public static bool HasWhereClause(string sql)
        {
            return Regex.IsMatch(
                sql,
                @"\bwhere\b",
                RegexOptions.IgnoreCase);
        }

        public static string? TryExtractFirstPredicateColumn(string? predicate)
        {
            if (string.IsNullOrEmpty(predicate) || string.IsNullOrWhiteSpace(predicate))
                return null;

            var cleaned = predicate
                .Replace("[", string.Empty)
                .Replace("]", string.Empty)
                .Replace("\"", string.Empty);

            var match = Regex.Match(
                cleaned,
                @"(?<column>[a-zA-Z_][a-zA-Z0-9_\.]*)\s*(=|>|<|>=|<=|<>|!=|LIKE|IN)\s*",
                RegexOptions.IgnoreCase);

            if (!match.Success)
                return null;

            var column = match.Groups["column"].Value;

            if (string.IsNullOrWhiteSpace(column))
                return null;

            var parts = column.Split('.', StringSplitOptions.RemoveEmptyEntries);

            return parts.LastOrDefault();
        }

        public static string? BuildCreateIndexSql(
            DatabaseTypes provider,
            string? objectName,
            string? columnName
            )
        {
            if (string.IsNullOrWhiteSpace(objectName) || string.IsNullOrWhiteSpace(columnName))
                return null;

            var cleanObject = objectName
                    .Replace("[", string.Empty)
                    .Replace("]", string.Empty)
                    .Replace("\"", string.Empty)
                    .Trim();

            var cleanColumn = columnName
                .Replace("[", string.Empty)
                .Replace("]", string.Empty)
                .Replace("\"", string.Empty)
                .Trim();

            var indexName = $"IX_{cleanObject.Replace(".", "_")}_{cleanColumn}";

            return provider switch
            {
                DatabaseTypes.SqlServer =>
                    $"CREATE INDEX {indexName} ON {cleanObject}({cleanColumn});",

                DatabaseTypes.PostgreSql =>
                    $"CREATE INDEX {indexName.ToLowerInvariant()} ON {cleanObject.ToLowerInvariant()}({cleanColumn.ToLowerInvariant()});",

                DatabaseTypes.Oracle =>
                    $"CREATE INDEX {indexName.ToUpperInvariant()} ON {cleanObject.ToUpperInvariant()}({cleanColumn.ToUpperInvariant()});",

                _ => null
            };
        }

        public static double? GetEstimatedActualRatio(long? estimatedRows, long? actualRows)
        {
            if (!estimatedRows.HasValue || !actualRows.HasValue)
                return null;

            if (estimatedRows.Value <= 0 || actualRows.Value <= 0)
                return null;

            var bigger = Math.Max(estimatedRows.Value, actualRows.Value);
            var smaller = Math.Min(estimatedRows.Value, actualRows.Value);

            return (double)bigger / smaller;
        }

    }
}
