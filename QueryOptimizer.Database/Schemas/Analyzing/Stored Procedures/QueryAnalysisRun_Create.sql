CREATE PROCEDURE [Analyzing].[QueryAnalysisRun_Create]
	@UserId INT,
    @Provider INT,
    @OriginalSql NVARCHAR(MAX),
    @NormalizedSqlHash NVARCHAR(128)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRANSACTION;

    INSERT INTO [Analyzing].[QueryAnalysisRuns]
    (
        UserId,
        Provider,
        OriginalSql,
        NormalizedSqlHash,
        StartedAt,
        FinishedAt,
        Status,
        ErrorMessage
    )
    VALUES
    (
        @UserId,
        @Provider,
        @OriginalSql,
        @NormalizedSqlHash,
        GETDATE(),
        NULL,
        N'Running',
        NULL
    );

    COMMIT TRANSACTION;
END
