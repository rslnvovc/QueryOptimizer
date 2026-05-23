using System;
using System.Collections.Generic;
using System.Text;

namespace QueryOptimizer.Models.Analyzing.DTO
{
    public class QueryAnalysisFullResultModel
    {
        public QueryAnalysisRuns? Run { get; set; }

        public IList<QueryExecutionMetrics> Metrics { get; set; } = new List<QueryExecutionMetrics>();

        public IList<OptimizationFindings> Findings { get; set; } = new List<OptimizationFindings>();

        public IList<OptimizationCandidates> Candidates { get; set; } = new List<OptimizationCandidates>();

        public IList<OptimizationExperiences> Experiences { get; set; } = new List<OptimizationExperiences>();
    }
}
