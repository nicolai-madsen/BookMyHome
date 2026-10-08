using BookMyHome.Application.Accommodations.CreateAccommodation;
using Microsoft.Extensions.DependencyInjection;

namespace BookMyHome.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<ICreateAccommodationUseCase, CreateAccommodationUseCase>();

            return services;
        }
    }
}
