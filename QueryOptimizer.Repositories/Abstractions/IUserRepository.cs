using QueryOptimizer.Models.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace QueryOptimizer.Repositories.Abstractions
{
    public interface IUserRepository
    {
        Task<int> CreateUserAsync(User user, CancellationToken cancellationToken = default);

        Task<User> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        Task<User> GetByUserName(string userName, CancellationToken cancellationToken = default);
    }
}
