using QueryOptimizer.Providers.Oracle.Models;
using QueryOptimizer.Shared.Common.Enums;
using QueryOptimizer.Shared.Common.Models.ExecutionPlan;
using QueryOptimizer.Shared.Common.Models.Metrics;
using QueryOptimizer.Shared.Infrastructure.Abstractions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace QueryOptimizer.Providers.Oracle.Parsing
{
    public class OracleExecutionPlanParser : IExecutionPlanParser
    {
        public DatabaseTypes Provider => DatabaseTypes.Oracle;

        public NormalizedExecutionPlan Parse(QueryPerformanceMetrics executionPlan)
        {
            var result = new NormalizedExecutionPlan()
            {
                Provider = DatabaseTypes.Oracle,
                RawPlan = executionPlan.ExecutionPlan ?? string.Empty,
                TotalExecutionTimeMs = executionPlan.DatabaseElapsedMs,
                TotalLogicalReads = executionPlan.LogicalReads,
                TotalPhysicalReads = executionPlan.PhysicalReads,
            };

            if (string.IsNullOrEmpty(result.RawPlan))
                return result;

            try
            {
                var predicates = ParsePredicates(result.RawPlan);
                var parsedRows = ParsePlanRows(result.RawPlan, predicates);

                BuildHierarchy(parsedRows);

                result.Nodes = parsedRows
                    .Select(x => x.Node)
                    .ToList();

                result.TotalCost = result.Nodes
                    .Where(x => x.EstimatedCost.HasValue)
                    .Select(x => x.EstimatedCost!.Value)
                    .DefaultIfEmpty()
                    .Max();

                result.TotalLogicalReads = result.Nodes
                    .Where(x => x.LogicalReads.HasValue)
                    .Sum(x => x.LogicalReads!.Value);

                result.TotalPhysicalReads = result.Nodes
                    .Where(x => x.PhysicalReads.HasValue)
                    .Sum(x => x.PhysicalReads!.Value);
            }
            catch (Exception ex)
            { 
            
            }

            return result;
        }

        private static List<ParsedOraclePlanRow> ParsePlanRows(
            string rawPlan,
            Dictionary<int, string> predicates)
        {
            var result = new List<ParsedOraclePlanRow>();

            var lines = rawPlan
                .Split(new[] { "\r\n", "\n" }, StringSplitOptions.None)
                .ToList();

            List<string>? headers = null;

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                if (line.Contains("Predicate Information", StringComparison.OrdinalIgnoreCase))
                    break;

                if (IsHeaderLine(line))
                {
                    headers = SplitOracleTableLine(line)
                        .Select(NormalizeHeaderName)
                        .ToList();

                    continue;
                }

                if (headers == null)
                    continue;

                if (!line.TrimStart().StartsWith("|"))
                    continue;

                if (IsSeparatorLine(line))
                    continue;

                var values = SplitOracleTableLine(line);

                if (values.Count < 2)
                    continue;

                var row = MapHeaderValues(headers, values);

                var idRaw = GetValue(row, "id");

                if (!TryParseOperationId(idRaw, out var operationId))
                    continue;

                var rawOperation = GetValue(row, "operation", trim: false);
                var operation = CleanOperation(rawOperation);

                if (string.IsNullOrWhiteSpace(operation))
                    continue;

                var name = GetValue(row, "name");

                var estimatedRows =
                    ParseOracleNumber(GetValue(row, "erows")) ??
                    ParseOracleNumber(GetValue(row, "rows"));

                var actualRows = ParseOracleNumber(GetValue(row, "arows"));

                var cost = ParseOracleNumber(GetValue(row, "costcpu")) ??
                           ParseOracleNumber(GetValue(row, "cost"));

                var buffers = ParseOracleNumber(GetValue(row, "buffers"));
                var reads = ParseOracleNumber(GetValue(row, "reads"));

                var actualTimeMs = ParseOracleTimeToMilliseconds(GetValue(row, "atime"));

                var node = new ExecutionPlanNode
                {
                    NodeType = operation,
                    NormalizedNodeType = NormalizeNodeType(operation),
                    ObjectName = GetObjectName(operation, name),
                    IndexName = GetIndexName(operation, name),
                    Predicate = predicates.TryGetValue(operationId, out var predicate) ? predicate : null,
                    JoinType = GetJoinType(operation),

                    EstimatedCost = cost,
                    EstimatedRows = estimatedRows,
                    ActualRows = actualRows,
                    ActualTimeMs = actualTimeMs,

                    LogicalReads = buffers,
                    PhysicalReads = reads
                };

                result.Add(new ParsedOraclePlanRow
                {
                    Id = operationId,
                    Depth = GetOperationDepth(rawOperation),
                    Node = node
                });
            }

            return result;
        }

        private static void BuildHierarchy(List<ParsedOraclePlanRow> rows)
        {
            var stack = new Stack<ParsedOraclePlanRow>();

            foreach (var row in rows)
            {
                while (stack.Count > 0 && stack.Peek().Depth >= row.Depth)
                {
                    stack.Pop();
                }

                if (stack.Count > 0)
                {
                    stack.Peek().Node.ChildrenNode.Add(row.Node);
                }

                stack.Push(row);
            }
        }

        private static Dictionary<int, string> ParsePredicates(string rawPlan)
        {
            var result = new Dictionary<int, string>();

            var lines = rawPlan.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            var inPredicateSection = false;
            int? currentId = null;

            foreach (var line in lines)
            {
                if (line.Contains("Predicate Information", StringComparison.OrdinalIgnoreCase))
                {
                    inPredicateSection = true;
                    continue;
                }

                if (!inPredicateSection)
                    continue;

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                if (IsSeparatorLine(line))
                    continue;

                var match = Regex.Match(line, @"^\s*(?<id>\d+)\s*-\s*(?<text>.+)$");

                if (match.Success)
                {
                    currentId = int.Parse(match.Groups["id"].Value, CultureInfo.InvariantCulture);
                    result[currentId.Value] = match.Groups["text"].Value.Trim();
                    continue;
                }

                if (currentId.HasValue && !line.TrimStart().StartsWith("|"))
                {
                    var continuation = line.Trim();

                    if (!string.IsNullOrWhiteSpace(continuation))
                    {
                        result[currentId.Value] += " " + continuation;
                    }
                }
            }

            return result;
        }

        private static bool IsHeaderLine(string line)
        {
            return line.TrimStart().StartsWith("|") &&
                   line.Contains("Operation", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsSeparatorLine(string line)
        {
            var trimmed = line.Trim();

            if (string.IsNullOrWhiteSpace(trimmed))
                return true;

            return trimmed.All(x => x == '-' || x == '+' || x == '|');
        }

        private static List<string> SplitOracleTableLine(string line)
        {
            return line
                .Trim()
                .Trim('|')
                .Split('|')
                .Select(x => x.TrimEnd())
                .ToList();
        }

        private static Dictionary<string, string> MapHeaderValues(
            List<string> headers,
            List<string> values)
        {
            var result = new Dictionary<string, string>();

            var count = Math.Min(headers.Count, values.Count);

            for (var i = 0; i < count; i++)
            {
                var header = headers[i];

                if (string.IsNullOrWhiteSpace(header))
                    continue;

                if (!result.ContainsKey(header))
                    result[header] = values[i];
            }

            return result;
        }

        private static string NormalizeHeaderName(string header)
        {
            var normalized = header
                .Trim()
                .ToLowerInvariant()
                .Replace(" ", string.Empty)
                .Replace("-", string.Empty)
                .Replace("_", string.Empty)
                .Replace("(", string.Empty)
                .Replace(")", string.Empty)
                .Replace("%", string.Empty);

            return normalized switch
            {
                "id" => "id",
                "operation" => "operation",
                "name" => "name",
                "rows" => "rows",
                "erows" => "erows",
                "arows" => "arows",
                "costcpu" => "costcpu",
                "cost" => "cost",
                "time" => "time",
                "atime" => "atime",
                "buffers" => "buffers",
                "reads" => "reads",
                "omem" => "omem",
                "1mem" => "1mem",
                "usedmem" => "usedmem",
                _ => normalized
            };
        }

        private static string? GetValue(
            Dictionary<string, string> row,
            string key,
            bool trim = true)
        {
            if (!row.TryGetValue(key, out var value))
                return null;

            return trim ? value.Trim() : value.TrimEnd();
        }

        private static bool TryParseOperationId(string? value, out int id)
        {
            id = 0;

            if (string.IsNullOrWhiteSpace(value))
                return false;

            var match = Regex.Match(value, @"\d+");

            if (!match.Success)
                return false;

            return int.TryParse(
                match.Value,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out id);
        }

        private static string CleanOperation(string? operation)
        {
            if (string.IsNullOrWhiteSpace(operation))
                return string.Empty;

            return operation
                .Replace("*", string.Empty)
                .Trim();
        }

        private static int GetOperationDepth(string? rawOperation)
        {
            if (string.IsNullOrEmpty(rawOperation))
                return 0;

            var leadingSpaces = rawOperation.TakeWhile(char.IsWhiteSpace).Count();

            return leadingSpaces / 2;
        }

        private static string? GetObjectName(string operation, string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            var upperOperation = operation.ToUpperInvariant();

            if (upperOperation.Contains("INDEX"))
                return null;

            return name.Trim();
        }

        private static string? GetIndexName(string operation, string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            var upperOperation = operation.ToUpperInvariant();

            if (!upperOperation.Contains("INDEX"))
                return null;

            return name.Trim();
        }

        private static string NormalizeNodeType(string operation)
        {
            if (string.IsNullOrWhiteSpace(operation))
                return "Other";

            var value = operation.ToUpperInvariant();

            if (value.Contains("TABLE ACCESS") && value.Contains("FULL"))
                return "FullTableScan";

            if (value.Contains("TABLE ACCESS") && value.Contains("BY INDEX ROWID"))
                return "TableAccessByIndexRowId";

            if (value.Contains("INDEX UNIQUE SCAN"))
                return "IndexSeek";

            if (value.Contains("INDEX RANGE SCAN"))
                return "IndexSeek";

            if (value.Contains("INDEX FULL SCAN"))
                return "IndexScan";

            if (value.Contains("INDEX FAST FULL SCAN"))
                return "IndexScan";

            if (value.Contains("INDEX SKIP SCAN"))
                return "IndexScan";

            if (value.Contains("NESTED LOOPS"))
                return "NestedLoopJoin";

            if (value.Contains("HASH JOIN"))
                return "HashJoin";

            if (value.Contains("MERGE JOIN"))
                return "MergeJoin";

            if (value.Contains("SORT GROUP BY") ||
                value.Contains("HASH GROUP BY") ||
                value.Contains("SORT AGGREGATE"))
                return "Aggregate";

            if (value.Contains("SORT"))
                return "Sort";

            if (value.Contains("FILTER"))
                return "Filter";

            if (value.Contains("VIEW"))
                return "View";

            if (value.Contains("SELECT STATEMENT"))
                return "Select";

            if (value.Contains("COUNT"))
                return "Aggregate";

            return "Other";
        }

        private static string? GetJoinType(string operation)
        {
            if (string.IsNullOrWhiteSpace(operation))
                return null;

            var value = operation.ToUpperInvariant();

            if (value.Contains("NESTED LOOPS"))
                return "Nested Loops";

            if (value.Contains("HASH JOIN"))
                return "Hash Join";

            if (value.Contains("MERGE JOIN"))
                return "Merge Join";

            return null;
        }

        private static long? ParseOracleNumber(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            value = value.Trim();

            if (value == "-")
                return null;

            value = value.Replace(",", string.Empty);

            var match = Regex.Match(
                value,
                @"(?<number>\d+(\.\d+)?)(?<suffix>[KMG])?",
                RegexOptions.IgnoreCase);

            if (!match.Success)
                return null;

            if (!double.TryParse(
                    match.Groups["number"].Value,
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out var number))
            {
                return null;
            }

            var suffix = match.Groups["suffix"].Value.ToUpperInvariant();

            number = suffix switch
            {
                "K" => number * 1_000,
                "M" => number * 1_000_000,
                "G" => number * 1_000_000_000,
                _ => number
            };

            return Convert.ToInt64(Math.Round(number));
        }

        private static double? ParseOracleTimeToMilliseconds(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            value = value.Trim();

            if (value == "-")
                return null;

            if (TimeSpan.TryParse(value, CultureInfo.InvariantCulture, out var timeSpan))
                return timeSpan.TotalMilliseconds;

            var match = Regex.Match(
                value,
                @"(?<hours>\d{2}):(?<minutes>\d{2}):(?<seconds>\d{2})(\.(?<fraction>\d+))?");

            if (!match.Success)
                return null;

            var hours = int.Parse(match.Groups["hours"].Value, CultureInfo.InvariantCulture);
            var minutes = int.Parse(match.Groups["minutes"].Value, CultureInfo.InvariantCulture);
            var seconds = int.Parse(match.Groups["seconds"].Value, CultureInfo.InvariantCulture);

            var milliseconds = 0;

            if (match.Groups["fraction"].Success)
            {
                var fraction = match.Groups["fraction"].Value;

                if (fraction.Length > 3)
                    fraction = fraction.Substring(0, 3);

                fraction = fraction.PadRight(3, '0');

                milliseconds = int.Parse(fraction, CultureInfo.InvariantCulture);
            }

            return new TimeSpan(0, hours, minutes, seconds, milliseconds).TotalMilliseconds;
        }
    }
}
