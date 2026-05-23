using System;
using System.Collections.Generic;
using System.Text;

namespace QueryOptimizer.Models.Analyzing
{
    public class OptimizationExperiences
    {
        public int Id { get; set; }
        public int Provider { get; set; }
        public string NormalizedSqlHash { get; set; } = default!;
        public string RuleCode { get; set; } = default!;
        public float OriginalExecutionTimeMs { get; set; }
        public float CandidateExecutionTimeMs { get; set; }
        public float ImprovementPercent { get; set; }
        public int OriginalLogicalReads { get; set; }
        public int CandidateLogicalReads { get; set; }
        public float LogicalReadsImprovementPercent { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
