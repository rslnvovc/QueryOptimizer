using QueryOptimizer.Shared.Common.Enums;
using QueryOptimizer.Shared.Infrastructure.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace QueryOptimizer.Bll.Factories.Abstractions
{
    public interface IExecutionPlanParserFactory
    {
        IExecutionPlanParser GetParser(DatabaseTypes provider);
    }
}
