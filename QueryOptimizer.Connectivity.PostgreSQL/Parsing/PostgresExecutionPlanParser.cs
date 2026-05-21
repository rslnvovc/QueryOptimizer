using QueryOptimizer.Shared.Common.Enums;
using QueryOptimizer.Shared.Common.Models.ExecutionPlan;
using QueryOptimizer.Shared.Common.Models.Metrics;
using QueryOptimizer.Shared.Infrastructure.Abstractions;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace QueryOptimizer.Providers.PostgreSQL.Parsing
{
    public class PostgresExecutionPlanParser : IExecutionPlanParser
    {
        public NormalizedExecutionPlan Parse(QueryPerformanceMetrics executionPlan)
        {
            var result = new NormalizedExecutionPlan()
            {
                Provider = DatabaseTypes.PostgreSql,
                RawPlan = executionPlan.ExecutionPlan ?? string.Empty,
                TotalExecutionTimeMs = executionPlan.DatabaseElapsedMs,
                TotalLogicalReads = executionPlan.LogicalReads,
                TotalPhysicalReads = executionPlan.PhysicalReads,
            };

            if (string.IsNullOrEmpty(result.RawPlan))
                return result;

            try
            {
                using var document = JsonDocument.Parse(result.RawPlan);

                var root = document.RootElement;

                if (root.ValueKind != JsonValueKind.Array || root.GetArrayLength() == 0)
                    return result;

                var explainRoot = root[0];

                if (explainRoot.TryGetProperty("Execution Time", out var executionTime))
                    result.TotalExecutionTimeMs = GetNullableDouble(executionTime);

                if (explainRoot.TryGetProperty("Plan", out var planElement))
                {
                    result.TotalCost = GetNullableDoubleProperty(planElement, "Total Cost");

                    var rootNode = ParsePlanNode(planElement);

                    result.Nodes.Add(rootNode);

                    AddChildrenToList(rootNode, result.Nodes);

                    result.TotalLogicalReads = result.Nodes
                        .Where(x => x.LogicalReads.HasValue)
                        .Sum(x => x.LogicalReads!.Value);

                    result.TotalPhysicalReads = result.Nodes
                        .Where(x => x.PhysicalReads.HasValue)
                        .Sum(x => x.PhysicalReads!.Value);
                }
                 
            }
            catch (Exception ex)
            { 
            
            }

            return result;
        }

        private static ExecutionPlanNode ParsePlanNode(JsonElement nodeElement)
        {
            var nodeType = GetStringProperty(nodeElement, "Node Type");

            var node = new ExecutionPlanNode
            {
                NodeType = nodeType ?? "Unknown",
                NormalizedNodeType = NormalizeNodeType(nodeType),
                ObjectName = GetObjectName(nodeElement),
                IndexName = GetStringProperty(nodeElement, "Index Name"),
                Predicate = GetPredicate(nodeElement),
                JoinType = GetStringProperty(nodeElement, "Join Type"),

                EstimatedCost = GetNullableDoubleProperty(nodeElement, "Total Cost"),
                EstimatedRows = GetNullableLongProperty(nodeElement, "Plan Rows"),

                ActualRows = GetNullableLongProperty(nodeElement, "Actual Rows"),
                ActualTimeMs = GetNullableDoubleProperty(nodeElement, "Actual Total Time"),

                LogicalReads = GetLogicalReads(nodeElement),
                PhysicalReads = GetPhysicalReads(nodeElement)
            };

            if (nodeElement.TryGetProperty("Plans", out var childPlans) &&
                childPlans.ValueKind == JsonValueKind.Array)
            {
                foreach (var childElement in childPlans.EnumerateArray())
                {
                    var childNode = ParsePlanNode(childElement);
                    node.ChildrenNode.Add(childNode);
                }
            }

            return node;
        }

        private static void AddChildrenToList(
            ExecutionPlanNode parent,
            List<ExecutionPlanNode> result
            )
        {
            foreach (var childrenNode in parent.ChildrenNode)
            {
                result.Add(childrenNode);
                AddChildrenToList(childrenNode, result);
            }
        }

        private static string NormalizeNodeType(string? nodeType)
        {
            if (string.IsNullOrWhiteSpace(nodeType))
                return "Other";

            var value = nodeType.ToUpperInvariant();

            if (value.Contains("SEQ SCAN"))
                return "FullTableScan";

            if (value.Contains("INDEX ONLY SCAN"))
                return "IndexOnlyScan";

            if (value.Contains("INDEX SCAN"))
                return "IndexScan";

            if (value.Contains("BITMAP HEAP SCAN"))
                return "BitmapHeapScan";

            if (value.Contains("BITMAP INDEX SCAN"))
                return "BitmapIndexScan";

            if (value.Contains("NESTED LOOP"))
                return "NestedLoopJoin";

            if (value.Contains("HASH JOIN"))
                return "HashJoin";

            if (value.Contains("MERGE JOIN"))
                return "MergeJoin";

            if (value.Contains("SORT"))
                return "Sort";

            if (value.Contains("AGGREGATE"))
                return "Aggregate";

            if (value.Contains("GROUP"))
                return "Aggregate";

            if (value.Contains("HASH"))
                return "Hash";

            if (value.Contains("LIMIT"))
                return "Limit";

            if (value.Contains("FILTER"))
                return "Filter";

            if (value.Contains("MATERIALIZE"))
                return "Materialize";

            if (value.Contains("GATHER"))
                return "ParallelGather";

            if (value.Contains("APPEND"))
                return "Append";

            if (value.Contains("UNIQUE"))
                return "Unique";

            return "Other";
        }

        private static string? GetObjectName(JsonElement nodeElement)
        {
            var schema = GetStringProperty(nodeElement, "Schema");
            var relationName = GetStringProperty(nodeElement, "Relation Name");

            if (string.IsNullOrWhiteSpace(relationName))
                return null;

            if (string.IsNullOrWhiteSpace(schema))
                return relationName;

            return $"{schema}.{relationName}";
        }

        private static string? GetPredicate(JsonElement nodeElement)
        {
            var predicates = new List<string>();

            AddIfExists(nodeElement, predicates, "Filter");
            AddIfExists(nodeElement, predicates, "Index Cond");
            AddIfExists(nodeElement, predicates, "Recheck Cond");
            AddIfExists(nodeElement, predicates, "Hash Cond");
            AddIfExists(nodeElement, predicates, "Merge Cond");
            AddIfExists(nodeElement, predicates, "Join Filter");

            return predicates.Count == 0
                ? null
                : string.Join(" AND ", predicates);
        }

        private static void AddIfExists(
            JsonElement nodeElement,
            List<string> values,
            string propertyName
            )
        {
            var value = GetStringProperty(nodeElement, propertyName);

            if (!string.IsNullOrEmpty(value))
                values.Add(value);
        }

        private static long? GetLogicalReads(JsonElement nodeElement)
        {
            long total = 0;
            var hasValue = false;

            AddBlockValue(nodeElement, "Shared Hit Blocks", ref total, ref hasValue);
            AddBlockValue(nodeElement, "Local Hit Blocks", ref total, ref hasValue);

            return hasValue ? total : null;
        }

        private static long? GetPhysicalReads(JsonElement nodeElement)
        {
            long total = 0;
            var hasValue = false;

            AddBlockValue(nodeElement, "Shared Read Blocks", ref total, ref hasValue);
            AddBlockValue(nodeElement, "Local Read Blocks", ref total, ref hasValue);
            AddBlockValue(nodeElement, "Temp Read Blocks", ref total, ref hasValue);

            return hasValue ? total : null;
        }

        private static void AddBlockValue(
            JsonElement nodeElement,
            string propertyName,
            ref long total,
            ref bool hasValue
            )
        {
            var value = GetNullableLongProperty(nodeElement, propertyName);

            if (!value.HasValue)
                return;

            total += value.Value;
            hasValue = true;
        }

        private static string? GetStringProperty(JsonElement element, string propertyName)
        {
            if (!element.TryGetProperty(propertyName, out var property))
                return null;

            return property.ValueKind switch
            {
                JsonValueKind.String => property.GetString(),
                JsonValueKind.Number => property.ToString(),
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                _ => null
            };
        }

        private static double? GetNullableDoubleProperty(JsonElement element, string propertyName)
        {
            if (!element.TryGetProperty(propertyName, out var property))
                return null;

            return GetNullableDouble(property);
        }

        private static double? GetNullableDouble(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Number &&
                element.TryGetDouble(out var number))
            {
                return number;
            }

            if (element.ValueKind == JsonValueKind.String &&
                double.TryParse(
                    element.GetString(),
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out var parsed))
            {
                return parsed;
            }

            return null;
        }

        private static long? GetNullableLongProperty(JsonElement element, string propertyName)
        {
            if (!element.TryGetProperty(propertyName, out var property))
                return null;

            if (property.ValueKind == JsonValueKind.Number)
            {
                if (property.TryGetInt64(out var longValue))
                    return longValue;

                if (property.TryGetDouble(out var doubleValue))
                    return Convert.ToInt64(Math.Round(doubleValue));
            }

            if (property.ValueKind == JsonValueKind.String &&
                long.TryParse(
                    property.GetString(),
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out var parsed))
            {
                return parsed;
            }

            return null;
        }
    }
}
