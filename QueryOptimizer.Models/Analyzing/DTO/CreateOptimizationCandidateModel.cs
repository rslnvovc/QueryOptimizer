using System;
using System.Collections.Generic;
using System.Text;

namespace QueryOptimizer.Models.Analyzing.DTO
{
    public class CreateOptimizationCandidateModel
    {
        public int QueryAnalysisRunId { get; set; }

        public string RuleCode { get; set; } = default!;

        public string CandidateSql { get; set; } = default!;

        public string? Description { get; set; }

        public bool WasTested { get; set; }

        public bool IsBest { get; set; }
    }
}
