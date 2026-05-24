using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using QueryOptimizer.Bll.Services.Abstractions;
using QueryOptimizer.Models.Users;

namespace QueryOptimizer.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        { 
            _userService = userService;
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromBody]User user, CancellationToken cancellationToken)
        {
            return Ok(await _userService.RegisterAsync(user,cancellationToken));
        }

        [HttpGet("{id}/id")]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        {
            return Ok(await _userService.GetByIdAsync(id, cancellationToken));
        }

        [HttpGet("{userName}/userName")]
        public async Task<IActionResult> GetByUserName(string userName, CancellationToken cancellationToken)
        {
            return Ok(await _userService.GetByUsername(userName, cancellationToken));
        }
    }
}
