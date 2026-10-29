var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddCors(o => o.AddPolicy("web", p => p
    .WithOrigins("http://localhost:5xxx", "https://localhost:7046")
    .AllowAnyHeader()
    .AllowAnyMethod()));

var app = builder.Build();

app.UseCors("web");

app.MapReverseProxy();

app.Run();
