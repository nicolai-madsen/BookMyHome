using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace BookMyHome.Api.Extensions
{
    public static class ClaimsPrincippalExtensions
    {
        public static Guid GetUserId(this ClaimsPrincipal user)
        {
            var sub = user.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? throw new InvalidOperationException("Token is missing sub-claim");

            return Guid.Parse(sub);
        }
    }
}
