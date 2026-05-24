using QueryOptimizer.Models.Application;
using QueryOptimizer.Shared.Common.Enums;
using QueryOptimizer.Shared.Common.Models.Optimization;
using System;
using System.Collections.Generic;
using System.Text;

namespace QueryOptimizer.Optimization.Services.Abstractions
{
    public interface ISqlQueryImprovementService
    {
        IList<SqlRewriteCandidate> BuildCandidates(
            string originalSql,
            IList<QueryOptimizationFinding> findings,
            DatabaseTypes provider);
    }
}
