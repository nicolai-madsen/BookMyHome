namespace BookMyHome.Application.Accommodations.CreateBooking
{
    public interface ICreateBookingUseCase
    {
        Task<Guid> ExecuteAsync(CreateBookingCommand command, CancellationToken ct = default);
    }
}
