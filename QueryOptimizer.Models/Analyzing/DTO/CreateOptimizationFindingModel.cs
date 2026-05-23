using System;
using System.Collections.Generic;
using System.Text;

namespace QueryOptimizer.Models.Analyzing.DTO
{
    public class CreateOptimizationFindingModel
    {
        public int QueryAnalysisRunId { get; set; }

        public string RuleCode { get; set; } = default!;

        public string Title { get; set; } = default!;

        public string Description { get; set; } = default!;

        public string Recommendation { get; set; } = default!;

        public string? AffectedObject { get; set; }

        public string? AffectedNodeType { get; set; }

        public string? SuggestedSql { get; set; }

        public string? SuggestedIndexSql { get; set; }

        public int Severity { get; set; }

        public double BaseConfidence { get; set; }

        public double AdaptiveConfidence { get; set; }
    }
}
