using QueryOptimizer.Models.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace QueryOptimizer.Bll.Services.Abstractions
{
    public interface IUserService
    {
        Task<int> RegisterAsync(User user, CancellationToken cancellationToken = default);

        Task<User> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        Task<User> GetByUsername(string username, CancellationToken cancellationToken = default);
    }
}
