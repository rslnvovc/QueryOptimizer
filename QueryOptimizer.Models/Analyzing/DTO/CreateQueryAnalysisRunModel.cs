using System;
using System.Collections.Generic;
using System.Text;

namespace QueryOptimizer.Models.Analyzing.DTO
{
    public class CreateQueryAnalysisRunModel
    {
        public int UserId { get; set; }

        public int Provider { get; set; }

        public string OriginalSql { get; set; } = default!;
        public string NormalizedSqlHash { get; set; } = default!;
    }
}
