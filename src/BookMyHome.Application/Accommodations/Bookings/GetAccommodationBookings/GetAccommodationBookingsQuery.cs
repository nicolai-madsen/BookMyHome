namespace BookMyHome.Application.Accommodations.Bookings.GetAccommodationBookings
{
    public record GetAccommodationBookingsQuery(
        Guid UserId,
        Guid AccommodationId);
}
