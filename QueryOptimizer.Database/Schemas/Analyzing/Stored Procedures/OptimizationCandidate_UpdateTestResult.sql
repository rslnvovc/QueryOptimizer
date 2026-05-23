CREATE PROCEDURE [Analyzing].[OptimizationCandidate_UpdateTestResult]
    @Id INT,
    @WasTested BIT,
    @IsBest BIT
AS
BEGIN
	SET NOCOUNT ON;

    UPDATE [Analyzing].[OptimizationCandidates]
    SET 
        WasTested = @WasTested,
        IsBest = @IsBest
    WHERE Id = @Id;
END