using System;
using System.Collections.Generic;
using System.Text;

namespace QueryOptimizer.Models.Analyzing
{
    public class OptimizationRuleWeights
    {
        public int Id { get; set; }
        public int Provider { get; set; }
        public string RuleCode { get; set; } = default!;
        public int AppliedCount { get; set; }
        public int SuccessfulCount { get; set; }
        public float AverageImprovementPercent { get; set; }
        public float ConfidenceBonus { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
