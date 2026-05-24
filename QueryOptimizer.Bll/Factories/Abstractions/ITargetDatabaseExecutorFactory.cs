using QueryOptimizer.DatabaseExecutor.Abstractions;
using QueryOptimizer.Shared.Common.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace QueryOptimizer.Bll.Factories.Abstractions
{
    public interface ITargetDatabaseExecutorFactory
    {
        IDatabaseExecutor Create(DatabaseTypes provider, string connectionString);
    }
}
