CREATE PROCEDURE [Analyzing].[OptimizationCandidate_Create]
    @QueryAnalysisRunId INT,
    @RuleCode NVARCHAR(100),
    @CandidateSql NVARCHAR(MAX),
    @Description NVARCHAR(MAX) = NULL,

    @WasTested BIT = 0,
    @IsBest BIT = 0
AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRANSACTION;

    INSERT INTO [Analyzing].[OptimizationCandidates]
    (
        QueryAnalysisRunId,

        RuleCode,
        CandidateSql,
        [Description],

        WasTested,
        IsBest,

        CreatedAt
    )
    VALUES
    (
        @QueryAnalysisRunId,

        @RuleCode,
        @CandidateSql,
        @Description,

        @WasTested,
        @IsBest,

        GETDATE()
    );

	COMMIT TRANSACTION;
END