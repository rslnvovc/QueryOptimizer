using QueryOptimizer.Bll.Factories.Abstractions;
using QueryOptimizer.Bll.Services.Abstractions;
using QueryOptimizer.DatabaseExecutor.Abstractions;
using QueryOptimizer.Models.Analyzing.DTO;
using QueryOptimizer.Models.Application;
using QueryOptimizer.Optimization.Rules;
using QueryOptimizer.Optimization.Services;
using QueryOptimizer.Optimization.Services.Abstractions;
using QueryOptimizer.Providers.Oracle.Connectivity;
using QueryOptimizer.Providers.PostgreSQL.Connectivity;
using QueryOptimizer.Providers.SQLServer.Connectivity;
using QueryOptimizer.Repositories.Abstractions;
using QueryOptimizer.Shared.Common.Enums;
using QueryOptimizer.Shared.Common.Exceptions.Database;
using QueryOptimizer.Shared.Common.Helpers;
using QueryOptimizer.Shared.Common.Models.Metrics;
using QueryOptimizer.Shared.Common.Models.Optimization;
using QueryOptimizer.Shared.Infrastructure.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace QueryOptimizer.Bll.Services
{
    public class QueryOptimizationWorkflowService : IQueryOptimizationWorkflowService
    {
        private readonly ITargetDatabaseExecutorFactory _targetDatabaseExecutorFactory;
        private readonly IExecutionPlanParserFactory _executionPlanParserFactory;
        private readonly OptimizationRuleService _queryOptimizationAnalyzer;
        private readonly IOptimizationCandidateGenerator _candidateGenerator;
        private readonly IAdaptiveLearningService _adaptiveLearningService;
        private readonly IAnalyzingRepository _analyzingRepository;

        public QueryOptimizationWorkflowService(
            ITargetDatabaseExecutorFactory targetDatabaseExecutorFactory,
            IExecutionPlanParserFactory executionPlanParserFactory,
            IOptimizationCandidateGenerator candidateGenerator,
            IAdaptiveLearningService adaptiveLearningService,
            IAnalyzingRepository analyzingRepository)
        { 
            _targetDatabaseExecutorFactory =  targetDatabaseExecutorFactory;
            _executionPlanParserFactory = executionPlanParserFactory;
            var rules = GetOptimizationRules();
            _queryOptimizationAnalyzer = new OptimizationRuleService(rules);
            _adaptiveLearningService = adaptiveLearningService;
            _analyzingRepository = analyzingRepository;
             _candidateGenerator = candidateGenerator;
        }

        public async Task<QueryOptimizationResult> AnalyzeAsync(QueryOptimizationRequest request, CancellationToken cancellationToken = default)
        {
            string connectionString = CreateConnectionString(request.ConnectionStringModel);

            var normalizedSqlHash = QueryHashHelper.ComputeHash(request.Sql);

            var analysisRunId = await _analyzingRepository.CreateQueryAnalysisRunAsync(
                new CreateQueryAnalysisRunModel
                {
                    UserId = request.UserId,
                    Provider = (int)request.ConnectionStringModel.Provider,
                    OriginalSql = request.Sql,
                    NormalizedSqlHash = normalizedSqlHash
                },
                cancellationToken);

            try
            {
                var executor = _targetDatabaseExecutorFactory.Create(
                    request.ConnectionStringModel.Provider,
                    connectionString);

                var originalMetrics = await executor.AnalyzeAsync(
                    request.Sql,
                    request.Parameters,
                    cancellationToken);

                await SaveMetricAsync(
                    analysisRunId,
                    "Original",
                    null,
                    originalMetrics,
                    cancellationToken);

                var parser = _executionPlanParserFactory.GetParser(request.ConnectionStringModel.Provider);

                var normalizedPlan = parser.Parse(originalMetrics);

                var findings = _queryOptimizationAnalyzer.Analyze(
                    normalizedPlan,
                    request.Sql);

                await _adaptiveLearningService.ApplyHistoryToFindingsAsync(
                    request.ConnectionStringModel.Provider,
                    findings,
                    cancellationToken);

                foreach (var finding in findings)
                {
                    await SaveFindingAsync(
                        analysisRunId,
                        finding,
                        cancellationToken);
                }

                var candidates = _candidateGenerator.Generate(
                    request.Sql,
                    findings,
                    request.ConnectionStringModel.Provider);

                var candidateResults = await TestAndSaveCandidatesAsync(
                    executor,
                    analysisRunId,
                    candidates,
                    request.Parameters,
                    originalMetrics,
                    cancellationToken);

                await _adaptiveLearningService.LearnFromCandidateResultsAsync(
                    request.ConnectionStringModel.Provider,
                    normalizedSqlHash,
                    originalMetrics,
                    candidateResults,
                    cancellationToken);

                await _analyzingRepository.CompleteQueryAnalysisRunAsync(
                    analysisRunId,
                    cancellationToken);

                return new QueryOptimizationResult
                {
                    AnalysisRunId = analysisRunId,
                    ProviderName = request.ConnectionStringModel.Provider.ToString(),
                    ExecutionPlanFormat = originalMetrics.ExecutionPlanFormat.ToString() ?? "Невідомо",
                    OriginalMetrics = originalMetrics,
                    NormalizedPlan = normalizedPlan,
                    Findings = findings,
                    CandidateResults = candidateResults,
                    BestCandidate = candidateResults.FirstOrDefault(x => x.IsBest)
                };
            }
            catch (Exception ex)
            {
                await _analyzingRepository.FailQueryAnalysisRunAsync(
                    analysisRunId,
                    ex.Message,
                    cancellationToken);

                throw;
            }
        }

        private async Task<IList<OptimizationCandidateResult>> TestAndSaveCandidatesAsync(
            IDatabaseExecutor executor,
            int analysisRunId,
            IList<OptimizationCandidate> candidates,
            Dictionary<string, object>? parameters,
            QueryPerformanceMetrics originalMetrics,
            CancellationToken cancellationToken)
        {
            var results = new List<OptimizationCandidateResult>();

            foreach (var candidate in candidates)
            {
                var candidateId = await _analyzingRepository.CreateOptimizationCandidateAsync(
                    new CreateOptimizationCandidateModel
                    {
                        QueryAnalysisRunId = analysisRunId,
                        RuleCode = candidate.RuleCode,
                        CandidateSql = candidate.CandidateSql,
                        Description = candidate.Description,
                        WasTested = false,
                        IsBest = false
                    },
                    cancellationToken);

                if (!IsSafeSelect(candidate.CandidateSql))
                {
                    results.Add(new OptimizationCandidateResult
                    {
                        CandidateId = candidateId,
                        RuleCode = candidate.RuleCode,
                        CandidateSql = candidate.CandidateSql,
                        Description = candidate.Description,
                        WasTested = false,
                        IsBest = false
                    });

                    continue;
                }

                var candidateMetrics = await executor.AnalyzeAsync(
                    candidate.CandidateSql,
                    parameters,
                    cancellationToken);

                await SaveMetricAsync(
                    analysisRunId,
                    "Candidate",
                    candidateId,
                    candidateMetrics,
                    cancellationToken);

                var improvementPercent = CalculateImprovementPercent(
                    originalMetrics.ClientElapsedMs,
                    candidateMetrics.ClientElapsedMs);

                results.Add(new OptimizationCandidateResult
                {
                    CandidateId = candidateId,
                    RuleCode = candidate.RuleCode,
                    CandidateSql = candidate.CandidateSql,
                    Description = candidate.Description,
                    WasTested = true,
                    IsBest = false,
                    Metrics = candidateMetrics,
                    ImprovementPercent = improvementPercent
                });
            }

            var bestCandidate = results
                .Where(x => x.WasTested && x.ImprovementPercent.HasValue)
                .OrderByDescending(x => x.ImprovementPercent!.Value)
                .FirstOrDefault();

            if (bestCandidate != null && bestCandidate.ImprovementPercent > 0)
            {
                bestCandidate.IsBest = true;
            }

            foreach (var candidateResult in results)
            {
                await _analyzingRepository.UpdateOptimizationCandidateTestResultAsync(
                    candidateResult.CandidateId,
                    candidateResult.WasTested,
                    candidateResult.IsBest,
                    cancellationToken);
            }

            return results;
        }

        private async Task SaveMetricAsync(
            int analysisRunId,
            string variantType,
            int? candidateId,
            QueryPerformanceMetrics metrics,
            CancellationToken cancellationToken)
        {
            await _analyzingRepository.CreateQueryExecutionMetricAsync(
                new CreateQueryExecutionMetricModel
                {
                    QueryAnalysisRunId = analysisRunId,
                    VariantType = variantType,
                    CandidateId = candidateId,

                    ExecutionTimeMs = metrics.ClientElapsedMs,
                    DatabaseElapsedMs = metrics.DatabaseElapsedMs,
                    CpuTimeMs = metrics.CpuTimeMs,
                    LogicalReads = metrics.LogicalReads,
                    PhysicalReads = metrics.PhysicalReads,
                    RowsReturned = metrics.RowsReturned,
                    RowsAffected = metrics.RowsAffected,

                    EstimatedCost = metrics.EstimatedCost,
                    PlanningTimeMs = metrics.PlanningTimeMs,

                    ExecutionPlan = metrics.ExecutionPlan,
                    ExecutionPlanFormat = metrics.ExecutionPlanFormat.HasValue
                        ? (int)metrics.ExecutionPlanFormat.Value
                        : null
                },
                cancellationToken);
        }

        private async Task SaveFindingAsync(
            int analysisRunId,
            QueryOptimizationFinding finding,
            CancellationToken cancellationToken)
        {
            await _analyzingRepository.CreateOptimizationFindingAsync(
                new CreateOptimizationFindingModel
                {
                    QueryAnalysisRunId = analysisRunId,

                    RuleCode = finding.RuleCode,
                    Title = finding.Title,
                    Description = finding.Description,
                    Recommendation = finding.Recommendation,

                    AffectedObject = finding.AffectedObject,
                    AffectedNodeType = finding.AffectedNodeType,

                    SuggestedSql = finding.SuggestedSql,
                    SuggestedIndexSql = finding.SuggestedIndexSql,

                    Severity = (int)finding.Severity,
                    BaseConfidence = finding.Confidence,
                    AdaptiveConfidence = finding.AdaptiveConfidence
                },
                cancellationToken);
        }

        private static bool IsSafeSelect(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql))
                return false;

            var trimmed = sql.TrimStart();

            return trimmed.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase);
        }

        private static double CalculateImprovementPercent(
            double originalExecutionTimeMs,
            double candidateExecutionTimeMs)
        {
            if (originalExecutionTimeMs <= 0)
                return 0;

            return ((originalExecutionTimeMs - candidateExecutionTimeMs) /
                    originalExecutionTimeMs) * 100.0;
        }

        private string CreateConnectionString(ConnectionStringModel model)
        {
            IConnectionStringFactory connectionStringFactory = CreateConnectionStringFactory(model.Provider);
            string connectionString = connectionStringFactory.Create(
                model.ServerName,
                model.UserName,
                model.Password,
                model.DatabaseName,
                model.Host,
                model.Port,
                model.ServiceName
                );

            return connectionString;
        }

        private IConnectionStringFactory CreateConnectionStringFactory(DatabaseTypes provider)
        {
            switch (provider)
            {
                case DatabaseTypes.SqlServer:
                    return new SqlServerConnectionStringFactory();
                case DatabaseTypes.PostgreSql:
                    return new PostgreSqlConnectionStringFactory();
                case DatabaseTypes.Oracle:
                    return new OracleConnectionStringFactory();
                default: throw new NotSupportedDBTypeException();
            }
        }

        private static List<IOptimizationRule> GetOptimizationRules()
        {
            return new List<IOptimizationRule>
            {
                new SelectStarRule(),
                new MissingWhereClauseRule(),
                new LeadingWildcardLikeRule(),
                new FunctionInWhereRule(),
                new FullTableScanRule(),
                new ExpensiveSortRule(),
                new KeyLookupRule(),
                new BadCardinalityEstimatedRule(),
                new ExpensiveNestedLoopRule(),
                new HighLogicalReadsRule(),
                new ImplicitJoinRule(),
                new UnnecessaryJoinRule()
            };
        }
    }
}
