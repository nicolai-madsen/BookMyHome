namespace BookMyHome.Shared.UseCaseDtos.Accommodations
{
    public record RescheduleBookingRequest(
        Guid UserId, // This gets fucked right off, once JWT is implemented
        DateOnly NewStartDate,
        DateOnly NewEndDate);
}
