namespace BookMyHome.Application.Accommodations.RescheduleBooking
{
    public interface IRescheduleBookingUseCase
    {
        Task ExecuteAsync(RescheduleBookingUseCaseCommand command, CancellationToken ct);
    }
}
