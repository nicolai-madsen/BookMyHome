namespace BookMyHome.Application.Accommodations.RescheduleBooking
{
    public sealed record RescheduleBookingUseCaseCommand(
        Guid AccommodationId,
        Guid BookingId,
        Guid GuestId,
        DateOnly NewStartDate,
        DateOnly NewEndDate);
}
