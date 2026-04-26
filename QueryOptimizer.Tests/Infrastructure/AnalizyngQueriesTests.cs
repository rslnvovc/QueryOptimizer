using QueryOptimizer.DatabaseExecutor;
using QueryOptimizer.Shared.Common.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace QueryOptimizer.Tests.Infrastructure
{
    public class AnalizyngQueriesTests
    {
        [Fact]
        public async Task DatabaseExecutor_Should_Return_ExecutionPlan()
        {
            string connectionString = "Data Source=localhost\\SQLEXPRESS;Initial Catalog=master;Integrated Security=True;Persist Security Info=False;Pooling=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;";

            string sql = @"select * from [Order Details]";

            DatabaseExecutorFactory.InitDatabase(DatabaseTypes.SqlServer, connectionString);

            var executor = DatabaseExecutorFactory.CreateDbExecutor();

            var plan = await executor.GetExecutionPlanAsync(sql, null, CancellationToken.None);

            File.WriteAllText("C:\\Users\\RuslanVovchok(Ext)\\OneDrive - Tietoevry\\Desktop\\executionplan.txt", plan);

            Assert.NotNull(plan);
        }

        [Fact]
        public async Task DatabaseExecutor_Should_Return_AnalysingDetails()
        {
            string connectionString = "Data Source=localhost\\SQLEXPRESS;Initial Catalog=master;Integrated Security=True;Persist Security Info=False;Pooling=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;";

            string sql = @"select * from [Order Details]";

            DatabaseExecutorFactory.InitDatabase(DatabaseTypes.SqlServer, connectionString);

            var executor = DatabaseExecutorFactory.CreateDbExecutor();

            var result = await executor.AnalyzeAsync(sql, null, CancellationToken.None);
            
            Assert.NotNull(result);
        }
    }
}
