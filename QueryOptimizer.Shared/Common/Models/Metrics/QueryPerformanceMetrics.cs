using QueryOptimizer.Shared.Common.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace QueryOptimizer.Shared.Common.Models.Metrics
{
    public class QueryPerformanceMetrics
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public DatabaseTypes Provider { get; set; }

        public string? OriginalSql { get; set; }

        public DateTime StartedAt { get; set; }

        public DateTime FinishedAt { get; set; }

        public long ClientElapsedMs { get; set; }

        public double? DatabaseElapsedMs { get; set; }

        public double? CpuTimeMs { get; set; }

        public long? LogicalReads { get; set; }

        public long? PhysicalReads { get; set; }

        public long? RowsReturned { get; set; }

        public long? RowsAffected { get; set; }

        public double? EstimatedCost { get; set; }

        public double? PlanningTimeMs { get; set; }

        public long? RequestedMemoryKb { get; set; }

        public long? GrantedMemoryKb { get; set; }

        public long? UsedMemoryKb { get; set; }

        public string? ExecutionPlan { get; set; }

        public ExecutionPlanFormats? ExecutionPlanFormat { get; set; }

        public List<QueryPlanNodeMetric> PlanNodes { get; set; } = new();

        public Dictionary<string, string> RawMetrics { get; set; } = new();

        public List<string> Warnings { get; set; } = new();
    }
}
