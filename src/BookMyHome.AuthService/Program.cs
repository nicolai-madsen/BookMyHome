using MySql.Data.MySqlClient;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Default") 
    ?? throw new InvalidOperationException("Connection string 'Default' is missing"); // fail-fast option, so the db fails on start if the connectionstring cant be found

var app = builder.Build();

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

app.MapPost("/login", async (UserLogin input) =>
{
    using var connection = new MySqlConnection(connectionString);
    await connection.OpenAsync();

    var cmd = new MySqlCommand(
        "SELECT Id, PasswordHash FROM Users WHERE Username = @u", connection);
    cmd.Parameters.AddWithValue("@u", input.Username);

    using var reader = await cmd.ExecuteReaderAsync();

    if (!await reader.ReadAsync())
    {
        BCrypt.Net.BCrypt.Verify(input.Password, dummyHash);
        return Results.Unauthorized();
    }

    var id = reader.GetGuid(0);
    var storedHash = reader.GetString(1);

    if (!BCrypt.Net.BCrypt.Verify(input.Password, storedHash))
        return Results.Unauthorized();

    return Results.Ok(new { id });
});

app.Run();

record UserLogin(string Username, string Password);