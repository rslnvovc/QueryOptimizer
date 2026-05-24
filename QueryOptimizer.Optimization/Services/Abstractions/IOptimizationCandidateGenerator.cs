using QueryOptimizer.Models.Application;
using QueryOptimizer.Shared.Common.Models.Optimization;
using System;
using System.Collections.Generic;
using System.Text;

namespace QueryOptimizer.Optimization.Services.Abstractions
{
    public interface IOptimizationCandidateGenerator
    {
        IList<OptimizationCandidate> Generate(
            string originalSql,
            IList<QueryOptimizationFinding> findings
            );
    }
}
