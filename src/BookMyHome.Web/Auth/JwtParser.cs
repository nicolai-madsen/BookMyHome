using System.Security.Claims;
using System.Text.Json;

namespace BookMyHome.Web.Auth;

public static class JwtParser
{
    public static List<Claim> ParseClaim(string jwt)
    {
        var payload = jwt.Split(".")[1].Replace("-", "+").Replace("_", "/");
        payload = (payload.Length % 4) switch { 2 => payload + "==", 3 => payload + "=", _ => payload };

        var values = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(Convert.FromBase64String(payload))!;

        var claims = new List<Claim>();
        foreach (var (key, value) in values)
        {
            if (value.ValueKind == JsonValueKind.Array)
                claims.AddRange(value.EnumerateArray().Select(v => new Claim(key, v.ToString())));
            else
                claims.Add(new Claim(key, value.ToString()));
        }
        return claims;
    }

    public static bool IsExpired(IEnumerable<Claim> claims)
    {
        var exp = claims.FirstOrDefault(c => c.Type == "exp")?.Value;
        return exp is not null && DateTimeOffset.FromUnixTimeSeconds(long.Parse(exp)) <= DateTimeOffset.UtcNow;
    }
}