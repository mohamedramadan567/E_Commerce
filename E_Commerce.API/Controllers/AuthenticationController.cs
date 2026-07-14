using E_Commerce.Application.Contracts;
using E_Commerce.Application.DTOs.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce.API.Controllers
{
    public class AuthenticationController : ApiBaseController
    {
        private readonly IAuthenticationService _authenticationService;

        public AuthenticationController(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        // Login
        [HttpPost("Login")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto, CancellationToken cancellationToken)
            => ToActionResult(await _authenticationService.LoginAsync(loginDto, cancellationToken));

        [HttpPost("Register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto, CancellationToken cancellationToken)
            => ToActionResult(await _authenticationService.RegisterAsync(registerDto, cancellationToken));

        //Check Email Exists
        [HttpGet("emailexists")]
        public async Task<ActionResult<bool>> CheckEmail(string email, CancellationToken ct)
            => ToActionResult(await _authenticationService.CheckEmailExistsAsync(email, ct));


        //Get Current User
        //Get Current User Address
        //Update Current User Address
    }
}
