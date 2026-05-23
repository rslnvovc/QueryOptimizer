using System;
using System.Collections.Generic;
using System.Text;

namespace QueryOptimizer.Models.Analyzing
{
    public class QueryAnalysisRuns
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int Provider { get; set; }
        public string OriginalSql { get; set; } = default!;
        public string NormalizedSqlHash { get; set; } = default!;
        public DateTime StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }
        public string Status { get; set; } = default!;
        public string? ErrorMessage { get; set; } = default!;
    }
}
