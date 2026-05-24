using QueryOptimizer.DatabaseExecutor;
using QueryOptimizer.Optimization.Rules;
using QueryOptimizer.Optimization.Services;
using QueryOptimizer.Shared.Common.Enums;
using QueryOptimizer.Shared.Infrastructure.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace QueryOptimizer.Tests.Infrastructure
{
    public class OptimizationRulesTests
    {
        private static List<IOptimizationRule> _optimizationRules = GetOptimizationRules();
        private static OptimizationRuleService _optimizationRuleService = new OptimizationRuleService(_optimizationRules);

        #region SqlServer
        [Fact]
        public async Task DatabaseExecutor_Should_Propose_Some_Recommendations_For_SqlServer()
        {
            string connectionString = "Data Source=localhost\\SQLEXPRESS;Initial Catalog=master;Integrated Security=True;Persist Security Info=False;Pooling=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;";

            string sql = @"SELECT 
    P.ProductName,
    C.CompanyName,
    SUM(OD.Quantity) AS TotalQuantity,
    SUM(OD.Quantity * OD.UnitPrice) AS TotalAmount
FROM Products P, [Order Details] OD, Orders O, Customers C, Categories Ctg
WHERE OD.ProductID = P.ProductID
  AND O.OrderID = OD.OrderID
  AND C.CustomerID = O.CustomerID
  AND Ctg.CategoryID = P.CategoryID
  AND O.OrderDate >= '1997-01-01'
GROUP BY 
    P.ProductName,
    C.CompanyName;";

            DatabaseExecutorFactory.InitDatabase(DatabaseTypes.SqlServer, connectionString);

            var executor = DatabaseExecutorFactory.CreateDbExecutor();

            var analizedModel = await executor.AnalyzeAsync(sql, null, CancellationToken.None);

            var parsedPlanModel = executor.ParseExecutionPlan(analizedModel);

            var rules = _optimizationRuleService.Analyze(parsedPlanModel, sql);

            foreach (var rule in rules)
            {
                Assert.NotNull(rule);
            }
        }
        #endregion

        #region PostgreSqlTests
        [Fact]
        public async Task DatabaseExecutor_Should_Propose_Some_Recommendations_For_PostgreSql()
        {
            string connectionString = "Host=localhost;Port=5432;Database=Northwind;Username=postgres;Password=admin";

            string sql = @"select orDit.*, ord.* 
from order_details as orDit
inner join orders as ord on ordit.order_id = ord.order_id";

            DatabaseExecutorFactory.InitDatabase(DatabaseTypes.PostgreSql, connectionString);

            var executor = DatabaseExecutorFactory.CreateDbExecutor();

            var analizedModel = await executor.AnalyzeAsync(sql, null, CancellationToken.None);

            var parsedPlanModel = executor.ParseExecutionPlan(analizedModel);

            var rules = _optimizationRuleService.Analyze(parsedPlanModel, sql);

            foreach (var rule in rules)
            {
                Assert.NotNull(rule);
            }
        }
        #endregion

        #region OracleTests
        [Fact]
        public async Task DatabaseExecutor_Should_Propose_Some_Recommendations_For_Oracle()
        {
            string connectionString =
                    "User Id=oe;Password=123;" +
                    "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=127.0.0.1)(PORT=1521))" +
                    "(CONNECT_DATA=(SERVICE_NAME=orclpdb)));" +
                    "Connection Timeout=60;Pooling=false;";

            string sql = @"select count(*) from orders";

            DatabaseExecutorFactory.InitDatabase(DatabaseTypes.Oracle, connectionString);

            var executor = DatabaseExecutorFactory.CreateDbExecutor();

            var analizedModel = await executor.AnalyzeAsync(sql, null, CancellationToken.None);

            var parsedPlanModel = executor.ParseExecutionPlan(analizedModel);

            var rules = _optimizationRuleService.Analyze(parsedPlanModel, sql);

            foreach (var rule in rules)
            {
                Assert.NotNull(rule);
            }
        }
        #endregion

        #region Common
        private static List<IOptimizationRule> GetOptimizationRules()
        { 
            return new List<IOptimizationRule>
            {
                new SelectStarRule(),
                new MissingWhereClauseRule(),
                new LeadingWildcardLikeRule(),
                new FunctionInWhereRule(),
                new FullTableScanRule(),
                new ExpensiveSortRule(),
                new KeyLookupRule(),
                new BadCardinalityEstimatedRule(),
                new ExpensiveNestedLoopRule(),
                new HighLogicalReadsRule(),
                new ImplicitJoinRule()
            };
        }
        #endregion
    }
}
