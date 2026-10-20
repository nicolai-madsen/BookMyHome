namespace BookMyHome.Shared.UseCaseDtos.Accommodations
{
    public record CreateBookingRequest(
        Guid GuestId,
        DateOnly StartDate,
        DateOnly EndDate);
}
