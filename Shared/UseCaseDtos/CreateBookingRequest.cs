namespace Shared.UseCaseDtos
{
    public record CreateBookingRequest(
        Guid GuestId,
        DateOnly StartDate,
        DateOnly EndDate);
}
