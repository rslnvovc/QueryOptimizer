using QueryOptimizer.DatabaseExecutor;
using QueryOptimizer.DatabaseExecutor.Abstractions;
using QueryOptimizer.Models.Analyzing;
using QueryOptimizer.Models.Analyzing.DTO;
using QueryOptimizer.Repositories.Abstractions;
using QueryOptimizer.Shared.Common.Enums;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace QueryOptimizer.Repositories
{
    public class AnalyzingRepository : IAnalyzingRepository
    {
        private readonly string _applicationConnectionString;

        public AnalyzingRepository(string applicationConnectionString)
        {
            _applicationConnectionString = applicationConnectionString;
        }

        public async Task CompleteQueryAnalysisRunAsync(int id, CancellationToken cancellationToken = default)
        {
            var parameters = new Dictionary<string, object>
            {
                ["Id"] = id
            };

            using var executor = CreateExecutor();
            await executor.ExecuteNonQueryAsync(
                "[Analyzing].[QueryAnalysisRun_Complete]",
                parameters,
                CommandType.StoredProcedure,
                cancellationToken
                );
        }

        public async Task<int> CreateOptimizationCandidateAsync(CreateOptimizationCandidateModel model, CancellationToken cancellationToken = default)
        {
            var parameters = new Dictionary<string, object>
            {
                ["QueryAnalysisRunId"] = model.QueryAnalysisRunId,
                ["RuleCode"] = model.RuleCode,
                ["CandidateSql"] = model.CandidateSql,
                ["Description"] = DbValue(model.Description),
                ["WasTested"] = model.WasTested,
                ["IsBest"] = model.IsBest
            };

            using var executor = CreateExecutor();
            var result = await executor.ExecuteScalarAsync<int>(
                "[Analyzing].[OptimizationCandidate_Create]",
                parameters,
                CommandType.StoredProcedure,
                cancellationToken
                );

            return result;
        }

        public async Task<int> CreateOptimizationExperienceAsync(CreateOptimizationExperienceModel model, CancellationToken cancellationToken = default)
        {
            var parameters = new Dictionary<string, object>
            {
                ["Provider"] = model.Provider,
                ["NormalizedSqlHash"] = model.NormalizedSqlHash,
                ["RuleCode"] = model.RuleCode,

                ["OriginalExecutionTimeMs"] = model.OriginalExecutionTimeMs,
                ["CandidateExecutionTimeMs"] = model.CandidateExecutionTimeMs,
                ["ImprovementPercent"] = model.ImprovementPercent,

                ["OriginalLogicalReads"] = DbValue(model.OriginalLogicalReads),
                ["CandidateLogicalReads"] = DbValue(model.CandidateLogicalReads),
                ["LogicalReadsImprovementPercent"] = DbValue(model.LogicalReadsImprovementPercent)
            };

            using var executor = CreateExecutor();
            var result = await executor.ExecuteScalarAsync<int>(
                "[Analyzing].[OptimizationExperience_Create]",
                parameters,
                CommandType.StoredProcedure,
                cancellationToken);

            return result;
        }

        public async Task<int> CreateOptimizationFindingAsync(CreateOptimizationFindingModel model, CancellationToken cancellationToken = default)
        {
            var parameters = new Dictionary<string, object>
            {
                ["QueryAnalysisRunId"] = model.QueryAnalysisRunId,

                ["RuleCode"] = model.RuleCode,
                ["Title"] = model.Title,
                ["Description"] = model.Description,
                ["Recommendation"] = model.Recommendation,

                ["AffectedObject"] = DbValue(model.AffectedObject),
                ["AffectedNodeType"] = DbValue(model.AffectedNodeType),

                ["SuggestedSql"] = DbValue(model.SuggestedSql),
                ["SuggestedIndexSql"] = DbValue(model.SuggestedIndexSql),

                ["Severity"] = model.Severity,
                ["BaseConfidence"] = model.BaseConfidence,
                ["AdaptiveConfidence"] = model.AdaptiveConfidence
            };

            using var executor = CreateExecutor();
            var result = await executor.ExecuteScalarAsync<int>(
                "[Analyzing].[OptimizationFinding_Create]",
                parameters,
                CommandType.StoredProcedure,
                cancellationToken
                );

            return result;
        }

        public async Task<int> CreateQueryAnalysisRunAsync(CreateQueryAnalysisRunModel model, CancellationToken cancellationToken = default)
        {
            var parameters = new Dictionary<string, object>
            {
                ["UserId"] = model.UserId,
                ["Provider"] = model.Provider,
                ["OriginalSql"] = model.OriginalSql,
                ["NormalizedSqlHash"] = model.NormalizedSqlHash
            };

            using var executor = CreateExecutor();
            var result = await executor.ExecuteScalarAsync<int>(
                "[Analyzing].[QueryAnalysisRun_Create]",
                parameters,
                CommandType.StoredProcedure,
                cancellationToken
                );

            return result;
        }

        public async Task<int> CreateQueryExecutionMetricAsync(CreateQueryExecutionMetricModel model, CancellationToken cancellationToken = default)
        {
            var parameters = new Dictionary<string, object>
            {
                ["QueryAnalysisRunId"] = model.QueryAnalysisRunId,
                ["VariantType"] = model.VariantType,
                ["CandidateId"] = DbValue(model.CandidateId),

                ["ExecutionTimeMs"] = DbValue(model.ExecutionTimeMs),
                ["DatabaseElapsedMs"] = DbValue(model.DatabaseElapsedMs),
                ["CpuTimeMs"] = DbValue(model.CpuTimeMs),
                ["LogicalReads"] = DbValue(model.LogicalReads),
                ["PhysicalReads"] = DbValue(model.PhysicalReads),
                ["RowsReturned"] = DbValue(model.RowsReturned),
                ["RowsAffected"] = DbValue(model.RowsAffected),

                ["EstimatedCost"] = DbValue(model.EstimatedCost),
                ["PlanningTimeMs"] = DbValue(model.PlanningTimeMs),

                ["ExecutionPlan"] = DbValue(model.ExecutionPlan),
                ["ExecutionPlanFormat"] = DbValue(model.ExecutionPlanFormat)
            };

            using var executor = CreateExecutor();
            var result = await executor.ExecuteScalarAsync<int>(
                "[Analyzing].[QueryExecutionMetric_Create]",
                parameters,
                CommandType.StoredProcedure,
                cancellationToken);

            return result;
        }

        public async Task FailQueryAnalysisRunAsync(int id, string errorMessage, CancellationToken cancellationToken = default)
        {
            var parameters = new Dictionary<string, object>
            {
                ["Id"] = id,
                ["ErrorMessage"] = errorMessage
            };

            using var executor = CreateExecutor();
            await executor.ExecuteNonQueryAsync(
                "[Analyzing].[QueryAnalysisRun_Fail]",
                parameters,
                CommandType.StoredProcedure,
                cancellationToken);
        }

        public async Task<OptimizationRuleWeights?> GetOptimizationRuleWeightAsync(int provider, string ruleCode, CancellationToken cancellationToken = default)
        {
            var parameters = new Dictionary<string, object>
            {
                ["Provider"] = provider,
                ["RuleCode"] = ruleCode
            };

            using var executor = CreateExecutor();
            var result = await executor.ExecuteQueryAsync<OptimizationRuleWeights>(
                "[Analyzing].[OptimizationRuleWeight_Get]",
                parameters,
                CommandType.StoredProcedure);

            return result.FirstOrDefault();
        }

        public async Task<IList<OptimizationRuleWeights>> GetOptimizationRuleWeightsByProviderAsync(int provider, CancellationToken cancellationToken = default)
        {
            var parameters = new Dictionary<string, object>
            {
                ["Provider"] = provider
            };

            using var executor = CreateExecutor();
            var result = await executor.ExecuteQueryAsync<OptimizationRuleWeights>(
                "[Analyzing].[OptimizationRuleWeights_GetByProvider]",
                parameters,
                CommandType.StoredProcedure,
                cancellationToken);

            return result;
        }

        public async Task<QueryAnalysisFullResultModel> GetQueryAnalysisRunFullAsync(int queryAnalysisRunId, CancellationToken cancellationToken = default)
        {
            var parameters = new Dictionary<string, object>
            {
                ["QueryAnalysisRunId"] = queryAnalysisRunId
            };

            var runs = new List<QueryAnalysisRuns>();
            var metrics = new List<QueryExecutionMetrics>();
            var findings = new List<OptimizationFindings>();
            var candidates = new List<OptimizationCandidates>();
            var experiences = new List<OptimizationExperiences>();

            using var executor = CreateExecutor();

            await executor.ExecuteComplexAsync(
                "[Analyzing].[QueryAnalysisRun_GetFull]",
                parameters,
                CommandType.StoredProcedure, 
                cancellationToken,
                runs,
                metrics,
                findings,
                candidates,
                experiences);

            return new QueryAnalysisFullResultModel
            {
                Run = runs.FirstOrDefault(),
                Metrics = metrics,
                Findings = findings,
                Candidates = candidates,
                Experiences = experiences
            };
        }

        public async Task<IList<QueryAnalysisRuns>> GetQueryAnalysisRunsByUserAsync(int userId, CancellationToken cancellationToken = default)
        {
            var parameters = new Dictionary<string, object>
            {
                ["UserId"] = userId
            };

            using var executor = CreateExecutor();
            var result = await executor.ExecuteQueryAsync<QueryAnalysisRuns>(
                "[Analyzing].[QueryAnalysisRuns_GetByUser]",
                parameters,
                CommandType.StoredProcedure,
                cancellationToken);

            return result;
        }

        public async Task UpdateOptimizationCandidateTestResultAsync(int id, bool wasTested, bool isBest, CancellationToken cancellationToken = default)
        {
            var parameters = new Dictionary<string, object>
            {
                ["Id"] = id,
                ["WasTested"] = wasTested,
                ["IsBest"] = isBest
            };

            using var executor = CreateExecutor();
            await executor.ExecuteNonQueryAsync(
                "[Analyzing].[OptimizationCandidate_UpdateTestResult]",
                parameters,
                CommandType.StoredProcedure);
        }

        public async Task UpsertOptimizationRuleWeightAsync(int provider, string ruleCode, double improvementPercent, CancellationToken cancellationToken = default)
        {
            var parameters = new Dictionary<string, object>
            {
                ["Provider"] = provider,
                ["RuleCode"] = ruleCode,
                ["ImprovementPercent"] = improvementPercent
            };

            using var executor = CreateExecutor();
            await executor.ExecuteNonQueryAsync(
                "[Analyzing].[OptimizationRuleWeight_Upsert]",
                parameters,
                CommandType.StoredProcedure);
        }

        private static object DbValue<T>(T? value)
        {
            return value is null ? DBNull.Value : value;
        }

        private IDatabaseExecutor CreateExecutor()
        {
            return DatabaseExecutorFactory.CreateDbExecutor(
                DatabaseTypes.SqlServer,
                _applicationConnectionString);
        }
    }
}
