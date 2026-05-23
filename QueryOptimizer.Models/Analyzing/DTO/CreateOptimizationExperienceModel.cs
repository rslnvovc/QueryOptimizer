using System;
using System.Collections.Generic;
using System.Text;

namespace QueryOptimizer.Models.Analyzing.DTO
{
    public class CreateOptimizationExperienceModel
    {
        public int Provider { get; set; }

        public string NormalizedSqlHash { get; set; } = default!;

        public string RuleCode { get; set; } = default!;

        public double OriginalExecutionTimeMs { get; set; }

        public double CandidateExecutionTimeMs { get; set; }

        public double ImprovementPercent { get; set; }

        public long? OriginalLogicalReads { get; set; }

        public long? CandidateLogicalReads { get; set; }

        public double? LogicalReadsImprovementPercent { get; set; }
    }
}
