namespace BookMyHome.Application.Accommodations.Bookings.CreateBooking
{
    public interface ICreateBookingUseCase
    {
        Task<Guid> ExecuteAsync(CreateBookingCommand command, CancellationToken ct = default);
    }
}
