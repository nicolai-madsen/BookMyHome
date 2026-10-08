namespace BookMyHome.Application.Accommodations.UpdateAccommodation
{
    public interface IUpdateAccommodationUseCase
    {
        Task ExecuteAsync(UpdateAccommodationUseCaseCommand command, CancellationToken ct = default);
    }
}