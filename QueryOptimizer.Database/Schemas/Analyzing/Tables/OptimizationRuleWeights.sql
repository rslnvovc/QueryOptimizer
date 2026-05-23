CREATE TABLE [Analyzing].[OptimizationRuleWeights]
(
	[Id] INT NOT NULL PRIMARY KEY,

	[Provider] INT NOT NULL,
    RuleCode NVARCHAR(100) NOT NULL,

    AppliedCount INT NOT NULL,
    SuccessfulCount INT NOT NULL,
    AverageImprovementPercent FLOAT NOT NULL,
    ConfidenceBonus FLOAT NOT NULL,

    UpdatedAt DATETIME NOT NULL,

    CONSTRAINT [PK_Analyzing.OptimizationRuleWeights] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT UQ_OptimizationRuleWeights_Provider_RuleCode
        UNIQUE (Provider, RuleCode)
)
