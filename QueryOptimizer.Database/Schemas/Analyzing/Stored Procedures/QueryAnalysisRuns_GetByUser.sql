CREATE PROCEDURE [Analyzing].[QueryAnalysisRuns_GetByUser]
	@UserId INT
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
    WHERE UserId = @UserId
    ORDER BY StartedAt DESC;
END