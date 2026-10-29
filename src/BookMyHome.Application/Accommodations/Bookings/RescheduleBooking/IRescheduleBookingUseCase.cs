namespace BookMyHome.Application.Accommodations.Bookings.RescheduleBooking
{
    public interface IRescheduleBookingUseCase
    {
        Task ExecuteAsync(RescheduleBookingUseCaseCommand command, CancellationToken ct);
    }
}
