namespace BookMyHome.Application.Accommodations.Bookings.RescheduleBooking
{
    public sealed record RescheduleBookingUseCaseCommand(
        Guid AccommodationId,
        Guid BookingId,
        Guid UserId,
        DateOnly NewStartDate,
        DateOnly NewEndDate);
}
