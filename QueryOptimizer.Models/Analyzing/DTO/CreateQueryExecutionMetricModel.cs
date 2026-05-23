using System;
using System.Collections.Generic;
using System.Text;

namespace QueryOptimizer.Models.Analyzing.DTO
{
    public class CreateQueryExecutionMetricModel
    {
        public int QueryAnalysisRunId { get; set; }

        public string VariantType { get; set; } = default!;

        public int? CandidateId { get; set; }

        public double? ExecutionTimeMs { get; set; }

        public double? DatabaseElapsedMs { get; set; }

        public double? CpuTimeMs { get; set; }

        public long? LogicalReads { get; set; }

        public long? PhysicalReads { get; set; }

        public long? RowsReturned { get; set; }

        public long? RowsAffected { get; set; }

        public double? EstimatedCost { get; set; }

        public double? PlanningTimeMs { get; set; }

        public string? ExecutionPlan { get; set; }

        public int? ExecutionPlanFormat { get; set; }
    }
}
