CREATE PROCEDURE [Analyzing].[OptimizationFinding_Create]
	@QueryAnalysisRunId INT,

    @RuleCode NVARCHAR(100),
    @Title NVARCHAR(300),
    @Description NVARCHAR(MAX),
    @Recommendation NVARCHAR(MAX),

    @AffectedObject NVARCHAR(300) = NULL,
    @AffectedNodeType NVARCHAR(200) = NULL,

    @SuggestedSql NVARCHAR(MAX) = NULL,
    @SuggestedIndexSql NVARCHAR(MAX) = NULL,

    @Severity INT,
    @BaseConfidence FLOAT,
    @AdaptiveConfidence FLOAT
AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRANSACTION;

    INSERT INTO [Analyzing].[OptimizationFindings]
    (
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
    )
    VALUES
    (
        @QueryAnalysisRunId,

        @RuleCode,
        @Title,
        @Description,
        @Recommendation,

        @AffectedObject,
        @AffectedNodeType,

        @SuggestedSql,
        @SuggestedIndexSql,

        @Severity,
        @BaseConfidence,
        @AdaptiveConfidence,

        GETDATE()
    );

	COMMIT TRANSACTION;

    DECLARE @NewID INT;

    SELECT @NewID = ISNULL(MAX(ofi.Id), 0)
    FROM [Analyzing].[OptimizationFindings] ofi WITH(NOLOCK);

    SELECT @NewID;
END