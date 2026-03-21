using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Domain.Interfaces.Services
{
    public interface IIdentityService
    {
        Task<(bool Success, string? Error)> RegisterAsync(string email, string password, Guid domainUserId);
        Task<(bool Success, string? Error)> ChangePasswordAsync(string email, string currentPassword, string newPassword);
        Task<bool> CheckPasswordAsync(string email, string password);
        Task<bool> DeleteAsync(string email);
    }
}