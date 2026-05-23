CREATE PROCEDURE [Analyzing].[OptimizationRuleWeight_Get]
	@Provider INT,
	@RuleCode NVARCHAR(100)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
        Id,
        Provider,
        RuleCode,
        AppliedCount,
        SuccessfulCount,
        AverageImprovementPercent,
        ConfidenceBonus,
        UpdatedAt
    FROM [Analyzing].[OptimizationRuleWeights]
    WHERE Provider = @Provider
      AND RuleCode = @RuleCode;
END