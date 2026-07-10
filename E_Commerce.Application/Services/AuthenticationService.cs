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

        public AuthenticationService(IIdentityService identityService)
        {
            _identityService = identityService;
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

            //Return Result + UserDto
            return Result<UserDto>.Ok(new UserDto()
            { 
                Email = loginDto.Email,
                DisplayName = userResult.data.DisplayName,
                Token = "Token"
            });

        }
    }
}
