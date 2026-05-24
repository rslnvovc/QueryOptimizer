using QueryOptimizer.DatabaseExecutor;
using QueryOptimizer.Repositories;
using QueryOptimizer.Repositories.Abstractions;
using QueryOptimizer.Shared.Common.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace QueryOptimizer.Tests.Repositories
{
    public class AnalyzingRepositoryTests
    {
        [Fact]
        public async Task DatabaseExecutory_Should_Return_Data()
        {
            string connectionString = "Data Source=localhost\\SQLEXPRESS;Initial Catalog=QueryOptimizer;Integrated Security=True;Persist Security Info=False;Pooling=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;";

            DatabaseExecutorFactory.InitDatabase(DatabaseTypes.SqlServer, connectionString);

            IAnalyzingRepository analyzingRepository = new AnalyzingRepository(connectionString);

            var result = await analyzingRepository.GetOptimizationRuleWeightsByProviderAsync(Convert.ToInt32(DatabaseTypes.SqlServer), CancellationToken.None);

            Assert.NotNull(result);
        }
    }
}
