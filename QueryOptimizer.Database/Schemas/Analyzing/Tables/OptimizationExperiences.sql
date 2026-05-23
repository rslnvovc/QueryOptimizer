CREATE TABLE [Analyzing].[OptimizationExperiences]
(
	[Id] INT NOT NULL,
	[Provider] INT NOT NULL,
    NormalizedSqlHash NVARCHAR(128) NOT NULL,
    RuleCode NVARCHAR(100) NOT NULL,

    OriginalExecutionTimeMs FLOAT NOT NULL,
    CandidateExecutionTimeMs FLOAT NOT NULL,
    ImprovementPercent FLOAT NOT NULL,

    OriginalLogicalReads BIGINT NULL,
    CandidateLogicalReads BIGINT NULL,
    LogicalReadsImprovementPercent FLOAT NULL,

    CreatedAt DATETIME NOT NULL

    CONSTRAINT [PK_Analyzing.OptimizationExperiences] PRIMARY KEY CLUSTERED ([Id] ASC)
)
