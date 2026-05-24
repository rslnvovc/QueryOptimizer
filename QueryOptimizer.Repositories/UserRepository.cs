using QueryOptimizer.DatabaseExecutor;
using QueryOptimizer.DatabaseExecutor.Abstractions;
using QueryOptimizer.Models.Users;
using QueryOptimizer.Repositories.Abstractions;
using QueryOptimizer.Shared.Common.Enums;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace QueryOptimizer.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly string _applicationConnectionString;

        public UserRepository(string applicationConnectionString)
        { 
            _applicationConnectionString = applicationConnectionString;
        }

        public async Task<int> CreateUserAsync(User user, CancellationToken cancellationToken = default)
        {
            var parameters = new Dictionary<string, object>
            {
                ["UserName"] = user.Username
            };

            using var executor = CreateExecutor();
            var result = await executor.ExecuteScalarAsync<int>(
                "[User].[User_Create]",
                parameters,
                CommandType.StoredProcedure,
                cancellationToken
                );

            return result;
        }

        public async Task<User> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var parameters = new Dictionary<string, object>
            {
                ["Id"] = id
            };

            using var executor = CreateExecutor();
            var result = await executor.ExecuteQueryAsync<User>(
                "[User].[User_GetById]",
                parameters,
                CommandType.StoredProcedure,
                cancellationToken
                );

            return result.FirstOrDefault()!;
        }

        public async Task<User> GetByUserName(string userName, CancellationToken cancellationToken = default)
        {
            var parameters = new Dictionary<string, object>
            {
                ["UserName"] = userName
            };

            using var executor = CreateExecutor();
            var result = await executor.ExecuteQueryAsync<User>(
                "[User].[User_GetByLogin]",
                parameters,
                CommandType.StoredProcedure,
                cancellationToken
                );

            return result.FirstOrDefault()!;
        }

        private IDatabaseExecutor CreateExecutor()
        {
            return DatabaseExecutorFactory.CreateDbExecutor(
                DatabaseTypes.SqlServer,
                _applicationConnectionString);
        }
    }
}
