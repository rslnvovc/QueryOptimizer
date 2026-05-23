CREATE PROCEDURE [Analyzing].[OptimizationRuleWeight_Upsert]
	@Provider INT,
    @RuleCode NVARCHAR(100),
    @ImprovementPercent FLOAT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @ExistingId INT;
    DECLARE @AppliedCount INT;
    DECLARE @SuccessfulCount INT;
    DECLARE @AverageImprovementPercent FLOAT;
    DECLARE @NewAppliedCount INT;
    DECLARE @NewSuccessfulCount INT;
    DECLARE @NewAverageImprovementPercent FLOAT;
    DECLARE @ConfidenceBonus FLOAT;
    DECLARE @SuccessRate FLOAT;
	
	BEGIN TRANSACTION;

    SELECT 
        @ExistingId = Id,
        @AppliedCount = AppliedCount,
        @SuccessfulCount = SuccessfulCount,
        @AverageImprovementPercent = AverageImprovementPercent
    FROM [Analyzing].[OptimizationRuleWeights] WITH (UPDLOCK, HOLDLOCK)
    WHERE Provider = @Provider
      AND RuleCode = @RuleCode;

    IF @ExistingId IS NULL
    BEGIN
        SET @NewAppliedCount = 1;
        SET @NewSuccessfulCount = CASE WHEN @ImprovementPercent > 5 THEN 1 ELSE 0 END;
        SET @NewAverageImprovementPercent = @ImprovementPercent;

        SET @SuccessRate = CAST(@NewSuccessfulCount AS FLOAT) / @NewAppliedCount;

        SET @ConfidenceBonus = 
            CASE 
                WHEN (@NewAverageImprovementPercent / 100.0) + (@SuccessRate * 0.10) > 0.35
                    THEN 0.35
                WHEN (@NewAverageImprovementPercent / 100.0) + (@SuccessRate * 0.10) < 0
                    THEN 0
                ELSE (@NewAverageImprovementPercent / 100.0) + (@SuccessRate * 0.10)
            END;

        INSERT INTO [Analyzing].[OptimizationRuleWeights]
        (
            Provider,
            RuleCode,
            AppliedCount,
            SuccessfulCount,
            AverageImprovementPercent,
            ConfidenceBonus,
            UpdatedAt
        )
        VALUES
        (
            @Provider,
            @RuleCode,
            @NewAppliedCount,
            @NewSuccessfulCount,
            @NewAverageImprovementPercent,
            @ConfidenceBonus,
            GETDATE()
        );
    END
    ELSE
    BEGIN
        SET @NewAppliedCount = @AppliedCount + 1;
        SET @NewSuccessfulCount = @SuccessfulCount + CASE WHEN @ImprovementPercent > 5 THEN 1 ELSE 0 END;

        SET @NewAverageImprovementPercent =
            ((@AverageImprovementPercent * @AppliedCount) + @ImprovementPercent) / @NewAppliedCount;

        SET @SuccessRate = CAST(@NewSuccessfulCount AS FLOAT) / @NewAppliedCount;

        SET @ConfidenceBonus = 
            CASE 
                WHEN (@NewAverageImprovementPercent / 100.0) + (@SuccessRate * 0.10) > 0.35
                    THEN 0.35
                WHEN (@NewAverageImprovementPercent / 100.0) + (@SuccessRate * 0.10) < 0
                    THEN 0
                ELSE (@NewAverageImprovementPercent / 100.0) + (@SuccessRate * 0.10)
            END;

        UPDATE [Analyzing].[OptimizationRuleWeights]
        SET
            AppliedCount = @NewAppliedCount,
            SuccessfulCount = @NewSuccessfulCount,
            AverageImprovementPercent = @NewAverageImprovementPercent,
            ConfidenceBonus = @ConfidenceBonus,
            UpdatedAt = GETDATE()
        WHERE Id = @ExistingId;
    END;

	COMMIT TRANSACTION;
END