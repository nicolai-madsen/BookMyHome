namespace BookMyHome.Application.Accommodations.Bookings.CreateBooking
{
    public sealed record CreateBookingCommand(
        Guid GuestId,
        Guid AccommodationId,
        DateOnly StartDate,
        DateOnly EndDate);
}
