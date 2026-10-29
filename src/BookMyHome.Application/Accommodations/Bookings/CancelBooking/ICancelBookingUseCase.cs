namespace BookMyHome.Application.Accommodations.Bookings.CancelBooking
{
    public interface ICancelBookingUseCase
    {
        Task ExecuteAsync(CancelBookingUseCaseCommand command, CancellationToken ct);
    }
}
