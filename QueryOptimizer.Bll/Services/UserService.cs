using QueryOptimizer.Bll.Services.Abstractions;
using QueryOptimizer.Models.Users;
using QueryOptimizer.Repositories.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace QueryOptimizer.Bll.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        { 
            _userRepository = userRepository;
        }

        public async Task<int> RegisterAsync(User user, CancellationToken cancellationToken = default)
        {
            return await _userRepository.CreateUserAsync(user, cancellationToken);
        }

        public async Task<User> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _userRepository.GetByIdAsync(id, cancellationToken);
        }

        public async Task<User> GetByUsername(string username, CancellationToken cancellationToken = default)
        {
            return await _userRepository.GetByUserName(username, cancellationToken);
        }
    }
}
