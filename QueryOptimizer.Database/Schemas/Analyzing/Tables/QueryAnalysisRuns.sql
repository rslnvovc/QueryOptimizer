CREATE TABLE [Analyzing].[QueryAnalysisRuns]
(
    Id INT NOT NULL,
    UserId INT NOT NULL,
    Provider INT NOT NULL,
    OriginalSql NVARCHAR(MAX) NOT NULL,
    NormalizedSqlHash NVARCHAR(128) NOT NULL,
    StartedAt DATETIME2 NOT NULL,
    FinishedAt DATETIME2 NULL,
    Status NVARCHAR(50) NOT NULL,
    ErrorMessage NVARCHAR(MAX) NULL

    CONSTRAINT [PK_Analyzing.QueryAnalysisRuns] PRIMARY KEY CLUSTERED([Id] ASC),
    CONSTRAINT [FK_Analyzing.QueryAnalysisRuns_User] FOREIGN KEY ([UserId]) REFERENCES [User].[User]([Id])
);