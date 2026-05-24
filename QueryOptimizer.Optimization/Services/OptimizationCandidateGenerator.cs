using QueryOptimizer.Models.Application;
using QueryOptimizer.Optimization.Services.Abstractions;
using QueryOptimizer.Shared.Common.Enums;
using QueryOptimizer.Shared.Common.Models.Optimization;
using System;
using System.Collections.Generic;
using System.Text;

namespace QueryOptimizer.Optimization.Services
{
    public class OptimizationCandidateGenerator : IOptimizationCandidateGenerator
    {
        private readonly ISqlQueryImprovementService _sqlQueryImprovementService;

        public OptimizationCandidateGenerator(ISqlQueryImprovementService sqlQueryImprovementService)
        { 
            _sqlQueryImprovementService = sqlQueryImprovementService;
        }

        public IList<OptimizationCandidate> Generate(string originalSql, IList<QueryOptimizationFinding> findings, DatabaseTypes provider)
        {
            var rewriteCandidates = _sqlQueryImprovementService.BuildCandidates(
                originalSql,
                findings,
                provider
                );

            return rewriteCandidates
                .Select(x => new OptimizationCandidate
                {
                    RuleCode = x.RuleCode,
                    CandidateSql = x.CandidateSql,
                    Description = x.Description
                })
                .ToList();
        }
    }
}
