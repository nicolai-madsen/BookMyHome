using BookMyHome.Application.Accommodations.Bookings.CancelBooking;
using BookMyHome.Application.Accommodations.Bookings.CreateBooking;
using BookMyHome.Application.Accommodations.Bookings.RescheduleBooking;
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

            services.AddScoped<ICreateBookingUseCase,  CreateBookingUseCase>();
            services.AddScoped<IRescheduleBookingUseCase, RescheduleBookingUseCase>();
            services.AddScoped<ICancelBookingUseCase, CancelBookingUseCase>();

            return services;
        }
    }
}
