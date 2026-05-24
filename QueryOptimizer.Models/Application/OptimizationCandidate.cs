using System;
using System.Collections.Generic;
using System.Text;

namespace QueryOptimizer.Models.Application
{
    public class OptimizationCandidate
    {
        public string RuleCode { get; set; } = default!;

        public string CandidateSql { get; set; } = default!;

        public string? Description { get; set; }
    }
}
