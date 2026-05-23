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

    DECLARE @NewID INT;

    SELECT @NewID = ISNULL(MAX(oc.Id), 0)
    FROM [Analyzing].[OptimizationCandidates] oc WITH(NOLOCK);

    SELECT @NewID;
END