using QueryOptimizer.Shared.Common.Enums;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;

namespace QueryOptimizer.DatabaseExecutor.Abstractions
{
    public interface IDatabaseSource : IDisposable
    {
        DatabaseTypes DbType { get; }

        Task<DbCommand> GetCommandAsync(string commandText);

        Task<DbCommand> GetCommandAsync(string commandText, CommandType commandType);
    }
}
