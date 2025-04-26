using MomentKeep.Core.Interfaces;

namespace MomentKeep.Middleware
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;

        public JwtMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IJwtService jwtService)
        {
            var token = context.Request.Headers["Authorization"].ToString()?.Replace("Bearer ", "");

            if (!string.IsNullOrEmpty(token))
            {
                if (jwtService.ValidateToken(token, out var principal))
                {
                    context.User = principal;
                }
            }

            await _next(context);
        }
    }
}