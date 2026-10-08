namespace BookMyHome.Application.Accommodations.DeleteAccommodation
{
    public interface IDeleteAccommodationUseCase
    {
        Task ExecuteAsync(DeleteAccommodationUseCaseCommand command, CancellationToken ct = default);
    }
}