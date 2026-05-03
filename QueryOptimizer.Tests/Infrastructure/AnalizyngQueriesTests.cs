using QueryOptimizer.DatabaseExecutor;
using QueryOptimizer.Shared.Common.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace QueryOptimizer.Tests.Infrastructure
{
    public class AnalizyngQueriesTests
    {
        #region SqlServerTests
        [Fact]
        public async Task DatabaseExecutor_Should_Return_ExecutionPlan_For_SqlServer()
        {
            string connectionString = "Data Source=DESKTOP-BACLO5O\\SQLEXPRESS;Initial Catalog=AdventureWorks2022;Integrated Security=True;Persist Security Info=False;Pooling=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;";

            string sql = @"select * from Production.Product";

            DatabaseExecutorFactory.InitDatabase(DatabaseTypes.SqlServer, connectionString);

            var executor = DatabaseExecutorFactory.CreateDbExecutor();

            var plan = await executor.GetExecutionPlanAsync(sql, null, CancellationToken.None);

            Assert.NotNull(plan);
        }

        [Fact]
        public async Task DatabaseExecutor_Should_Return_AnalysingDetails_For_SqlServer()
        {
            string connectionString = "Data Source=localhost\\SQLEXPRESS;Initial Catalog=master;Integrated Security=True;Persist Security Info=False;Pooling=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;";

            string sql = @"select * from [Order Details]";

            DatabaseExecutorFactory.InitDatabase(DatabaseTypes.SqlServer, connectionString);

            var executor = DatabaseExecutorFactory.CreateDbExecutor();

            var result = await executor.AnalyzeAsync(sql, null, CancellationToken.None);

            Assert.NotNull(result);
        }
        #endregion

        #region PostgreSqlTests
        [Fact]
        public async Task DatabaseExecutor_Should_Return_ExecutionPlan_For_Postgres()
        { 
            string connectionString = "Host=localhost;Port=5432;Database=northwind;Username=postgres;Password=panda13yu7";

            string sql = @"select * from order_details";

            DatabaseExecutorFactory.InitDatabase(DatabaseTypes.PostgreSql, connectionString);

            var executor = DatabaseExecutorFactory.CreateDbExecutor();

            var result = await executor.GetExecutionPlanAsync(sql, null, CancellationToken.None);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task DatabaseExecutor_Should_Return_AnalysingDetails_For_Postgres()
        {
            string connectionString = "Host=localhost;Port=5432;Database=northwind;Username=postgres;Password=panda13yu7";

            string sql = @"select * from order_details";

            DatabaseExecutorFactory.InitDatabase(DatabaseTypes.PostgreSql, connectionString);

            var executor = DatabaseExecutorFactory.CreateDbExecutor();

            var result = await executor.AnalyzeAsync(sql, null, CancellationToken.None);

            Assert.NotNull(result);
        }
        #endregion

        #region OracleTests
        [Fact]
        public async Task DatabaseExecutor_Should_Return_ExecutionPlan_For_Oracle()
        {
            string connectionString =
                "User Id=oe;Password=123;" +
                "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=127.0.0.1)(PORT=1521))" +
                "(CONNECT_DATA=(SERVICE_NAME=orclpdb)));" +
                "Connection Timeout=60;Pooling=false;";

            string sql = @"select count(*) from orders";

            DatabaseExecutorFactory.InitDatabase(DatabaseTypes.Oracle, connectionString);

            var executor = DatabaseExecutorFactory.CreateDbExecutor();

            var result = await executor.GetExecutionPlanAsync(sql, null, CancellationToken.None);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task DatabaseExecutor_Should_Return_AnalysingDetails_For_Oracle()
        {
            string connectionString =
                "User Id=oe;Password=123;" +
                "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=127.0.0.1)(PORT=1521))" +
                "(CONNECT_DATA=(SERVICE_NAME=orclpdb)));" +
                "Connection Timeout=60;Pooling=false;";

            string sql = @"select count(*) from orders";

            DatabaseExecutorFactory.InitDatabase(DatabaseTypes.Oracle, connectionString);

            var executor = DatabaseExecutorFactory.CreateDbExecutor();

            var result = await executor.AnalyzeAsync(sql, null, CancellationToken.None);

            Assert.NotNull(result);
        }
        #endregion
    }
}
