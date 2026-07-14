using E_Commerce.Application.Common;
using E_Commerce.Application.Contracts;
using E_Commerce.Application.DTOs.Identity;
using E_Commerce.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_Commerce.Infrastructure.Services
{
    internal class IdentityService : IIdentityService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public IdentityService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }
        public async Task<Result<bool>> CheckPasswordAsync(string email, string password, CancellationToken ct = default)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return Result<bool>.Fail(Error.NotFound("User.NotFound", $"User with Email {email} Is Not Found"));

            return await _userManager.CheckPasswordAsync(user, password);
        }

        public async Task<Result<IdentityUserResult>> CreateUserAsync(RegisterDto registerDto, CancellationToken ct = default)
        {
            var user = new ApplicationUser()
            {
                Email = registerDto.Email,
                UserName = registerDto.UserName,
                DisplayName = registerDto.DisplayName,
                PhoneNumber = registerDto.PhoneNumber
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);
            if(!result.Succeeded)
            {
                var errors = result.Errors.Select(e => new Error(e.Code, e.Description)).ToList();
                return Result<IdentityUserResult>.Fail(errors);
            }
            return Result<IdentityUserResult>.Ok(new IdentityUserResult(user.Id, user.DisplayName, user.Email, user.UserName));
        }

        public async Task<Result<bool>> EmailExistsAsync(string email, CancellationToken ct = default)
            => await _userManager.FindByEmailAsync(email) is not null;

        public async Task<Result<IdentityUserResult>> FindUserByEmailAsync(string email, CancellationToken ct = default)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return Result<IdentityUserResult>.Fail(Error.NotFound("User.NotFound", $"User with Email {email} Is Not Found"));
            return Result<IdentityUserResult>.Ok(new IdentityUserResult(user.Id, user.DisplayName, user.Email, user.UserName));
        }

        public async Task<Result<AddressDto>> GetUserAddressByEmailAsync(string email, CancellationToken ct = default)
        {
            var user = await _userManager.Users.Include(u => u.Address).FirstOrDefaultAsync(u => u.Email == email, ct);
            if (user?.Address == null)
                return Result<AddressDto>.Fail(Error.NotFound("Address.NotFound", $"Address to User with Email {email} is Not Found"));

            var address = user.Address;
            return new AddressDto()
            { 
                City = address.City,
                Street = address.Street,
                Country = address.Country,
                FirstName = address.FirstName,
                LastName = address.LastName
            };

        }

        public async Task<Result<IReadOnlyList<string>>> GetUserRoles(string email, CancellationToken ct = default)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return Error.NotFound("User.NotFound", $"User with Email {email} Is Not Found");

            var roles = await _userManager.GetRolesAsync(user);
            return roles.ToList();
        }

        public async Task<Result<AddressDto>> UpdateOrInsertUserAddressAsync(string email, AddressDto address, CancellationToken ct = default)
        {
            var user = await _userManager.Users.Include(u => u.Address).FirstOrDefaultAsync(u => u.Email == email, ct);

            if(user?.Address == null)
            {
                user.Address = new Address()
                {
                    FirstName = address.FirstName,
                    LastName = address.LastName,
                    Country = address.Country,
                    City = address.City,
                    Street = address.Street
                };
            }
            else
            {
                user.Address.FirstName = address.FirstName;
                user.Address.LastName = address.LastName;
                user.Address.Country = address.Country;
                user.Address.City = address.City;
                user.Address.Street = address.Street;
            }

            var result = await _userManager.UpdateAsync(user);
            if(result.Succeeded)
                return address;

            return Error.Failure("Failure", string.Join(";", result.Errors.Select(e => e.Description)));
        }
    }
}
