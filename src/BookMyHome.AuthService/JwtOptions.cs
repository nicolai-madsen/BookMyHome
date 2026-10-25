namespace BookMyHome.AuthService;

public class JwtOptions
{
    public string PrivateKeyPath { get; set; } = "";
    public string KeyId { get; set; } = "";
    public string Issuer { get; init; } = "";
    public string Audience { get; init; } = "";
    public int ExpiryMinutes { get; init; }
}