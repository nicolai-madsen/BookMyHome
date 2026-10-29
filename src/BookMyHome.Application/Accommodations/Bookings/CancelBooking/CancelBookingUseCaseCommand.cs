namespace BookMyHome.Application.Accommodations.Bookings.CancelBooking
{
    public record CancelBookingUseCaseCommand(
        Guid AccommodationId,
        Guid BookingId,
        Guid UserId);
}
