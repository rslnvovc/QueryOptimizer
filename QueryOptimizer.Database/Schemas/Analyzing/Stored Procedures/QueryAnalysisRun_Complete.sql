CREATE PROCEDURE [Analyzing].[QueryAnalysisRun_Complete]
	@Id INT
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE [Analyzing].[QueryAnalysisRuns]
    SET 
        FinishedAt = GETDATE(),
        Status = N'Completed',
        ErrorMessage = NULL
    WHERE Id = @Id;
END