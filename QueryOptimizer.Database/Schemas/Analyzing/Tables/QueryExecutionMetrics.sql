CREATE TABLE [Analyzing].[QueryExecutionMetrics]
(
	Id INT NOT NULL,
    QueryAnalysisRunId INT NOT NULL,
    VariantType NVARCHAR(50) NOT NULL, -- Original / Candidate
    CandidateId INT NULL,

    ExecutionTimeMs FLOAT NULL,
    DatabaseElapsedMs FLOAT NULL,
    CpuTimeMs FLOAT NULL,
    LogicalReads BIGINT NULL,
    PhysicalReads BIGINT NULL,
    RowsReturned BIGINT NULL,
    RowsAffected BIGINT NULL,

    EstimatedCost FLOAT NULL,
    PlanningTimeMs FLOAT NULL,

    ExecutionPlan NVARCHAR(MAX) NULL,
    ExecutionPlanFormat INT NULL,

    CreatedAt DATETIME NOT NULL,

    CONSTRAINT [PK_Analyzing.QueryExecutionMetrics] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT FK_QueryExecutionMetrics_QueryAnalysisRuns
        FOREIGN KEY ([QueryAnalysisRunId])
        REFERENCES [Analyzing].[QueryAnalysisRuns](Id)
)
