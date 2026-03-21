using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Domain.Interfaces.Services
{
    public interface IJwtService
    {
        string GenerateToken(Guid userId, string email, IEnumerable<string> roles);
        bool ValidateToken(string token, out Guid userId);
    }
}