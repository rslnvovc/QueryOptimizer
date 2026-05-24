using QueryOptimizer.Bll.Factories.Abstractions;
using QueryOptimizer.DatabaseExecutor;
using QueryOptimizer.DatabaseExecutor.Abstractions;
using QueryOptimizer.Shared.Common.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace QueryOptimizer.Bll.Factories
{
    public class TargetDatabaseExecutorFactory : ITargetDatabaseExecutorFactory
    {
        public IDatabaseExecutor Create(DatabaseTypes provider, string connectionString)
        {
            DatabaseExecutorFactory.InitDatabase(provider, connectionString);

            return DatabaseExecutorFactory.CreateDbExecutor();
        }
    }
}
