CREATE PROCEDURE [Analyzing].[QueryAnalysisRun_GetFull]
    @QueryAnalysisRunId INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT 
        Id,
        UserId,
        Provider,
        OriginalSql,
        NormalizedSqlHash,
        StartedAt,
        FinishedAt,
        Status,
        ErrorMessage
    FROM [Analyzing].[QueryAnalysisRuns]
    WHERE Id = @QueryAnalysisRunId;

    SELECT
        Id,
        QueryAnalysisRunId,
        VariantType,
        CandidateId,

        ExecutionTimeMs,
        DatabaseElapsedMs,
        CpuTimeMs,
        LogicalReads,
        PhysicalReads,
        RowsReturned,
        RowsAffected,

        EstimatedCost,
        PlanningTimeMs,

        ExecutionPlan,
        ExecutionPlanFormat,

        CreatedAt
    FROM [Analyzing].[QueryExecutionMetrics]
    WHERE QueryAnalysisRunId = @QueryAnalysisRunId
    ORDER BY CreatedAt ASC;

    SELECT
        Id,
        QueryAnalysisRunId,

        RuleCode,
        Title,
        [Description],
        Recommendation,

        AffectedObject,
        AffectedNodeType,

        SuggestedSql,
        SuggestedIndexSql,

        Severity,
        BaseConfidence,
        AdaptiveConfidence,

        CreatedAt
    FROM [Analyzing].[OptimizationFindings]
    WHERE QueryAnalysisRunId = @QueryAnalysisRunId
    ORDER BY Severity DESC, AdaptiveConfidence DESC;

    SELECT
        Id,
        QueryAnalysisRunId,

        RuleCode,
        CandidateSql,
        [Description],

        WasTested,
        IsBest,

        CreatedAt
    FROM [Analyzing].[OptimizationCandidates]
    WHERE QueryAnalysisRunId = @QueryAnalysisRunId
    ORDER BY IsBest DESC, CreatedAt ASC;

    SELECT
        e.Id,
        e.Provider,
        e.NormalizedSqlHash,
        e.RuleCode,

        e.OriginalExecutionTimeMs,
        e.CandidateExecutionTimeMs,
        e.ImprovementPercent,

        e.OriginalLogicalReads,
        e.CandidateLogicalReads,
        e.LogicalReadsImprovementPercent,

        e.CreatedAt
    FROM [Analyzing].[OptimizationExperiences] e
    INNER JOIN [Analyzing].[QueryAnalysisRuns] r
        ON r.NormalizedSqlHash = e.NormalizedSqlHash
       AND r.Provider = e.Provider
    WHERE r.Id = @QueryAnalysisRunId
    ORDER BY e.CreatedAt DESC;
END