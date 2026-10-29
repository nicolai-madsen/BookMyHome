using BookMyHome.Domain.Aggregates.Accommodations;

namespace BookMyHome.Application.Accommodations.Bookings.GetAccommodationBookings
{
    public interface IGetAccommodationBookingsUseCase
    {
        Task<IReadOnlyCollection<Booking>> ExecuteAsync(GetAccommodationBookingsQuery query, CancellationToken ct);
    }
}
