using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Interfaces.Services;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public IdentityService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<(bool Success, string? Error)> RegisterAsync(
        string email, string password, Guid domainUserId)
        {
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                DomainUserId = domainUserId
            };

            var result = await _userManager.CreateAsync(user, password);

            return result.Succeeded
                ? (true, null)
                : (false, result.Errors.First().Description);
        }

        public async Task<(bool Success, string? Error)> ChangePasswordAsync(
            string email, string currentPassword, string newPassword)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user is null) return (false, "User not found.");

            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);

            return result.Succeeded
                ? (true, null)
                : (false, result.Errors.First().Description);
        }

        public async Task<bool> CheckPasswordAsync(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user is null) return false;

            return await _userManager.CheckPasswordAsync(user, password);
        }

        public async Task<bool> DeleteAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user is null) return false;

            var result = await _userManager.DeleteAsync(user);
            return result.Succeeded;
        }
    }
}