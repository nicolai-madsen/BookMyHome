using Domain;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Persistence.Repositories;

namespace Persistence
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            services.AddDbContext<BookMyHomeContext>(options => 
                options
                    .UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
                    .LogTo(Console.WriteLine, LogLevel.Information) // Send EF Core logs to the console, so they're easy to find
                   
                    .EnableSensitiveDataLogging()); // Shows actual parameter values. !! IMPORTANTE !! REMOVE THIS IN PRODUCTION. It's a development tool, so yea

            services.AddScoped<IAccommodationRepository, AccommodationRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}
