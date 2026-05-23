using System;
using System.Collections.Generic;
using System.Text;

namespace QueryOptimizer.Models.Analyzing
{
    public class QueryExecutionMetrics
    {
        public int Id { get; set; }
        public int QueryAnalysisRunId { get; set; }
        public string VariantType { get; set; } = default!;
        public int CandidateId { get; set; }
        public float ExecutionTimeMs { get; set; }
        public float DatabaseElapsedMs { get; set; }
        public float CpuTimeMs { get; set; }
        public int LogicalReads { get; set; }
        public int PhysicalReads { get; set; }
        public int RowsReturned { get; set; }
        public int RowsAffected { get; set; }
        public float EstimatedCost { get; set; }
        public float PlanningTimeMs { get; set; }
        public string? ExecutionPlan { get; set; }
        public int ExecutionPlanFormat { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
