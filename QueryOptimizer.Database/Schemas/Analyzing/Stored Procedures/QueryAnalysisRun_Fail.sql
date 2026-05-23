CREATE PROCEDURE [Analyzing].[QueryAnalysisRun_Fail]
	@Id INT,
	@ErrorMessage NVARCHAR(MAX)
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE [Analyzing].[QueryAnalysisRuns]
    SET 
        FinishedAt = GETDATE(),
        Status = N'Failed',
        ErrorMessage = @ErrorMessage
    WHERE Id = @Id;
END