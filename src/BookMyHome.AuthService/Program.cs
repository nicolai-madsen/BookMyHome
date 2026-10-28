using MySql.Data.MySqlClient;
using BookMyHome.AuthService;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Default") 
    ?? throw new InvalidOperationException("Connection string 'Default' is missing"); // fail-fast option, so the db fails on start if the connectionstring cant be found

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddSingleton<TokenService>();

// With a symmetric signing key, every service that can verify tokens can also forge them.
// With RSA, only AuthService has the private key and can sign tokens;
// other services verify with the public key, so a compromised service can't forge tokens.
builder.Services.AddSingleton(sp =>
{
    var jwt = sp.GetRequiredService<IOptions<JwtOptions>>().Value;

    var rsa = RSA.Create();
 
    rsa.ImportFromPem(File.ReadAllText(jwt.PrivateKeyPath));

    // The key needs a KeyId to be set, otherwise the JWT token will not contain a "kid" header, which is required for clients to verify their tokens, if the key changes (key rotation).
    var key = new RsaSecurityKey(rsa) { KeyId = jwt.KeyId };
    return new SigningCredentials(key, SecurityAlgorithms.RsaSha256);
});

var app = builder.Build();

// Fail fast: resolve the signing credentials at startup, so a missing .pem file crashes the app immediately instead of on the first login.
_ = app.Services.GetRequiredService<SigningCredentials>();

var dummyHash = BCrypt.Net.BCrypt.HashPassword("dummy-password");

app.MapPost("/register", async (UserLogin input) =>
{
    var id = Guid.NewGuid().ToString();
    var hash = BCrypt.Net.BCrypt.HashPassword(input.Password);

    using var connection = new MySqlConnection(connectionString);
    await connection.OpenAsync();

    var cmd = new MySqlCommand(
        "INSERT INTO Users (Id, Username, PasswordHash) VALUES (@id, @u, @p)", connection);
    cmd.Parameters.AddWithValue("@id", id);
    cmd.Parameters.AddWithValue("@u", input.Username);
    cmd.Parameters.AddWithValue("@p", hash);

    try
    {
        await cmd.ExecuteNonQueryAsync();
    }
    catch (MySqlException ex) when (ex.Number == (int)MySqlErrorCode.DuplicateKeyEntry)
    {
        return Results.Conflict("Username already exists.");
    }

    return Results.Ok(new { id });
});

app.MapPost("/login", async (UserLogin input, TokenService tokens) =>
{
    using var connection = new MySqlConnection(connectionString);
    await connection.OpenAsync();

    var cmd = new MySqlCommand(
        "SELECT Id, PasswordHash FROM Users WHERE Username = @u", connection);
    cmd.Parameters.AddWithValue("@u", input.Username);

    using var reader = await cmd.ExecuteReaderAsync();

    if (!await reader.ReadAsync())
    {
        BCrypt.Net.BCrypt.Verify(input.Password, dummyHash); // Verify against a dummy hash so unknown usernames takes as long as a wrong password,
                                                             // to prevent timing attacks that could reveal which accounts exists (user enumeration).
        return Results.Unauthorized();
    }

    var id = reader.GetGuid(0);
    var storedHash = reader.GetString(1);

    if (!BCrypt.Net.BCrypt.Verify(input.Password, storedHash))
        return Results.Unauthorized();

    return Results.Ok(new { token = tokens.CreateToken(id, input.Username) });
});

app.Run();

record UserLogin(string Username, string Password);