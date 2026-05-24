using QueryOptimizer.Models.Application;
using QueryOptimizer.Optimization.Services.Abstractions;
using QueryOptimizer.Shared.Common.Models.Optimization;
using System;
using System.Collections.Generic;
using System.Text;

namespace QueryOptimizer.Optimization.Services
{
    public class OptimizationCandidateGenerator : IOptimizationCandidateGenerator
    {
        public IList<OptimizationCandidate> Generate(string originalSql, IList<QueryOptimizationFinding> findings)
        {
            return findings
                .Where(x => !string.IsNullOrWhiteSpace(x.SuggestedSql))
                .Select(x => new OptimizationCandidate
                {
                    RuleCode = x.RuleCode,
                    CandidateSql = x.SuggestedSql!,
                    Description = x.Recommendation
                })
                .GroupBy(x => x.CandidateSql)
                .Select(x => x.First())
                .ToList();
        }
    }
}
