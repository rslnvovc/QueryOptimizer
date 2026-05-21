using QueryOptimizer.Shared.Common.Enums;
using QueryOptimizer.Shared.Common.Models.ExecutionPlan;
using QueryOptimizer.Shared.Common.Models.Metrics;
using QueryOptimizer.Shared.Infrastructure.Abstractions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace QueryOptimizer.Providers.SQLServer.Parsing
{
    public class SqlServerExecutionPlanParser : IExecutionPlanParser
    {
        private static readonly XNamespace ShowPlanNamespace = "http://schemas.microsoft.com/sqlserver/2004/07/showplan";

        public NormalizedExecutionPlan Parse(QueryPerformanceMetrics executionPlan)
        {
            var result = new NormalizedExecutionPlan()
            {
                Provider = DatabaseTypes.SqlServer,
                RawPlan = executionPlan.ExecutionPlan ?? string.Empty,
                TotalExecutionTimeMs = executionPlan.DatabaseElapsedMs,
                TotalLogicalReads = executionPlan.LogicalReads,
                TotalPhysicalReads = executionPlan.PhysicalReads,
            };

            if (string.IsNullOrEmpty(result.RawPlan))
                return result;

            var xmlPlans = ExtractShowPlanXmlDocuments(result.RawPlan);

            foreach (var xmlPlan in xmlPlans)
            {
                try
                {
                    var document = XDocument.Parse(xmlPlan);

                    ParseTotalCost(document, result);
                    ParseMemoryGrant(document, result);
                    ParseRelOpNodes(document, result);
                }
                catch (Exception ex)
                { 
                
                }
            }

            return result;
        }

        private static void ParseTotalCost(XDocument document, NormalizedExecutionPlan result)
        {
            var statementCosts = document
                .Descendants(ShowPlanNamespace + "StmtSimple")
                .Select(x => GetNullableDoubleAttribute(x, "StatementSubTreeCost"))
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .ToList();

            if (statementCosts.Count > 0)
            {
                result.TotalCost = statementCosts.Sum();
                return;
            }

            var rootRelOpCost = document
                .Descendants(ShowPlanNamespace + "RelOp")
                .Select(x => GetNullableDoubleAttribute(x, "EstimatedTotalSubtreeCost"))
                .FirstOrDefault(x => x.HasValue);

            result.TotalCost = rootRelOpCost;
        }

        private static void ParseMemoryGrant(XDocument document, NormalizedExecutionPlan result)
        { 
            var memoryGrantInfo = document
                .Descendants(ShowPlanNamespace + "MemoryGrantInfo")
                .FirstOrDefault();

            if (memoryGrantInfo is null)
                return;

            result.RequestedMemoryKb = GetNullableLongAttribute(memoryGrantInfo, "RequestedMemory");
            result.GrantedMemoryKb = GetNullableLongAttribute(memoryGrantInfo, "GrantedMemory");
            result.MaxUsedMemoryKb = GetNullableLongAttribute(memoryGrantInfo, "MaxUsedMemory");
        }

        private static void ParseRelOpNodes(XDocument document, NormalizedExecutionPlan result)
        {
            var relOps = document
                .Descendants(ShowPlanNamespace + "RelOp")
                .ToList();

            foreach (var relOp in relOps)
            {
                var physicalOp = GetStringAttribute(relOp, "PhysicalOp");
                var logicalOp = GetStringAttribute(relOp, "LogicalOp");

                var node = new ExecutionPlanNode
                {
                    NodeType = physicalOp ?? logicalOp ?? "Unknown",
                    NormalizedNodeType = NormalizeNodeType(physicalOp, logicalOp),
                    ObjectName = GetObjectName(relOp),
                    IndexName = GetIndexName(relOp),
                    Predicate = GetPredicate(relOp),
                    JoinType = GetJoinType(physicalOp, logicalOp),

                    EstimatedCost = GetNullableDoubleAttribute(relOp, "EstimatedTotalSubtreeCost"),
                    EstimatedRows = GetNullableLongFromDoubleAttribute(relOp, "EstimateRows"),

                    ActualRows = GetActualRows(relOp),
                    ActualTimeMs = GetActualElapsedTimeMs(relOp),
                    LogicalReads = GetActualLogicalReads(relOp),
                    PhysicalReads = GetActualPhysicalReads(relOp)
                };

                result.Nodes.Add(node);
            }
        }

        private static string NormalizeNodeType(string? physicalOp, string? logicalOp)
        { 
            var value = $"{physicalOp} {logicalOp}".ToUpperInvariant();

            if (value.Contains("TABLE SCAN"))
                return "FullTableScan";

            if (value.Contains("CLUSTERED INDEX SCAN"))
                return "IndexScan";

            if (value.Contains("INDEX SCAN"))
                return "IndexScan";

            if (value.Contains("CLUSTERED INDEX SEEK"))
                return "IndexSeek";

            if (value.Contains("INDEX SEEK"))
                return "IndexSeek";

            if (value.Contains("KEY LOOKUP"))
                return "KeyLookup";

            if (value.Contains("RID LOOKUP"))
                return "RidLookup";

            if (value.Contains("NESTED LOOPS"))
                return "NestedLoopJoin";

            if (value.Contains("HASH MATCH") || value.Contains("HASH JOIN"))
                return "HashJoin";

            if (value.Contains("MERGE JOIN"))
                return "MergeJoin";

            if (value.Contains("SORT"))
                return "Sort";

            if (value.Contains("STREAM AGGREGATE") || value.Contains("HASH AGGREGATE"))
                return "Aggregate";

            if (value.Contains("COMPUTE SCALAR"))
                return "ComputeScalar";

            if (value.Contains("FILTER"))
                return "Filter";

            if (value.Contains("CONCATENATION"))
                return "Concatenation";

            return "Other";
        }

        private static string? GetJoinType(string physicalOp, string logicalOp)
        {
            var value = $"{physicalOp} {logicalOp}";

            if (value.Contains("Nested Loops", StringComparison.OrdinalIgnoreCase))
                return "Nested Loops";

            if (value.Contains("Hash Match", StringComparison.OrdinalIgnoreCase) ||
                value.Contains("Hash Join", StringComparison.OrdinalIgnoreCase))
                return "Hash Join";

            if (value.Contains("Merge Join", StringComparison.OrdinalIgnoreCase))
                return "Merge Join";

            return null;
        }

        private static string? GetObjectName(XElement relOp)
        {
            var objectElement = relOp
                .Descendants(ShowPlanNamespace + "Object")
                .FirstOrDefault();

            if (objectElement == null)
                return null;

            var schema = CleanSqlServerIdentifier(GetStringAttribute(objectElement, "Schema"));
            var table = CleanSqlServerIdentifier(GetStringAttribute(objectElement, "Table"));

            if (string.IsNullOrWhiteSpace(table))
                return null;

            if (string.IsNullOrWhiteSpace(schema))
                return table;

            return $"{schema}.{table}";
        }

        private static string? GetIndexName(XElement relOp)
        {
            var objectElement = relOp
                .Descendants(ShowPlanNamespace + "Object")
                .FirstOrDefault();

            if (objectElement == null)
                return null;

            return CleanSqlServerIdentifier(GetStringAttribute(objectElement, "Index"));
        }

        private static string? GetPredicate(XElement relOp)
        {
            var scalarStrings = relOp
                    .Descendants()
                    .Where(x =>
                        x.Name.LocalName == "Predicate" ||
                        x.Name.LocalName == "SeekPredicates" ||
                        x.Name.LocalName == "SeekPredicate" ||
                        x.Name.LocalName == "SeekPredicateNew")
                    .SelectMany(x => x.Descendants(ShowPlanNamespace + "ScalarOperator"))
                    .Select(x => GetStringAttribute(x, "ScalarString"))
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct()
                    .ToList();

            if (scalarStrings.Count == 0)
                return null;

            return string.Join(" AND ", scalarStrings);
        }

        private static long? GetActualRows(XElement relOp)
        {
            var runtimeCounters = relOp
                .Descendants(ShowPlanNamespace + "RunTimeCountersPerThread")
                .ToList();

            if (runtimeCounters.Count == 0)
                return null;

            var values = runtimeCounters
                .Select(x => GetNullableLongFromDoubleAttribute(x, "ActualRows"))
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .ToList();

            return values.Count == 0 ? null : values.Sum();
        }

        private static double? GetActualElapsedTimeMs(XElement relOp)
        {
            var runtimeCounters = relOp
                .Descendants(ShowPlanNamespace + "RunTimeCountersPerThread")
                .ToList();

            if (runtimeCounters.Count == 0)
                return null;

            var values = runtimeCounters
                .Select(x => GetNullableDoubleAttribute(x, "ActualElapsedms"))
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .ToList();

            return values.Count == 0 ? null : values.Max();
        }

        private static long? GetActualLogicalReads(XElement relOp)
        {
            var runtimeCounters = relOp
                .Descendants(ShowPlanNamespace + "RunTimeCountersPerThread")
                .ToList();

            if (runtimeCounters.Count == 0)
                return null;

            var values = runtimeCounters
                .Select(x => GetNullableLongAttribute(x, "ActualLogicalReads"))
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .ToList();

            return values.Count == 0 ? null : values.Sum();
        }

        private static long? GetActualPhysicalReads(XElement relOp)
        {
            var runtimeCounters = relOp
                .Descendants(ShowPlanNamespace + "RunTimeCountersPerThread")
                .ToList();

            if (runtimeCounters.Count == 0)
                return null;

            var values = runtimeCounters
                .Select(x => GetNullableLongAttribute(x, "ActualPhysicalReads"))
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .ToList();

            return values.Count == 0 ? null : values.Sum();
        }

        private static List<string> ExtractShowPlanXmlDocuments(string rawPlan)
        {
            var result = new List<string>();

            var matches = Regex.Matches(
                rawPlan,
                @"<ShowPlanXML[\s\S]*?</ShowPlanXML>",
                RegexOptions.IgnoreCase);

            foreach (Match match in matches)
            {
                result.Add(match.Value);
            }

            if (result.Count == 0 && rawPlan.TrimStart().StartsWith("<ShowPlanXML", StringComparison.OrdinalIgnoreCase))
            {
                result.Add(rawPlan);
            }

            return result;
        }

        private static string? GetStringAttribute(XElement element, string attributeName)
        {
            return element.Attribute(attributeName)?.Value;
        }

        private static double? GetNullableDoubleAttribute(XElement element, string attributeName)
        {
            var value = element.Attribute(attributeName)?.Value;

            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
                return result;

            return null;
        }

        private static long? GetNullableLongAttribute(XElement element, string attributeName)
        {
            var value = element.Attribute(attributeName)?.Value;

            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (long.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
                return result;

            return null;
        }

        private static long? GetNullableLongFromDoubleAttribute(XElement element, string attributeName)
        {
            var value = GetNullableDoubleAttribute(element, attributeName);

            if (!value.HasValue)
                return null;

            return Convert.ToInt64(Math.Round(value.Value));
        }

        private static string? CleanSqlServerIdentifier(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            return value
                .Replace("[", string.Empty)
                .Replace("]", string.Empty)
                .Trim();
        }
    }
}
