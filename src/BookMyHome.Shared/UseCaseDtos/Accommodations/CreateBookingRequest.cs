namespace BookMyHome.Shared.UseCaseDtos.Accommodations
{
    public record CreateBookingRequest(
        DateOnly StartDate,
        DateOnly EndDate);
}
