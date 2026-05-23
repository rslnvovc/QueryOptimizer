CREATE TABLE [Analyzing].[OptimizationFindings]
(
	Id INT NOT NULL PRIMARY KEY,
    QueryAnalysisRunId INT NOT NULL,

    RuleCode NVARCHAR(100) NOT NULL,
    Title NVARCHAR(300) NOT NULL,
    [Description] NVARCHAR(MAX) NOT NULL,
    Recommendation NVARCHAR(MAX) NOT NULL,

    AffectedObject NVARCHAR(300) NULL,
    AffectedNodeType NVARCHAR(200) NULL,

    SuggestedSql NVARCHAR(MAX) NULL,
    SuggestedIndexSql NVARCHAR(MAX) NULL,

    Severity INT NOT NULL,
    BaseConfidence FLOAT NOT NULL,
    AdaptiveConfidence FLOAT NOT NULL,

    CreatedAt DATETIME NOT NULL,

    CONSTRAINT [PK_Analyzing.OptimizationFindings] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT FK_OptimizationFindings_QueryAnalysisRuns
        FOREIGN KEY (QueryAnalysisRunId)
        REFERENCES [Analyzing].[QueryAnalysisRuns](Id)
)