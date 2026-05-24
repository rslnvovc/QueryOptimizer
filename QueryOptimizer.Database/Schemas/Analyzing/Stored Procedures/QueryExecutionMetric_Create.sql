CREATE PROCEDURE [Analyzing].[QueryExecutionMetric_Create]
	@QueryAnalysisRunId INT,
    @VariantType NVARCHAR(50),
    @CandidateId INT = NULL,

    @ExecutionTimeMs FLOAT = NULL,
    @DatabaseElapsedMs FLOAT = NULL,
    @CpuTimeMs FLOAT = NULL,
    @LogicalReads BIGINT = NULL,
    @PhysicalReads BIGINT = NULL,
    @RowsReturned BIGINT = NULL,
    @RowsAffected BIGINT = NULL,

    @EstimatedCost FLOAT = NULL,
    @PlanningTimeMs FLOAT = NULL,

    @ExecutionPlan NVARCHAR(MAX) = NULL,
    @ExecutionPlanFormat INT = NULL
AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRANSACTION;

    INSERT INTO [Analyzing].[QueryExecutionMetrics]
    (
        QueryAnalysisRunId,
        VariantType,
        CandidateId,

        ExecutionTimeMs,
        DatabaseElapsedMs,
        CpuTimeMs,
        LogicalReads,
        PhysicalReads,
        RowsReturned,
        RowsAffected,

        EstimatedCost,
        PlanningTimeMs,

        ExecutionPlan,
        ExecutionPlanFormat,

        CreatedAt
    )
    VALUES
    (
        @QueryAnalysisRunId,
        @VariantType,
        @CandidateId,

        @ExecutionTimeMs,
        @DatabaseElapsedMs,
        @CpuTimeMs,
        @LogicalReads,
        @PhysicalReads,
        @RowsReturned,
        @RowsAffected,

        @EstimatedCost,
        @PlanningTimeMs,

        @ExecutionPlan,
        @ExecutionPlanFormat,

        GETDATE()
    );


	COMMIT TRANSACTION;

    DECLARE @NewID INT;

    SELECT @NewID = ISNULL(MAX(qem.Id), 0)
    FROM [Analyzing].[QueryExecutionMetrics] qem WITH(NOLOCK);

    SELECT @NewID;
END