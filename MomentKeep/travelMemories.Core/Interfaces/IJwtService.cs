using System.Security.Claims;
using TravelMemories.Core.Models;

namespace TravelMemories.Core.Interfaces
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