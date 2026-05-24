using QueryOptimizer.Models.Analyzing.DTO;
using QueryOptimizer.Models.Application;
using QueryOptimizer.Optimization.Services.Abstractions;
using QueryOptimizer.Repositories.Abstractions;
using QueryOptimizer.Shared.Common.Enums;
using QueryOptimizer.Shared.Common.Models.Metrics;
using QueryOptimizer.Shared.Common.Models.Optimization;
using System;
using System.Collections.Generic;
using System.Text;

namespace QueryOptimizer.Optimization.Services
{
    public class AdaptiveLearningService : IAdaptiveLearningService
    {
        private readonly IAnalyzingRepository _analyzingRepository;

        public AdaptiveLearningService(IAnalyzingRepository analyzingRepository)
        { 
            _analyzingRepository = analyzingRepository;
        }

        public async Task ApplyHistoryToFindingsAsync(DatabaseTypes provider, IList<QueryOptimizationFinding> findings, CancellationToken cancellationToken = default)
        {
            var weights = await _analyzingRepository.GetOptimizationRuleWeightsByProviderAsync(
                (int)provider,
                cancellationToken);

            foreach (var finding in findings)
            {
                var weight = weights.FirstOrDefault(x => x.RuleCode == finding.RuleCode);

                var baseConfidence = finding.Confidence;

                finding.AdaptiveConfidence = weight == null
                    ? baseConfidence
                    : Math.Min(1.0, baseConfidence + weight.ConfidenceBonus);
            }
        }

        public async Task LearnFromCandidateResultsAsync(DatabaseTypes provider, string normalizedSqlHash, QueryPerformanceMetrics originalMetrics, IList<OptimizationCandidateResult> candidateResults, CancellationToken cancellationToken = default)
        {
            foreach (var candidateResult in candidateResults)
            {
                if (!candidateResult.WasTested)
                    continue;

                if (candidateResult.Metrics == null)
                    continue;

                var improvementPercent = candidateResult.ImprovementPercent ?? 0;

                await _analyzingRepository.CreateOptimizationExperienceAsync(
                    new CreateOptimizationExperienceModel
                    {
                        Provider = (int)provider,
                        NormalizedSqlHash = normalizedSqlHash,
                        RuleCode = candidateResult.RuleCode,

                        OriginalExecutionTimeMs = originalMetrics.ClientElapsedMs,
                        CandidateExecutionTimeMs = candidateResult.Metrics.ClientElapsedMs,
                        ImprovementPercent = improvementPercent,

                        OriginalLogicalReads = originalMetrics.LogicalReads,
                        CandidateLogicalReads = candidateResult.Metrics.LogicalReads,
                        LogicalReadsImprovementPercent = CalculateLogicalReadsImprovementPercent(
                            originalMetrics.LogicalReads,
                            candidateResult.Metrics.LogicalReads)
                    },
                    cancellationToken);

                await _analyzingRepository.UpsertOptimizationRuleWeightAsync(
                    (int)provider,
                    candidateResult.RuleCode,
                    improvementPercent,
                    cancellationToken);
            }
        }

        private static double? CalculateLogicalReadsImprovementPercent(
            long? originalLogicalReads,
            long? candidateLogicalReads)
        {
            if (!originalLogicalReads.HasValue)
                return null;

            if (!candidateLogicalReads.HasValue)
                return null;

            if (originalLogicalReads.Value <= 0)
                return null;

            return ((double)(originalLogicalReads.Value - candidateLogicalReads.Value) /
                    originalLogicalReads.Value) * 100.0;
        }
    }
}
