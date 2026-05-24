using Npgsql.Replication;
using QueryOptimizer.Bll.Factories;
using QueryOptimizer.Bll.Factories.Abstractions;
using QueryOptimizer.Bll.Services;
using QueryOptimizer.Bll.Services.Abstractions;
using QueryOptimizer.DatabaseExecutor;
using QueryOptimizer.Models.Application;
using QueryOptimizer.Optimization.Services;
using QueryOptimizer.Optimization.Services.Abstractions;
using QueryOptimizer.Providers.Oracle.Parsing;
using QueryOptimizer.Providers.PostgreSQL.Parsing;
using QueryOptimizer.Providers.SQLServer.Parsing;
using QueryOptimizer.Repositories;
using QueryOptimizer.Repositories.Abstractions;
using QueryOptimizer.Shared.Common.Enums;
using QueryOptimizer.Shared.Infrastructure.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace QueryOptimizer.Tests.Bll
{
    public class QueryOptimizationWorkflowServiceTests
    {
        [Fact]
        public async Task QueryOptimizationWorkflowService_Should_Save_And_Return_OptimizationFindings()
        {
            string applicationConnectionString = "Data Source=localhost\\SQLEXPRESS;Initial Catalog=QueryOptimizer;Integrated Security=True;Persist Security Info=False;Pooling=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;";
            string testingDBConnectionString = "Data Source=localhost\\SQLEXPRESS;Initial Catalog=master;Integrated Security=True;Persist Security Info=False;Pooling=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;";

            string sql = @"SELECT ProductName, Total=SUM(Quantity)
FROM Products P, [Order Details] OD, Orders O, Customers C
WHERE C.CustomerID = O.CustomerID AND O.OrderID = OD.OrderID AND OD.ProductID = P.ProductID
GROUP BY ProductName";

            DatabaseExecutorFactory.InitDatabase(DatabaseTypes.SqlServer, applicationConnectionString);

            ITargetDatabaseExecutorFactory targetDatabaseExecutorFactory = new TargetDatabaseExecutorFactory();
            IExecutionPlanParserFactory executionPlanParserFactory = new ExecutionPlanParserFactory(GetParsers());
            IOptimizationCandidateGenerator candidateGenerator = new OptimizationCandidateGenerator();
            IAnalyzingRepository analyzingRepository = new AnalyzingRepository();
            IAdaptiveLearningService adaptiveLearningService = new AdaptiveLearningService(analyzingRepository);
            IQueryOptimizationWorkflowService workflowService = new QueryOptimizationWorkflowService(
                targetDatabaseExecutorFactory,
                executionPlanParserFactory,
                candidateGenerator,
                adaptiveLearningService,
                analyzingRepository
                );

            var request = new QueryOptimizationRequest()
            {
                UserId = 1, /*Admin*/
                Provider = DatabaseTypes.SqlServer,
                ConnectionString = testingDBConnectionString,
                Sql = sql
            };

            var result = workflowService.AnalyzeAsync(request, CancellationToken.None);

            Assert.NotNull(result);
        }

        private static List<IExecutionPlanParser> GetParsers()
        {
            return new List<IExecutionPlanParser>()
            { 
                new SqlServerExecutionPlanParser(),
                new PostgresExecutionPlanParser(),
                new OracleExecutionPlanParser()
            };
        }
    }
}
