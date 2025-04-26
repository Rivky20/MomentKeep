using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using MomentKeep.Core.Interfaces;
using MomentKeep.Core.Models;
using MomentKeep.Service.Helpers;

namespace MomentKeep.Service.Services
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateJwtToken(User user)
        {
            return _configuration.GenerateJwtToken(user);
        }

        public bool ValidateToken(string token, out ClaimsPrincipal claimsPrincipal)
        {
            return _configuration.ValidateToken(token, out claimsPrincipal);
        }

        public Guid GetUserIdFromToken(string token)
        {
            if (ValidateToken(token, out var principal))
            {
                return principal.GetUserIdFromToken();
            }

            return Guid.Empty;
        }

        public IEnumerable<Claim> GetClaimsFromToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;

                return jwtToken?.Claims ?? Enumerable.Empty<Claim>();
            }
            catch
            {
                return Enumerable.Empty<Claim>();
            }
        }

        public bool IsTokenExpired(string token)
        {
            try
            {
                var expiration = token.GetTokenExpirationTime();
                return expiration < DateTime.UtcNow;
            }
            catch
            {
                return true;
            }
        }
    }
}