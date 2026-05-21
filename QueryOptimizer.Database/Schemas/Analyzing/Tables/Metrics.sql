CREATE TABLE [Analyzing].[Metrics]
(
	[MetricsId] NVARCHAR(36) NOT NULL,
	[UserId] NVARCHAR(36) NOT NULL,
	[CreationDate] DATETIME NOT NULL,
	[Provider] NVARCHAR(255) NOT NULL,
	[OriginalSql] NVARCHAR(MAX) NOT NULL,
	[ClientElapsedMs] INT NULL,
	[DatabaseElapsedMs] FLOAT NULL,
	[CpuTimeMs] FLOAT NULL,
	[LogicalReads] INT NULL,
	[PhysicalReads] INT NULL,
	[RowsReturned] INT NULL,
	[RowsAffected] INT NULL,
	[PlanningTimeMs] FLOAT NULL,
	[RequestedMemoryKb] INT NULL,
	[GrantedMemoryKb] FLOAT NULL,
	[UsedMemoryKb] INT NULL,
	[ExecutionPlan] NVARCHAR(MAX) NULL

	CONSTRAINT [PK_Analyzing.Metrics] PRIMARY KEY CLUSTERED ([MetricsId] ASC),
	CONSTRAINT [FK_Analyzing.Metrics_User] FOREIGN KEY ([UserId]) REFERENCES [User].[User]([UserId])
)
