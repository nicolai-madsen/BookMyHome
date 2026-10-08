using BookMyHome.Application.Accommodations.CreateAccommodation;
using BookMyHome.Application.Accommodations.DeleteAccommodation;
using BookMyHome.Application.Accommodations.UpdateAccommodation;
using Microsoft.Extensions.DependencyInjection;

namespace BookMyHome.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddSingleton(TimeProvider.System);

            services.AddScoped<ICreateAccommodationUseCase, CreateAccommodationUseCase>();
            services.AddScoped<IUpdateAccommodationUseCase, UpdateAccommodationUseCase>();
            services.AddScoped<IDeleteAccommodationUseCase, DeleteAccommodationUseCase>();

            return services;
        }
    }
}
