using QueryOptimizer.Shared.Common.Models.Metrics;
using System;
using System.Collections.Generic;
using System.Text;

namespace QueryOptimizer.Models.Application
{
    public class OptimizationCandidateResult
    {
        public int CandidateId { get; set; }
        public string RuleCode { get; set; } = default!;
        public string CandidateSql { get; set; } = default!;
        public string? Description { get; set; }

        public bool WasTested { get; set; }

        public bool IsBest { get; set; }

        public QueryPerformanceMetrics? Metrics { get; set; }

        public double? ImprovementPercent { get; set; }
    }
}
