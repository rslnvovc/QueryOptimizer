using QueryOptimizer.Shared.Common.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace QueryOptimizer.Shared.Common.Models.Optimization
{
    public class QueryOptimizationFinding
    {
        public string RuleCode { get; set; } = default!;

        public string Title { get; set; } = default!;

        public string Description { get; set; } = default!;

        public string Recommendation { get; set; } = default!;

        public string? SuggestedSql { get; set; } = default!;

        public string? SuggestedIndexSql { get; set; } = default!;

        public string? AffectedObject { get; set; } = default!;

        public string? AffectedNodeType { get; set; } = default!;

        public FindingSeverity Severity { get; set; }

        public double Confidence { get; set; } = 0.7;

        public double AdaptiveConfidence { get; set; } = 0.7;
    }
}
