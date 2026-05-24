using System;
using System.Collections.Generic;
using System.Text;

namespace QueryOptimizer.Models.Application
{
    public class SqlRewriteCandidate
    {
        public string RuleCode { get; set; } = default!;

        public string OriginalSql { get; set; } = default!;

        public string CandidateSql { get; set; } = default!;

        public string Description { get; set; } = default!;

        public bool IsSafeToBenchmark { get; set; } = true;
    }
}
