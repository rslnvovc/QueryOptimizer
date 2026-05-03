using QueryOptimizer.Shared.Common.Enums;
using QueryOptimizer.Shared.Common.Models.Metrics;
using QueryOptimizer.Shared.Infrastructure.Abstractions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace QueryOptimizer.Providers.PostgreSQL.Analyzing
{
    public class PostgreSqlQueryPerformanceAnalyzer : IQueryPerformanceAnalyzer
    {
        public async Task<QueryPerformanceMetrics> AnalyzeAsync(DbCommand command, CancellationToken cancellationToken = default)
        {
            QueryPerformanceMetrics result = new()
            { 
                Provider = DatabaseTypes.PostgreSql,
                OriginalSql = command.CommandText,
                StartedAt = DateTime.UtcNow,
                ExecutionPlanFormat = ExecutionPlanFormats.Json
            };

            var explainSql = $"EXPLAIN (ANALYZE, BUFFERS, FORMAT JSON) {command.CommandText}";

            command.CommandText = explainSql;

            var stopwatch = Stopwatch.StartNew();

            var json = Convert.ToString(await command.ExecuteScalarAsync(cancellationToken));

            stopwatch.Stop();

            result.FinishedAt = DateTime.UtcNow;
            result.ClientElapsedMs = stopwatch.ElapsedMilliseconds;
            result.ExecutionPlan = json;

            ParseExecutionPlan(json ?? string.Empty, result);

            return result;
        }

        public async Task<string> GetEstimatedExecutionPlanAsync(DbCommand command, CancellationToken cancellationToken = default)
        {
            var explainSql = $"EXPLAIN (FORMAT JSON) {command.CommandText}";

            command.CommandText = explainSql;

            var plan = Convert.ToString(await command.ExecuteScalarAsync(cancellationToken));

            return plan ?? string.Empty;
        }

        private void ParseExecutionPlan(string json, QueryPerformanceMetrics metrics)
        {
            if (string.IsNullOrEmpty(json)) return;

            using var document = JsonDocument.Parse(json);

            var root = document.RootElement[0];

            if (root.TryGetProperty("Planning Time", out var planningTime))
            {
                metrics.PlanningTimeMs = planningTime.GetDouble();
            }

            if (root.TryGetProperty("Execution Time", out var executionTime))
                metrics.DatabaseElapsedMs = executionTime.GetDouble();

            if (root.TryGetProperty("Plan", out var plan))
            {
                ParsePlanNode(plan, metrics);
            }
        }

        private static void ParsePlanNode(JsonElement node, QueryPerformanceMetrics metrics)
        {
            var planNode = new QueryPlanNodeMetric();

            if (node.TryGetProperty("Node Type", out var nodeType))
                planNode.NodeType = nodeType.GetString();

            if (node.TryGetProperty("Relation Name", out var relationName))
                planNode.RelationName = relationName.GetString();

            if (node.TryGetProperty("Total Cost", out var totalCost))
                planNode.EstimatedCost = totalCost.GetDouble();

            if (node.TryGetProperty("Actual Total Time", out var actualTime))
                planNode.ActualTimeMs = actualTime.GetDouble();

            if (node.TryGetProperty("Plan Rows", out var estimatedRows))
                planNode.EstimatedRows = estimatedRows.GetInt64();

            if (node.TryGetProperty("Actual Rows", out var actualRows))
                planNode.ActualRows = actualRows.GetInt64();

            if (node.TryGetProperty("Shared Hit Blocks", out var sharedHitBlocks))
                planNode.LogicalReads = sharedHitBlocks.GetInt64();

            if (node.TryGetProperty("Shared Read Blocks", out var sharedReadBlocks))
                planNode.PhysicalReads = sharedReadBlocks.GetInt64();

            metrics.PlanNodes.Add(planNode);

            if (planNode.LogicalReads.HasValue)
                metrics.LogicalReads = (metrics.LogicalReads ?? 0) + planNode.LogicalReads.Value;

            if (planNode.PhysicalReads.HasValue)
                metrics.PhysicalReads = (metrics.PhysicalReads ?? 0) + planNode.PhysicalReads.Value;

            if (node.TryGetProperty("Plans", out var childPlans))
            {
                foreach (var child in childPlans.EnumerateArray())
                {
                    ParsePlanNode(child, metrics);
                }
            }
        }
    }
}
