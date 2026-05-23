CREATE PROCEDURE [Analyzing].[OptimizationRuleWeights_GetByProvider]
	@Provider INT
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
    ORDER BY ConfidenceBonus DESC, AverageImprovementPercent DESC;
END