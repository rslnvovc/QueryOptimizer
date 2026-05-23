CREATE TABLE [Analyzing].[OptimizationCandidates]
(
	[Id] INT NOT NULL PRIMARY KEY,
	QueryAnalysisRunId INT NOT NULL,

    RuleCode NVARCHAR(100) NOT NULL,
    CandidateSql NVARCHAR(MAX) NOT NULL,
    [Description] NVARCHAR(MAX) NULL,

    WasTested BIT NOT NULL DEFAULT 0,
    IsBest BIT NOT NULL DEFAULT 0,

    CreatedAt DATETIME NOT NULL,

    CONSTRAINT [PK_Analyzing.OptimizationCandidates] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT FK_OptimizationCandidates_QueryAnalysisRuns
        FOREIGN KEY (QueryAnalysisRunId)
        REFERENCES [Analyzing].[QueryAnalysisRuns](Id)
)
