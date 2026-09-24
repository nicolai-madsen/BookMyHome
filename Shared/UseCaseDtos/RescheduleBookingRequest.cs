namespace Shared.UseCaseDtos
{
    public record RescheduleBookingRequest(
        DateOnly NewStartDate,
        DateOnly NewEndDate);
}
