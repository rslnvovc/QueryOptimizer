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

        public string SuggestedSql { get; set; } = default!;

        public string SuggestedIndexSql { get; set; } = default!;

        public FindingSeverity Severity { get; set; }
    }
}
