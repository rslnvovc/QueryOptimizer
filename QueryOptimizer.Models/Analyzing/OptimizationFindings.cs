using System;
using System.Collections.Generic;
using System.Text;

namespace QueryOptimizer.Models.Analyzing
{
    public class OptimizationFindings
    {
        public int Id { get; set; }
        public int QueryAnalysisRunId { get; set; }
        public string RuleCode { get; set; } = default!;
        public string Title { get; set; } = default!;
        public string Description { get; set; } = default!;
        public string Recommendation { get; set; } = default!;
        public string AffectedObject { get; set; } = default!;
        public string? AffectedNodeType { get; set; } = default!;
        public string? SuggestedSql { get; set; } = default!;
        public string? SuggestedIndexSql { get; set; }
        public int Severity { get; set; }
        public float BaseConfidence { get; set; }
        public float AdaptiveConfidence { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
