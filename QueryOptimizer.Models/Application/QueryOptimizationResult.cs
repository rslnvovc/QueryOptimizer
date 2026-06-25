using QueryOptimizer.Shared.Common.Models.ExecutionPlan;
using QueryOptimizer.Shared.Common.Models.Metrics;
using QueryOptimizer.Shared.Common.Models.Optimization;
using System;
using System.Collections.Generic;
using System.Text;

namespace QueryOptimizer.Models.Application
{
    public class QueryOptimizationResult
    {
        public int AnalysisRunId { get; set; }

        public string ProviderName { get; set; } = string.Empty;

        public string ExecutionPlanFormat { get; set; } = string.Empty;

        public QueryPerformanceMetrics OriginalMetrics { get; set; } = default!;

        public NormalizedExecutionPlan NormalizedPlan { get; set; } = default!;

        public IList<QueryOptimizationFinding> Findings { get; set; } = new List<QueryOptimizationFinding>();

        public IList<OptimizationCandidateResult> CandidateResults { get; set; } = new List<OptimizationCandidateResult>();

        public OptimizationCandidateResult? BestCandidate { get; set; }
    }
}
