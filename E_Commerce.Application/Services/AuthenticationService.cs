using E_Commerce.Application.Common;
using E_Commerce.Application.Contracts;
using E_Commerce.Application.DTOs.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_Commerce.Application.Services
{
    internal class AuthenticationService : IAuthenticationService
    {
        private readonly IIdentityService _identityService;
        private readonly ITokenService _tokenService;

        public AuthenticationService(IIdentityService identityService, ITokenService tokenService)
        {
            _identityService = identityService;
            _tokenService = tokenService;
        }

        public async Task<Result<bool>> CheckEmailExistsAsync(string email, CancellationToken ct = default)
            => await _identityService.EmailExistsAsync(email, ct);

        public async Task<Result<UserDto>> GetCurrentUserAsync(string email, CancellationToken ct = default)
        {
            var userResult = await _identityService.FindUserByEmailAsync(email, ct);
            var rolesResult = await _identityService.GetUserRoles(email, ct);
            var user = userResult.data;

            var token = _tokenService.CreateToken(user.Id, user.Email, user.UserName, rolesResult.data);
            return new UserDto() { DisplayName = user.DisplayName, Email = user.Email, Token = token };
        }

        public async Task<Result<UserDto>> LoginAsync(LoginDto loginDto, CancellationToken ct = default)
        {
            //Check User By Email
            var userResult = await _identityService.FindUserByEmailAsync(loginDto.Email, ct);
            if (!userResult.IsSucces)
                return Result<UserDto>.Fail(userResult.Errors);

            //Check Password
            var passwordResult = await _identityService.CheckPasswordAsync(loginDto.Email, loginDto.Password, ct);
            if(!passwordResult.IsSucces)
                return Result<UserDto>.Fail(userResult.Errors);
            if (!passwordResult.data)
                return Result<UserDto>.Fail(Error.Unauthorized("User.Unauthorized", "Not Valid Email or Password"));

            var user = userResult.data;
            var rolesResult = await _identityService.GetUserRoles(user.Email);
            var roles = rolesResult.data;
            var token = _tokenService.CreateToken(user.Id, user.Email, user.UserName, roles);

            //Return Result + UserDto
            return Result<UserDto>.Ok(new UserDto()
            { 
                Email = loginDto.Email,
                DisplayName = userResult.data.DisplayName,
                Token = token
            });

        }

        public async Task<Result<UserDto>> RegisterAsync(RegisterDto registerDto, CancellationToken ct = default)
        {
            var result = await _identityService.CreateUserAsync(registerDto, ct);
            if(!result.IsSucces)
            {
                return Result<UserDto>.Fail(result.Errors);
            }
            var user = result.data;
            var rolesResult = await _identityService.GetUserRoles(user.Email);
            var roles = rolesResult.data;
            var token = _tokenService.CreateToken(user.Id, user.Email, user.UserName, roles);
            return Result<UserDto>.Ok(new UserDto()
            {
                DisplayName = user.DisplayName,
                Email = user.Email,
                Token = token
            });
        }
    }
}
