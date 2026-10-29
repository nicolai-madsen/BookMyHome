namespace BookMyHome.Shared.UseCaseDtos.Accommodations
{
    public record RescheduleBookingRequest(
        DateOnly NewStartDate,
        DateOnly NewEndDate);
}
