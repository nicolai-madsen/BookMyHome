namespace BookMyHome.Application.Accommodations.Bookings.GetBookingById
{
    public record GetBookingByIdQuery(
        Guid AccommodationId,
        Guid BookingId);
}
