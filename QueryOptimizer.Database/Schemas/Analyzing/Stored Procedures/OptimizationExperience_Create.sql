CREATE PROCEDURE [Analyzing].[OptimizationExperience_Create]
	@Provider INT,
    @NormalizedSqlHash NVARCHAR(128),
    @RuleCode NVARCHAR(100),

    @OriginalExecutionTimeMs FLOAT,
    @CandidateExecutionTimeMs FLOAT,
    @ImprovementPercent FLOAT,

    @OriginalLogicalReads BIGINT = NULL,
    @CandidateLogicalReads BIGINT = NULL,
    @LogicalReadsImprovementPercent FLOAT = NULL
AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRANSACTION;

    INSERT INTO [Analyzing].[OptimizationExperiences]
    (
        Provider,
        NormalizedSqlHash,
        RuleCode,

        OriginalExecutionTimeMs,
        CandidateExecutionTimeMs,
        ImprovementPercent,

        OriginalLogicalReads,
        CandidateLogicalReads,
        LogicalReadsImprovementPercent,

        CreatedAt
    )
    VALUES
    (
        @Provider,
        @NormalizedSqlHash,
        @RuleCode,

        @OriginalExecutionTimeMs,
        @CandidateExecutionTimeMs,
        @ImprovementPercent,

        @OriginalLogicalReads,
        @CandidateLogicalReads,
        @LogicalReadsImprovementPercent,

        GETDATE()
    );

	COMMIT TRANSACTION;
END