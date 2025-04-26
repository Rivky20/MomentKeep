using System.Security.Claims;
using MomentKeep.Core.Models;

namespace MomentKeep.Core.Interfaces
{
    public interface IJwtService
    {
        string GenerateJwtToken(User user);
        bool ValidateToken(string token, out ClaimsPrincipal claimsPrincipal);
        Guid GetUserIdFromToken(string token);
        IEnumerable<Claim> GetClaimsFromToken(string token);
        bool IsTokenExpired(string token);
    }
}