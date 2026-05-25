using QueryOptimizer.Bll.Factories.Abstractions;
using QueryOptimizer.Providers.Oracle.Parsing;
using QueryOptimizer.Providers.PostgreSQL.Parsing;
using QueryOptimizer.Providers.SQLServer.Parsing;
using QueryOptimizer.Shared.Common.Enums;
using QueryOptimizer.Shared.Common.Exceptions.Database;
using QueryOptimizer.Shared.Infrastructure.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace QueryOptimizer.Bll.Factories
{
    public class ExecutionPlanParserFactory : IExecutionPlanParserFactory
    {
        private readonly IEnumerable<IExecutionPlanParser> _parsers;

        public ExecutionPlanParserFactory(IEnumerable<IExecutionPlanParser> parsers)
        { 
            _parsers = GetParsers();
        }

        public IExecutionPlanParser GetParser(DatabaseTypes provider)
        {
            var parser = _parsers.FirstOrDefault(x => x.Provider == provider);

            if (parser is null) throw new NotSupportedDBTypeException();

            return parser;
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
