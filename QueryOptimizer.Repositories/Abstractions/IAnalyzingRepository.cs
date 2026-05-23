using QueryOptimizer.Models.Analyzing;
using QueryOptimizer.Models.Analyzing.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace QueryOptimizer.Repositories.Abstractions
{
    public interface IAnalyzingRepository
    {
        Task<int> CreateQueryAnalysisRunAsync(
            CreateQueryAnalysisRunModel model,
            CancellationToken cancellationToken = default);

        Task CompleteQueryAnalysisRunAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task FailQueryAnalysisRunAsync(
            int id,
            string errorMessage,
            CancellationToken cancellationToken = default);

        Task<int> CreateQueryExecutionMetricAsync(
            CreateQueryExecutionMetricModel model,
            CancellationToken cancellationToken = default);

        Task<int> CreateOptimizationFindingAsync(
            CreateOptimizationFindingModel model,
            CancellationToken cancellationToken = default);

        Task<int> CreateOptimizationCandidateAsync(
            CreateOptimizationCandidateModel model,
            CancellationToken cancellationToken = default);

        Task UpdateOptimizationCandidateTestResultAsync(
            int id,
            bool wasTested,
            bool isBest,
            CancellationToken cancellationToken = default);

        Task<int> CreateOptimizationExperienceAsync(
            CreateOptimizationExperienceModel model,
            CancellationToken cancellationToken = default);

        Task UpsertOptimizationRuleWeightAsync(
            int provider,
            string ruleCode,
            double improvementPercent,
            CancellationToken cancellationToken = default);

        Task<IList<QueryAnalysisRuns>> GetQueryAnalysisRunsByUserAsync(
            int userId,
            CancellationToken cancellationToken = default);

        Task<IList<OptimizationRuleWeights>> GetOptimizationRuleWeightsByProviderAsync(
            int provider,
            CancellationToken cancellationToken = default);

        Task<OptimizationRuleWeights?> GetOptimizationRuleWeightAsync(
            int provider,
            string ruleCode,
            CancellationToken cancellationToken = default);

        Task<QueryAnalysisFullResultModel> GetQueryAnalysisRunFullAsync(
            int queryAnalysisRunId,
            CancellationToken cancellationToken = default);
    }
}
