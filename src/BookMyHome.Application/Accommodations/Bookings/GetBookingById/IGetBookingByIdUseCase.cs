using BookMyHome.Domain.Aggregates.Accommodations;

namespace BookMyHome.Application.Accommodations.Bookings.GetBookingById
{
    public interface IGetBookingByIdUseCase
    {
        Task<Booking> ExecuteAsync(GetBookingByIdQuery query, CancellationToken ct);
    }
}
